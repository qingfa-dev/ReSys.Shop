# ReSys.Api

## Stripe webhooks (local)

1. Set the Stripe key once in the AppHost project's user-secrets (the AppHost
   reads it from `Stripe:ApiKey`; the process env var `STRIPE_SECRET_KEY` is a
   fallback):
   ```
   dotnet user-secrets set "Stripe:ApiKey" "sk_test_..." --project infra/Aspire/src/ReSys.AppHost
   ```
   Without a key the `stripe-listen` resource is not created (the AppHost
   prints a warning). Then start the app (API on `https://localhost:5001`).
2. The AppHost starts the `stripe-listen` resource automatically when
   `STRIPE_SECRET_KEY` is set, forwarding to
   `https://localhost:5001/api/storefront/billing/webhooks/stripe`.
3. No `whsec_...` wiring is needed in Development: signature verification is
   skipped only when no `WebhookSecret` is configured (see
   `StripeWebhookDispatcher.ValidateSignature`); a configured secret is always
   verified, including in Development.
4. Pay via a Checkout Session: `checkout.session.completed` fires → payment `Completed` → order auto-placed. Abandon a session: `checkout.session.expired` fires → payment `Void` + stock released + cart regresses to `Delivery`.
- Note: the API uses the ASP.NET Core dev certificate, so `stripe listen` may
  need the cert trusted (`dotnet dev-certs https --trust`) or the localhost TLS
  check bypassed for the dev session.
- Optional fallback: the manual script `./scripts/dev-stripe-listen.sh` (with
  `STRIPE_SECRET_KEY=sk_test_...`) still works, but is superseded by the
  AppHost `stripe-listen` resource.
