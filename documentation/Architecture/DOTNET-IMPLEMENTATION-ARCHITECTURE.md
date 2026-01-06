# .NET Implementation Architecture

**Project**: Elia Group Workflow Manager
**Date**: 2026-01-06
**Status**: Implementation Plan
**Stack**: ASP.NET Core + Elsa Workflows + PostgreSQL + MongoDB

---

## Executive Summary

This document defines the .NET-based architecture for the Workflow Manager, replacing the original Node.js/TypeScript backend while **keeping the SvelteKit frontend**.

### Technology Stack

**Backend:**

- ASP.NET Core 8.0 (C#)
- Elsa Workflows 3.x (workflow engine)
- PostgreSQL (structured data + RLS multi-tenancy)
- MongoDB (workflow state + events)
- HotChocolate (GraphQL server)
- Entity Framework Core (PostgreSQL ORM)
- MongoDB.Driver (MongoDB client)

**Frontend:**

- SvelteKit (already implemented)
- TypeScript
- GraphQL client (connects to .NET backend)

**Infrastructure:**

- Docker + Docker Compose (local dev)
- Kubernetes (production - future)

---

## Table of Contents

1. [System Architecture](#system-architecture)
2. [Project Structure](#project-structure)
3. [Database Architecture](#database-architecture)
4. [Elsa Workflows Integration](#elsa-workflows-integration)
5. [Multi-Tenancy Implementation](#multi-tenancy-implementation)
6. [API Layer (GraphQL)](#api-layer-graphql)
7. [Authentication & Authorization](#authentication--authorization)
8. [Code Examples](#code-examples)
9. [Migration from TypeScript](#migration-from-typescript)
10. [Development Workflow](#development-workflow)

---

## System Architecture

### High-Level Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Layer                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SvelteKit Frontend (TypeScript)                     │  │
│  │  - Admin UI for Market Ops                           │  │
│  │  - Forms, task lists, dashboards                     │  │
│  │  - GraphQL client (urql/apollo)                      │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓ HTTPS/GraphQL
┌─────────────────────────────────────────────────────────────┐
│                  API Gateway Layer                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  ASP.NET Core Web API                                │  │
│  │  - HotChocolate GraphQL                              │  │
│  │  - JWT Authentication                                │  │
│  │  - Multi-tenant middleware                           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│               Application Services Layer                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  Workflow    │  │  Tenant      │  │  User        │     │
│  │  Service     │  │  Service     │  │  Service     │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  Approval    │  │  Notification│  │  Audit       │     │
│  │  Service     │  │  Service     │  │  Service     │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                 Workflow Engine Layer                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Elsa Workflows 3.x                                  │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐    │  │
│  │  │ Workflow   │  │  Activity  │  │  Bookmark  │    │  │
│  │  │ Runtime    │  │  Executor  │  │  Manager   │    │  │
│  │  └────────────┘  └────────────┘  └────────────┘    │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐    │  │
│  │  │ Custom     │  │  Human     │  │  API Call  │    │  │
│  │  │ Activities │  │  Task      │  │  Activity  │    │  │
│  │  └────────────┘  └────────────┘  └────────────┘    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                   Data Layer                                │
│  ┌─────────────────────┐  ┌─────────────────────────────┐  │
│  │ PostgreSQL          │  │ MongoDB                     │  │
│  │ ─────────────────── │  │ ─────────────────────────── │  │
│  │ • Tenants           │  │ • Workflow instances        │  │
│  │ • Users             │  │ • Workflow state (nested)   │  │
│  │ • Workflow templates│  │ • Workflow events           │  │
│  │ • Workflow index    │  │ • Execution history         │  │
│  │ • Tenant RLS        │  │ • Audit log                 │  │
│  └─────────────────────┘  └─────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Key Principles

1. **Separation of Concerns**: Frontend (SvelteKit) ↔ API (GraphQL) ↔ Business Logic ↔ Workflow Engine ↔ Data
2. **Multi-Tenancy First**: RLS in PostgreSQL, tenant context throughout
3. **Hybrid Storage**: PostgreSQL (relational) + MongoDB (workflow state/events)
4. **Event Sourcing**: Complete audit trail via MongoDB events
5. **Elsa Integration**: Use Elsa for workflow orchestration, NOT for UI

---

## Project Structure

### .NET Solution Structure

```
WorkflowManager/
├── src/
│   ├── WorkflowManager.Api/                    # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── GraphQL/                            # HotChocolate GraphQL
│   │   │   ├── Queries/
│   │   │   ├── Mutations/
│   │   │   ├── Types/
│   │   │   └── Subscriptions/
│   │   ├── Middleware/
│   │   │   ├── TenantMiddleware.cs
│   │   │   ├── AuthenticationMiddleware.cs
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── WorkflowManager.Api.csproj
│   │
│   ├── WorkflowManager.Core/                   # Domain models & interfaces
│   │   ├── Entities/
│   │   │   ├── Tenant.cs
│   │   │   ├── User.cs
│   │   │   ├── WorkflowTemplate.cs
│   │   │   ├── WorkflowInstance.cs
│   │   │   └── WorkflowEvent.cs
│   │   ├── Enums/
│   │   │   ├── MarketRole.cs
│   │   │   ├── WorkflowStatus.cs
│   │   │   └── StepType.cs
│   │   ├── Interfaces/
│   │   │   ├── IWorkflowService.cs
│   │   │   ├── ITenantService.cs
│   │   │   ├── IEventStore.cs
│   │   │   └── IMultiTenantContext.cs
│   │   ├── ValueObjects/
│   │   │   ├── TenantId.cs
│   │   │   ├── WorkflowId.cs
│   │   │   └── StepConfiguration.cs
│   │   └── WorkflowManager.Core.csproj
│   │
│   ├── WorkflowManager.Application/            # Business logic & services
│   │   ├── Services/
│   │   │   ├── WorkflowService.cs
│   │   │   ├── TenantService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── ApprovalService.cs
│   │   │   └── NotificationService.cs
│   │   ├── DTOs/
│   │   │   ├── CreateWorkflowRequest.cs
│   │   │   ├── ExecuteStepRequest.cs
│   │   │   └── WorkflowResponse.cs
│   │   ├── Validators/
│   │   │   └── FluentValidation validators
│   │   └── WorkflowManager.Application.csproj
│   │
│   ├── WorkflowManager.Infrastructure/         # Data access & external services
│   │   ├── Persistence/
│   │   │   ├── PostgreSQL/
│   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── TenantConfiguration.cs
│   │   │   │   │   ├── UserConfiguration.cs
│   │   │   │   │   └── WorkflowIndexConfiguration.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── TenantRepository.cs
│   │   │   │   │   └── WorkflowIndexRepository.cs
│   │   │   │   └── Migrations/
│   │   │   └── MongoDB/
│   │   │       ├── MongoDbContext.cs
│   │   │       ├── Repositories/
│   │   │       │   ├── WorkflowInstanceRepository.cs
│   │   │       │   └── WorkflowEventRepository.cs
│   │   │       └── Configurations/
│   │   ├── EventSourcing/
│   │   │   ├── EventStore.cs
│   │   │   └── EventHandlers/
│   │   ├── MultiTenancy/
│   │   │   ├── TenantContext.cs
│   │   │   └── TenantAccessor.cs
│   │   └── WorkflowManager.Infrastructure.csproj
│   │
│   ├── WorkflowManager.Workflows/              # Elsa workflow definitions
│   │   ├── Activities/                         # Custom Elsa activities
│   │   │   ├── HumanTaskActivity.cs
│   │   │   ├── FormActivity.cs
│   │   │   ├── ApprovalActivity.cs
│   │   │   ├── ApiCallActivity.cs
│   │   │   └── NotificationActivity.cs
│   │   ├── Definitions/                        # Workflow definitions
│   │   │   ├── BRP/
│   │   │   │   └── BrpOnboardingWorkflow.cs
│   │   │   ├── BSP/
│   │   │   │   └── BspOnboardingWorkflow.cs
│   │   │   └── Common/
│   │   │       └── BaseOnboardingWorkflow.cs
│   │   ├── Handlers/                           # Activity handlers
│   │   │   ├── CompanyInfoHandler.cs
│   │   │   ├── PortfolioHandler.cs
│   │   │   └── ComplianceHandler.cs
│   │   ├── Extensions/
│   │   │   └── ElsaServiceExtensions.cs
│   │   └── WorkflowManager.Workflows.csproj
│   │
│   └── WorkflowManager.Tests/
│       ├── Unit/
│       ├── Integration/
│       └── E2E/
│
├── frontend/                                    # Keep existing SvelteKit
│   └── (existing SvelteKit structure)
│
├── docker-compose.yml
├── WorkflowManager.sln
└── README.md
```

### NuGet Packages

**Core Dependencies:**

```xml
<!-- WorkflowManager.Api.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
  <PackageReference Include="HotChocolate.AspNetCore" Version="13.9.0" />
  <PackageReference Include="HotChocolate.Data.EntityFramework" Version="13.9.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
</ItemGroup>

<!-- WorkflowManager.Infrastructure.csproj -->
<ItemGroup>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
  <PackageReference Include="MongoDB.Driver" Version="2.23.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  <PackageReference Include="Dapper" Version="2.1.0" /> <!-- For raw SQL queries -->
</ItemGroup>

<!-- WorkflowManager.Workflows.csproj -->
<ItemGroup>
  <PackageReference Include="Elsa" Version="3.0.0" />
  <PackageReference Include="Elsa.EntityFrameworkCore" Version="3.0.0" />
  <PackageReference Include="Elsa.EntityFrameworkCore.PostgreSql" Version="3.0.0" />
  <PackageReference Include="Elsa.Http" Version="3.0.0" />
  <PackageReference Include="Elsa.Workflows.Api" Version="3.0.0" />
</ItemGroup>

<!-- WorkflowManager.Application.csproj -->
<ItemGroup>
  <PackageReference Include="FluentValidation" Version="11.9.0" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
  <PackageReference Include="MediatR" Version="12.2.0" />
  <PackageReference Include="AutoMapper" Version="12.0.0" />
</ItemGroup>
```

---

## Database Architecture

### PostgreSQL Schema (Structured Data)

```sql
-- Enable Row-Level Security
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tenants table
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name VARCHAR(255) NOT NULL,
    vat_number VARCHAR(50) UNIQUE NOT NULL,
    legal_entity_id VARCHAR(100),
    status VARCHAR(50) NOT NULL DEFAULT 'active', -- active, inactive, suspended
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Tenant market roles
CREATE TABLE tenant_market_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    market_role VARCHAR(50) NOT NULL, -- BRP, BSP, GU, ACH, CRM, ESP, DSO, SA, OPA, VSP
    status VARCHAR(50) NOT NULL DEFAULT 'onboarding', -- onboarding, active, inactive
    onboarded_at TIMESTAMPTZ,
    contract_reference VARCHAR(100),
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(tenant_id, market_role)
);

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID REFERENCES tenants(id) ON DELETE CASCADE,
    email VARCHAR(255) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL, -- market_ops, tenant_admin, tenant_operator, tenant_viewer
    password_hash VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Workflow templates
CREATE TABLE workflow_templates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    market_role VARCHAR(50) NOT NULL,
    elsa_workflow_definition_id VARCHAR(255) NOT NULL, -- Elsa's workflow ID
    version INTEGER NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT true,
    definition JSONB NOT NULL, -- Template metadata/config
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(market_role, version)
);

-- Workflow instances index (for fast queries)
CREATE TABLE workflow_instances_index (
    id UUID PRIMARY KEY, -- Same as MongoDB _id
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    template_id UUID NOT NULL REFERENCES workflow_templates(id),
    market_role VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL, -- draft, in_progress, paused, submitted, completed, failed
    current_step_id VARCHAR(100),
    elsa_instance_id VARCHAR(255) NOT NULL, -- Elsa's workflow instance ID
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_workflow_instances_tenant ON workflow_instances_index(tenant_id, status);
CREATE INDEX idx_workflow_instances_market_role ON workflow_instances_index(market_role, status);
CREATE INDEX idx_workflow_instances_created_at ON workflow_instances_index(created_at DESC);
CREATE INDEX idx_users_tenant ON users(tenant_id);
CREATE INDEX idx_tenant_market_roles_tenant ON tenant_market_roles(tenant_id);

-- Row-Level Security (RLS)
ALTER TABLE workflow_instances_index ENABLE ROW LEVEL SECURITY;

-- Market Ops can see all workflows
CREATE POLICY market_ops_all ON workflow_instances_index
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE users.id = current_setting('app.current_user_id')::uuid
            AND users.role = 'market_ops'
        )
    );

-- Tenant users can only see their own workflows
CREATE POLICY tenant_isolation ON workflow_instances_index
    FOR ALL
    USING (
        tenant_id = current_setting('app.current_tenant_id')::uuid
    );

-- Function to set tenant context
CREATE OR REPLACE FUNCTION set_tenant_context(p_tenant_id UUID, p_user_id UUID)
RETURNS void AS $$
BEGIN
    PERFORM set_config('app.current_tenant_id', p_tenant_id::text, true);
    PERFORM set_config('app.current_user_id', p_user_id::text, true);
END;
$$ LANGUAGE plpgsql;
```

### MongoDB Schema (Workflow State & Events)

```javascript
// Database: workflows

// Collection: workflow_instances
{
  _id: ObjectId("..."),
  tenantId: "uuid-string",
  tenantName: "Engie Belgium", // Denormalized
  templateId: "uuid-string",
  marketRole: "BRP",
  elsaInstanceId: "elsa-workflow-instance-id",
  status: "in_progress",

  // Nested workflow state
  state: {
    currentStepId: "portfolio-definition",
    stepStates: {
      "company-info": {
        status: "completed",
        data: {
          companyName: "Engie Belgium",
          vatNumber: "BE0403170701",
          legalAddress: {
            street: "Boulevard Simon Bolivar 34",
            city: "Brussels",
            postalCode: "1000"
          }
        },
        completedAt: ISODate("2026-01-06T10:30:00Z"),
        completedBy: "user-id"
      },
      "portfolio-definition": {
        status: "in_progress",
        data: {
          accessPoints: ["EAN123", "EAN456"],
          deliveryPoints: []
        },
        startedAt: ISODate("2026-01-06T11:00:00Z")
      }
    },
    metadata: {
      startedAt: ISODate("2026-01-06T10:00:00Z"),
      pausedDuration: 0,
      totalSteps: 5,
      completedSteps: 1,
      priority: "high"
    }
  },

  createdAt: ISODate("2026-01-06T10:00:00Z"),
  updatedAt: ISODate("2026-01-06T11:00:00Z"),
  createdBy: "user-id"
}

// Collection: workflow_events (Event Sourcing)
{
  _id: ObjectId("..."),
  workflowInstanceId: "workflow-uuid",
  tenantId: "tenant-uuid",
  eventType: "STEP_COMPLETED",
  stepId: "company-info",
  eventData: {
    stepData: { /* ... */ },
    previousState: { /* ... */ },
    newState: { /* ... */ }
  },
  performedBy: "user-id",
  occurredAt: ISODate("2026-01-06T10:30:00Z")
}

// Indexes
db.workflow_instances.createIndex({ tenantId: 1, status: 1 });
db.workflow_instances.createIndex({ elsaInstanceId: 1 }, { unique: true });
db.workflow_instances.createIndex({ "state.currentStepId": 1 });
db.workflow_instances.createIndex({ createdAt: -1 });

db.workflow_events.createIndex({ workflowInstanceId: 1, occurredAt: 1 });
db.workflow_events.createIndex({ tenantId: 1, occurredAt: -1 });
db.workflow_events.createIndex({ eventType: 1 });
```

---

## Elsa Workflows Integration

### Elsa Configuration

**Program.cs:**

```csharp
using Elsa.Extensions;
using WorkflowManager.Workflows.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Elsa services
builder.Services.AddElsa(elsa =>
{
    // Use PostgreSQL for Elsa's own persistence
    elsa.UseEntityFrameworkPersistence(ef =>
        ef.UsePostgreSql(builder.Configuration.GetConnectionString("Elsa")));

    // Register custom activities
    elsa.AddActivitiesFrom<WorkflowManager.Workflows.Activities>();

    // Register workflow definitions
    elsa.AddWorkflowsFrom<WorkflowManager.Workflows.Definitions>();

    // Enable HTTP activities (for API calls)
    elsa.UseHttp();

    // Enable workflow management API
    elsa.UseWorkflowsApi();
});

// Add workflow context (for multi-tenancy)
builder.Services.AddScoped<IWorkflowContextProvider, TenantWorkflowContextProvider>();

var app = builder.Build();

// Use Elsa middleware
app.UseWorkflowsApi();
app.UseWorkflows();

app.Run();
```

### Custom Elsa Activities

**Activities/HumanTaskActivity.cs:**

```csharp
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace WorkflowManager.Workflows.Activities;

/// <summary>
/// Represents a human task that requires user input via the UI
/// </summary>
[Activity("WorkflowManager", "Human Tasks", "Waits for human input")]
public class HumanTaskActivity : Activity
{
    [Input(Description = "The form schema for this task")]
    public Input<FormSchema> FormSchema { get; set; } = default!;

    [Input(Description = "Assigned user or role")]
    public Input<string> AssignedTo { get; set; } = default!;

    [Output(Description = "The submitted form data")]
    public Output<object> FormData { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Create a bookmark (pause point) waiting for user input
        var bookmark = new Bookmark(
            name: $"HumanTask_{context.Id}",
            payload: new { FormSchema, AssignedTo = AssignedTo.Get(context) }
        );

        context.CreateBookmark(bookmark);

        // Workflow will pause here until ResumeAsync is called
    }

    /// <summary>
    /// Called when user submits the form from the UI
    /// </summary>
    protected override async ValueTask OnResumeAsync(ActivityExecutionContext context)
    {
        // Get the form data from the resume payload
        var formData = context.GetInput<object>("formData");

        // Set output
        FormData.Set(context, formData);

        // Complete the activity
        await context.CompleteActivityAsync();
    }
}
```

**Activities/ApprovalActivity.cs:**

```csharp
[Activity("WorkflowManager", "Human Tasks", "Waits for approval")]
public class ApprovalActivity : Activity
{
    [Input(Description = "Approval request details")]
    public Input<ApprovalRequest> Request { get; set; } = default!;

    [Input(Description = "Who can approve (user IDs or roles)")]
    public Input<List<string>> Approvers { get; set; } = default!;

    [Output(Description = "Approval decision")]
    public Output<ApprovalDecision> Decision { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var request = Request.Get(context);
        var approvers = Approvers.Get(context);

        // Store approval request in database (via injected service)
        var approvalService = context.GetRequiredService<IApprovalService>();
        await approvalService.CreateApprovalRequestAsync(
            context.WorkflowInstanceId,
            context.Id,
            request,
            approvers
        );

        // Create bookmark
        var bookmark = new Bookmark(
            name: $"Approval_{context.Id}",
            payload: new { Request = request, Approvers = approvers }
        );

        context.CreateBookmark(bookmark);
    }

    protected override async ValueTask OnResumeAsync(ActivityExecutionContext context)
    {
        var decision = context.GetInput<ApprovalDecision>("decision");
        Decision.Set(context, decision);

        await context.CompleteActivityAsync();
    }
}
```

**Activities/ApiCallActivity.cs:**

```csharp
[Activity("WorkflowManager", "Integration", "Calls external API")]
public class ApiCallActivity : Activity<object>
{
    [Input(Description = "HTTP method")]
    public Input<string> Method { get; set; } = new("GET");

    [Input(Description = "API URL")]
    public Input<string> Url { get; set; } = default!;

    [Input(Description = "Request body")]
    public Input<object?> Body { get; set; } = default!;

    [Output(Description = "API response")]
    public Output<object> Response { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var httpClient = context.GetRequiredService<IHttpClientFactory>()
            .CreateClient("ExternalApi");

        var method = Method.Get(context);
        var url = Url.Get(context);
        var body = Body.Get(context);

        HttpResponseMessage response;

        switch (method.ToUpper())
        {
            case "POST":
                response = await httpClient.PostAsJsonAsync(url, body);
                break;
            case "PUT":
                response = await httpClient.PutAsJsonAsync(url, body);
                break;
            case "DELETE":
                response = await httpClient.DeleteAsync(url);
                break;
            default:
                response = await httpClient.GetAsync(url);
                break;
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<object>();

        Response.Set(context, result);
        await context.CompleteActivityAsync();
    }
}
```

### Workflow Definitions

**Definitions/BRP/BrpOnboardingWorkflow.cs:**

```csharp
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkflowManager.Workflows.Activities;

namespace WorkflowManager.Workflows.Definitions.BRP;

/// <summary>
/// BRP (Balance Responsible Party) onboarding workflow
/// </summary>
public class BrpOnboardingWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        builder.Name = "BRP Contract Onboarding";
        builder.Description = "Onboarding process for Balance Responsible Parties";

        builder.Root = new Sequence
        {
            Activities =
            {
                // Step 1: Company Information
                new HumanTaskActivity
                {
                    FormSchema = new(new FormSchema
                    {
                        Title = "Company Information",
                        Fields = new List<FormField>
                        {
                            new() { Name = "companyName", Type = "string", Required = true },
                            new() { Name = "vatNumber", Type = "string", Required = true, Pattern = @"^BE\d{10}$" },
                            new() { Name = "legalAddress", Type = "object", Required = true }
                        }
                    }),
                    AssignedTo = new("market-ops")
                },

                // Step 2: Portfolio Definition
                new HumanTaskActivity
                {
                    FormSchema = new(new FormSchema
                    {
                        Title = "Portfolio Definition",
                        Fields = new List<FormField>
                        {
                            new() { Name = "accessPoints", Type = "array", Required = true, MinItems = 1 },
                            new() { Name = "deliveryPoints", Type = "array" },
                            new() { Name = "dsoNetworkAreas", Type = "array" }
                        }
                    }),
                    AssignedTo = new("market-ops")
                },

                // Step 3: Compliance Review (Approval)
                new ApprovalActivity
                {
                    Request = new(new ApprovalRequest
                    {
                        Title = "Compliance Review",
                        Description = "Review BRP onboarding application for compliance"
                    }),
                    Approvers = new(new List<string> { "compliance-team" })
                },

                // Step 4: Decision based on approval
                new If
                {
                    Condition = new(context =>
                    {
                        var decision = context.GetVariable<ApprovalDecision>("Decision");
                        return decision?.Approved == true;
                    }),
                    Then = new Sequence
                    {
                        Activities =
                        {
                            // Approved: Provision access via Kong
                            new ApiCallActivity
                            {
                                Method = new("POST"),
                                Url = new("https://kong-api/provision"),
                                Body = new(context => new
                                {
                                    TenantId = context.GetVariable<string>("TenantId"),
                                    Role = "BRP",
                                    Portfolio = context.GetVariable<object>("PortfolioData")
                                })
                            },

                            // Send success notification
                            new NotificationActivity
                            {
                                Recipient = new(context => context.GetVariable<string>("CreatedByEmail")),
                                Template = new("brp-onboarding-approved"),
                                Data = new(context => new { WorkflowId = context.WorkflowInstanceId })
                            }
                        }
                    },
                    Else = new Sequence
                    {
                        Activities =
                        {
                            // Rejected: Send rejection notification
                            new NotificationActivity
                            {
                                Recipient = new(context => context.GetVariable<string>("CreatedByEmail")),
                                Template = new("brp-onboarding-rejected"),
                                Data = new(context => new { WorkflowId = context.WorkflowInstanceId })
                            }
                        }
                    }
                }
            }
        };
    }
}
```

---

## Multi-Tenancy Implementation

### Tenant Context

**Infrastructure/MultiTenancy/TenantContext.cs:**

```csharp
namespace WorkflowManager.Infrastructure.MultiTenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantName { get; }
    Guid UserId { get; }
    string UserRole { get; }
}

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserRole { get; set; } = string.Empty;
}

public class TenantAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ITenantContext GetTenantContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var tenantId = user.FindFirst("tenant_id")?.Value;
        var tenantName = user.FindFirst("tenant_name")?.Value;
        var userId = user.FindFirst("sub")?.Value;
        var role = user.FindFirst("role")?.Value;

        return new TenantContext
        {
            TenantId = Guid.Parse(tenantId!),
            TenantName = tenantName!,
            UserId = Guid.Parse(userId!),
            UserRole = role!
        };
    }
}
```

### Tenant Middleware

**Api/Middleware/TenantMiddleware.cs:**

```csharp
namespace WorkflowManager.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ApplicationDbContext dbContext,
        TenantAccessor tenantAccessor)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantContext = tenantAccessor.GetTenantContext();

            // Set tenant context in PostgreSQL for RLS
            await dbContext.Database.ExecuteSqlRawAsync(
                "SELECT set_tenant_context({0}, {1})",
                tenantContext.TenantId,
                tenantContext.UserId
            );
        }

        await _next(context);
    }
}

// Extension method
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
```

---

## API Layer (GraphQL)

### GraphQL Schema

**GraphQL/Types/WorkflowInstanceType.cs:**

```csharp
namespace WorkflowManager.Api.GraphQL.Types;

public class WorkflowInstanceType : ObjectType<WorkflowInstanceDto>
{
    protected override void Configure(IObjectTypeDescriptor<WorkflowInstanceDto> descriptor)
    {
        descriptor.Field(w => w.Id).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.TenantName).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.MarketRole).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.Status).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.CurrentStepId).Type<StringType>();
        descriptor.Field(w => w.State).Type<JsonType>();
        descriptor.Field(w => w.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(w => w.UpdatedAt).Type<NonNullType<DateTimeType>>();

        // Resolve related data
        descriptor
            .Field("template")
            .Type<WorkflowTemplateType>()
            .ResolveWith<WorkflowInstanceResolvers>(r => r.GetTemplateAsync(default!, default!));
    }
}

public class WorkflowInstanceResolvers
{
    public async Task<WorkflowTemplateDto> GetTemplateAsync(
        [Parent] WorkflowInstanceDto instance,
        [Service] IWorkflowService workflowService)
    {
        return await workflowService.GetTemplateAsync(instance.TemplateId);
    }
}
```

### Queries

**GraphQL/Queries/WorkflowQueries.cs:**

```csharp
namespace WorkflowManager.Api.GraphQL.Queries;

[ExtendObjectType("Query")]
public class WorkflowQueries
{
    /// <summary>
    /// Get all workflow instances for the current tenant
    /// </summary>
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<WorkflowInstanceDto>> GetWorkflowsAsync(
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        return await workflowService.GetWorkflowsForTenantAsync(tenantContext.TenantId);
    }

    /// <summary>
    /// Get a specific workflow instance by ID
    /// </summary>
    public async Task<WorkflowInstanceDto?> GetWorkflowAsync(
        Guid id,
        [Service] IWorkflowService workflowService)
    {
        return await workflowService.GetWorkflowAsync(id);
    }

    /// <summary>
    /// Get workflow templates available for the current tenant
    /// </summary>
    public async Task<List<WorkflowTemplateDto>> GetWorkflowTemplatesAsync(
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        return await workflowService.GetAvailableTemplatesAsync(tenantContext.TenantId);
    }
}
```

### Mutations

**GraphQL/Mutations/WorkflowMutations.cs:**

```csharp
namespace WorkflowManager.Api.GraphQL.Mutations;

[ExtendObjectType("Mutation")]
public class WorkflowMutations
{
    /// <summary>
    /// Create a new workflow instance from a template
    /// </summary>
    public async Task<WorkflowInstanceDto> CreateWorkflowAsync(
        CreateWorkflowInput input,
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        var command = new CreateWorkflowCommand
        {
            TenantId = tenantContext.TenantId,
            TemplateId = input.TemplateId,
            MarketRole = input.MarketRole,
            CreatedBy = tenantContext.UserId
        };

        return await workflowService.CreateWorkflowAsync(command);
    }

    /// <summary>
    /// Execute/complete a workflow step (resume from bookmark)
    /// </summary>
    public async Task<WorkflowInstanceDto> ExecuteStepAsync(
        ExecuteStepInput input,
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        var command = new ExecuteStepCommand
        {
            WorkflowId = input.WorkflowId,
            StepId = input.StepId,
            Data = input.Data,
            PerformedBy = tenantContext.UserId
        };

        return await workflowService.ExecuteStepAsync(command);
    }

    /// <summary>
    /// Pause a workflow
    /// </summary>
    public async Task<WorkflowInstanceDto> PauseWorkflowAsync(
        Guid workflowId,
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        return await workflowService.PauseWorkflowAsync(workflowId, tenantContext.UserId);
    }

    /// <summary>
    /// Resume a paused workflow
    /// </summary>
    public async Task<WorkflowInstanceDto> ResumeWorkflowAsync(
        Guid workflowId,
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        return await workflowService.ResumeWorkflowAsync(workflowId, tenantContext.UserId);
    }

    /// <summary>
    /// Submit approval decision
    /// </summary>
    public async Task<WorkflowInstanceDto> SubmitApprovalAsync(
        SubmitApprovalInput input,
        [Service] IWorkflowService workflowService,
        [Service] ITenantContext tenantContext)
    {
        var command = new SubmitApprovalCommand
        {
            WorkflowId = input.WorkflowId,
            ApprovalId = input.ApprovalId,
            Approved = input.Approved,
            Comments = input.Comments,
            ApprovedBy = tenantContext.UserId
        };

        return await workflowService.SubmitApprovalAsync(command);
    }
}
```

---

## Code Examples

### Workflow Service Implementation

**Application/Services/WorkflowService.cs:**

```csharp
namespace WorkflowManager.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowIndexRepository _indexRepository;
    private readonly IEventStore _eventStore;
    private readonly IElsaWorkflowRuntime _elsaRuntime;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowIndexRepository indexRepository,
        IEventStore eventStore,
        IElsaWorkflowRuntime elsaRuntime,
        ILogger<WorkflowService> logger)
    {
        _instanceRepository = instanceRepository;
        _indexRepository = indexRepository;
        _eventStore = eventStore;
        _elsaRuntime = elsaRuntime;
        _logger = logger;
    }

    public async Task<WorkflowInstanceDto> CreateWorkflowAsync(CreateWorkflowCommand command)
    {
        // 1. Get template
        var template = await _templateRepository.GetByIdAsync(command.TemplateId);

        // 2. Start Elsa workflow
        var elsaInstance = await _elsaRuntime.StartWorkflowAsync(
            template.ElsaWorkflowDefinitionId,
            new WorkflowInput
            {
                TenantId = command.TenantId,
                MarketRole = command.MarketRole,
                CreatedBy = command.CreatedBy
            });

        // 3. Create workflow instance in MongoDB
        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            TemplateId = command.TemplateId,
            MarketRole = command.MarketRole,
            ElsaInstanceId = elsaInstance.Id,
            Status = WorkflowStatus.Draft,
            State = new WorkflowState
            {
                CurrentStepId = template.Definition.Steps[0].Id,
                StepStates = new Dictionary<string, StepState>(),
                Metadata = new WorkflowMetadata
                {
                    StartedAt = DateTime.UtcNow,
                    TotalSteps = template.Definition.Steps.Count
                }
            },
            CreatedBy = command.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _instanceRepository.InsertAsync(instance);

        // 4. Create index entry in PostgreSQL
        await _indexRepository.InsertAsync(new WorkflowInstanceIndex
        {
            Id = instance.Id,
            TenantId = instance.TenantId,
            TemplateId = instance.TemplateId,
            MarketRole = instance.MarketRole,
            Status = instance.Status.ToString(),
            ElsaInstanceId = instance.ElsaInstanceId,
            CreatedBy = instance.CreatedBy,
            CreatedAt = instance.CreatedAt
        });

        // 5. Record event
        await _eventStore.AppendAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instance.Id,
            TenantId = instance.TenantId,
            EventType = WorkflowEventType.WorkflowCreated,
            EventData = new { TemplateId = template.Id, MarketRole = instance.MarketRole },
            PerformedBy = command.CreatedBy
        });

        return MapToDto(instance);
    }

    public async Task<WorkflowInstanceDto> ExecuteStepAsync(ExecuteStepCommand command)
    {
        // 1. Get workflow instance
        var instance = await _instanceRepository.GetByIdAsync(command.WorkflowId);

        // 2. Validate step transition
        // ... validation logic ...

        // 3. Resume Elsa workflow (this will trigger the bookmark resume)
        await _elsaRuntime.ResumeWorkflowAsync(
            instance.ElsaInstanceId,
            bookmarkId: command.StepId,
            input: new { formData = command.Data }
        );

        // 4. Update workflow state in MongoDB
        instance.State.StepStates[command.StepId] = new StepState
        {
            Status = StepStatus.Completed,
            Data = command.Data,
            CompletedAt = DateTime.UtcNow,
            CompletedBy = command.PerformedBy
        };

        instance.State.Metadata.CompletedSteps++;
        instance.UpdatedAt = DateTime.UtcNow;

        await _instanceRepository.UpdateAsync(instance);

        // 5. Update index in PostgreSQL
        await _indexRepository.UpdateStatusAsync(
            instance.Id,
            instance.Status.ToString(),
            instance.State.CurrentStepId
        );

        // 6. Record event
        await _eventStore.AppendAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instance.Id,
            TenantId = instance.TenantId,
            EventType = WorkflowEventType.StepCompleted,
            StepId = command.StepId,
            EventData = new
            {
                StepData = command.Data,
                PreviousState = /* snapshot */,
                NewState = instance.State
            },
            PerformedBy = command.PerformedBy
        });

        return MapToDto(instance);
    }

    public async Task<WorkflowInstanceDto> PauseWorkflowAsync(Guid workflowId, Guid userId)
    {
        var instance = await _instanceRepository.GetByIdAsync(workflowId);

        // Elsa workflows naturally pause at bookmarks (HumanTask, Approval)
        // This is more about marking the workflow as "explicitly paused by user"

        instance.Status = WorkflowStatus.Paused;
        instance.State.Metadata.PausedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;

        await _instanceRepository.UpdateAsync(instance);
        await _indexRepository.UpdateStatusAsync(workflowId, "paused", instance.State.CurrentStepId);

        await _eventStore.AppendAsync(new WorkflowEvent
        {
            WorkflowInstanceId = workflowId,
            TenantId = instance.TenantId,
            EventType = WorkflowEventType.WorkflowPaused,
            PerformedBy = userId
        });

        return MapToDto(instance);
    }
}
```

---

## Migration from TypeScript

### What to Keep from TS Prototype

1. **SvelteKit Frontend** - No changes needed
   - Update GraphQL endpoint to point to .NET backend
   - Keep all UI components, forms, routing

2. **Database schemas** - Mostly compatible
   - PostgreSQL schema: Copy as-is
   - MongoDB schema: Copy as-is
   - Migrations: Rewrite in C# (EF Core)

3. **ADRs** - Still valid
   - ADR-001 (Database): ✅ Still applies
   - ADR-002 (Monorepo): ⚠️ .NET projects instead of npm workspaces
   - ADR-003 (Workflow Engine): ✅ Replaced by Elsa (better fit)
   - ADR-004 (GraphQL): ✅ HotChocolate instead of Mercurius
   - ADR-005 (Auth): ✅ Still applies

4. **Requirements** - Unchanged
   - All functional requirements still valid
   - Implementation changes, requirements don't

### Migration Strategy

**Phase 1: Backend Foundation (2 weeks)**

1. Create .NET solution structure
2. Set up PostgreSQL + MongoDB connections
3. Implement core domain entities
4. Set up EF Core migrations
5. Configure Elsa

**Phase 2: Core Services (2 weeks)** 6. Implement WorkflowService 7. Implement TenantService 8. Multi-tenancy middleware 9. Event store implementation

**Phase 3: Elsa Integration (2 weeks)** 10. Create custom activities (HumanTask, Approval, etc.) 11. Implement BRP onboarding workflow 12. Test pause/resume/rollback

**Phase 4: GraphQL API (1 week)** 13. Set up HotChocolate 14. Implement queries/mutations 15. Connect to Elsa runtime

**Phase 5: Frontend Integration (1 week)** 16. Update SvelteKit GraphQL client 17. End-to-end testing 18. Polish UI

**Total: 8 weeks to production-ready**

---

## Development Workflow

### Running Locally

**docker-compose.yml:**

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: workflow_manager
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - '5432:5432'
    volumes:
      - postgres_data:/var/lib/postgresql/data

  mongodb:
    image: mongo:7
    ports:
      - '27017:27017'
    volumes:
      - mongo_data:/data/db

  api:
    build:
      context: .
      dockerfile: src/WorkflowManager.Api/Dockerfile
    ports:
      - '5000:8080'
    environment:
      ConnectionStrings__PostgreSQL: 'Host=postgres;Database=workflow_manager;Username=postgres;Password=postgres'
      ConnectionStrings__MongoDB: 'mongodb://mongodb:27017'
      ConnectionStrings__Elsa: 'Host=postgres;Database=elsa;Username=postgres;Password=postgres'
    depends_on:
      - postgres
      - mongodb

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - '5173:5173'
    environment:
      VITE_API_URL: 'http://localhost:5000/graphql'

volumes:
  postgres_data:
  mongo_data:
```

### Development Commands

```bash
# Start infrastructure
docker-compose up -d postgres mongodb

# Run migrations
dotnet ef database update --project src/WorkflowManager.Infrastructure

# Run API
dotnet run --project src/WorkflowManager.Api

# Run frontend (in separate terminal)
cd frontend
pnpm dev

# Run tests
dotnet test

# GraphQL playground
# Open: http://localhost:5000/graphql
```

---

## Next Steps

1. **Set up .NET solution**
   - Create solution structure
   - Add NuGet packages
   - Configure Docker Compose

2. **Implement domain entities**
   - Port TypeScript entities to C#
   - Set up EF Core
   - Create migrations

3. **Integrate Elsa**
   - Install Elsa packages
   - Create first custom activity
   - Build simple test workflow

4. **POC Goal: BRP Onboarding**
   - Implement full BRP workflow in Elsa
   - Connect to GraphQL API
   - Test from SvelteKit UI

**Timeline: 2-3 weeks for working POC**

---

## Questions?

- Need help with C# syntax vs TypeScript?
- Want to pair on Elsa workflow design?
- Need clarification on multi-tenancy implementation?
- Want to see more code examples?

Let me know what would be most helpful!
