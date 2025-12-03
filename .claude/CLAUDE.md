# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Workflow Manager** is a full-stack workflow management system for Elia Group's market participant onboarding and portfolio management. The system supports multiple market roles (BRP, BSP, GU, ACH, CRM, ESP, DSO, SA, OPA, VSP) with extensible workflow definitions.

**Current Status**: Initial architecture defined, folder structure created, ready for implementation.

## Architecture

- **Frontend**: SvelteKit with TypeScript, TailwindCSS
- **Backend**: Fastify + Mercurius (GraphQL)
- **Databases**: PostgreSQL (relational data) + MongoDB (workflow state & events)
- **Workflow Engine**: Hybrid State Machine + Event Sourcing + Saga Pattern
- **Monorepo**: pnpm workspaces with Turborepo

## Key Architectural Decisions

All architectural decisions are documented in `documentation/Architecture/ADRs/`:

- **ADR-001**: Hybrid Database Architecture (PostgreSQL + MongoDB)
- **ADR-002**: Hybrid Modular Monorepo Structure
- **ADR-003**: Workflow Engine Design (State Machine + Event Sourcing)
- **ADR-004**: GraphQL API Architecture (Fastify + Mercurius)
- **ADR-005**: Authentication & Authorization (JWT + RBAC + RLS)

**IMPORTANT**: Always read relevant ADRs before making significant architectural changes.

## Monorepo Structure

```
workflow-manager/
├── apps/           # Deployable applications
├── libs/           # Core infrastructure libraries
├── modules/        # Business capabilities (pluggable)
├── extensions/     # Optional/custom functionality
└── tools/          # Development utilities
```

### Key Folders

- **apps/api/** - GraphQL API server
- **apps/admin-ui/** - Market Operations dashboard
- **apps/worker/** - Background job processor
- **libs/workflow-engine/** - Core workflow execution engine
- **libs/database/** - Database clients (PG + Mongo)
- **modules/market-roles/** - Market role customizations (BRP, BSP, etc.)
- **modules/workflows/** - Workflow definitions
- **modules/integrations/** - External system integrations

## Key Commands

### Root Level (Workspace)

```bash
pnpm install              # Install all dependencies
pnpm dev                  # Start all apps in dev mode
pnpm build                # Build all packages
pnpm test                 # Run all tests
pnpm lint                 # Lint all packages
pnpm type-check           # TypeScript type checking
pnpm clean                # Clean all build artifacts
```

### Individual Apps

```bash
pnpm --filter @workflow-manager/api dev        # Start API server
pnpm --filter @workflow-manager/admin-ui dev   # Start admin UI
pnpm --filter @workflow-manager/worker dev     # Start worker
```

### Database

```bash
pnpm db:migrate           # Run PostgreSQL migrations
pnpm db:seed              # Seed databases with test data
```

### Code Generation

```bash
pnpm generate:types       # Generate TypeScript types from GraphQL schema
```

## Development Practices

### Test-Driven Development (TDD)

**CRITICAL**: This project follows **Test-Driven Development** strictly.

#### TDD Process

1. **Red**: Write a failing test first
2. **Green**: Write minimal code to make test pass
3. **Refactor**: Improve code while keeping tests green

#### Test Structure

```typescript
// libs/workflow-engine/tests/StateMachine.test.ts

describe('StateMachine', () => {
  describe('transition', () => {
    it('should transition to valid next step', async () => {
      // Arrange
      const state = createInitialState();
      const template = createTestTemplate();
      const machine = new StateMachine(state, template);

      // Act
      const result = await machine.transition('step-2');

      // Assert
      expect(result.success).toBe(true);
      expect(result.currentStepId).toBe('step-2');
    });

    it('should reject invalid transition', async () => {
      // Arrange
      const state = createInitialState();
      const template = createTestTemplate();
      const machine = new StateMachine(state, template);

      // Act & Assert
      await expect(machine.transition('invalid-step')).rejects.toThrow(InvalidTransitionError);
    });
  });
});
```

#### Test Coverage Requirements

- **Unit Tests**: All business logic functions
- **Integration Tests**: API endpoints, database operations
- **End-to-End Tests**: Critical user workflows

**Target**: 80%+ code coverage

### Testing Guidelines

1. **Write tests before implementation**
2. **Test behavior, not implementation details**
3. **Use descriptive test names**: `it('should [expected behavior] when [condition]')`
4. **Follow AAA pattern**: Arrange, Act, Assert
5. **Mock external dependencies** (databases, APIs)
6. **Keep tests fast**: Unit tests < 100ms, integration < 1s

### Code Style

- **TypeScript**: Strict mode enabled
- **Formatting**: Prettier (see `.prettierrc.js`)
- **Linting**: ESLint with TypeScript rules
- **Naming Conventions**:
  - `PascalCase` for classes and types
  - `camelCase` for functions and variables
  - `UPPER_SNAKE_CASE` for constants
  - `kebab-case` for file names

### Git Workflow

#### Commit Messages (Conventional Commits)

Format: `<type>(<scope>): <subject>`

**Types**:

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples**:

```bash
feat(workflow-engine): implement pause/resume functionality
fix(api): resolve tenant isolation bug in workflow query
docs(adr): add ADR-006 for deployment strategy
test(database): add integration tests for workflow repository
refactor(ui): extract common form components
```

#### Branch Strategy

- **main**: Production-ready code
- **feature/**: New features (`feature/brp-onboarding`)
- **fix/**: Bug fixes (`fix/auth-token-expiry`)
- **docs/**: Documentation (`docs/api-guide`)

#### Pull Request Process

1. Create feature branch from `main`
2. Write tests (TDD)
3. Implement feature
4. Ensure all tests pass
5. Update documentation
6. Create PR with clear description
7. Address review feedback
8. Merge after approval

### Code Review Guidelines

**Reviewers should check**:

- Tests written first (TDD)
- All tests passing
- Code follows style guide
- No security vulnerabilities
- Performance considerations
- Documentation updated

## Multi-Tenancy

**Tenant = Company/Organization** (not market role)

- Users belong to one tenant
- Market Ops can access all tenants
- Row-Level Security (RLS) enforces isolation at database level
- Tenant context set via middleware

```typescript
// Tenant isolation example
app.addHook('onRequest', async (request, reply) => {
  if (request.user && request.user.role !== 'market_ops') {
    await db.query('SET LOCAL app.current_tenant = $1', [request.user.tenantId]);
  }
});
```

## Workflow System

### Core Concepts

- **Workflow Template**: Definition of steps and transitions
- **Workflow Instance**: Execution of a template for a tenant
- **Step**: Unit of work (form, approval, API call, etc.)
- **State**: Current position and data in workflow
- **Event**: Audit log entry for state change

### Workflow Operations

```typescript
// Create workflow
const workflowId = await engine.create({
  tenantId,
  templateId: 'brp-onboarding',
  createdBy: userId,
});

// Execute step
await engine.executeStep(workflowId, 'company-info', formData, context);

// Pause
await engine.pause(workflowId, context);

// Resume
await engine.resume(workflowId, context);

// Rollback
await engine.rollback(workflowId, 'previous-step-id', context);

// Validate
const validation = await engine.validate(workflowId, context);

// Submit
if (validation.valid) {
  await engine.submit(workflowId, context);
}
```

### Adding New Market Role

1. Create module: `modules/market-roles/new-role/`
2. Define workflow template
3. Create validators
4. Define TypeScript types
5. Register with template registry
6. Write tests
7. Document in README

## Authentication & Authorization

### User Roles

- **market_ops**: Elia staff, access to all tenants
- **tenant_admin**: Tenant administrator
- **tenant_operator**: Can execute workflows
- **tenant_viewer**: Read-only access

### JWT Structure

```typescript
{
  sub: 'user-id',
  tenantId: 'tenant-id',
  email: 'user@example.com',
  role: 'tenant_admin',
  permissions: ['workflow:create', 'workflow:execute'],
  exp: 1234567890
}
```

### Permission Checks

```typescript
// In resolver
@RequirePermission('workflow:execute')
async executeStep(parent, args, context) {
  // Implementation
}

// Manual check
if (!context.user.permissions.includes('workflow:delete')) {
  throw new ForbiddenError();
}
```

## Database Patterns

### PostgreSQL (Structured Data)

- Tenants, users, workflow templates
- Workflow index (for queries)
- Row-Level Security for tenant isolation

### MongoDB (Workflow State & Events)

- Workflow instances (complete state)
- Workflow events (audit log)
- Event sourcing for rollback

### Cross-Database Operations

```typescript
// Use transactions where possible
const pgClient = await pg.connect();
const mongoSession = await mongo.startSession();

try {
  await pgClient.query('BEGIN');
  await mongoSession.startTransaction();

  // Operations on both databases

  await mongoSession.commitTransaction();
  await pgClient.query('COMMIT');
} catch (error) {
  await mongoSession.abortTransaction();
  await pgClient.query('ROLLBACK');
  throw error;
}
```

## Security Best Practices

1. **Never commit secrets** (.env files in .gitignore)
2. **Validate all inputs** (use Zod schemas)
3. **Use parameterized queries** (prevent SQL injection)
4. **Sanitize user content** (prevent XSS)
5. **Enforce HTTPS** in production
6. **Set secure HTTP headers**
7. **Rate limit API endpoints**
8. **Log security events**

## Performance Considerations

1. **Use DataLoader** for GraphQL N+1 problem
2. **Index database queries** appropriately
3. **Cache frequently accessed data**
4. **Paginate large result sets**
5. **Optimize MongoDB queries** with indexes
6. **Use connection pooling** for databases
7. **Monitor query performance**

## Common Pitfalls

### ❌ Don't

- Commit without running tests
- Skip writing tests (breaks TDD)
- Hard-code tenant IDs or user IDs
- Store passwords in plain text
- Ignore TypeScript errors
- Use `any` type excessively
- Create circular dependencies between packages
- Mix business logic in API resolvers

### ✅ Do

- Write tests first (TDD)
- Use TypeScript strict mode
- Follow conventional commit format
- Update documentation with code changes
- Use dependency injection
- Keep functions small and focused
- Validate inputs at API boundary
- Log important operations
- Handle errors gracefully

## Troubleshooting

### Tests Failing

1. Check test isolation (no shared state)
2. Verify mocks are properly set up
3. Ensure database is seeded correctly
4. Check for async/await issues

### TypeScript Errors

1. Run `pnpm type-check` to see all errors
2. Check tsconfig.json paths
3. Ensure all dependencies are installed
4. Verify import statements

### Database Issues

1. Check connection strings in .env
2. Verify migrations have run
3. Check PostgreSQL RLS policies
4. Verify MongoDB indexes exist

## Resources

- **ADRs**: `documentation/Architecture/ADRs/`
- **API Documentation**: `documentation/api/`
- **Guides**: `documentation/guides/`
- **GraphQL Schema**: `apps/api/src/graphql/schema.graphql`

## Support

For questions or issues:

1. Check relevant ADR
2. Review documentation
3. Search existing issues
4. Ask in team chat
5. Create GitHub issue if needed

---

**Remember**: Always follow TDD, write clear commit messages, and keep documentation up to date.
