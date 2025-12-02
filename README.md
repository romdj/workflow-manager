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
        MG1[(workflow_instances<br/>{state, stepStates, ...})]
        MG2[(workflow_events<br/>{eventType, stepId, ...})]
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
