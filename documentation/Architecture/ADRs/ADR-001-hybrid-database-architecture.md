# ADR-001: Hybrid Database Architecture (PostgreSQL + MongoDB)

## Status
Accepted

## Date
2025-12-02

## Context

The Workflow Manager system needs to handle multiple concerns:

1. **Structured relational data** with strong consistency requirements:
   - Tenant (company) management with referential integrity
   - User authentication and authorization
   - Market role assignments with contractual relationships
   - Workflow template definitions and versioning

2. **Flexible document-oriented data** with evolving schemas:
   - Workflow instance runtime state (frequently changing, deeply nested)
   - Event sourcing for audit trails (append-only, high write volume)
   - Step-specific state data (varies significantly by workflow type)
   - Market role-specific metadata (different structure per role)

3. **Multi-tenancy** with strong isolation:
   - Market participants (companies) must have complete data isolation
   - Market Operations team needs cross-tenant visibility
   - Compliance requires complete audit trails

4. **Operational requirements**:
   - Workflow state changes are frequent (pause, resume, step transitions)
   - Queries span both structured relationships and flexible state
   - Schema evolution for workflow state without downtime
   - Event sourcing pattern for complete history

## Decision

We will implement a **hybrid database architecture** using both PostgreSQL and MongoDB:

### PostgreSQL: Core Domain & Referential Integrity
PostgreSQL will store:
- **Tenants**: Company information, status, legal details
- **Users**: Authentication, authorization, tenant membership
- **Tenant Market Roles**: Market role assignments per company
- **Workflow Templates**: Versioned workflow definitions
- **Workflow Metadata**: Searchable workflow instance metadata (id, tenant, status, timestamps)

### MongoDB: Workflow Runtime & Events
MongoDB will store:
- **Workflow Instances**: Complete runtime state with nested step data
- **Workflow Events**: Append-only event log for audit and rollback
- **Step State**: Flexible, market-role-specific step data

### Data Synchronization Pattern
```
PostgreSQL (Source of Truth)          MongoDB (Workflow Runtime)
├── tenants                           ├── workflow_instances
│   ├── id (PK)                       │   ├── _id
│   ├── company_name                  │   ├── tenantId (denormalized)
│   └── status                        │   ├── tenantName (denormalized)
│                                     │   ├── marketRole
├── workflow_templates                │   ├── templateId
│   ├── id (PK)                       │   ├── status
│   ├── definition (JSONB)            │   └── state (nested document)
│   └── version                       │
                                      └── workflow_events
├── workflow_instances_index              ├── _id
│   ├── id (PK)                          ├── workflowInstanceId
│   ├── tenant_id (FK)                   ├── tenantId
│   ├── mongo_id                         ├── eventType
│   ├── status                           ├── stepId
│   └── current_step                     └── eventData
```

## Architecture Patterns

### 1. Write Pattern
```typescript
// When creating a workflow instance
async createWorkflow(tenantId: string, marketRole: MarketRole, templateId: string) {
  // Start transaction
  const pgClient = await pg.connect();
  const mongoSession = await mongo.startSession();

  try {
    await pgClient.query('BEGIN');
    await mongoSession.startTransaction();

    // 1. Verify tenant in PostgreSQL
    const tenant = await pgClient.query(
      'SELECT * FROM tenants WHERE id = $1 AND status = $2',
      [tenantId, 'active']
    );

    if (!tenant.rows[0]) throw new Error('Invalid tenant');

    // 2. Get template definition from PostgreSQL
    const template = await pgClient.query(
      'SELECT * FROM workflow_templates WHERE id = $1',
      [templateId]
    );

    // 3. Create workflow instance in MongoDB
    const workflowDoc = {
      tenantId,
      tenantName: tenant.rows[0].company_name, // Denormalized
      marketRole,
      templateId,
      templateVersion: template.rows[0].version,
      status: 'draft',
      state: initializeWorkflowState(template.rows[0].definition),
      createdAt: new Date()
    };

    const result = await mongo.db('workflows')
      .collection('workflow_instances')
      .insertOne(workflowDoc, { session: mongoSession });

    // 4. Create index entry in PostgreSQL
    await pgClient.query(
      `INSERT INTO workflow_instances_index
       (id, tenant_id, mongo_id, status, current_step)
       VALUES ($1, $2, $3, $4, $5)`,
      [
        result.insertedId.toString(),
        tenantId,
        result.insertedId.toString(),
        'draft',
        workflowDoc.state.currentStepId
      ]
    );

    // 5. Commit both
    await mongoSession.commitTransaction();
    await pgClient.query('COMMIT');

    return result.insertedId;

  } catch (error) {
    await mongoSession.abortTransaction();
    await pgClient.query('ROLLBACK');
    throw error;
  } finally {
    await mongoSession.endSession();
    pgClient.release();
  }
}
```

### 2. Query Pattern
```typescript
// Market Ops: Cross-tenant reporting (PostgreSQL)
async getWorkflowSummaryByTenant(): Promise<TenantWorkflowSummary[]> {
  return await pg.query(`
    SELECT
      t.company_name,
      t.vat_number,
      wi.status,
      COUNT(*) as workflow_count,
      AVG(EXTRACT(EPOCH FROM (NOW() - wi.created_at))/3600) as avg_duration_hours
    FROM workflow_instances_index wi
    JOIN tenants t ON wi.tenant_id = t.id
    GROUP BY t.id, t.company_name, t.vat_number, wi.status
    ORDER BY t.company_name, wi.status
  `);
}

// Tenant-specific: Detailed workflow state (MongoDB)
async getWorkflowInstance(workflowId: string): Promise<WorkflowInstance> {
  return await mongo.db('workflows')
    .collection('workflow_instances')
    .findOne({ _id: new ObjectId(workflowId) });
}

// Combined: Search with filters (PostgreSQL) + Details (MongoDB)
async searchWorkflows(filters: WorkflowFilters): Promise<WorkflowInstance[]> {
  // 1. Query PostgreSQL index for fast filtering
  const indexResults = await pg.query(`
    SELECT mongo_id
    FROM workflow_instances_index
    WHERE tenant_id = $1
      AND status = ANY($2)
      AND market_role = $3
    ORDER BY created_at DESC
    LIMIT $4
  `, [filters.tenantId, filters.statuses, filters.marketRole, filters.limit]);

  const mongoIds = indexResults.rows.map(r => new ObjectId(r.mongo_id));

  // 2. Fetch full documents from MongoDB
  return await mongo.db('workflows')
    .collection('workflow_instances')
    .find({ _id: { $in: mongoIds } })
    .toArray();
}
```

### 3. Event Sourcing Pattern
```typescript
// Events always written to MongoDB
async recordWorkflowEvent(event: WorkflowEvent): Promise<void> {
  // No transaction needed - MongoDB single document write is atomic
  await mongo.db('workflows')
    .collection('workflow_events')
    .insertOne({
      workflowInstanceId: event.workflowId,
      tenantId: event.tenantId,
      eventType: event.type,
      stepId: event.stepId,
      eventData: event.data,
      performedBy: event.userId,
      occurredAt: new Date()
    });

  // Update PostgreSQL index for queryable fields
  if (event.type === 'STEP_COMPLETED' || event.type === 'STATUS_CHANGED') {
    await pg.query(`
      UPDATE workflow_instances_index
      SET status = $1, current_step = $2, updated_at = NOW()
      WHERE mongo_id = $3
    `, [event.data.newStatus, event.data.currentStep, event.workflowId]);
  }
}

// Rebuild workflow state from events (for rollback)
async rebuildWorkflowState(workflowId: string, untilTimestamp?: Date): Promise<WorkflowState> {
  const events = await mongo.db('workflows')
    .collection('workflow_events')
    .find({
      workflowInstanceId: workflowId,
      ...(untilTimestamp && { occurredAt: { $lte: untilTimestamp } })
    })
    .sort({ occurredAt: 1 })
    .toArray();

  // Replay events to rebuild state
  let state = createInitialState();
  for (const event of events) {
    state = applyEvent(state, event);
  }

  return state;
}
```

## Consequences

### Positive

1. **Best of Both Worlds**
   - PostgreSQL provides ACID guarantees for critical relationships
   - MongoDB provides flexibility for evolving workflow schemas
   - Each database used for its strengths

2. **Schema Flexibility**
   - Workflow state can evolve without database migrations
   - Different market roles can have different state structures
   - Step definitions can change without schema changes

3. **Performance Optimization**
   - PostgreSQL indexes for fast tenant filtering and reporting
   - MongoDB for fast document retrieval and nested queries
   - Can optimize each database independently

4. **Event Sourcing**
   - MongoDB excellent for append-only event logs
   - Fast writes for high-frequency state changes
   - Complete audit trail for compliance

5. **Scalability Path**
   - PostgreSQL for structured data (vertical scaling sufficient)
   - MongoDB for workflow documents (horizontal sharding if needed)
   - Can scale each tier independently

### Negative

1. **Operational Complexity**
   - Two databases to manage, monitor, backup, and restore
   - Need expertise in both PostgreSQL and MongoDB
   - More complex deployment and infrastructure

2. **No Distributed Transactions**
   - Cannot guarantee atomic commits across both databases
   - Must implement compensating transactions for failures
   - Potential for temporary inconsistency

3. **Data Synchronization**
   - Must keep PostgreSQL index in sync with MongoDB
   - Denormalized data (tenant name) can become stale
   - Need reconciliation processes

4. **Testing Complexity**
   - Tests must set up both databases
   - Integration tests more complex
   - Local development requires both databases running

5. **Learning Curve**
   - Team must understand both databases
   - Different query languages and patterns
   - More documentation and training needed

### Mitigation Strategies

1. **Eventual Consistency**
   - Accept that PostgreSQL index may lag MongoDB slightly
   - Use MongoDB as source of truth for workflow state
   - PostgreSQL index is for search/filtering only

2. **Compensating Transactions**
   - Implement saga pattern for multi-database operations
   - Idempotent operations for retry safety
   - Background reconciliation jobs

3. **Monitoring & Alerting**
   - Monitor sync lag between databases
   - Alert on failed transactions or inconsistencies
   - Health checks for both databases

4. **Development Tools**
   - Docker Compose for local setup (both databases)
   - Seeding scripts for consistent test data
   - Database migration tools for PostgreSQL

## Alternatives Considered

### Alternative 1: PostgreSQL Only (with JSONB)
- **Pros**: Single database, ACID transactions, simpler operations
- **Cons**: Less flexible for nested workflow state, schema migrations needed
- **Rejected**: Workflow state evolution would require frequent migrations

### Alternative 2: MongoDB Only
- **Pros**: Maximum flexibility, natural document model
- **Cons**: No foreign key constraints, tenant isolation in application code
- **Rejected**: Loss of relational integrity for critical tenant/user data

### Alternative 3: Event Store (EventStoreDB) + Read Database
- **Pros**: Purpose-built for event sourcing, excellent audit trail
- **Cons**: Additional technology, less mature ecosystem
- **Rejected**: Too specialized, team unfamiliarity

## Implementation Plan

### Phase 1: Core Schema (Week 1-2)
- Set up PostgreSQL schema (tenants, users, workflow_templates)
- Set up MongoDB collections (workflow_instances, workflow_events)
- Implement connection management and health checks

### Phase 2: Basic CRUD (Week 3-4)
- Implement create/read workflows with both databases
- Build synchronization layer
- Add basic event sourcing

### Phase 3: Advanced Features (Week 5-6)
- Implement pause/resume/rollback using events
- Add cross-database queries
- Build reconciliation jobs

### Phase 4: Production Readiness (Week 7-8)
- Monitoring and alerting
- Backup and restore procedures
- Performance optimization

## References

- [PostgreSQL JSONB Documentation](https://www.postgresql.org/docs/current/datatype-json.html)
- [MongoDB Transaction Documentation](https://docs.mongodb.com/manual/core/transactions/)
- [Saga Pattern for Distributed Transactions](https://microservices.io/patterns/data/saga.html)
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)

## Related ADRs

- ADR-002: Multi-Tenancy Pattern (Row-Level Security) [Pending]
- ADR-003: Workflow Engine Design (Hybrid State Machine + Event Sourcing) [Pending]
- ADR-004: API Layer Architecture (GraphQL) [Pending]
