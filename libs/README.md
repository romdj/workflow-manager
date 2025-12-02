# Core Libraries

Infrastructure and reusable libraries.

## Structure

- **workflow-engine/** - Core workflow execution engine
- **database/** - PostgreSQL + MongoDB clients and repositories
- **shared/** - Shared code
  - **types/** - TypeScript type definitions
  - **validation/** - Zod validation schemas
  - **utils/** - Utility functions
- **ui/** - Svelte component library

## TODO

- [ ] Implement workflow engine (state machine + event sourcing)
- [ ] Set up PostgreSQL migrations
- [ ] Configure MongoDB collections
- [ ] Define shared TypeScript types
- [ ] Create Zod validation schemas
- [ ] Build reusable UI components
