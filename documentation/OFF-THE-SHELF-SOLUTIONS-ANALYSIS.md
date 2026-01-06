# Off-the-Shelf Workflow Solutions Analysis

**Project**: Elia Group Workflow Manager
**Date**: 2026-01-05
**Purpose**: Evaluate commercial and open-source workflow engines for integration/adoption

---

## Executive Summary

This document evaluates **9 off-the-shelf workflow management solutions** against the Elia Workflow Manager requirements to determine if any can replace or augment the custom-built approach.

### Quick Verdict

| Solution           | Fit Score | Recommendation                                           |
| ------------------ | --------- | -------------------------------------------------------- |
| **Temporal**       | 85%       | 🟢 **Strong candidate** - Best for complex orchestration |
| **Camunda 8**      | 80%       | 🟢 **Strong candidate** - Best for BPMN/visual workflows |
| **n8n**            | 65%       | 🟡 **Partial fit** - Good for simple automation          |
| **Prefect**        | 70%       | 🟡 **Partial fit** - Good for data workflows             |
| **Conductor**      | 75%       | 🟡 **Consider** - Good microservices orchestration       |
| **Windmill**       | 60%       | 🟡 **Partial fit** - Developer-friendly scripts          |
| **Apache Airflow** | 50%       | 🔴 **Poor fit** - Batch/ETL focused, not long-running    |
| **Flowable**       | 70%       | 🟡 **Consider** - Strong BPMN support, Java-based        |
| **Kestra**         | 65%       | 🟡 **Partial fit** - YAML workflows, growing community   |

---

## Table of Contents

1. [Evaluation Criteria](#evaluation-criteria)
2. [Solution Deep-Dives](#solution-deep-dives)
3. [Feature Comparison Matrix](#feature-comparison-matrix)
4. [Cost Analysis](#cost-analysis)
5. [Integration Recommendations](#integration-recommendations)
6. [Risk Assessment](#risk-assessment)

---

## Evaluation Criteria

Based on the Workflow Manager requirements, solutions are evaluated on:

### Must-Have Features (Eliminatory)

- ✅ **Pause/Resume**: Can workflows be paused and resumed?
- ✅ **Rollback/Compensation**: Support for saga pattern/compensation?
- ✅ **Long-running**: Support workflows running days/weeks?
- ✅ **Audit Trail**: Complete event history?
- ✅ **Custom Logic**: Can we add custom step types?

### Scoring Criteria (0-10 scale)

| Criterion                  | Weight | Description                                    |
| -------------------------- | ------ | ---------------------------------------------- |
| **Feature Match**          | 30%    | How well does it meet functional requirements? |
| **Multi-tenancy**          | 20%    | Native support or easy to implement?           |
| **TypeScript/Node.js**     | 15%    | Integration with our stack                     |
| **TCO (Total Cost)**       | 15%    | Licensing + hosting + maintenance              |
| **Time to MVP**            | 10%    | How fast can we ship?                          |
| **Operational Complexity** | 10%    | Ease of deployment/monitoring                  |

---

## Solution Deep-Dives

### 1. Temporal (★★★★★ - Top Recommendation)

**Website**: https://temporal.io/
**License**: MIT (open source) + Commercial Cloud
**Language**: Go (server), TypeScript SDK available
**Maturity**: Production-ready, used by Netflix, Stripe, Coinbase

#### Overview

Temporal is a durable execution platform that guarantees workflow completion even through failures. It's built on the foundation of Uber's Cadence project.

#### Architecture

```
┌─────────────────────────────────────────────────────────┐
│ Your Application (TypeScript)                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Workflow    │  │  Activities  │  │  GraphQL API │  │
│  │  Definitions │  │  (Steps)     │  │              │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↕ (TypeScript SDK)
┌─────────────────────────────────────────────────────────┐
│ Temporal Server (Self-hosted or Cloud)                  │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────┐   │
│  │ Workflow   │  │ Event      │  │ Task Queues     │   │
│  │ Engine     │  │ History    │  │                 │   │
│  └────────────┘  └────────────┘  └─────────────────┘   │
│                                                          │
│  Storage: PostgreSQL or Cassandra                       │
└─────────────────────────────────────────────────────────┘
```

#### Pros

- ✅ **Exceptional pause/resume**: Built-in, rock-solid
- ✅ **Automatic retries**: Configurable retry policies per activity
- ✅ **Versioning**: Deploy new workflow versions without breaking in-flight workflows
- ✅ **Event sourcing**: Complete event history for every workflow
- ✅ **TypeScript SDK**: First-class support
- ✅ **Saga pattern**: Built-in compensation/rollback
- ✅ **Observability**: Excellent UI for workflow inspection
- ✅ **Battle-tested**: Running millions of workflows in production
- ✅ **Open source**: Can self-host

#### Cons

- ❌ **Multi-tenancy**: Not built-in, need to implement at app level
- ❌ **Operational overhead**: Need to run Temporal cluster (4+ services)
- ❌ **Learning curve**: New mental model (workflows vs activities)
- ❌ **Over-engineered**: May be overkill for simple workflows
- ❌ **No visual workflow builder**: Code-first approach
- ❌ **Resource intensive**: Requires dedicated infrastructure

#### How It Maps to Your Requirements

| Requirement         | Support    | Notes                              |
| ------------------- | ---------- | ---------------------------------- |
| Pause/Resume        | ⭐⭐⭐⭐⭐ | Best-in-class, built into core     |
| Rollback            | ⭐⭐⭐⭐⭐ | Saga pattern native                |
| Event Sourcing      | ⭐⭐⭐⭐⭐ | Complete event history             |
| Multi-tenancy       | ⭐⭐⭐     | Implement via workflow namespacing |
| Custom Steps        | ⭐⭐⭐⭐⭐ | Activities = custom steps          |
| TypeScript          | ⭐⭐⭐⭐⭐ | Excellent SDK                      |
| Long-running        | ⭐⭐⭐⭐⭐ | Designed for this                  |
| GraphQL Integration | ⭐⭐⭐⭐   | Build GraphQL layer on top         |

#### Code Example: BRP Onboarding Workflow

```typescript
// workflows/brp-onboarding.ts
import { proxyActivities } from '@temporalio/workflow';
import * as activities from '../activities';

const { validateCompanyInfo, savePortfolioData, requestApproval, provisionAccess, sendNotification } = proxyActivities<
  typeof activities
>({
  startToCloseTimeout: '1 hour',
  retry: { maximumAttempts: 3 },
});

export async function brpOnboardingWorkflow(tenantId: string, data: OnboardingData): Promise<WorkflowResult> {
  // Step 1: Validate company info
  const companyInfo = await validateCompanyInfo(data.company);

  // Step 2: Portfolio definition (can pause here via signal)
  await condition(() => portfolioDataReceived, '7 days'); // Wait up to 7 days
  const portfolio = await savePortfolioData(companyInfo.id, data.portfolio);

  // Step 3: Compliance approval
  const approved = await requestApproval(tenantId, 'compliance');
  if (!approved) {
    // Rollback: compensation logic
    await compensatePortfolio(portfolio.id);
    throw new Error('Approval rejected');
  }

  // Step 4: Technical setup
  const credentials = await provisionAccess(tenantId, portfolio);

  // Step 5: Notification
  await sendNotification(tenantId, 'onboarding-complete', credentials);

  return { status: 'completed', credentials };
}
```

#### Integration Approach

1. **Temporal Server**: Self-host on Kubernetes or use Temporal Cloud
2. **Workers**: Run TypeScript workers in your infrastructure
3. **GraphQL API**: Wrap Temporal client in GraphQL resolvers
4. **Multi-tenancy**:
   - Use workflow ID prefix: `tenant-{tenantId}-{workflowId}`
   - Store tenant context in workflow memo
   - Implement tenant filtering in GraphQL layer

#### Cost Estimate (Self-hosted)

- **Infrastructure**: ~€200/month (Temporal cluster + PostgreSQL)
- **Development**: 1-2 months to integrate
- **Maintenance**: Low (stable platform)
- **Total Year 1**: ~€30k (dev) + €2.4k (hosting)

#### Cost Estimate (Temporal Cloud)

- **Pricing**: ~$200/month base + usage
- **Development**: 2-4 weeks (easier than self-hosted)
- **Maintenance**: Minimal
- **Total Year 1**: ~€15k (dev) + €3k (cloud)

#### Verdict

🟢 **Strong Candidate** - Best for complex, mission-critical workflows with strong durability requirements.

---

### 2. Camunda Platform 8 (★★★★☆)

**Website**: https://camunda.com/
**License**: Community (self-managed) or Commercial Cloud
**Language**: Java (server), JavaScript/TypeScript connectors
**Maturity**: Industry leader in BPM, 15+ years

#### Overview

Camunda is a BPMN 2.0-based workflow engine with visual workflow modeling. Platform 8 is the cloud-native rewrite (based on Zeebe orchestration engine).

#### Architecture

```
┌─────────────────────────────────────────────────────────┐
│ Camunda Modeler (Desktop App)                           │
│  - Visual BPMN workflow designer                        │
│  - Exports .bpmn XML files                              │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Your Application (TypeScript)                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Job Workers │  │  GraphQL API │  │  Multi-tenant│  │
│  │  (Steps)     │  │              │  │  Logic       │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↕ (Zeebe Client)
┌─────────────────────────────────────────────────────────┐
│ Camunda Platform 8 (Self-hosted or SaaS)               │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────┐   │
│  │ Zeebe      │  │ Operate    │  │ Tasklist        │   │
│  │ (Engine)   │  │ (Monitor)  │  │ (User Tasks)    │   │
│  └────────────┘  └────────────┘  └─────────────────┘   │
│                                                          │
│  Storage: Elasticsearch                                 │
└─────────────────────────────────────────────────────────┘
```

#### Pros

- ✅ **Visual workflow builder**: BPMN 2.0 standard, non-devs can design
- ✅ **Industry standard**: BPMN widely understood
- ✅ **Production UI**: Tasklist for human tasks, Operate for monitoring
- ✅ **Long-running**: Designed for long-running processes
- ✅ **TypeScript support**: Official Node.js client
- ✅ **Event sourcing**: Complete audit trail
- ✅ **Saga/compensation**: Built-in BPMN compensation events
- ✅ **Mature**: 15+ years in production at enterprises

#### Cons

- ❌ **Java ecosystem**: Core engine is Java (though workers can be Node.js)
- ❌ **Multi-tenancy**: Not built-in, implement at app level
- ❌ **Complexity**: Steeper learning curve (BPMN notation)
- ❌ **Elasticsearch dependency**: Requires ES cluster
- ❌ **Resource heavy**: Multiple components to deploy
- ❌ **License cost**: Commercial features require paid license

#### How It Maps to Your Requirements

| Requirement      | Support    | Notes                           |
| ---------------- | ---------- | ------------------------------- |
| Pause/Resume     | ⭐⭐⭐⭐⭐ | Native via process interruption |
| Rollback         | ⭐⭐⭐⭐   | BPMN compensation events        |
| Event Sourcing   | ⭐⭐⭐⭐⭐ | Complete history                |
| Multi-tenancy    | ⭐⭐⭐     | Custom implementation needed    |
| Custom Steps     | ⭐⭐⭐⭐⭐ | Service tasks = custom workers  |
| TypeScript       | ⭐⭐⭐⭐   | Good Node.js support            |
| Long-running     | ⭐⭐⭐⭐⭐ | Designed for this               |
| Visual Workflows | ⭐⭐⭐⭐⭐ | BPMN Modeler included           |

#### Code Example: BRP Onboarding BPMN

```xml
<!-- brp-onboarding.bpmn -->
<bpmn:process id="brp-onboarding" name="BRP Contract Onboarding">
  <bpmn:startEvent id="start"/>

  <bpmn:serviceTask id="validate-company"
                    name="Validate Company Info"
                    zeebe:type="validate-company-task"/>

  <bpmn:userTask id="portfolio-input"
                 name="Enter Portfolio Data"
                 zeebe:assignee="${tenantAdmin}"/>

  <bpmn:serviceTask id="compliance-check"
                    name="Compliance Review"
                    zeebe:type="compliance-approval"/>

  <bpmn:boundaryEvent id="rejection"
                      attachedToRef="compliance-check">
    <bpmn:compensateEventDefinition/>
  </bpmn:boundaryEvent>

  <bpmn:serviceTask id="provision-access"
                    name="Provision Kong Access"
                    zeebe:type="kong-provisioning"/>

  <bpmn:endEvent id="end"/>

  <!-- Compensation for rollback -->
  <bpmn:serviceTask id="compensate-portfolio"
                    name="Rollback Portfolio"
                    isForCompensation="true"
                    zeebe:type="delete-portfolio"/>
</bpmn:process>
```

```typescript
// workers/company-validator.ts
import { ZBClient } from 'zeebe-node';

const zbc = new ZBClient();

zbc.createWorker({
  taskType: 'validate-company-task',
  taskHandler: async job => {
    const { tenantId, companyData } = job.variables;

    // Custom validation logic
    const validated = await validateCompanyInfo(companyData);

    return job.complete({
      companyId: validated.id,
      validated: true,
    });
  },
});
```

#### Integration Approach

1. **Camunda Modeler**: Design workflows visually (BPMN)
2. **Deploy workflows**: Via Camunda Operate or API
3. **Job Workers**: Implement in TypeScript for each step type
4. **Multi-tenancy**:
   - Use process variables to store `tenantId`
   - Filter queries by tenant in GraphQL layer
   - Implement RLS in your app DB (Camunda uses its own storage)

#### Cost Estimate (Self-managed)

- **Infrastructure**: ~€250/month (Zeebe cluster + Elasticsearch)
- **Development**: 2-3 months (BPMN learning curve)
- **License**: Free for self-managed community edition
- **Total Year 1**: ~€45k (dev) + €3k (hosting)

#### Cost Estimate (Camunda SaaS)

- **Pricing**: Starts at €500/month (includes hosting)
- **Development**: 1-2 months
- **Total Year 1**: ~€30k (dev) + €6k (SaaS)

#### Verdict

🟢 **Strong Candidate** - Best if you want visual workflow modeling and industry-standard BPMN.

---

### 3. n8n (★★★☆☆)

**Website**: https://n8n.io/
**License**: Fair-code (source available) + Commercial Cloud
**Language**: TypeScript (Node.js)
**Maturity**: Growing, 40k+ GitHub stars

#### Overview

n8n is a workflow automation tool with a visual node-based editor. Think "Zapier but self-hosted".

#### Pros

- ✅ **TypeScript native**: Built in Node.js
- ✅ **Visual editor**: Low-code, drag-and-drop
- ✅ **300+ integrations**: Pre-built connectors
- ✅ **Self-hostable**: Docker deployment
- ✅ **Easy to learn**: Intuitive UI
- ✅ **Code nodes**: Custom JavaScript/TypeScript logic

#### Cons

- ❌ **Not designed for long-running workflows**: More for automation
- ❌ **Limited multi-tenancy**: Not a core feature
- ❌ **No built-in rollback/compensation**: Would need custom implementation
- ❌ **Event sourcing**: Limited audit trail
- ❌ **Pause/resume**: Basic support, not battle-tested for days/weeks

#### How It Maps to Your Requirements

| Requirement      | Support    | Notes                               |
| ---------------- | ---------- | ----------------------------------- |
| Pause/Resume     | ⭐⭐       | Basic, not designed for long pauses |
| Rollback         | ⭐         | No native compensation              |
| Event Sourcing   | ⭐⭐       | Basic execution logs                |
| Multi-tenancy    | ⭐⭐       | Implement via workflow variables    |
| Custom Steps     | ⭐⭐⭐⭐⭐ | Easy to add custom nodes            |
| TypeScript       | ⭐⭐⭐⭐⭐ | Native                              |
| Long-running     | ⭐⭐       | Not the primary use case            |
| Visual Workflows | ⭐⭐⭐⭐⭐ | Excellent UI                        |

#### Verdict

🟡 **Partial Fit** - Good for simple automation, insufficient for complex long-running workflows with rollback.

---

### 4. Prefect (★★★☆☆)

**Website**: https://www.prefect.io/
**License**: Apache 2.0 (open source) + Commercial Cloud
**Language**: Python (with TypeScript client in beta)
**Maturity**: Production-ready, 13k+ GitHub stars

#### Overview

Prefect is a dataflow orchestration platform, primarily used for data pipelines and ETL workflows.

#### Pros

- ✅ **Strong observability**: Excellent UI and monitoring
- ✅ **Event sourcing**: Complete flow run history
- ✅ **Pause/resume**: Built-in
- ✅ **Retries**: Automatic retry logic
- ✅ **Open source**: Self-hostable

#### Cons

- ❌ **Python-first**: TypeScript support is experimental
- ❌ **Data pipeline focus**: Not optimized for business workflows
- ❌ **Multi-tenancy**: Not a core feature
- ❌ **Rollback**: Limited compensation support
- ❌ **Learning curve**: Python ecosystem

#### How It Maps to Your Requirements

| Requirement    | Support  | Notes                             |
| -------------- | -------- | --------------------------------- |
| Pause/Resume   | ⭐⭐⭐⭐ | Good support                      |
| Rollback       | ⭐⭐     | Limited                           |
| Event Sourcing | ⭐⭐⭐⭐ | Good audit trail                  |
| Multi-tenancy  | ⭐⭐     | Custom implementation             |
| Custom Steps   | ⭐⭐⭐⭐ | Python tasks                      |
| TypeScript     | ⭐⭐     | Beta support                      |
| Long-running   | ⭐⭐⭐   | Possible but not primary use case |

#### Verdict

🟡 **Partial Fit** - Better for data workflows than business process management.

---

### 5. Conductor (Netflix OSS) (★★★★☆)

**Website**: https://conductor-oss.org/
**License**: Apache 2.0
**Language**: Java (server), TypeScript SDK available
**Maturity**: Battle-tested at Netflix

#### Overview

Conductor is Netflix's microservices orchestration engine, designed to coordinate distributed tasks.

#### Pros

- ✅ **Proven at scale**: Runs millions of workflows at Netflix
- ✅ **Visual workflow designer**: JSON-based with UI
- ✅ **Pause/resume**: Built-in
- ✅ **Retry logic**: Configurable per task
- ✅ **Event sourcing**: Complete execution history
- ✅ **TypeScript SDK**: Available
- ✅ **Compensation**: Event-driven rollback

#### Cons

- ❌ **Java-centric**: Core is Java
- ❌ **Multi-tenancy**: Not built-in
- ❌ **Operational overhead**: Requires multiple services
- ❌ **Learning curve**: Designed for microservices
- ❌ **Community**: Smaller than Temporal

#### How It Maps to Your Requirements

| Requirement    | Support    | Notes                            |
| -------------- | ---------- | -------------------------------- |
| Pause/Resume   | ⭐⭐⭐⭐   | Good support                     |
| Rollback       | ⭐⭐⭐⭐   | Event-driven compensation        |
| Event Sourcing | ⭐⭐⭐⭐⭐ | Complete history                 |
| Multi-tenancy  | ⭐⭐⭐     | Implement via workflow variables |
| Custom Steps   | ⭐⭐⭐⭐⭐ | Workers for tasks                |
| TypeScript     | ⭐⭐⭐⭐   | Good SDK                         |
| Long-running   | ⭐⭐⭐⭐   | Designed for this                |

#### Verdict

🟡 **Consider** - Good option if you're running microservices architecture.

---

### 6. Apache Airflow (★★☆☆☆)

**Website**: https://airflow.apache.org/
**License**: Apache 2.0
**Language**: Python
**Maturity**: Industry standard for data pipelines

#### Overview

Airflow is a batch workflow scheduler, primarily for ETL and data pipelines.

#### Pros

- ✅ **Widely adopted**: Industry standard
- ✅ **Visual DAG editor**: Good UI
- ✅ **Extensive integrations**: 1000+ operators

#### Cons

- ❌ **Batch-oriented**: Not for real-time/long-running workflows
- ❌ **No pause/resume**: Designed for scheduled runs
- ❌ **Python-only**: No TypeScript support
- ❌ **Not event-driven**: Cron-based scheduling
- ❌ **Poor fit**: Fundamentally wrong tool for this use case

#### Verdict

🔴 **Poor Fit** - Designed for batch data pipelines, not business process management.

---

### 7. Flowable (★★★☆☆)

**Website**: https://www.flowable.com/
**License**: Apache 2.0 (open source) + Commercial
**Language**: Java
**Maturity**: Fork of Activiti, 15+ years lineage

#### Overview

Flowable is a BPMN 2.0 engine (similar to Camunda), focused on business process management.

#### Pros

- ✅ **BPMN 2.0**: Visual workflow design
- ✅ **Human tasks**: Built-in user task management
- ✅ **Pause/resume**: Native support
- ✅ **Compensation**: BPMN compensation events
- ✅ **Open source**: Full-featured free version

#### Cons

- ❌ **Java-based**: Harder to integrate with TypeScript
- ❌ **Multi-tenancy**: Custom implementation
- ❌ **Less modern**: Older architecture than Camunda 8

#### Verdict

🟡 **Consider** - Good if you prefer open-source alternative to Camunda.

---

### 8. Windmill (★★★☆☆)

**Website**: https://www.windmill.dev/
**License**: AGPLv3 (open source) + Commercial Cloud
**Language**: Rust (server), TypeScript/Python/Go for scripts
**Maturity**: Newer, growing

#### Overview

Windmill is a developer-friendly workflow engine with code-first approach.

#### Pros

- ✅ **TypeScript native**: First-class support
- ✅ **Fast**: Written in Rust
- ✅ **Visual editor**: Code + visual flows
- ✅ **Self-hostable**: Docker deployment

#### Cons

- ❌ **Newer platform**: Less battle-tested
- ❌ **Limited enterprise features**: Still maturing
- ❌ **Multi-tenancy**: Basic support
- ❌ **Compensation**: Would need custom logic

#### Verdict

🟡 **Partial Fit** - Interesting option but less mature than Temporal/Camunda.

---

### 9. Kestra (★★★☆☆)

**Website**: https://kestra.io/
**License**: Apache 2.0
**Language**: Java (server), YAML for workflows
**Maturity**: Newer, growing community

#### Overview

Kestra is an event-driven orchestration platform with YAML-based workflow definitions.

#### Pros

- ✅ **Event-driven**: Good for reactive workflows
- ✅ **YAML-based**: Infrastructure-as-code approach
- ✅ **Visual editor**: Good UI
- ✅ **Open source**: Free to use

#### Cons

- ❌ **Newer platform**: Less proven
- ❌ **Multi-tenancy**: Not built-in
- ❌ **TypeScript**: Not native (YAML + plugins)
- ❌ **Enterprise features**: Still developing

#### Verdict

🟡 **Partial Fit** - Interesting but too new for mission-critical use.

---

## Feature Comparison Matrix

| Feature             | Temporal   | Camunda 8  | n8n        | Prefect    | Conductor  | Airflow    | Flowable   | Windmill   | Kestra   |
| ------------------- | ---------- | ---------- | ---------- | ---------- | ---------- | ---------- | ---------- | ---------- | -------- |
| **Pause/Resume**    | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐       | ⭐⭐⭐⭐   | ⭐⭐⭐⭐   | ❌         | ⭐⭐⭐⭐⭐ | ⭐⭐⭐     | ⭐⭐⭐   |
| **Rollback/Saga**   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐         | ⭐⭐       | ⭐⭐⭐⭐   | ❌         | ⭐⭐⭐⭐   | ⭐⭐       | ⭐⭐     |
| **Event Sourcing**  | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐       | ⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐     | ⭐⭐⭐⭐   | ⭐⭐⭐     | ⭐⭐⭐   |
| **Multi-tenancy**   | ⭐⭐⭐     | ⭐⭐⭐     | ⭐⭐       | ⭐⭐       | ⭐⭐⭐     | ⭐⭐       | ⭐⭐⭐     | ⭐⭐       | ⭐⭐     |
| **TypeScript SDK**  | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐ | ⭐⭐       | ⭐⭐⭐⭐   | ❌         | ⭐⭐⭐     | ⭐⭐⭐⭐⭐ | ⭐⭐⭐   |
| **Long-running**    | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐       | ⭐⭐⭐     | ⭐⭐⭐⭐   | ❌         | ⭐⭐⭐⭐⭐ | ⭐⭐⭐     | ⭐⭐⭐   |
| **Custom Steps**    | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐ |
| **Visual Designer** | ❌         | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐     | ⭐⭐⭐⭐   | ⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐ |
| **Observability**   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐     | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐   | ⭐⭐⭐⭐   | ⭐⭐⭐     | ⭐⭐⭐   |
| **Maturity**        | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐⭐   | ⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐   | ⭐⭐⭐     | ⭐⭐⭐   |
| **Open Source**     | ✅         | ✅         | ⚠️         | ✅         | ✅         | ✅         | ✅         | ✅         | ✅       |
| **Self-hostable**   | ✅         | ✅         | ✅         | ✅         | ✅         | ✅         | ✅         | ✅         | ✅       |

---

## Cost Analysis

### 5-Year Total Cost of Ownership (Self-hosted)

| Solution         | Infra/Year | Dev Cost | Maintenance/Year | 5-Year TCO |
| ---------------- | ---------- | -------- | ---------------- | ---------- |
| **Temporal**     | €2.4k      | €30k     | €10k             | €92k       |
| **Camunda 8**    | €3k        | €45k     | €12k             | €120k      |
| **n8n**          | €1.2k      | €20k     | €8k              | €66k       |
| **Prefect**      | €2k        | €35k     | €10k             | €95k       |
| **Conductor**    | €2.5k      | €40k     | €12k             | €110k      |
| **Flowable**     | €2k        | €40k     | €12k             | €108k      |
| **Custom Build** | €5k        | €150k    | €30k             | €325k      |

### Cloud-Hosted Options

| Solution           | Monthly Cost | Setup Cost | Year 1 Total | 5-Year TCO |
| ------------------ | ------------ | ---------- | ------------ | ---------- |
| **Temporal Cloud** | €250         | €15k       | €18k         | €30k       |
| **Camunda SaaS**   | €500         | €30k       | €36k         | €60k       |
| **n8n Cloud**      | €100         | €15k       | €16.2k       | €21k       |

**Key Insight**: Cloud-hosted solutions have **3-5x lower TCO** than custom build!

---

## Integration Recommendations

### Recommendation #1: Temporal + Custom Multi-tenant Layer (Hybrid)

**Best for**: Maximum control + proven orchestration

#### Architecture

```
┌──────────────────────────────────────────────────────┐
│ Your Application Layer (TypeScript)                  │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────┐  │
│  │ GraphQL API │  │ Multi-tenant │  │ PostgreSQL │  │
│  │             │  │ RLS Layer    │  │ (tenants,  │  │
│  │             │  │              │  │  users)    │  │
│  └─────────────┘  └──────────────┘  └────────────┘  │
└──────────────────────────────────────────────────────┘
         ↓                    ↓
┌──────────────────────────────────────────────────────┐
│ Temporal (Workflow Orchestration)                    │
│  - Pause/Resume/Rollback (battle-tested)            │
│  - Event sourcing (audit trail)                     │
│  - Durable execution                                │
└──────────────────────────────────────────────────────┘
```

#### What You Build

- ✅ Multi-tenancy layer (PostgreSQL with RLS)
- ✅ GraphQL API (Apollo Server / Mercurius)
- ✅ Workflow definitions (TypeScript activities)
- ✅ Admin UI (SvelteKit - already started)

#### What You Reuse

- ✅ Temporal: Workflow engine + event sourcing
- ✅ Temporal UI: Workflow monitoring
- ✅ SDKs: TypeScript client libraries

#### Pros

- ✅ **Best of both worlds**: Custom + proven orchestration
- ✅ **Lower risk**: Temporal handles hard parts (durability, retries)
- ✅ **Faster to market**: 2-3 months vs 6 months custom
- ✅ **Battle-tested**: Netflix, Stripe scale
- ✅ **Full control**: Your multi-tenancy + business logic

#### Cons

- ❌ **Two systems**: Your app + Temporal cluster
- ❌ **Operational overhead**: Run Temporal infrastructure
- ❌ **Learning curve**: New paradigm (2-3 weeks)

#### Timeline

- **Month 1**: Temporal setup + first workflow (BRP onboarding)
- **Month 2**: Multi-tenancy + 3 more market roles
- **Month 3**: Production hardening + monitoring
- **MVP**: 3 months

---

### Recommendation #2: Camunda 8 + Custom Integration

**Best for**: Visual workflow modeling + BPMN standard

#### Architecture

```
┌──────────────────────────────────────────────────────┐
│ Camunda Modeler (Visual BPMN Designer)              │
│  - Market Ops team designs workflows                │
│  - Export .bpmn files                               │
└──────────────────────────────────────────────────────┘
         ↓
┌──────────────────────────────────────────────────────┐
│ Your Application (TypeScript)                        │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────┐  │
│  │ Job Workers │  │ Multi-tenant │  │ PostgreSQL │  │
│  │ (Step impls)│  │ GraphQL API  │  │            │  │
│  └─────────────┘  └──────────────┘  └────────────┘  │
└──────────────────────────────────────────────────────┘
         ↓
┌──────────────────────────────────────────────────────┐
│ Camunda Platform 8 (Zeebe + Operate + Tasklist)    │
└──────────────────────────────────────────────────────┘
```

#### Pros

- ✅ **Visual workflows**: Non-devs can modify processes
- ✅ **BPMN standard**: Widely understood notation
- ✅ **Production UIs**: Built-in monitoring + tasklist
- ✅ **15+ years mature**: Industry leader

#### Cons

- ❌ **Java ecosystem**: Core is Java
- ❌ **BPMN learning curve**: 2-4 weeks for team
- ❌ **Elasticsearch required**: Additional infrastructure

#### Timeline

- **Month 1**: Camunda setup + BPMN training + first workflow
- **Month 2-3**: Multi-tenancy + all market roles
- **Month 4**: Production hardening
- **MVP**: 4 months

---

### Recommendation #3: Custom Build (Original Plan)

**Best for**: Full control, no external dependencies

#### Pros

- ✅ **Perfect domain fit**: Exactly what you need
- ✅ **Full ownership**: No vendor dependencies
- ✅ **Optimized multi-tenancy**: Built-in from day 1

#### Cons

- ❌ **Longer timeline**: 6 months to production
- ❌ **Higher risk**: Reinventing complex patterns
- ❌ **Maintenance burden**: All bugs/features on your team

#### Timeline

- **Months 1-2**: Workflow engine core
- **Months 3-4**: All market roles + step types
- **Months 5-6**: Production hardening
- **MVP**: 6 months

---

## Final Recommendation

### 🏆 Winner: Temporal + Custom Multi-tenant Layer

#### Why?

1. **Risk Reduction**: Let Temporal handle the hard parts (durability, retries, event sourcing)
2. **Faster to Market**: 3 months vs 6 months
3. **Battle-Tested**: Proven at Netflix/Stripe scale
4. **TypeScript Native**: Perfect fit for your stack
5. **Cost Effective**: ~€30k vs €150k custom build
6. **Flexibility**: Can still customize business logic fully

#### Migration Path

- **Phase 1 (Now)**: Temporal Cloud trial (free)
- **Phase 2 (Month 1)**: Proof of concept with BRP workflow
- **Phase 3 (Months 2-3)**: Production implementation
- **Phase 4 (Future)**: Self-host if you want (Temporal is open source)

#### Next Steps

1. ✅ **Start Temporal Cloud trial** (free, no credit card)
2. ✅ **Build POC**: BRP onboarding workflow
3. ✅ **Evaluate**: Does it meet requirements?
4. ✅ **Decide**: Temporal vs Custom by end of POC
5. ✅ **Document**: Update ADRs with decision

---

## Risk Assessment

### Temporal Risks

| Risk                   | Likelihood | Impact | Mitigation                         |
| ---------------------- | ---------- | ------ | ---------------------------------- |
| Vendor lock-in         | Low        | Medium | Open source, can self-host         |
| Learning curve         | Medium     | Low    | Good docs, 2-3 week ramp-up        |
| Operational complexity | Medium     | Medium | Use Temporal Cloud initially       |
| Cost escalation        | Low        | Low    | Predictable pricing, can self-host |

### Custom Build Risks

| Risk                | Likelihood | Impact | Mitigation                        |
| ------------------- | ---------- | ------ | --------------------------------- |
| Timeline overrun    | High       | High   | Reduce scope or use Temporal      |
| Bugs in core engine | Medium     | High   | TDD + extensive testing           |
| Team turnover       | Medium     | High   | Documentation + knowledge sharing |
| Scaling issues      | Low        | High   | Load testing + profiling          |

### Camunda Risks

| Risk                    | Likelihood | Impact | Mitigation               |
| ----------------------- | ---------- | ------ | ------------------------ |
| BPMN learning curve     | Medium     | Medium | Training + documentation |
| Java ecosystem friction | Medium     | Low    | Use Node.js workers      |
| License costs           | Low        | Medium | Use open-source version  |
| Elasticsearch overhead  | Medium     | Low    | Managed ES service       |

---

## Appendix: Quick Start Guide

### Try Temporal (15 minutes)

```bash
# 1. Sign up for Temporal Cloud (free trial)
# https://cloud.temporal.io/signup

# 2. Install Temporal SDK
npm install @temporalio/client @temporalio/worker @temporalio/workflow

# 3. Create a workflow
// workflows/hello.ts
export async function helloWorkflow(name: string): Promise<string> {
  return `Hello, ${name}!`;
}

# 4. Run it
npm run workflow:start
```

### Try Camunda (30 minutes)

```bash
# 1. Download Camunda Modeler
# https://camunda.com/download/modeler/

# 2. Sign up for Camunda SaaS trial
# https://signup.camunda.com/accounts

# 3. Design BPMN workflow in Modeler

# 4. Deploy via Camunda Console
```

---

**Questions? Next Steps?**

Let me know if you'd like me to:

- 🔍 Deep-dive into Temporal integration
- 📊 Create detailed comparison for specific features
- 🛠️ Build POC with Temporal
- 📝 Update ADRs with Temporal decision
