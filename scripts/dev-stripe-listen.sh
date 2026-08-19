#!/usr/bin/env bash
# Manual fallback: forward real Stripe webhook events to the local API.
# Superseded by the AppHost `stripe-listen` resource: set STRIPE_SECRET_KEY
# and start via Aspire for the normal flow
# (`dotnet run --project infra/Aspire/src/ReSys.AppHost`).
# Aspire pins the API HTTPS port to 5001 (see infra/Aspire/src/ReSys.AppHost/AppHost.cs).
# Usage: STRIPE_SECRET_KEY=sk_test_... ./scripts/dev-stripe-listen.sh
set -euo pipefail
if [ -z "${STRIPE_SECRET_KEY:-}" ]; then
  echo "Set STRIPE_SECRET_KEY (sk_test_...) first." >&2
  exit 1
fi
echo "Forwarding Stripe events to https://localhost:5001/api/storefront/billing/webhooks/stripe"
echo ""
echo "Copy the printed 'webhook signing secret' (whsec_...) into the API, then restart the API:"
echo "  dotnet user-secrets set \"GatewayProviders:stripe:WebhookSecret\" \"<whsec_...>\" --project service/Api/src/Api/Api.csproj"
echo ""
echo "NOTE: the secret is per 'stripe listen' run. Keep this process running; if you restart it, re-set the secret."
stripe listen --forward-to https://localhost:5001/api/storefront/billing/webhooks/stripe --api-key "$STRIPE_SECRET_KEY"
