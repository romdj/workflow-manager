# Bootstrap Recommendations - Reusable Assets Analysis

## Executive Summary

After analyzing three reference projects (`3point-game-nhl-standing`, `OTW.sport`, and `tempsdarret.studio`), I've identified key configurations, patterns, and reusable components that will accelerate workflow-manager development.

**Key Finding**: All three projects use modern ESLint flat config, Vitest for testing, and npm workspaces (not Turborepo). The tempsdarret.studio project is the most architecturally mature with microservices pattern.

---

## Project Analysis Summary

### 1. 3point-game-nhl-standing

**Status**: Most mature SvelteKit + GraphQL implementation
**Stack**: Fastify + Mercurius (GraphQL), SvelteKit, TailwindCSS + DaisyUI, Vitest, npm workspaces
**Maturity**: Production-ready with comprehensive testing

**Structure**:

```
3point-game-nhl-standing/
├── frontend/           # SvelteKit app
├── graphql-server/     # Fastify + Mercurius
├── shared/             # Shared configs (ESLint, TypeScript)
└── package.json        # npm workspaces
```

### 2. OTW.sport

**Status**: Similar to 3point but with mobile app
**Stack**: Same as 3point + Flutter app
**Maturity**: Medium - similar architecture to 3point

**Structure**: Nearly identical to 3point-game with added `app/` folder for Flutter

### 3. tempsdarret.studio

**Status**: Most advanced microservices architecture
**Stack**: Fastify microservices, MongoDB, KafkaJS, SvelteKit, TypeSpec for API design
**Maturity**: High architectural sophistication

**Structure**:

```
tempsdarret.studio/
├── frontend/                    # SvelteKit client portal
├── api-gateway/                 # GraphQL federation gateway
├── services/                    # Microservices
│   ├── user-service/
│   ├── invite-service/
│   ├── shoot-service/
│   ├── portfolio-service/
│   ├── file-service/
│   └── notification-service/
├── packages/
│   ├── shared/                  # Configs, utilities (with subpath exports!)
│   ├── models/                  # TypeSpec API definitions
│   ├── types/                   # Shared TypeScript types
│   └── events/                  # Event schemas (KafkaJS)
└── dev-tools/
```

---

## Priority 1: MUST ADOPT

### 1. **ESLint Flat Config** (from 3point-game or tempsdarret)

**Source**: `3point-game-nhl-standing/shared/eslint.config.base.js`

**Why**: Modern ESLint 9+ flat config with shared base configurations

**Reusable Pattern**:

```javascript
// shared/eslint.config.base.js
export const baseEslintConfig = js.configs.recommended;
export const baseTypeScriptConfig = { /* ... */ };
export const baseTestConfig = { /* test-specific rules */ };
export const baseIgnoreConfig = { ignores: [...] };
export const nodeGlobals = globals.node;
export const browserGlobals = globals.browser;
```

**Recommendation**:

- Copy `shared/eslint.config.base.js` to `libs/shared/configs/`
- Frontend extends base + adds Svelte plugin
- Backend extends base + adds Node globals
- Single source of truth for linting rules

**Files to Copy**:

- ✅ `3point-game-nhl-standing/shared/eslint.config.base.js`
- ✅ `3point-game-nhl-standing/frontend/eslint.config.js` (adapt for our apps/admin-ui)
- ✅ `3point-game-nhl-standing/graphql-server/eslint.config.js` (adapt for our apps/api)

### 2. **Vitest Configuration** (from 3point-game frontend)

**Source**: `3point-game-nhl-standing/frontend/vitest.config.ts`

**Why**: Excellent coverage configuration with per-directory thresholds

**Key Features**:

- Happy-DOM for Svelte components
- Coverage thresholds per directory (`utils/` = 90%, `stores/` = 85%)
- Multiple reporters (text, lcov, html, json-summary, cobertura)
- Explicit includes/excludes

**Recommendation**:

- Use this exact config for `apps/admin-ui/vitest.config.ts`
- Create simpler version for backend libs (no Svelte plugin)

**Files to Copy**:

- ✅ `3point-game-nhl-standing/frontend/vitest.config.ts`

### 3. **Shared Package Pattern** (from tempsdarret)

**Source**: `tempsdarret.studio/packages/shared/package.json`

**Why**: Advanced subpath exports pattern for granular imports

**Pattern**:

```json
{
  "exports": {
    ".": "./dist/index.js",
    "./config": "./dist/config/index.js",
    "./types": "./dist/types/index.js",
    "./utils": "./dist/utils/index.js",
    "./schemas/shoot.schema": "./dist/schemas/shoot.schema.js"
  }
}
```

**Recommendation**:

- Apply to `@workflow-manager/shared-types`, `@workflow-manager/utils`, `@workflow-manager/validation`
- Allows: `import { z } from '@workflow-manager/validation/schemas'`
- Instead of: `import { z } from '@workflow-manager/validation'` (imports everything)

### 4. **TypeScript Base Config** (from tempsdarret - STRICTER)

**Source**: `tempsdarret.studio/tsconfig.json`

**Why**: Much stricter than our current config

**Key Additions**:

```json
{
  "noImplicitAny": true,
  "noImplicitReturns": true,
  "noImplicitThis": true,
  "noImplicitOverride": true,
  "noPropertyAccessFromIndexSignature": true,
  "noUncheckedSideEffectImports": true,
  "allowUnusedLabels": false,
  "allowUnreachableCode": false,
  "exactOptionalPropertyTypes": true // Already in ours
}
```

**Recommendation**:

- **Merge** these additional flags into our `tsconfig.base.json`
- Will catch more bugs at compile time

---

## Priority 2: SHOULD ADOPT

### 5. **Semantic Release Setup** (from all 3 projects)

**Sources**: All three use identical semantic-release setup

**Dependencies**:

```json
{
  "@semantic-release/changelog": "^6.0.3",
  "@semantic-release/git": "^10.0.1",
  "@semantic-release/github": "^11.0.3",
  "conventional-changelog-conventionalcommits": "^9.1.0",
  "semantic-release": "^24.2.7",
  "husky": "^9.1.7"
}
```

**Recommendation**:

- Add to root `package.json` devDependencies
- Create `.releaserc.json` config
- Enables automated versioning and changelog generation from conventional commits

### 6. **Docker Compose Patterns** (from all 3 projects)

**Common Scripts**:

```json
{
  "docker:build": "docker-compose build",
  "docker:up": "docker-compose up",
  "docker:up:build": "docker-compose up --build",
  "docker:down": "docker-compose down",
  "docker:clean": "docker-compose down --rmi all -v && docker system prune -f"
}
```

**Recommendation**:

- Add to root package.json
- Create `docker-compose.yml` and `docker-compose.dev.yml`

### 7. **Utility Scripts** (from 3point-game and OTW)

**Helpful Scripts**:

```json
{
  "complete-build": "clear; npm run clean; npm run install:all; npm run build; npm test",
  "validate:all": "npm run test && npm run lint && npm run check && npm run build",
  "check:all": "npm run lint && npm run check && npm audit --audit-level=high",
  "health": "curl -f http://localhost:4000/health && echo ' ✓ Server healthy'",
  "reset": "npm run clean && npm run install:all && npm run build"
}
```

**Recommendation**: Add these convenience scripts to root package.json

### 8. **GraphQL + Mercurius Pattern** (from 3point-game/graphql-server)

**Source**: `3point-game-nhl-standing/graphql-server/`

**Dependencies**:

```json
{
  "fastify": "^5.4.0",
  "mercurius": "^15.1.0",
  "graphql": "^16.11.0",
  "@graphql-tools/graphql-file-loader": "^8.0.22",
  "@graphql-tools/load": "^8.1.2"
}
```

**Recommendation**:

- Use this exact stack for `apps/api`
- Already matches our ADR-004 decision

---

## Priority 3: CONSIDER FOR LATER

### 9. **DaisyUI for TailwindCSS** (from 3point-game frontend)

**Source**: `3point-game-nhl-standing/frontend/package.json`

**Dependency**: `"daisyui": "^4.12.24"`

**Why**: Pre-built Tailwind component library (buttons, modals, forms)

**Recommendation**:

- Consider for `apps/admin-ui` if we want rapid UI development
- Alternative: Build custom component library in `libs/ui`

### 10. **KafkaJS for Event-Driven Architecture** (from tempsdarret)

**Source**: `tempsdarret.studio/`

**Dependency**: `"kafkajs": "^2.2.4"`

**Why**: Event-driven microservices communication

**Recommendation**:

- NOT needed for MVP
- Consider for Phase 2 if we need inter-service events
- Our MongoDB event store is sufficient for now

### 11. **TypeSpec for API Design** (from tempsdarret)

**Source**: `tempsdarret.studio/packages/models/`

**Dependencies**: TypeSpec compiler + OpenAPI generator

**Why**: Design-first API development with code generation

**Recommendation**:

- Interesting for Phase 2
- Overkill for current stage (we have GraphQL schema)
- Worth exploring if we add REST endpoints

### 12. **Testcontainers** (from tempsdarret services)

**Source**: `tempsdarret.studio/services/shoot-service/package.json`

**Dependencies**:

```json
{
  "@testcontainers/mongodb": "^10.13.0",
  "@testcontainers/kafka": "^10.13.0",
  "testcontainers": "^10.13.0"
}
```

**Why**: Spin up real MongoDB/PostgreSQL for integration tests

**Recommendation**:

- HIGH VALUE for integration testing
- Add after unit tests are in place
- Ensures tests run against real databases

---

## Files to Copy Immediately

### 1. Configuration Files

```bash
# ESLint
cp ~/testzone/3point-game-nhl-standing/shared/eslint.config.base.js \
   ~/Work/Elia/workflow-manager/libs/shared/configs/eslint.config.base.js

# Vitest (Frontend)
cp ~/testzone/3point-game-nhl-standing/frontend/vitest.config.ts \
   ~/Work/Elia/workflow-manager/apps/admin-ui/vitest.config.ts

# Prettier (if not already done)
cp ~/testzone/3point-game-nhl-standing/.prettierrc.js \
   ~/Work/Elia/workflow-manager/.prettierrc.js
```

### 2. Package.json Scripts

Add to root `package.json`:

```json
{
  "scripts": {
    "complete-build": "clear; pnpm run clean; pnpm install; pnpm run build; pnpm test",
    "validate:all": "pnpm run test && pnpm run lint && pnpm run check && pnpm run build",
    "check:all": "pnpm run lint && pnpm run check && pnpm audit --audit-level=high",
    "reset": "pnpm run clean && pnpm install && pnpm run build",
    "docker:build": "docker-compose build",
    "docker:up": "docker-compose up",
    "docker:up:build": "docker-compose up --build",
    "docker:down": "docker-compose down",
    "docker:clean": "docker-compose down --rmi all -v && docker system prune -f"
  }
}
```

### 3. Update Dependencies

Add to root `devDependencies`:

```json
{
  "@semantic-release/changelog": "^6.0.3",
  "@semantic-release/git": "^10.0.1",
  "@semantic-release/github": "^11.0.3",
  "conventional-changelog-conventionalcommits": "^9.1.0",
  "semantic-release": "^24.2.7",
  "husky": "^9.1.7"
}
```

Add to `apps/api/`:

```json
{
  "@graphql-tools/graphql-file-loader": "^8.0.22",
  "@graphql-tools/load": "^8.1.2"
}
```

Add to `apps/admin-ui/`:

```json
{
  "daisyui": "^4.12.24",
  "happy-dom": "^18.0.1",
  "@testing-library/svelte": "^5.2.8",
  "@testing-library/jest-dom": "^6.6.3"
}
```

---

## Architectural Insights

### Observation 1: npm Workspaces, NOT Turborepo

**Finding**: All three projects use **npm workspaces** without Turborepo

**Implications for workflow-manager**:

- We already added Turborepo (which is good - more advanced)
- Reference projects work fine without it
- Turborepo gives us caching + task orchestration benefits
- **Keep Turborepo** - it's an upgrade over reference projects

### Observation 2: ESM Everywhere

**Finding**: All three projects use `"type": "module"` (ESM)

**Implications**:

- Matches our setup ✅
- No CommonJS compatibility needed

### Observation 3: Shared Configs Pattern

**Finding**: All projects have a `shared/` or `packages/shared/` workspace

**Implications**:

- We should create `libs/shared/configs/` for:
  - `eslint.config.base.js`
  - `tsconfig.base.json` (already exists at root)
  - `vitest.config.base.ts`
  - Prettier config

### Observation 4: Testing Strategy

**3point-game approach**:

- Vitest for unit + component tests
- Coverage thresholds per directory
- Happy-DOM for Svelte
- Jest for backend (older choice)

**tempsdarret approach**:

- Vitest everywhere (modern)
- Separate test types: `test:unit`, `test:component`, `test:integration`, `test:contract`
- Testcontainers for integration tests

**Recommendation**: Follow tempsdarret's test categorization:

```json
{
  "test": "vitest run",
  "test:unit": "vitest run src/**/*.test.ts",
  "test:integration": "vitest run tests/integration/**/*.test.ts",
  "test:contract": "vitest run tests/contract/**/*.test.ts"
}
```

---

## Migration Action Plan

### Phase 1: Foundation (This Sprint)

1. ✅ **Copy ESLint base config** from 3point-game
2. ✅ **Copy Vitest config** from 3point-game frontend
3. ✅ **Update tsconfig.base.json** with stricter flags from tempsdarret
4. ✅ **Add semantic-release** dependencies
5. ✅ **Add utility scripts** to root package.json
6. ✅ **Add subpath exports** to shared packages

### Phase 2: Developer Experience (Next Sprint)

1. ⏳ **Setup Husky** for pre-commit hooks
2. ⏳ **Create docker-compose.yml** for local dev environment
3. ⏳ **Add DaisyUI** to admin-ui for rapid prototyping
4. ⏳ **Setup semantic-release** workflow

### Phase 3: Testing Infrastructure (Sprint 3)

1. ⏳ **Add Testcontainers** for integration tests
2. ⏳ **Categorize test scripts** (unit, integration, contract)
3. ⏳ **Setup GitHub Actions** CI pipeline (learn from reference projects)

---

## Key Differences: Our Stack vs Reference Projects

| Feature           | Reference Projects | workflow-manager     | Notes                            |
| ----------------- | ------------------ | -------------------- | -------------------------------- |
| Monorepo Tool     | npm workspaces     | **pnpm + Turborepo** | ✅ We're more advanced           |
| Package Manager   | npm                | **pnpm**             | ✅ Better choice                 |
| GraphQL Server    | Apollo (3point)    | **Mercurius**        | ✅ Matches 3point updated choice |
| Frontend          | SvelteKit          | **SvelteKit**        | ✅ Same                          |
| Backend Framework | Fastify            | **Fastify**          | ✅ Same                          |
| Testing           | Jest + Vitest mix  | **Vitest**           | ✅ Modern choice                 |
| Database          | N/A                | **PG + MongoDB**     | ⚠️ More complex                  |
| Event Sourcing    | No                 | **Yes**              | ⚠️ More complex                  |
| Microservices     | tempsdarret only   | **Modular monolith** | ℹ️ Simpler than microservices    |

**Summary**: Our architecture is more sophisticated than the reference projects. This is good but means less direct copy-paste.

---

## Conclusion

**Priority Actions**:

1. Copy ESLint flat config base pattern ← **DO THIS FIRST**
2. Copy Vitest configuration for frontend ← **DO THIS FIRST**
3. Update tsconfig.base.json with stricter TypeScript flags ← **DO THIS FIRST**
4. Add semantic-release for automated versioning
5. Add utility scripts to package.json
6. Consider DaisyUI for rapid UI development

**Don't Copy**:

- npm workspaces (we have pnpm + Turborepo)
- Apollo Server (3point migrated to Mercurius, we already chose Mercurius)
- Microservices architecture from tempsdarret (our modular monolith is simpler)

**Future Exploration**:

- Testcontainers for integration tests
- TypeSpec for REST API design (if needed)
- KafkaJS for event-driven workflows (Phase 2+)
