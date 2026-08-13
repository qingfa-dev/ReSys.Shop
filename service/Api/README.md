# ReSys.Api

## Stripe webhooks (local)

1. The terminal that runs `dotnet run --project infra/Aspire/src/ReSys.AppHost`
   must have `export STRIPE_SECRET_KEY=sk_test_...` set as a process
   environment variable. This is separate from the user-secrets `SecretKey`
   written by `./scripts/setup-dev-secrets.sh`; the AppHost reads only the
   process env var. Without it the `stripe-listen` resource is not created.
   Then start the app (API on `https://localhost:5001`).
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
