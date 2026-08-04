#!/usr/bin/env bash
set -euo pipefail

PROJECT="service/Api/src/Api/Api.csproj"

if ! grep -q "UserSecretsId" "$PROJECT"; then
  echo "Api.csproj must declare <UserSecretsId>resys.shop.api</UserSecretsId> in the first <PropertyGroup>." >&2
  exit 1
fi

JWT_SECRET="${JWT_SECRET:-$(openssl rand -base64 48 | tr -d '\n' | head -c 64)}"
ENC_KEY="${SETTINGS_ENCRYPTION_KEY:-$(openssl rand -base64 32 | tr -d '\n' | head -c 44)}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_CONNECTION="Host=localhost;Database=resys_shop;Username=postgres;Password=${DB_PASSWORD}"

dotnet user-secrets set "Authentication:Jwt:Secret" "$JWT_SECRET" --project "$PROJECT"
dotnet user-secrets set "GatewayProviders:SettingsEncryptionKey" "$ENC_KEY" --project "$PROJECT"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$DB_CONNECTION" --project "$PROJECT"

if [ -n "${STRIPE_SECRET_KEY:-}" ]; then
  dotnet user-secrets set "GatewayProviders:stripe:SecretKey" "$STRIPE_SECRET_KEY" --project "$PROJECT"
  dotnet user-secrets set "GatewayProviders:stripe:WebhookSecret" "${STRIPE_WEBHOOK_SECRET:-}" --project "$PROJECT"
  dotnet user-secrets set "GatewayProviders:stripe:PublishableKey" "${STRIPE_PUBLISHABLE_KEY:-}" --project "$PROJECT"
  echo "Stripe secrets set from STRIPE_SECRET_KEY / STRIPE_WEBHOOK_SECRET / STRIPE_PUBLISHABLE_KEY env vars."
fi

echo "Dev secrets set. Verify with: dotnet user-secrets list --project $PROJECT"
