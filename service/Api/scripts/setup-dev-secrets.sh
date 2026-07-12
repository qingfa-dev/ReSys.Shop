#!/usr/bin/env bash
set -euo pipefail

PROJECT="service/Api/src/Api/Api.csproj"

if ! grep -q "UserSecretsId" "$PROJECT"; then
  echo "Api.csproj must declare <UserSecretsId>resys.shop.api</UserSecretsId> in the first <PropertyGroup>." >&2
  exit 1
fi

JWT_SECRET="${JWT_SECRET:-$(openssl rand -base64 48 | tr -d '\n' | head -c 64)}"
ENC_KEY="${SETTINGS_ENCRYPTION_KEY:-$(openssl rand -base64 32 | tr -d '\n' | head -c 44)}"

dotnet user-secrets set "Authentication:Jwt:Secret" "$JWT_SECRET" --project "$PROJECT"
dotnet user-secrets set "GatewayProviders:SettingsEncryptionKey" "$ENC_KEY" --project "$PROJECT"

echo "Dev secrets set. Verify with: dotnet user-secrets list --project $PROJECT"
