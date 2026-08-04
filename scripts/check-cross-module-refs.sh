#!/usr/bin/env bash
# scripts/check-cross-module-refs.sh
# Check for cross-module namespace references in Module/ source files.
# Exit 0 if count is at or below the expected baseline, 1 if violations exceed it.
# Baseline counts current violations; decrease as cleanup progresses.
set -euo pipefail

command -v rg &>/dev/null || { echo "FATAL: ripgrep (rg) not found. Install it first."; exit 1; }

MODULE_DIR="service/Api/src/Module"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'

# Expected baseline: 32 violations as of 2026-07-24.
# Reduce this number as violations are removed. Set to 0 when fully clean.
EXPECTED_BASELINE=26
FAIL=0

MODULES=(
  "Catalog"
  "Identity"
  "Inventory"
  "Location"
  "Ordering"
  "Payment"
  "Profile"
  "Shipping"
  "Dashboard"
)

echo "--- Cross-Module Namespace Reference Check ---"

TOTAL=0

for source_module in "${MODULES[@]}"; do
  for target_module in "${MODULES[@]}"; do
    [[ "$source_module" == "$target_module" ]] && continue

    pattern="using Module\.${target_module}\."
    search_path="${MODULE_DIR}/${source_module}"

    [[ ! -d "$search_path" ]] && continue

    count=$(rg -l "$pattern" "$search_path" -g '*.cs' 2>/dev/null | wc -l) || true

    if [[ "$count" -gt 0 ]]; then
      TOTAL=$((TOTAL + count))
      printf "  %-12s -> %-12s : %3d file(s)\n" "$source_module" "$target_module" "$count"
    fi
  done
done

echo "---"
echo "Total cross-module reference files: $TOTAL"
echo "Expected baseline:                 $EXPECTED_BASELINE"

if [[ "$TOTAL" -gt "$EXPECTED_BASELINE" ]]; then
  echo -e "${RED}FAIL${NC}: Cross-module references increased ($TOTAL > $EXPECTED_BASELINE)."
  echo "  Do not add new cross-module using statements."
  echo "  Use MediatR ISender for cross-module communication."
  FAIL=1
elif [[ "$TOTAL" -lt "$EXPECTED_BASELINE" ]]; then
  echo -e "${YELLOW}WARN${NC}: Cross-module references decreased ($TOTAL < $EXPECTED_BASELINE)."
  echo "  Update EXPECTED_BASELINE in this script to $TOTAL."
elif [[ "$TOTAL" -eq 0 ]]; then
  echo -e "${GREEN}PASS${NC}: No cross-module references. Modules are fully isolated."
else
  echo -e "${YELLOW}INFO${NC}: Baseline unchanged ($TOTAL). Keep cleaning; target is 0."
fi

echo ""
echo "--- Store vs Storefront Directory Naming Check ---"
store_dirs=$(find "$MODULE_DIR" -type d -path "*/Features/Store/*" 2>/dev/null | grep -v '/__' || true)
storefront_count=0
while IFS= read -r dir; do
  [[ -z "$dir" ]] && continue
  storefront_count=$((storefront_count + 1))
  echo "  $dir"
done <<< "$store_dirs"

if [[ "$storefront_count" -gt 0 ]]; then
  echo -e "${YELLOW}WARN${NC}: $storefront_count directory/directories use Features/Store/ (should be Features/Storefront/)."
  echo "  Convention: rename to Features/Storefront/."
else
  echo -e "${GREEN}PASS${NC}: No Features/Store/ directories found."
fi

exit $FAIL
