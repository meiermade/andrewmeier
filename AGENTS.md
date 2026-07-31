# Repository Agent Instructions

## App watch command

- Start the app watcher from the `app/` directory:
  - `./fake.sh Watch`
- Preferred flow:
  - `cd app && ./fake.sh Watch`

## Pre-PR checklist

Before creating a PR, always run:

1. `cd app && ./fake.sh Test` — all tests must pass
2. `cd pulumi && npm ci && npm run check` — Pulumi TypeScript must compile
3. Maintainers with access to the `andymeier/prod` ESC environment run `pulumi preview` when infrastructure changes

External contributors are not expected to have Pulumi credentials. CI runs `pulumi up` on merge to main, so do **not** run `pulumi up` manually.
