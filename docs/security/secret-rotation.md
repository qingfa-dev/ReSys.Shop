# Secret Rotation — WIP-MVP

The current `appsettings.Development.json` contains a hardcoded JWT secret. This is acceptable for local development but MUST be replaced before any non-dev deployment.

## Production deployment checklist

- [ ] Move `Jwt:Secret`, `Stripe:SecretKey`, `SendGrid:ApiKey`, `Sinch:*`, `Postgres:Password`, `Redis:Password` to environment variables or a secrets manager.
- [ ] Use `dotnet user-secrets` for local dev secrets.
- [ ] Configure Aspire parameter provider for production secrets.
- [ ] Set up secret rotation policy (90-day rotation recommended).

## Status

`[WIP-MVP]` — deferred from MVP scope. See `docs/superpowers/specs/2026-07-07-mvp-cut-design.md` (Goal 2, WIP item 2).
