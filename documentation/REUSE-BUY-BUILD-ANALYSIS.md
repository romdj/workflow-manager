# Reuse vs Buy vs Build Analysis - Workflow Manager

**Date**: 2025-12-05
**Author**: Claude (AI Assistant) + Romain (Product Owner)
**Status**: Draft for Review

---

## Executive Summary

This document evaluates whether to **reuse existing open-source solutions**, **buy commercial products**, or **build custom** for the Elia Group Workflow Manager project.

**TL;DR Recommendation**: **Hybrid Approach**

- **Reuse**: Configuration & tooling from reference projects (ESLint, Vitest, etc.)
- **Build**: Custom workflow engine (too domain-specific)
- **Evaluate**: Temporal.io for orchestration (strong candidate for Phase 2)

---

## Table of Contents

1. [Core Requirements](#core-requirements)
2. [Evaluation Criteria](#evaluation-criteria)
3. [Solutions Analysis](#solutions-analysis)
4. [Component-by-Component Analysis](#component-by-component-analysis)
5. [Decision Matrix](#decision-matrix)
6. [Recommendations](#recommendations)
7. [Risk Assessment](#risk-assessment)

---

## Core Requirements

### Functional Requirements

#### 1. Workflow Management

- ✅ Create workflow templates for 7 market roles (BRP, BSP, GU, ACH, CRM, ESP, DSO)
- ✅ Execute multi-step workflows (forms, approvals, API calls, notifications)
- ✅ Pause/resume workflows at any step
- ✅ Rollback to previous steps with compensation logic
- ✅ Validate workflow state before submission
- ✅ Support long-running workflows (days/weeks)

#### 2. Multi-Tenancy

- ✅ Strict tenant isolation (market participants are tenants)
- ✅ Row-Level Security (RLS) in PostgreSQL
- ✅ Tenant-aware audit logs
- ✅ Per-tenant customization of workflows

#### 3. Audit & Compliance

- ✅ Complete audit trail of all state changes
- ✅ Event sourcing for point-in-time recovery
- ✅ Immutable event log
- ✅ User action tracking (who, what, when)
- ✅ Regulatory compliance (GDPR, energy sector regulations)

#### 4. State Management

- ✅ Persist intermediate state at every step
- ✅ Query workflow state at any point
- ✅ Recover from failures gracefully
- ✅ Support concurrent workflow execution

#### 5. Integration

- ✅ GraphQL API for frontend consumption
- ✅ Integration with Kong API gateway
- ✅ Notification system integration
- ✅ External system API calls during workflow execution

### Non-Functional Requirements

#### Performance

- 🎯 Support 100+ concurrent active workflows
- 🎯 Sub-second state queries
- 🎯 Handle 1000+ tenants
- 🎯 Event replay in <5 seconds for workflows with <100 events

#### Security

- 🔒 JWT-based authentication
- 🔒 RBAC (Role-Based Access Control)
- 🔒 Tenant isolation at database level (RLS)
- 🔒 Encrypted sensitive data at rest

#### Reliability

- 🛡️ 99.9% uptime SLA
- 🛡️ Automatic failure recovery
- 🛡️ Graceful degradation
- 🛡️ Zero data loss on crashes

#### Maintainability

- 🔧 TypeScript throughout (type safety)
- 🔧 Test-Driven Development (TDD)
- 🔧 Clear separation of concerns
- 🔧 Extensible architecture

#### Cost

- 💰 Minimize operational costs
- 💰 Avoid vendor lock-in
- 💰 Predictable scaling costs

---

## Evaluation Criteria

### Must-Have Features

1. **Multi-tenancy with RLS** - Non-negotiable for security
2. **Event sourcing** - Required for audit trail
3. **Pause/resume** - Core workflow requirement
4. **Rollback with compensation** - Essential for error handling
5. **Custom step types** - Market role-specific logic
6. **TypeScript support** - Team expertise

### Decision Factors

| Factor                      | Weight | Description                             |
| --------------------------- | ------ | --------------------------------------- |
| **Feature Match**           | 30%    | How well does it meet our requirements? |
| **Total Cost of Ownership** | 25%    | License + hosting + maintenance         |
| **Time to Market**          | 20%    | How fast can we ship MVP?               |
| **Flexibility**             | 15%    | Can we customize to our domain?         |
| **Team Expertise**          | 10%    | Learning curve for team                 |

---

## Solutions Analysis

### Option 1: Build Custom (Current ADR Decision)

**What we build**:

- Custom workflow engine (State Machine + Event Sourcing + Saga)
- Custom step handlers (Form, Approval, API Call, Notification)
- Multi-tenant database architecture
- GraphQL API layer

**Pros**:

- ✅ **Perfect fit** for domain requirements
- ✅ **Full control** over features and roadmap
- ✅ **No licensing costs** (open-source stack)
- ✅ **Team owns the knowledge**
- ✅ **Tight integration** with existing systems
- ✅ **Optimized for multi-tenancy** from day 1

**Cons**:

- ❌ **Longer time to market** (3-6 months vs weeks)
- ❌ **Maintenance burden** (bug fixes, features, scaling)
- ❌ **Reinventing proven patterns** (workflow orchestration)
- ❌ **Team responsibility** for reliability/scalability
- ❌ **No out-of-box UI** for workflow visualization

**Estimated Effort**:

- **MVP (3 months)**: Core engine + 2 market roles
- **Production (6 months)**: All 7 market roles + monitoring
- **Team**: 2-3 developers

**Cost (Annual)**:

- **Development**: ~€150k (3 months @ blended rate)
- **Hosting**: ~€5k (Postgres + MongoDB + Node.js)
- **Maintenance**: ~€30k/year (10% time post-launch)
- **Total Year 1**: ~€185k

---

### Option 2: Temporal.io (Workflow Orchestration Platform)

**What it is**: Open-source workflow orchestration platform with commercial cloud offering

**What we reuse**:

- Durable execution engine
- Event sourcing & replay
- Compensation/saga patterns
- Workflow visualization UI
- SDKs for TypeScript

**What we build**:

- Workflow definitions (activities)
- Multi-tenant data layer
- GraphQL API
- Frontend UI (Temporal UI is for ops only)

**Pros**:

- ✅ **Proven at scale** (Netflix, Stripe, Coinbase use it)
- ✅ **Built-in fault tolerance** (automatic retries, timeouts)
- ✅ **Workflow versioning** (deploy new versions safely)
- ✅ **Excellent observability** (built-in workflow inspector)
- ✅ **TypeScript SDK** (matches our stack)
- ✅ **Open source** (Apache 2.0 license)
- ✅ **Strong community** & documentation

**Cons**:

- ❌ **Learning curve** (new paradigm: "activities" vs "steps")
- ❌ **Multi-tenancy not built-in** (we'd implement on top)
- ❌ **Operational complexity** (run Temporal cluster + workers)
- ❌ **Vendor risk** (cloud offering, though self-hostable)
- ❌ **Over-engineered?** (designed for microservices, we have monolith)
- ❌ **Event sourcing abstracted** (less control over events)

**Architecture with Temporal**:

```
┌──────────────────────────────────────────────────────┐
│  Our Application                                     │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐    │
│  │  GraphQL   │  │  Database  │  │  Frontend  │    │
│  │    API     │  │ (PG + Mongo│  │  (Svelte)  │    │
│  └─────┬──────┘  └─────┬──────┘  └────────────┘    │
│        │                │                            │
│  ┌─────▼────────────────▼──────┐                   │
│  │   Temporal Client SDK        │                   │
│  │  (Workflow Definitions)      │                   │
│  └──────────┬───────────────────┘                   │
└─────────────┼───────────────────────────────────────┘
              │
      ┌───────▼───────┐
      │  Temporal     │  ← Managed service or self-hosted
      │   Cluster     │
      └───────────────┘
```

**Estimated Effort**:

- **MVP (1.5 months)**: Temporal setup + 2 market roles
- **Production (3 months)**: All market roles + monitoring
- **Team**: 2 developers

**Cost (Annual - Self-Hosted)**:

- **Development**: ~€75k (1.5 months)
- **Hosting**: ~€15k (Temporal cluster + workers + DBs)
- **Maintenance**: ~€20k/year
- **Total Year 1**: ~€110k

**Cost (Annual - Temporal Cloud)**:

- **Development**: ~€75k
- **Temporal Cloud**: ~€30k-60k (depends on execution volume)
- **Hosting (our app)**: ~€5k
- **Total Year 1**: ~€110k-140k

**When to choose**:

- ✅ If we plan to scale to 1000s of workflows
- ✅ If we need distributed execution across services
- ✅ If we want battle-tested reliability
- ❌ NOT for MVP (overkill, learning curve)

---

### Option 3: Camunda Platform (BPMN Workflow Engine)

**What it is**: BPMN-based workflow engine (open-source + commercial)

**Pros**:

- ✅ **Industry standard** (BPMN 2.0)
- ✅ **Visual workflow designer** (drag-and-drop)
- ✅ **Strong Java ecosystem** (but we use Node.js...)
- ✅ **Enterprise features** (Camunda 8 cloud)

**Cons**:

- ❌ **Java-centric** (our stack is TypeScript)
- ❌ **BPMN overkill** (we don't need complex process diagrams)
- ❌ **Heavyweight** (JVM + Zeebe + Elasticsearch)
- ❌ **Multi-tenancy add-on** (not built-in)
- ❌ **License costs** (commercial features expensive)

**Verdict**: ❌ **Not a good fit** - Wrong tech stack, too heavy

---

### Option 4: n8n / Node-RED (Visual Workflow Automation)

**What they are**: No-code/low-code workflow automation tools

**Pros**:

- ✅ **Fast prototyping** (visual editor)
- ✅ **Many integrations** (APIs, databases, etc.)
- ✅ **Self-hostable**

**Cons**:

- ❌ **Not designed for multi-tenancy**
- ❌ **Limited customization** (UI-driven, not code-first)
- ❌ **No event sourcing** (basic state persistence)
- ❌ **Not built for complex workflows** (better for automations)
- ❌ **Limited audit trail**

**Verdict**: ❌ **Not a good fit** - Too simple, lacks audit/compliance features

---

### Option 5: AWS Step Functions / Azure Durable Functions

**What they are**: Serverless workflow orchestration

**Pros**:

- ✅ **Fully managed** (no ops burden)
- ✅ **Pay-per-execution** pricing
- ✅ **Built-in retry/timeout logic**

**Cons**:

- ❌ **Vendor lock-in** (AWS/Azure only)
- ❌ **Limited multi-tenancy** (we'd implement on top)
- ❌ **Cost unpredictable** at scale
- ❌ **Less control** over event storage
- ❌ **Our stack is on-prem/hybrid** (may not fit deployment)

**Verdict**: ⚠️ **Maybe for future** - If we go cloud-native later

---

## Component-by-Component Analysis

### 1. Workflow Engine Core

| Component             | Reuse    | Buy | Build  | Recommendation                       |
| --------------------- | -------- | --- | ------ | ------------------------------------ |
| **State Machine**     | Temporal | -   | Custom | **Build** (simple, domain-specific)  |
| **Event Sourcing**    | Temporal | -   | Custom | **Build** (need control over events) |
| **Saga/Compensation** | Temporal | -   | Custom | **Build** (custom logic per step)    |

**Rationale**: Core engine is too domain-specific. Temporal is overkill for MVP.

---

### 2. Data Layer

| Component            | Reuse          | Buy     | Build     | Recommendation                      |
| -------------------- | -------------- | ------- | --------- | ----------------------------------- |
| **PostgreSQL**       | ✅ Open source | AWS RDS | Self-host | **Reuse** (standard DB)             |
| **MongoDB**          | ✅ Open source | Atlas   | Self-host | **Reuse** (standard DB)             |
| **Multi-tenant RLS** | -              | -       | Custom    | **Build** (PostgreSQL RLS policies) |
| **Repositories**     | -              | -       | Custom    | **Build** (TypeScript DAOs)         |

**Rationale**: Use standard databases, build custom data access layer for multi-tenancy.

---

### 3. API Layer

| Component          | Reuse        | Buy   | Build  | Recommendation                  |
| ------------------ | ------------ | ----- | ------ | ------------------------------- |
| **GraphQL Server** | ✅ Mercurius | -     | -      | **Reuse** (Fastify + Mercurius) |
| **Resolvers**      | -            | -     | Custom | **Build** (our domain logic)    |
| **Authentication** | ✅ JWT       | Auth0 | Custom | **Reuse** (standard JWT)        |
| **DataLoader**     | ✅ NPM pkg   | -     | -      | **Reuse** (graphql-dataloader)  |

**Rationale**: Reuse standard libraries, build custom resolvers.

---

### 4. Frontend

| Component          | Reuse        | Buy | Build  | Recommendation                    |
| ------------------ | ------------ | --- | ------ | --------------------------------- |
| **Framework**      | ✅ SvelteKit | -   | -      | **Reuse** (modern, fast)          |
| **UI Components**  | ✅ DaisyUI   | -   | Custom | **Reuse** (Tailwind + DaisyUI)    |
| **GraphQL Client** | ✅ URQL      | -   | -      | **Reuse** (Svelte-friendly)       |
| **Workflow UI**    | Temporal UI  | -   | Custom | **Build** (domain-specific forms) |

**Rationale**: Reuse framework + UI lib, build custom workflow interface.

---

### 5. DevOps & Tooling

| Component      | Reuse             | Buy      | Build | Recommendation                   |
| -------------- | ----------------- | -------- | ----- | -------------------------------- |
| **Monorepo**   | ✅ Turborepo      | -        | -     | **Reuse** (already set up)       |
| **Testing**    | ✅ Vitest         | -        | -     | **Reuse** (modern, fast)         |
| **Linting**    | ✅ ESLint 9       | -        | -     | **Reuse** (flat config)          |
| **CI/CD**      | ✅ GitHub Actions | CircleCI | -     | **Reuse** (free for open-source) |
| **Monitoring** | ✅ Prometheus     | Datadog  | -     | **Reuse** (open-source)          |

**Rationale**: Maximize reuse of open-source tooling.

---

## Decision Matrix

### Scoring (1-5 scale, 5 = best)

| Solution               | Feature Match | Cost | Time to Market | Flexibility | Team Expertise | **Weighted Score** |
| ---------------------- | ------------- | ---- | -------------- | ----------- | -------------- | ------------------ |
| **Build Custom**       | 5             | 3    | 2              | 5           | 4              | **3.75**           |
| **Temporal.io**        | 4             | 4    | 4              | 3           | 2              | **3.55**           |
| **Camunda**            | 2             | 2    | 3              | 2           | 1              | **2.05**           |
| **n8n/Node-RED**       | 1             | 5    | 5              | 1           | 3              | **2.60**           |
| **AWS Step Functions** | 3             | 3    | 4              | 2           | 3              | **3.00**           |

**Calculation** (weights: 30%, 25%, 20%, 15%, 10%):

- **Build Custom**: `(5×0.3) + (3×0.25) + (2×0.2) + (5×0.15) + (4×0.1) = 3.75`
- **Temporal.io**: `(4×0.3) + (4×0.25) + (4×0.2) + (3×0.15) + (2×0.1) = 3.55`

---

## Recommendations

### Phase 1: MVP (Months 1-3) - **Build Custom**

**Decision**: Build custom workflow engine for MVP

**Rationale**:

1. ✅ **Perfect domain fit** - Our requirements are very specific (market roles, RLS, compliance)
2. ✅ **Team learning** - Build domain expertise, no external dependency
3. ✅ **Full control** - Own the roadmap, no vendor limitations
4. ✅ **Cost-effective** - No licensing, predictable hosting costs
5. ✅ **ADR alignment** - Already decided in ADR-003

**What to reuse from reference projects**:

- ✅ ESLint flat config (3point-game)
- ✅ Vitest configuration (3point-game)
- ✅ Stricter TypeScript config (tempsdarret)
- ✅ Semantic release setup (all 3 projects)
- ✅ DaisyUI for rapid UI (3point-game)

**What to build**:

- State Machine
- Event Sourcing
- Saga Coordinator
- Step Handlers (Form, Approval, API Call, Notification)
- Multi-tenant data layer
- GraphQL resolvers
- Admin UI

**Timeline**:

- **Month 1**: Workflow engine core + event sourcing
- **Month 2**: Step handlers + multi-tenant DB + GraphQL API
- **Month 3**: Admin UI + 2 market roles (BRP, BSP)

---

### Phase 2: Scale (Months 4-12) - **Evaluate Temporal.io**

**Decision**: Re-evaluate Temporal.io after MVP proves concept

**When to migrate**:

1. ✅ If workflow complexity grows significantly (>10 step types)
2. ✅ If we need distributed execution (microservices)
3. ✅ If we hit scaling limits (>1000 concurrent workflows)
4. ✅ If team bandwidth for maintenance is limited

**Migration path** (if needed):

1. Keep existing workflows running on custom engine
2. Implement new workflows on Temporal
3. Gradual migration over 6-12 months
4. Maintain compatibility layer during transition

**Cost-benefit re-check**:

- Compare custom engine maintenance cost vs Temporal licensing
- Evaluate if Temporal's features (versioning, visibility) justify switch
- Check if team has bandwidth to learn Temporal

---

### Configuration & Tooling - **Reuse Immediately**

**Action items** (from BOOTSTRAP_RECOMMENDATIONS.md):

1. **Copy ESLint config** from reference projects

   ```bash
   cp ~/testzone/3point-game-nhl-standing/shared/eslint.config.base.js \
      libs/shared/configs/
   ```

2. **Copy Vitest config** for frontend testing

   ```bash
   cp ~/testzone/3point-game-nhl-standing/frontend/vitest.config.ts \
      apps/admin-ui/
   ```

3. **Update tsconfig.base.json** with stricter flags
   - Add `noImplicitReturns`, `noImplicitThis`, etc.

4. **Add semantic-release** for automated versioning

   ```bash
   pnpm add -D @semantic-release/changelog @semantic-release/git
   ```

5. **Add utility scripts** to root package.json
   - `complete-build`, `validate:all`, `check:all`, `reset`

6. **Add DaisyUI** to admin-ui for rapid prototyping
   ```bash
   cd apps/admin-ui && pnpm add daisyui
   ```

---

## Risk Assessment

### Build Custom Risks

| Risk                      | Impact | Probability | Mitigation                                 |
| ------------------------- | ------ | ----------- | ------------------------------------------ |
| **Longer time to market** | High   | Medium      | Aggressive MVP scoping, reuse patterns     |
| **Maintenance burden**    | Medium | High        | Invest in tests (TDD), monitoring, docs    |
| **Scaling challenges**    | Medium | Low         | Design for scale from day 1, load testing  |
| **Team knowledge loss**   | High   | Low         | Document extensively, pair programming     |
| **Feature creep**         | High   | Medium      | Strict ADR process, product owner approval |

### Temporal.io Risks

| Risk                       | Impact | Probability | Mitigation                                |
| -------------------------- | ------ | ----------- | ----------------------------------------- |
| **Learning curve**         | Medium | High        | Invest in training, POCs before migration |
| **Vendor lock-in**         | Medium | Medium      | Self-host option available                |
| **Operational complexity** | High   | Medium      | Start with managed cloud, learn ops later |
| **Over-engineering**       | Low    | High        | Only adopt when proven need               |

---

## Appendix A: Reference Projects Analysis

### Reusable Assets from Bootstrap Analysis

From `/documentation/BOOTSTRAP_RECOMMENDATIONS.md`:

| Asset                  | Source         | Priority    | Status     |
| ---------------------- | -------------- | ----------- | ---------- |
| **ESLint base config** | 3point-game    | ✅ Must     | ⏳ Pending |
| **Vitest config**      | 3point-game    | ✅ Must     | ⏳ Pending |
| **Stricter tsconfig**  | tempsdarret    | ✅ Must     | ⏳ Pending |
| **Semantic release**   | All 3 projects | ✅ Should   | ⏳ Pending |
| **Docker scripts**     | All 3 projects | ✅ Should   | ⏳ Pending |
| **DaisyUI**            | 3point-game    | ⚠️ Consider | ⏳ Pending |
| **Testcontainers**     | tempsdarret    | ⚠️ Consider | 📅 Later   |

---

## Appendix B: Alternative Workflow Engines Evaluated

### Briefly Considered (Rejected)

1. **Airflow** - Python-based, designed for data pipelines (not user workflows)
2. **Prefect** - Similar to Airflow, Python ecosystem
3. **Conductor** - Netflix OSS, Java-based (wrong stack)
4. **Argo Workflows** - Kubernetes-native, too DevOps-focused
5. **Apache Camel** - Java integration framework (too heavy)

**Common rejection reason**: Wrong tech stack or wrong use case (data/DevOps vs user workflows)

---

## Conclusion

**Final Recommendation**: **Hybrid Approach**

1. ✅ **Build custom workflow engine** for MVP (Months 1-3)
   - Perfect domain fit
   - Team ownership
   - Full control

2. ✅ **Reuse open-source tooling** aggressively
   - ESLint, Vitest, SvelteKit, Fastify, Mercurius
   - DaisyUI for rapid UI development
   - Semantic release, Husky, Prettier

3. ⏳ **Evaluate Temporal.io** post-MVP (Month 6+)
   - Re-assess if custom engine scales
   - Consider migration if maintenance burden high
   - Strong candidate for Phase 2

4. ❌ **Don't buy** commercial workflow engines
   - Wrong tech stack (Java vs TypeScript)
   - Vendor lock-in concerns
   - Cost not justified for MVP

**Next Steps**:

1. Finalize this analysis with product owner
2. Update ADRs if needed (reaffirm ADR-003)
3. Begin implementation following BOOTSTRAP_RECOMMENDATIONS.md
4. Set up monitoring/metrics to measure if custom engine scales

---

**Document Status**: 📝 Draft - Awaiting Product Owner Review

**Questions for Product Owner**:

1. Do you agree with "build custom for MVP" approach?
2. Any concerns about Temporal.io for Phase 2?
3. Budget for Year 1 acceptable (~€185k)?
4. Timeline realistic (3 months MVP, 6 months production)?
