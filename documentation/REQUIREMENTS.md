# Workflow Manager - Comprehensive Requirements Specification

**Project**: Elia Group Workflow Manager
**Version**: 1.0
**Date**: 2025-12-05
**Status**: Draft for Review

---

## Document Purpose

This document provides a complete taxonomy of requirements for the Workflow Manager system, properly categorized using industry-standard requirement types.

---

## Requirements Taxonomy

Requirements are organized into the following categories:

1. **Functional Requirements** - What the system must DO
2. **Non-Functional Requirements** - How the system must BE (quality attributes)
3. **Constraints** - Limitations we must work within
4. **Interface Requirements** - How system interacts externally
5. **Data Requirements** - What data system must handle

---

## 1. Functional Requirements

### 1.1 Workflow Management

#### FR-WF-001: Workflow Template Creation

**Priority**: Must Have
**Description**: System must allow authorized users to create workflow templates for different market roles.

**Acceptance Criteria**:

- ✅ Create templates for 7 market roles: BRP, BSP, GU, ACH, CRM, ESP, DSO
- ✅ Define steps with custom configuration (forms, approvals, API calls, notifications)
- ✅ Configure step transitions and conditional logic
- ✅ Version templates (V1, V2, etc.)
- ✅ Activate/deactivate templates

**User Story**:

> As a Market Ops Admin, I want to create a BRP onboarding workflow template so that new Balance Responsible Parties can be onboarded consistently.

---

#### FR-WF-002: Workflow Instance Creation

**Priority**: Must Have
**Description**: System must create workflow instances from templates for specific tenants.

**Acceptance Criteria**:

- ✅ Instantiate workflow from template
- ✅ Associate with tenant (market participant)
- ✅ Set initial state to "draft"
- ✅ Generate unique workflow ID
- ✅ Record creator and creation timestamp

**User Story**:

> As a Market Ops User, I want to start a new BRP onboarding workflow for "Engie Belgium" so that we can manage their contract setup.

---

#### FR-WF-003: Step Execution

**Priority**: Must Have
**Description**: System must execute workflow steps according to template definition.

**Acceptance Criteria**:

- ✅ Execute steps in defined order
- ✅ Support step types: Form, Approval, API Call, Notification
- ✅ Validate step input before execution
- ✅ Persist step state after execution
- ✅ Transition to next step on success
- ✅ Handle step failure gracefully

**User Story**:

> As a Market Ops User, I want to fill out the "Company Information" form step and have it automatically advance to the next step when complete.

---

#### FR-WF-004: Pause Workflow

**Priority**: Must Have
**Description**: System must allow pausing workflows at any step.

**Acceptance Criteria**:

- ✅ Pause workflow mid-execution
- ✅ Preserve current state exactly
- ✅ Record pause timestamp and user
- ✅ Show "paused" status in UI
- ✅ Prevent further execution until resumed

**User Story**:

> As a Market Ops User, I want to pause a workflow when waiting for external information so that I can resume it later without losing progress.

---

#### FR-WF-005: Resume Workflow

**Priority**: Must Have
**Description**: System must allow resuming paused workflows from exact pause point.

**Acceptance Criteria**:

- ✅ Resume from exact step where paused
- ✅ Restore all intermediate state
- ✅ Record resume timestamp and user
- ✅ Transition status from "paused" to "in_progress"
- ✅ Continue execution seamlessly

**User Story**:

> As a Market Ops User, I want to resume a paused workflow after receiving missing documents so that we can complete the onboarding process.

---

#### FR-WF-006: Rollback Workflow

**Priority**: Must Have
**Description**: System must support rolling back workflows to previous steps with compensation logic.

**Acceptance Criteria**:

- ✅ Rollback to any previously completed step
- ✅ Execute compensation logic for rolled-back steps (undo actions)
- ✅ Preserve audit trail of rollback operation
- ✅ Allow re-execution of steps after rollback
- ✅ Validate rollback target is valid

**User Story**:

> As a Market Ops User, I want to rollback to the "Company Information" step if we discover incorrect data later in the workflow.

---

#### FR-WF-007: Workflow Validation

**Priority**: Must Have
**Description**: System must validate complete workflow state before submission.

**Acceptance Criteria**:

- ✅ Validate all completed steps
- ✅ Check for missing required data
- ✅ Run custom validation rules per market role
- ✅ Return detailed validation errors with field-level information
- ✅ Prevent submission if validation fails

**User Story**:

> As a Market Ops User, I want the system to validate all data before I submit the workflow so that I catch errors early.

---

#### FR-WF-008: Workflow Submission

**Priority**: Must Have
**Description**: System must allow submitting validated workflows for approval.

**Acceptance Criteria**:

- ✅ Transition status to "submitted"
- ✅ Run final validation before submission
- ✅ Record submission timestamp and user
- ✅ Generate submission confirmation
- ✅ Trigger approval notifications

**User Story**:

> As a Market Ops User, I want to submit a completed workflow for approval so that the onboarding can be finalized.

---

#### FR-WF-009: Workflow Completion

**Priority**: Must Have
**Description**: System must mark workflows as completed after approval.

**Acceptance Criteria**:

- ✅ Transition status to "completed"
- ✅ Record completion timestamp
- ✅ Finalize all state changes
- ✅ Trigger completion notifications
- ✅ Archive workflow (read-only mode)

---

#### FR-WF-010: Long-Running Workflow Support

**Priority**: Must Have
**Description**: System must support workflows that run for days or weeks.

**Acceptance Criteria**:

- ✅ No timeout on workflow duration
- ✅ Persist state reliably for long periods
- ✅ Handle server restarts without data loss
- ✅ Support workflows with 100+ steps
- ✅ Maintain performance over long-running workflows

---

### 1.2 Step Types

#### FR-ST-001: Form Step

**Priority**: Must Have
**Description**: System must support form input steps with validation.

**Acceptance Criteria**:

- ✅ Define form schema (fields, types, constraints)
- ✅ Validate input against schema
- ✅ Support field types: text, number, date, select, file upload, etc.
- ✅ Show validation errors inline
- ✅ Save form data to workflow state

---

#### FR-ST-002: Approval Step

**Priority**: Must Have
**Description**: System must support approval steps with configurable approvers.

**Acceptance Criteria**:

- ✅ Assign approver (user or role)
- ✅ Notify approver of pending approval
- ✅ Approver can approve or reject with comments
- ✅ Rejection sends workflow back to previous step
- ✅ Approval advances workflow

---

#### FR-ST-003: API Call Step

**Priority**: Must Have
**Description**: System must support calling external APIs during workflow execution.

**Acceptance Criteria**:

- ✅ Configure API endpoint, method, headers, body
- ✅ Execute HTTP request
- ✅ Handle response (success/error)
- ✅ Retry on transient failures (configurable)
- ✅ Store response in workflow state

**Example Use Case**:

> Call Kong API gateway to create API credentials for new market participant

---

#### FR-ST-004: Notification Step

**Priority**: Must Have
**Description**: System must support sending notifications during workflow execution.

**Acceptance Criteria**:

- ✅ Send email notifications
- ✅ Support templates with variable substitution
- ✅ Configure recipients (static or dynamic)
- ✅ Record notification sent in audit log
- ✅ Handle delivery failures gracefully

---

### 1.3 Multi-Tenancy

#### FR-MT-001: Tenant Isolation

**Priority**: Must Have
**Description**: System must enforce strict tenant isolation at all layers.

**Acceptance Criteria**:

- ✅ Each workflow belongs to exactly one tenant
- ✅ Users can only access workflows for their tenant(s)
- ✅ Database-level isolation using Row-Level Security (RLS)
- ✅ No cross-tenant data leakage
- ✅ Audit logs include tenant context

**Security Implication**: Critical for regulatory compliance and data privacy

---

#### FR-MT-002: Tenant Management

**Priority**: Must Have
**Description**: System must allow creating and managing tenants (market participants).

**Acceptance Criteria**:

- ✅ Create tenant with company name, VAT number, etc.
- ✅ Assign market roles to tenant (BRP, BSP, GU, etc.)
- ✅ Activate/deactivate tenants
- ✅ Track tenant status (onboarding, active, suspended)

---

#### FR-MT-003: Market Ops Cross-Tenant Access

**Priority**: Must Have
**Description**: Market Ops users must be able to access workflows across all tenants.

**Acceptance Criteria**:

- ✅ Market Ops role bypasses RLS restrictions
- ✅ Can view/manage workflows for any tenant
- ✅ Audit log records which tenant data was accessed
- ✅ Cannot impersonate tenant users

---

### 1.4 Audit & Compliance

#### FR-AU-001: Event Sourcing

**Priority**: Must Have
**Description**: System must record all workflow state changes as immutable events.

**Acceptance Criteria**:

- ✅ Every state change creates an event
- ✅ Events are append-only (never modified/deleted)
- ✅ Events include: timestamp, user, event type, data
- ✅ Can replay events to reconstruct state at any point in time
- ✅ Events stored permanently (7-year retention for compliance)

**Regulatory Driver**: Energy sector audit requirements

---

#### FR-AU-002: Audit Trail Query

**Priority**: Must Have
**Description**: System must provide queryable audit trail for all workflows.

**Acceptance Criteria**:

- ✅ Query events by workflow ID
- ✅ Query events by tenant
- ✅ Query events by user
- ✅ Query events by date range
- ✅ Filter by event type
- ✅ Export audit logs (CSV, JSON)

**User Story**:

> As a Compliance Officer, I want to view all state changes for a specific workflow to demonstrate audit compliance.

---

#### FR-AU-003: User Action Tracking

**Priority**: Must Have
**Description**: System must track who performed which actions.

**Acceptance Criteria**:

- ✅ Record user ID for every action
- ✅ Record timestamp with millisecond precision
- ✅ Support "performed on behalf of" scenarios
- ✅ Cannot delete or modify historical actions

---

#### FR-AU-004: Point-in-Time Recovery

**Priority**: Should Have
**Description**: System must support reconstructing workflow state at any historical point.

**Acceptance Criteria**:

- ✅ Replay events up to specific timestamp
- ✅ Show workflow state as it was on specific date
- ✅ Compare current state vs historical state
- ✅ Performance: <5 seconds for workflows with <100 events

**Use Case**: Debugging, regulatory inquiries, dispute resolution

---

### 1.5 User Management

#### FR-UM-001: Authentication

**Priority**: Must Have
**Description**: System must authenticate users via JWT tokens.

**Acceptance Criteria**:

- ✅ Issue JWT on successful login
- ✅ Include user ID, tenant ID, role, permissions in token
- ✅ Token expiration (configurable, default 8 hours)
- ✅ Refresh token mechanism
- ✅ Logout invalidates token

---

#### FR-UM-002: Authorization (RBAC)

**Priority**: Must Have
**Description**: System must enforce role-based access control.

**Roles**:

- **Market Ops Admin**: Full access to all tenants and workflows
- **Market Ops User**: View/edit workflows for all tenants
- **Tenant Admin**: Manage users and workflows for their tenant
- **Tenant User**: View/edit assigned workflows for their tenant

**Acceptance Criteria**:

- ✅ Assign roles to users
- ✅ Enforce permissions at API layer
- ✅ GraphQL resolvers check user permissions
- ✅ Return 403 Forbidden for unauthorized access

---

### 1.6 Integrations

#### FR-INT-001: Kong API Gateway Integration

**Priority**: Must Have
**Description**: System must integrate with Kong to create API credentials for new market participants.

**Acceptance Criteria**:

- ✅ Call Kong Admin API to create consumer
- ✅ Generate API key for consumer
- ✅ Store API key in workflow state
- ✅ Handle Kong errors gracefully
- ✅ Retry on transient failures

---

#### FR-INT-002: Notification Service Integration

**Priority**: Must Have
**Description**: System must send email notifications via notification service.

**Acceptance Criteria**:

- ✅ Call notification service API
- ✅ Support email templates
- ✅ Track notification delivery status
- ✅ Handle service unavailability

---

---

## 2. Non-Functional Requirements

### 2.1 Performance Requirements

#### NFR-PERF-001: API Response Time

**Priority**: Must Have
**Requirement**: GraphQL API must respond within acceptable latency thresholds.

**Acceptance Criteria**:

- ✅ 95th percentile: <200ms for queries
- ✅ 95th percentile: <500ms for mutations
- ✅ 99th percentile: <1s for complex operations
- ✅ Measured under normal load (100 concurrent users)

**Measurement**: Use APM tooling (Prometheus + Grafana)

---

#### NFR-PERF-002: Database Query Performance

**Priority**: Must Have
**Requirement**: Database queries must be optimized for sub-second response.

**Acceptance Criteria**:

- ✅ Workflow list query: <100ms
- ✅ Workflow detail query (with events): <200ms
- ✅ Event query (last 100 events): <50ms
- ✅ Tenant query with workflows: <150ms

**Implementation**: Proper indexing on PostgreSQL and MongoDB

---

#### NFR-PERF-003: Concurrent Workflow Execution

**Priority**: Must Have
**Requirement**: System must support 100+ concurrent active workflows.

**Acceptance Criteria**:

- ✅ No degradation with 100 concurrent workflows
- ✅ Linear scaling up to 500 workflows
- ✅ Load testing validates performance

---

#### NFR-PERF-004: Event Replay Performance

**Priority**: Should Have
**Requirement**: Replaying events to rebuild state must be fast.

**Acceptance Criteria**:

- ✅ Replay <100 events: <1 second
- ✅ Replay 100-500 events: <3 seconds
- ✅ Replay >500 events: <10 seconds

**Optimization**: Event snapshots every 100 events

---

### 2.2 Scalability Requirements

#### NFR-SCALE-001: Tenant Scalability

**Priority**: Must Have
**Requirement**: System must scale to 1000+ tenants.

**Acceptance Criteria**:

- ✅ No performance degradation with 1000 tenants
- ✅ RLS policies remain performant
- ✅ Proper partitioning if needed

---

#### NFR-SCALE-002: Workflow Scalability

**Priority**: Must Have
**Requirement**: System must handle large numbers of workflows.

**Acceptance Criteria**:

- ✅ Support 10,000+ total workflows
- ✅ Support 1,000+ active (in-progress) workflows simultaneously
- ✅ No performance degradation with workflow count growth

---

#### NFR-SCALE-003: Event Log Scalability

**Priority**: Must Have
**Requirement**: Event log must scale to millions of events.

**Acceptance Criteria**:

- ✅ Append-only design (no updates)
- ✅ Archival strategy for old events (>1 year)
- ✅ Partitioning by tenant or date if needed

---

### 2.3 Reliability & Availability

#### NFR-REL-001: Uptime SLA

**Priority**: Must Have
**Requirement**: System must achieve 99.9% uptime.

**Calculation**: Max 8.76 hours downtime per year
**Acceptance Criteria**:

- ✅ Monitored uptime via health checks
- ✅ Automated alerts on downtime
- ✅ Incident response SLA: <30 minutes

---

#### NFR-REL-002: Fault Tolerance

**Priority**: Must Have
**Requirement**: System must gracefully handle failures.

**Acceptance Criteria**:

- ✅ Database connection failures: retry with exponential backoff
- ✅ External API failures: circuit breaker pattern
- ✅ Worker process crashes: restart automatically
- ✅ No data loss on crashes

---

#### NFR-REL-003: Data Durability

**Priority**: Must Have
**Requirement**: Zero data loss guarantee.

**Acceptance Criteria**:

- ✅ PostgreSQL replication (master-replica)
- ✅ MongoDB replica set (3 nodes minimum)
- ✅ Daily backups with point-in-time recovery
- ✅ Backup retention: 30 days

---

#### NFR-REL-004: Disaster Recovery

**Priority**: Should Have
**Requirement**: System must recover from catastrophic failures.

**Acceptance Criteria**:

- ✅ RTO (Recovery Time Objective): <4 hours
- ✅ RPO (Recovery Point Objective): <15 minutes
- ✅ Documented disaster recovery runbook
- ✅ Quarterly DR drills

---

### 2.4 Security Requirements

#### NFR-SEC-001: Authentication Security

**Priority**: Must Have
**Requirement**: Secure authentication using industry standards.

**Acceptance Criteria**:

- ✅ JWT signed with RS256 (asymmetric)
- ✅ Password hashing with bcrypt (cost factor 12)
- ✅ HTTPS only (TLS 1.2+)
- ✅ No credentials in logs

---

#### NFR-SEC-002: Data Encryption

**Priority**: Must Have
**Requirement**: Sensitive data must be encrypted.

**Acceptance Criteria**:

- ✅ Encryption at rest: PostgreSQL + MongoDB disk encryption
- ✅ Encryption in transit: TLS 1.2+ for all connections
- ✅ Encrypted backups
- ✅ Secrets managed via vault (not in code)

---

#### NFR-SEC-003: GDPR Compliance

**Priority**: Must Have
**Requirement**: System must comply with GDPR.

**Acceptance Criteria**:

- ✅ User consent tracking
- ✅ Right to access (data export)
- ✅ Right to deletion (anonymization)
- ✅ Data retention policies enforced
- ✅ Privacy policy displayed

---

#### NFR-SEC-004: Row-Level Security (RLS)

**Priority**: Must Have
**Requirement**: Database enforces tenant isolation.

**Acceptance Criteria**:

- ✅ PostgreSQL RLS policies on all tenant tables
- ✅ SET app.current_tenant before every query
- ✅ RLS bypassed only for Market Ops role
- ✅ Audit log of RLS bypasses

---

#### NFR-SEC-005: Vulnerability Management

**Priority**: Must Have
**Requirement**: Dependencies must be kept secure.

**Acceptance Criteria**:

- ✅ Weekly dependency scans (npm audit)
- ✅ Critical vulnerabilities patched within 7 days
- ✅ High vulnerabilities patched within 30 days
- ✅ Automated alerts on new vulnerabilities

---

### 2.5 Usability Requirements

#### NFR-USE-001: User Onboarding

**Priority**: Should Have
**Requirement**: New users should be productive quickly.

**Acceptance Criteria**:

- ✅ Onboarding wizard for first login
- ✅ Contextual help/tooltips in UI
- ✅ Video tutorials for common tasks
- ✅ Time to first workflow creation: <15 minutes

---

#### NFR-USE-002: Responsive Design

**Priority**: Should Have
**Requirement**: UI must work on different screen sizes.

**Acceptance Criteria**:

- ✅ Desktop (1920x1080): optimal experience
- ✅ Laptop (1366x768): fully functional
- ✅ Tablet (768x1024): read-only acceptable
- ✅ Mobile (<768px): not supported (market ops desktop app)

---

#### NFR-USE-003: Accessibility

**Priority**: Should Have
**Requirement**: UI must be accessible to users with disabilities.

**Acceptance Criteria**:

- ✅ WCAG 2.1 Level AA compliance
- ✅ Keyboard navigation support
- ✅ Screen reader compatible
- ✅ Color contrast ratios meet standards

---

### 2.6 Maintainability Requirements

#### NFR-MAIN-001: Code Quality

**Priority**: Must Have
**Requirement**: Codebase must maintain high quality standards.

**Acceptance Criteria**:

- ✅ TypeScript strict mode enabled
- ✅ ESLint with no warnings
- ✅ Prettier for consistent formatting
- ✅ Code review required for all PRs
- ✅ No commented-out code in main branch

---

#### NFR-MAIN-002: Test Coverage

**Priority**: Must Have
**Requirement**: Code must be thoroughly tested.

**Acceptance Criteria**:

- ✅ Unit test coverage: >80%
- ✅ Integration test coverage: >70%
- ✅ E2E tests for critical workflows
- ✅ TDD approach (test-first)
- ✅ CI fails if coverage drops

---

#### NFR-MAIN-003: Documentation

**Priority**: Must Have
**Requirement**: System must be well-documented.

**Acceptance Criteria**:

- ✅ README with getting started guide
- ✅ API documentation (GraphQL schema + comments)
- ✅ Architecture Decision Records (ADRs)
- ✅ Runbooks for operations
- ✅ Code comments for complex logic

---

#### NFR-MAIN-004: Monitoring & Observability

**Priority**: Must Have
**Requirement**: System must be observable in production.

**Acceptance Criteria**:

- ✅ Structured logging (JSON format)
- ✅ Metrics exported to Prometheus
- ✅ Distributed tracing (OpenTelemetry)
- ✅ Health check endpoints
- ✅ Alerts on critical errors

---

### 2.7 Portability Requirements

#### NFR-PORT-001: Deployment Flexibility

**Priority**: Should Have
**Requirement**: System should deploy to multiple environments.

**Acceptance Criteria**:

- ✅ Docker containers for all services
- ✅ docker-compose for local development
- ✅ Kubernetes manifests (future)
- ✅ Deploy on AWS, Azure, or on-prem

---

#### NFR-PORT-002: Database Independence (Partial)

**Priority**: Nice to Have
**Requirement**: Minimize vendor lock-in to specific databases.

**Acceptance Criteria**:

- ⚠️ Use standard SQL for PostgreSQL (no proprietary extensions)
- ⚠️ MongoDB queries use standard API (not vendor-specific)
- ⚠️ Repository pattern abstracts database access

**Note**: Full database portability not required for MVP

---

---

## 3. Constraints

### 3.1 Technology Constraints

#### CONST-TECH-001: TypeScript Requirement

**Type**: Mandated
**Description**: All backend and frontend code must be TypeScript.

**Rationale**: Team expertise, type safety, maintainability

---

#### CONST-TECH-002: Node.js Runtime

**Type**: Mandated
**Description**: Backend must run on Node.js v22+.

**Rationale**: Team expertise, ecosystem maturity

---

#### CONST-TECH-003: PostgreSQL Database

**Type**: Mandated
**Description**: Use PostgreSQL 16+ for structured data.

**Rationale**: RLS support, team expertise, ACID compliance

---

#### CONST-TECH-004: MongoDB for Events

**Type**: Mandated
**Description**: Use MongoDB 7+ for workflow state and events.

**Rationale**: Flexible schema, fast writes for events, document model fits workflow state

**ADR Reference**: ADR-001 Hybrid Database Architecture

---

### 3.2 Business Constraints

#### CONST-BUS-001: Budget Limitation

**Type**: Imposed
**Description**: MVP development budget: €200k maximum.

**Impact**: Influences build vs buy decisions

---

#### CONST-BUS-002: Timeline

**Type**: Imposed
**Description**: MVP must launch within 3 months.

**Impact**: Aggressive timeline requires focus on core features

---

#### CONST-BUS-003: Team Size

**Type**: Imposed
**Description**: Development team: 2-3 developers.

**Impact**: Limits scope, requires efficient tooling

---

### 3.3 Regulatory Constraints

#### CONST-REG-001: GDPR Compliance

**Type**: Mandated
**Description**: Must comply with EU General Data Protection Regulation.

**Impact**: Data retention, anonymization, consent tracking required

---

#### CONST-REG-002: Energy Sector Regulations

**Type**: Mandated
**Description**: Must comply with Belgian energy market regulations.

**Impact**: Audit trail, 7-year retention, specific market role handling

---

### 3.4 Organizational Constraints

#### CONST-ORG-001: Existing Infrastructure

**Type**: Preference
**Description**: Prefer to use existing Elia infrastructure (Kong, PostgreSQL, etc.).

**Impact**: Integration requirements, hosting constraints

---

#### CONST-ORG-002: Open Source Preference

**Type**: Preference
**Description**: Prefer open-source solutions to minimize licensing costs.

**Impact**: Influences technology choices

---

---

## 4. Interface Requirements

### 4.1 External System Interfaces

#### INT-EXT-001: Kong API Gateway

**Type**: Outbound Integration
**Description**: Call Kong Admin API to create API credentials.

**Interface Specification**:

- **Protocol**: HTTP REST
- **Authentication**: Kong Admin Token
- **Endpoints**: POST /consumers, POST /consumers/{id}/key-auth
- **Data Format**: JSON

---

#### INT-EXT-002: Notification Service

**Type**: Outbound Integration
**Description**: Send email notifications.

**Interface Specification**:

- **Protocol**: HTTP REST or SMTP
- **Authentication**: API Key or SMTP credentials
- **Data Format**: JSON (REST) or MIME (SMTP)

---

### 4.2 User Interfaces

#### INT-UI-001: Admin Dashboard

**Type**: Web Application
**Description**: SvelteKit-based admin dashboard for Market Ops.

**Interface Specification**:

- **Technology**: SvelteKit, TailwindCSS, DaisyUI
- **Communication**: GraphQL over HTTP
- **Authentication**: JWT in Authorization header

---

#### INT-UI-002: GraphQL API

**Type**: API
**Description**: GraphQL API for frontend consumption.

**Interface Specification**:

- **Protocol**: HTTP POST to /graphql
- **Query Language**: GraphQL
- **Authentication**: JWT Bearer token
- **Data Format**: JSON
- **Schema**: See `apps/api/src/graphql/schema.graphql`

---

---

## 5. Data Requirements

### 5.1 Data Models

#### DATA-001: Tenant Data

**Storage**: PostgreSQL
**Retention**: Permanent (until tenant deletion)

**Schema**:

```sql
tenants (
  id UUID PRIMARY KEY,
  company_name TEXT NOT NULL,
  vat_number TEXT UNIQUE NOT NULL,
  status TEXT CHECK (status IN ('onboarding', 'active', 'suspended', 'deleted')),
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
)
```

---

#### DATA-002: Workflow Instance Index

**Storage**: PostgreSQL
**Retention**: Permanent (soft delete)

**Schema**:

```sql
workflow_instances_index (
  id UUID PRIMARY KEY,
  tenant_id UUID REFERENCES tenants(id),
  mongo_id TEXT NOT NULL,
  template_id UUID NOT NULL,
  status TEXT,
  current_step_id TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
)
```

**RLS Policy**: `tenant_id = current_setting('app.current_tenant')::uuid`

---

#### DATA-003: Workflow Instance State

**Storage**: MongoDB `workflow_instances` collection
**Retention**: Permanent

**Document Structure**:

```typescript
{
  _id: ObjectId,
  tenantId: ObjectId,
  templateId: ObjectId,
  status: 'draft' | 'in_progress' | 'paused' | 'submitted' | 'completed',
  currentStepId: string,
  state: {
    stepStates: {
      [stepId: string]: {
        status: 'pending' | 'in_progress' | 'completed' | 'failed',
        data: any,
        startedAt?: Date,
        completedAt?: Date,
        pausedAt?: Date
      }
    },
    metadata: any
  },
  createdAt: Date,
  updatedAt: Date
}
```

---

#### DATA-004: Workflow Events

**Storage**: MongoDB `workflow_events` collection
**Retention**: 7 years (regulatory requirement)

**Document Structure**:

```typescript
{
  _id: ObjectId,
  workflowId: ObjectId,
  tenantId: ObjectId,
  eventType: 'WORKFLOW_CREATED' | 'STEP_STARTED' | 'STEP_COMPLETED' | ...,
  stepId?: string,
  eventData: any,
  performedBy: ObjectId,
  occurredAt: Date
}
```

**Index**: `{ workflowId: 1, occurredAt: 1 }`

---

### 5.2 Data Retention & Archival

#### DATA-RET-001: Event Log Retention

**Requirement**: Event logs must be retained for 7 years for regulatory compliance.

**Implementation**:

- ✅ Hot storage: Last 2 years (queryable)
- ✅ Warm storage: Years 2-5 (archival, slower queries)
- ✅ Cold storage: Years 5-7 (archive-only, export to S3/Azure Blob)

---

#### DATA-RET-002: Completed Workflow Retention

**Requirement**: Completed workflows must be retained permanently unless tenant requests deletion.

**Implementation**:

- ✅ Soft delete (mark as deleted, don't purge)
- ✅ Hard delete only on explicit tenant request + legal hold check

---

### 5.3 Data Backup & Recovery

#### DATA-BACK-001: Database Backups

**Requirement**: Daily automated backups with point-in-time recovery.

**Implementation**:

- ✅ PostgreSQL: WAL archiving + daily snapshots
- ✅ MongoDB: Daily replica set snapshots
- ✅ Backup retention: 30 days
- ✅ Tested restore procedure (monthly drills)

---

---

## 6. Quality Attributes (Summary)

The following quality attributes (sometimes called "-ilities") are priorities:

| Quality Attribute    | Priority     | Target Metric                 |
| -------------------- | ------------ | ----------------------------- |
| **Availability**     | Must Have    | 99.9% uptime                  |
| **Reliability**      | Must Have    | Zero data loss                |
| **Security**         | Must Have    | GDPR compliant, RLS enforced  |
| **Performance**      | Must Have    | <200ms API response (p95)     |
| **Scalability**      | Must Have    | 1000 tenants, 10k workflows   |
| **Maintainability**  | Must Have    | 80% test coverage             |
| **Testability**      | Must Have    | TDD approach                  |
| **Auditability**     | Must Have    | Complete event log            |
| **Extensibility**    | Should Have  | Plugin architecture for steps |
| **Usability**        | Should Have  | <15min time to first workflow |
| **Portability**      | Nice to Have | Docker-based deployment       |
| **Interoperability** | Nice to Have | Standard GraphQL API          |

---

## Appendix A: Requirement Prioritization (MoSCoW)

### Must Have (MVP Blockers)

All requirements marked "Must Have" are blockers for MVP launch.

### Should Have (High Priority Post-MVP)

Defer to Phase 2 if time/budget constrained.

### Could Have (Nice to Have)

Include if time permits, otherwise defer.

### Won't Have (Out of Scope)

Explicitly out of scope for this project.

---

## Appendix B: Traceability Matrix

| Requirement ID | ADR Reference | Implementation Package | Test Suite                                             |
| -------------- | ------------- | ---------------------- | ------------------------------------------------------ |
| FR-WF-001      | ADR-003       | `libs/workflow-engine` | `libs/workflow-engine/src/engine/StateMachine.test.ts` |
| FR-AU-001      | ADR-003       | `libs/workflow-engine` | `libs/workflow-engine/src/events/EventStore.test.ts`   |
| FR-MT-001      | ADR-001       | `libs/database`        | `libs/database/tests/rls.test.ts`                      |
| NFR-PERF-001   | ADR-004       | `apps/api`             | `apps/api/tests/performance.test.ts`                   |
| ...            | ...           | ...                    | ...                                                    |

_(Full matrix to be populated during implementation)_

---

## Document Revision History

| Version | Date       | Author          | Changes                            |
| ------- | ---------- | --------------- | ---------------------------------- |
| 1.0     | 2025-12-05 | Claude + Romain | Initial comprehensive requirements |

---

**Status**: 📝 Draft - Awaiting Product Owner Approval
**Next Review**: 2025-12-10
