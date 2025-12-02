# Business Modules

Pluggable business capabilities.

## Structure

- **workflows/** - Workflow definitions
  - **contract-onboarding/** - Generic contract onboarding
  - **portfolio-management/** - Portfolio management workflows

- **market-roles/** - Market role customizations
  - **brp/** - Balance Responsible Party
  - **bsp/** - Balance Service Provider
  - **grid-user/** - Grid User
  - **ach/** - Access Contract Holder
  - **crm/** - Customer Relationship Management
  - **esp/** - Energy Service Provider
  - **dso/** - Distribution System Operator

- **integrations/** - External system integrations
  - **kong/** - Kong Dev Portal integration
  - **notification-service/** - Email/SMS notifications

## TODO

- [ ] Create generic contract onboarding workflow
- [ ] Implement BRP-specific onboarding workflow
- [ ] Implement BSP-specific onboarding workflow
- [ ] Define market role validators
- [ ] Integrate Kong Dev Portal provisioning
- [ ] Set up notification service client
