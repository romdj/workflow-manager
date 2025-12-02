# Workflow Manager

A full-stack workflow management system for Elia Group's market participant onboarding and portfolio management.

## 🏗️ Architecture Overview

```mermaid
graph TB
    subgraph "Client Layer"
        A[Market Ops Dashboard<br/>SvelteKit]
        B[Market Participant Portal<br/>Future]
    end

    subgraph "API Layer"
        C[GraphQL API<br/>Fastify + Mercurius]
    end

    subgraph "Business Logic Layer"
        D[Workflow Engine<br/>State Machine + Events]
        E[Database Layer<br/>Repositories]
        F[Worker<br/>Background Jobs]
    end

    subgraph "Data Layer"
        G[(PostgreSQL<br/>RLS Enabled)]
        H[(MongoDB<br/>Events)]
    end

    subgraph "Business Modules"
        I[Market Roles<br/>BRP, BSP, GU, ACH, CRM, ESP, DSO]
        J[Integrations<br/>Kong, Notifications]
    end

    A --> C
    B --> C
    C --> D
    C --> E
    C --> F
    D --> I
    D --> J
    E --> G
    E --> H
    F --> E
```

## 🗂️ Monorepo Structure

```mermaid
graph LR
    subgraph "workflow-manager"
        subgraph "apps"
            A1[api]
            A2[admin-ui]
            A3[worker]
        end

        subgraph "libs"
            L1[workflow-engine]
            L2[database]
            L3[shared]
            L4[ui]
        end

        subgraph "modules"
            M1[workflows]
            M2[market-roles]
            M3[integrations]
        end

        subgraph "extensions"
            E1[custom plugins]
        end

        subgraph "tools"
            T1[cli]
            T2[scripts]
            T3[test-utils]
        end
    end

    A1 --> L1
    A1 --> L2
    A2 --> L4
    A3 --> L2
    L1 --> M1
    L1 --> M2
    M1 --> M2
```

## 🔄 Workflow Engine Architecture

```mermaid
graph TD
    subgraph "Workflow Engine"
        A[Template Registry]
        B[Workflow Instance]

        subgraph "Core Components"
            C[State Machine]
            D[Event Sourcing]
            E[Saga Coordinator]
        end

        subgraph "Step Handlers"
            F[Form Handler]
            G[Approval Handler]
            H[API Call Handler]
            I[Notification Handler]
        end

        J[Operations API]
    end

    A -->|loads| B
    B --> C
    B --> D
    B --> E
    C -->|executes| F
    C -->|executes| G
    C -->|executes| H
    C -->|executes| I
    D -->|logs| K[(Event Log)]
    E -->|compensates| C
    J -->|execute step| C
    J -->|pause/resume| C
    J -->|rollback| E
    J -->|validate| C
```

## 🔐 Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant U as User
    participant API as API Gateway
    participant Auth as Auth Service
    participant PG as PostgreSQL
    participant Resolver as GraphQL Resolver

    U->>API: 1. Login (email/password)
    API->>Auth: 2. Verify credentials
    Auth->>PG: 3. Check user
    PG-->>Auth: 4. User found
    Auth-->>API: 5. Generate JWT (tenantId, role, permissions)
    API-->>U: 6. Return access token

    U->>API: 7. GraphQL Request + JWT
    API->>API: 8. Verify JWT signature
    API->>API: 9. Extract tenantId from token
    API->>PG: 10. SET app.current_tenant = tenantId
    API->>Resolver: 11. Execute query
    Note over PG: Row-Level Security<br/>automatically filters by tenant
    Resolver->>PG: 12. Query data
    PG-->>Resolver: 13. Filtered results
    Resolver-->>API: 14. GraphQL response
    API-->>U: 15. Data (tenant-isolated)
```

## 🛡️ Multi-Tenant Security Layers

```mermaid
graph TD
    A[Incoming Request] --> B{Has JWT?}
    B -->|No| C[401 Unauthorized]
    B -->|Yes| D[Verify JWT Signature]
    D -->|Invalid| C
    D -->|Valid| E[Extract tenantId from JWT]
    E --> F{User Role?}

    F -->|market_ops| G[No RLS restriction<br/>Access all tenants]
    F -->|tenant_*| H[SET app.current_tenant]

    H --> I[PostgreSQL RLS Policy]
    I --> J[Filter: WHERE tenant_id = current_setting]

    G --> K[Execute Query]
    J --> K

    K --> L[Return Results]

    style I fill:#f9f,stroke:#333,stroke-width:4px
    style J fill:#f9f,stroke:#333,stroke-width:4px
```

## 💾 Database Architecture

```mermaid
graph TB
    subgraph "PostgreSQL - Structured Data"
        PG1[(tenants<br/>id, company_name, vat_number)]
        PG2[(users<br/>id, tenant_id, email, role)]
        PG3[(tenant_market_roles<br/>tenant_id, role, status)]
        PG4[(workflow_templates<br/>id, name, definition)]
        PG5[(workflow_instances_index<br/>id, tenant_id, mongo_id, status)]
    end

    subgraph "MongoDB - Flexible State & Events"
        MG1[(workflow_instances<br/>state, stepStates, ...)]
        MG2[(workflow_events<br/>eventType, stepId, ...)]
    end

    subgraph "API Layer"
        API[GraphQL API]
    end

    PG1 -.belongs to.- PG2
    PG1 -.has.- PG3
    PG5 -.references.- MG1
    MG1 -.events.- MG2

    API -->|Queries| PG5
    API -->|Full State| MG1
    API -->|Audit Trail| MG2
    API -->|Updates| PG5
    API -->|Updates| MG1
    API -->|Appends| MG2

    style PG5 fill:#bbf,stroke:#333,stroke-width:2px
    style MG1 fill:#bfb,stroke:#333,stroke-width:2px
```

## 🎯 Workflow State Machine

```mermaid
stateDiagram-v2
    [*] --> DRAFT: createWorkflow()
    DRAFT --> IN_PROGRESS: startWorkflow()

    IN_PROGRESS --> PAUSED: pause()
    PAUSED --> IN_PROGRESS: resume()

    IN_PROGRESS --> AWAITING_VALIDATION: executeLastStep()
    AWAITING_VALIDATION --> SUBMITTED: validate() + submit()
    SUBMITTED --> COMPLETED: approve()

    IN_PROGRESS --> IN_PROGRESS: executeStep()
    IN_PROGRESS --> IN_PROGRESS: rollback(toStepId)

    PAUSED --> ROLLED_BACK: rollback(toStepId)
    ROLLED_BACK --> IN_PROGRESS: resume()

    IN_PROGRESS --> FAILED: error
    PAUSED --> FAILED: timeout/error

    COMPLETED --> [*]
    FAILED --> [*]

    note right of IN_PROGRESS
        Each step transition
        creates an immutable
        event for audit trail
    end note
```

## 🔄 Workflow Execution Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant Engine as Workflow Engine
    participant SM as State Machine
    participant ES as Event Store
    participant PG as PostgreSQL
    participant MG as MongoDB

    User->>API: Execute Step (stepId, data)
    API->>Engine: executeStep()
    Engine->>MG: Load current state
    MG-->>Engine: Workflow state

    Engine->>SM: Build state machine
    SM->>SM: Validate transition

    Engine->>Engine: Get step handler (form/approval/etc)
    Engine->>Engine: Execute step logic

    Engine->>ES: Append event: STEP_STARTED
    ES->>MG: Write event

    alt Step Successful
        Engine->>SM: Transition to next step
        Engine->>ES: Append event: STEP_COMPLETED
        ES->>MG: Write event
        Engine->>MG: Update workflow state
        Engine->>PG: Update index (status, current_step)
        Engine-->>API: Success
    else Step Failed
        Engine->>ES: Append event: STEP_FAILED
        ES->>MG: Write event
        Engine-->>API: Error
    end

    API-->>User: Result
```

## 🏭 Market Role Modules

```mermaid
graph TB
    subgraph "Generic Workflows"
        GW1[Contract Onboarding<br/>Base Template]
        GW2[Portfolio Management<br/>Base Template]
    end

    subgraph "Market Role Customizations"
        BRP[BRP<br/>Balance Responsible Party]
        BSP[BSP<br/>Balance Service Provider]
        GU[GU<br/>Grid User]
        ACH[ACH<br/>Access Contract Holder]
        CRM[CRM<br/>Customer Relationship]
        ESP[ESP<br/>Energy Service Provider]
        DSO[DSO<br/>Distribution System Operator]
    end

    subgraph "Workflow Engine"
        WE[Template Registry]
    end

    GW1 -.customized by.-> BRP
    GW1 -.customized by.-> BSP
    GW1 -.customized by.-> GU
    GW1 -.customized by.-> ACH
    GW1 -.customized by.-> CRM
    GW1 -.customized by.-> ESP
    GW1 -.customized by.-> DSO

    BRP --> WE
    BSP --> WE
    GU --> WE
    ACH --> WE
    CRM --> WE
    ESP --> WE
    DSO --> WE
```

## 📊 Detailed Component Interaction Sequences

### Sequence 1: Contract Onboarding - Full Flow

```mermaid
sequenceDiagram
    participant MO as Market Ops
    participant UI as Admin UI
    participant API as GraphQL API
    participant Engine as Workflow Engine
    participant PG as PostgreSQL
    participant MG as MongoDB

    Note over MO,MG: Creating BRP Contract Onboarding Workflow

    MO->>UI: Select "Create Workflow"
    UI->>API: mutation createWorkflow(tenantId, marketRole: BRP, templateId)
    API->>PG: Verify tenant exists
    PG-->>API: Tenant: Engie Belgium
    API->>PG: Load workflow template
    PG-->>API: BRP Onboarding Template

    API->>Engine: create(tenantId, marketRole, template)
    Engine->>MG: Insert workflow_instance
    Note over MG: {<br/>  tenantId: "engie-id",<br/>  marketRole: "BRP",<br/>  status: "draft",<br/>  state: { stepStates: {} }<br/>}
    MG-->>Engine: workflowId

    Engine->>MG: Append event: WORKFLOW_CREATED
    Engine->>PG: Insert workflow_instances_index
    Note over PG: {<br/>  id, tenant_id,<br/>  mongo_id, status: "draft"<br/>}

    Engine-->>API: Workflow created
    API-->>UI: workflowId, status
    UI-->>MO: Show workflow editor

    Note over MO,MG: Executing Steps

    MO->>UI: Fill company info form
    UI->>API: mutation executeStep(workflowId, "company-info", data)
    API->>Engine: executeStep()
    Engine->>MG: Load workflow state
    Engine->>Engine: Validate: company name, VAT number
    Engine->>MG: Update state.stepStates["company-info"]
    Engine->>MG: Append event: STEP_COMPLETED
    Engine->>PG: Update index: current_step, updated_at
    Engine-->>API: Success
    API-->>UI: Step completed
    UI-->>MO: Show next step

    MO->>UI: Fill portfolio details
    UI->>API: mutation executeStep(workflowId, "portfolio", data)
    Note over Engine,MG: Same pattern: validate, update MG state, log event, update PG index

    MO->>UI: Submit for approval
    UI->>API: mutation submitWorkflow(workflowId)
    API->>Engine: validate() then submit()
    Engine->>MG: Load all step states
    Engine->>Engine: Run validators on all steps
    Engine->>MG: Update status: "submitted"
    Engine->>MG: Append event: WORKFLOW_SUBMITTED
    Engine->>PG: Update index: status = "submitted"
    Engine-->>API: Submitted successfully
    API-->>UI: Workflow submitted
    UI-->>MO: Success notification
```

### Sequence 2: Data Storage Pattern - PostgreSQL vs MongoDB

```mermaid
sequenceDiagram
    participant API
    participant PG as PostgreSQL<br/>(Structured)
    participant MG as MongoDB<br/>(Flexible)

    Note over API,MG: Storing Tenant & Market Role Data

    API->>PG: INSERT INTO tenants
    Note over PG: {<br/>  id: uuid,<br/>  company_name: "Engie Belgium",<br/>  vat_number: "BE0403170701",<br/>  status: "active"<br/>}

    API->>PG: INSERT INTO tenant_market_roles
    Note over PG: {<br/>  tenant_id: uuid,<br/>  role: "BRP",<br/>  status: "onboarding",<br/>  contract_reference: "BRP-2025-001"<br/>}

    Note over API,MG: Storing Workflow Instance & State

    API->>PG: INSERT INTO workflow_instances_index
    Note over PG: Lightweight index for queries:<br/>{<br/>  id, tenant_id, mongo_id,<br/>  status, current_step,<br/>  created_at, updated_at<br/>}

    API->>MG: db.workflow_instances.insertOne()
    Note over MG: Full workflow state:<br/>{<br/>  _id: ObjectId,<br/>  tenantId: "engie-id",<br/>  marketRole: "BRP",<br/>  status: "in_progress",<br/>  currentStepId: "portfolio",<br/>  state: {<br/>    stepStates: {<br/>      "company-info": {<br/>        status: "completed",<br/>        data: {<br/>          companyName: "Engie",<br/>          vatNumber: "BE...",<br/>          address: {...}<br/>        },<br/>        completedAt: ISODate()<br/>      },<br/>      "portfolio": {<br/>        status: "in_progress",<br/>        data: {<br/>          accessPoints: [...],<br/>          deliveryPoints: [...]<br/>        }<br/>      }<br/>    },<br/>    metadata: {...}<br/>  }<br/>}

    Note over API,MG: Storing Events for Audit

    API->>MG: db.workflow_events.insertOne()
    Note over MG: Append-only event log:<br/>{<br/>  workflowInstanceId: ObjectId,<br/>  eventType: "STEP_COMPLETED",<br/>  stepId: "company-info",<br/>  eventData: {<br/>    previousState: {...},<br/>    newState: {...}<br/>  },<br/>  performedBy: "user-id",<br/>  occurredAt: ISODate()<br/>}

    Note over API,MG: Why This Split?

    Note over PG: PostgreSQL stores:<br/>• Tenants & users (relational)<br/>• Templates (versioned)<br/>• Workflow index (fast queries)<br/>• RLS enforces tenant isolation

    Note over MG: MongoDB stores:<br/>• Workflow state (nested, flexible)<br/>• Events (append-only, audit)<br/>• No schema changes needed<br/>• Fast writes for state updates
```

### Sequence 3: Query Pattern - Cross-Database Operations

```mermaid
sequenceDiagram
    participant User as Market Ops
    participant API
    participant PG as PostgreSQL
    participant MG as MongoDB

    Note over User,MG: Query: List all workflows for a tenant

    User->>API: query workflows(filter: {tenantId, status: "in_progress"})
    API->>API: Extract tenantId from JWT
    API->>PG: SET app.current_tenant = tenantId

    API->>PG: SELECT FROM workflow_instances_index<br/>WHERE tenant_id = $1 AND status = $2
    Note over PG: RLS policy automatically filters<br/>Fast query on indexed fields
    PG-->>API: [workflow_id_1, workflow_id_2, ...]

    API->>MG: db.workflow_instances.find({<br/>  _id: {$in: [id1, id2, ...]}})
    Note over MG: Batch load full workflow states
    MG-->>API: [full_workflow_1, full_workflow_2, ...]

    API->>API: Merge: PG metadata + MG state
    API-->>User: [{id, tenant, status, currentStep, state}, ...]

    Note over User,MG: Query: Get workflow with full history

    User->>API: query workflow(id) { id, tenant, state, events }
    API->>PG: Verify access (RLS)
    API->>MG: db.workflow_instances.findOne({_id})
    MG-->>API: Workflow state
    API->>MG: db.workflow_events.find({workflowInstanceId})<br/>.sort({occurredAt: 1})
    MG-->>API: [event1, event2, ..., eventN]

    API->>API: Enrich with tenant data from PG
    API-->>User: Complete workflow + audit trail
```

### Sequence 4: Rollback Operation - Event Sourcing

```mermaid
sequenceDiagram
    participant User
    participant API
    participant Engine as Workflow Engine
    participant ES as Event Store
    participant Saga as Saga Coordinator
    participant MG as MongoDB
    participant PG as PostgreSQL

    User->>API: mutation rollbackWorkflow(workflowId, toStepId: "company-info")
    API->>Engine: rollback(workflowId, toStepId, context)

    Engine->>MG: Load current workflow state
    MG-->>Engine: Current state (at "approval" step)

    Engine->>ES: getEvents(workflowId)
    ES->>MG: db.workflow_events.find({workflowInstanceId})<br/>.sort({occurredAt: 1})
    MG-->>ES: [WORKFLOW_CREATED, STEP_COMPLETED: company-info,<br/>STEP_COMPLETED: portfolio, STEP_COMPLETED: approval]
    ES-->>Engine: Event history

    Engine->>Engine: Find target event: STEP_COMPLETED(company-info)
    Engine->>Engine: Identify steps to compensate:<br/>["approval", "portfolio"]

    Engine->>Saga: compensate(["approval", "portfolio"])

    loop For each step in reverse
        Saga->>Saga: Get compensation handler for step type
        Saga->>Saga: Execute compensation logic
        Note over Saga: e.g., Delete saved data,<br/>Revoke approvals,<br/>Notify stakeholders
        Saga->>MG: Append event: STEP_COMPENSATED
    end

    Engine->>ES: rebuildState(workflowId, until: targetEvent.occurredAt)
    ES->>Engine: State at "company-info" completion

    Engine->>MG: Update workflow_instance<br/>SET state = rebuiltState,<br/>    status = "in_progress",<br/>    currentStepId = "company-info"

    Engine->>MG: Append event: WORKFLOW_ROLLED_BACK
    Engine->>PG: Update workflow_instances_index<br/>SET current_step = "company-info",<br/>    status = "in_progress"

    Engine-->>API: Rollback complete
    API-->>User: Workflow rolled back to company-info step
```

## 📦 Package Dependency Graph

```mermaid
graph TD
    subgraph "apps"
        API[api]
        UI[admin-ui]
        WORKER[worker]
    end

    subgraph "libs"
        ENGINE[workflow-engine]
        DB[database]
        TYPES[shared-types]
        VALIDATION[validation]
        UTILS[utils]
        UIKIT[ui]
    end

    subgraph "modules"
        WORKFLOWS[workflows]
        ROLES[market-roles]
        INTEGRATIONS[integrations]
    end

    API --> ENGINE
    API --> DB
    API --> TYPES
    API --> VALIDATION

    UI --> UIKIT
    UI --> TYPES

    WORKER --> ENGINE
    WORKER --> DB

    ENGINE --> TYPES
    ENGINE --> VALIDATION

    DB --> TYPES

    WORKFLOWS --> ENGINE
    WORKFLOWS --> TYPES

    ROLES --> ENGINE
    ROLES --> WORKFLOWS
    ROLES --> TYPES
    ROLES --> VALIDATION

    INTEGRATIONS --> TYPES

    style API fill:#e1f5ff
    style UI fill:#e1f5ff
    style WORKER fill:#e1f5ff
    style ENGINE fill:#fff4e1
    style DB fill:#fff4e1
    style ROLES fill:#f0e1ff
```

## 🔧 Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | SvelteKit | Web framework |
| | TypeScript | Type safety |
| | TailwindCSS | Styling |
| | URQL | GraphQL client |
| | Vitest | Testing |
| **Backend** | Fastify | HTTP server |
| | Mercurius | GraphQL server |
| | TypeScript | Type safety |
| | DataLoader | N+1 optimization |
| | Zod | Validation |
| | Vitest/Jest | Testing |
| **Databases** | PostgreSQL 16+ | Structured data + RLS |
| | MongoDB 7+ | Workflow state + events |
| **Auth** | JWT | Token-based auth |
| | bcrypt | Password hashing |
| | RBAC | Role-based access |
| **DevOps** | pnpm | Package manager |
| | Turborepo | Monorepo build |
| | Docker | Containerization |
| | GitHub Actions | CI/CD |

## 🚀 Getting Started

### Prerequisites

- Node.js v22+
- pnpm v9+
- PostgreSQL 16+
- MongoDB 7+
- Docker (optional)

### Installation

```bash
# Install dependencies
pnpm install

# Set up environment
cp .env.example .env

# Run database migrations
pnpm db:migrate

# Seed test data
pnpm db:seed

# Start development servers
pnpm dev
```

### Development Commands

```bash
pnpm dev                  # Start all apps in dev mode
pnpm build                # Build all packages
pnpm test                 # Run all tests (TDD!)
pnpm lint                 # Lint all code
pnpm type-check           # TypeScript validation
```

## 📚 Documentation

- **Architecture**: See [Architecture Decision Records](./documentation/Architecture/ADRs/)
- **Development**: See [CLAUDE.md](./.claude/CLAUDE.md)
- **API**: See [GraphQL Schema](./apps/api/src/graphql/schema.graphql)

## 🧪 Test-Driven Development

This project **strictly follows TDD**:

1. **Red**: Write failing test
2. **Green**: Write code to pass test
3. **Refactor**: Improve code

```typescript
// Example: libs/workflow-engine/tests/StateMachine.test.ts
describe('StateMachine', () => {
  it('should transition to valid next step', async () => {
    // Arrange
    const machine = new StateMachine(state, template);

    // Act
    const result = await machine.transition('next-step');

    // Assert
    expect(result.success).toBe(true);
  });
});
```

## 🤝 Contributing

1. Create feature branch from `main`
2. Write tests first (TDD)
3. Implement feature
4. Ensure all tests pass
5. Create PR with conventional commit

## 📄 License

[License details]

---

**Built with ❤️ for Elia Group**
