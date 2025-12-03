# Git Hooks Configuration

This project uses [Husky](https://typicode.github.io/husky/) to manage Git hooks that ensure code quality before commits and pushes.

## Hooks Overview

### Pre-commit Hook

Runs before every commit:

- ✅ **Format check** - Ensures code follows Prettier formatting

### Pre-push Hook

Runs before every push (mirrors CI pipeline):

1. ✅ **Format check** - Code formatting validation
2. ✅ **Lint** - ESLint validation across all workspaces
3. ✅ **Type check** - TypeScript type validation
4. ✅ **Tests** - Run test suites across all packages

## Usage

### Normal Workflow

Hooks run automatically:

```bash
git commit -m "feat: add new feature"  # Pre-commit runs
git push                                # Pre-push runs (4 checks)
```

### Bypassing Hooks (Use with Caution!)

**Skip single commit:**

```bash
git commit -m "wip: work in progress" --no-verify
```

**Skip push validation:**

```bash
git push --no-verify
```

**Note:** Only bypass hooks when absolutely necessary (WIP commits, emergency hotfixes). The CI pipeline will still enforce these checks.

## Fixing Hook Failures

### Format Check Failed

```bash
pnpm format
git add .
git commit -m "your message"
```

### Lint Failed

```bash
pnpm lint:fix  # Auto-fix issues
# OR manually fix linting errors
git add .
git commit -m "your message"
```

### Type Check Failed

Fix TypeScript errors in your IDE or:

```bash
pnpm type-check  # See all errors
# Fix errors manually
```

### Tests Failed

```bash
pnpm test        # Run all tests
pnpm test:watch  # Run tests in watch mode
# Fix failing tests
```

## Troubleshooting

### Hooks Not Running

Reinstall hooks:

```bash
pnpm hooks:install
# OR
rm -rf .husky && pnpm install
```

### Hooks Taking Too Long

The pre-push hook runs the full validation pipeline (~1-3 minutes for large projects). This is intentional to catch issues before CI.

If you need to push quickly for a WIP:

```bash
git push --no-verify  # Skip validation
```

**Remember:** CI will still run these checks!

## CI Pipeline Parity

The pre-push hook mirrors the GitHub Actions pipeline:

- ✅ Format check (`format:check`)
- ✅ Lint (`lint`)
- ✅ Type check (`type-check`)
- ✅ Tests (`test`)
- ⚠️ Build step not included (too slow for pre-push)

This ensures most issues are caught locally before pushing to CI.

## Customization

To modify hooks, edit files in `.husky/`:

- `.husky/pre-commit` - Runs before commit
- `.husky/pre-push` - Runs before push

After modifying, make hooks executable:

```bash
chmod +x .husky/pre-commit .husky/pre-push
```
