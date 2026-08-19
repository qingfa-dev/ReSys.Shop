#!/usr/bin/env bash
# scripts/check-route-conventions.sh
# Verify every leaf Route constant follows api/admin/{module} or api/storefront/{module} prefix.
# Exit 0 if all pass, 1 if any violation found.
set -euo pipefail

command -v rg &>/dev/null || { echo "FATAL: ripgrep (rg) not found. Install it first."; exit 1; }

MODULE_DIR="service/Api/src/Module"
FAIL=0
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

pass() { echo -e "${GREEN}PASS${NC}: $1"; }
fail() { echo -e "${RED}FAIL${NC}: $1"; FAIL=1; }

MODULES="catalog|identity|inventory|location|ordering|billing|customer|shipping|dashboard"

echo "--- Route Convention Check ---"

# Extract all public Route const string values (leaf routes only)
VIOLATIONS=$(rg -n 'public const string Route = "' -g '*Feature*.cs' -g '*Feature*.cs' "$MODULE_DIR" \
  | sed -E 's/.*"([^"]*)".*/\1/' \
  | grep -vE "^api/(admin|storefront)/($MODULES)" || true)

if [ -n "$VIOLATIONS" ]; then
  fail "Found routes not matching api/admin/{{module}} or api/storefront/{{module}}:"
  echo "$VIOLATIONS"
else
  pass "All leaf Route constants follow convention"
fi

# Check OptionValueFeature routes (file may have been removed)
OV_FILE="$MODULE_DIR/Catalog/Domain/OptionTypes/Values/OptionValue.Feature.cs"
if [ -f "$OV_FILE" ]; then
  OV_VIOLATIONS=$( rg -n 'const string \w+ = "' "$OV_FILE" \
    | grep -v 'Tags' \
    | grep -v 'OptionValue = ' \
    | sed -E 's/.*"([^"]*)".*/\1/' \
    | grep -vE "^api/admin/catalog/option-types/" || true)

  if [ -n "$OV_VIOLATIONS" ]; then
    fail "OptionValueFeature routes not matching convention:"
    echo "$OV_VIOLATIONS"
  else
    pass "OptionValueFeature routes follow convention"
  fi
else
  pass "OptionValueFeature.cs removed (routes consolidated into CatalogFeature.Admin)"
fi

exit $FAIL
