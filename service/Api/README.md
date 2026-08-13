# ReSys.Api

## Stripe webhooks (local)

1. Start the app: `dotnet run --project infra/Aspire/src/ReSys.AppHost` (API on `https://localhost:5001`).
2. `STRIPE_SECRET_KEY=sk_test_... ./scripts/dev-stripe-listen.sh`.
3. Copy the printed `whsec_...` into `dotnet user-secrets set "GatewayProviders:stripe:WebhookSecret" "<whsec_...>" --project service/Api/src/Api/Api.csproj`, then restart the API resource.
4. Pay via a Checkout Session: `checkout.session.completed` fires → payment `Completed` → order auto-placed. Abandon a session: `checkout.session.expired` fires → payment `Void` + stock released + cart regresses to `Delivery`.
