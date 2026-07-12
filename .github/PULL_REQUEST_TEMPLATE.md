## What

<!-- Brief description of the change and why -->

## Checklist

- [ ] `dotnet build` passes (warnings-as-errors)
- [ ] `dotnet test service/Api/tests/Module.UnitTests` passes
- [ ] `dotnet test service/Api/tests/Shared.UnitTests` passes
- [ ] Relevant frontend `pnpm run lint && pnpm run test:unit` passes (Admin / Store / both)
- [ ] Python `uv run ruff check . && uv run pytest` passes (if touching `service/Embedding/`)
- [ ] New features follow vertical-slice layout (Handler + Request + Response + Endpoint + Validator)
- [ ] Cross-module work uses `ISender`, not direct namespace reference
- [ ] Domain operations return `Result` / `Result<T>`, not exceptions
- [ ] `.harness/domains.yml` LOC counts are up-to-date for affected modules
- [ ] `AGENTS.md` is updated if conventions or structure changed
