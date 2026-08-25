## What

<!-- Brief description of the change and why -->

## Checklist

- [ ] `dotnet build` passes (warnings-as-errors)
- [ ] `dotnet test service/Api/tests/Module.UnitTests` passes
- [ ] `dotnet test service/Api/tests/Shared.UnitTests` passes
- [ ] Relevant frontend `pnpm run lint && pnpm run test:unit` passes (Admin / Store / both)
- [ ] Python `uv run ruff check . && uv run pytest` passes (if touching `service/Embedding/`)
- [ ] `bash scripts/check-feature-conventions.sh` passes (C# changes)
- [ ] New features follow vertical-slice layout (Handler + Request + Response + Endpoint + Validator)
- [ ] Cross-module behavior uses `ISender` or direct service/navigation calls as fits the feature slice
- [ ] Domain operations return `Result` / `Result<T>`, not exceptions

### Security

- [ ] No secrets, tokens, or credentials committed
- [ ] New endpoints have rate-limit policy or documented exemption
- [ ] New file types added to upload allowlist/blocklist if relevant
- [ ] New external dependencies reviewed for supply-chain risk
