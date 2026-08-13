# ReSys.Api

## Stripe webhooks (local)

1. Set `STRIPE_SECRET_KEY=sk_test_...` for the AppHost, then start the app:
   `dotnet run --project infra/Aspire/src/ReSys.AppHost` (API on `https://localhost:5001`).
2. The AppHost starts the `stripe-listen` resource automatically when
   `STRIPE_SECRET_KEY` is set, forwarding to
   `https://localhost:5001/api/storefront/billing/webhooks/stripe`.
3. No `whsec_...` wiring is needed in Development: signature verification is
   skipped there (see `StripeWebhookDispatcher.ValidateSignature`).
4. Pay via a Checkout Session: `checkout.session.completed` fires → payment `Completed` → order auto-placed. Abandon a session: `checkout.session.expired` fires → payment `Void` + stock released + cart regresses to `Delivery`.
- Note: the API uses the ASP.NET Core dev certificate, so `stripe listen` may
  need the cert trusted (`dotnet dev-certs https --trust`) or the localhost TLS
  check bypassed for the dev session.
- Optional fallback: the manual script `./scripts/dev-stripe-listen.sh` (with
  `STRIPE_SECRET_KEY=sk_test_...`) still works, but is superseded by the
  AppHost `stripe-listen` resource.
