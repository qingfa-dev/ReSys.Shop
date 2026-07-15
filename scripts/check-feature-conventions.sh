#!/usr/bin/env bash
# scripts/check-feature-conventions.sh
# Exit 0 if all pass, 1 if any violation found.
set -euo pipefail

MODULE_DIR="service/Api/src/Module"
FAIL=0
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

pass() { echo -e "${GREEN}PASS${NC}: $1"; }
fail() { echo -e "${RED}FAIL${NC}: $1"; FAIL=1; }

# AC-001: No Command/Query inlines domain fields (only Request/Id/Slug/Parameters allowed)
echo "--- AC-001: Command/Query must wrap Request/Id/Slug/Parameters ---"
violations=$(rg -n 'sealed record (Command|Query)\(' -g '*.cs' "$MODULE_DIR" \
  | rg -v '\((Request|Guid Id|string Slug|Parameters Parameters|ICommand<|IQuery<|IPagedQuery<)' \
  | rg 'sealed record' || true)
if [[ -z "$violations" ]]; then
  pass "AC-001: All Commands/Queries follow allowed patterns"
else
  fail "AC-001: Found Commands/Queries with inlined fields"
  echo "$violations"
fi

# AC-002: No standalone Response records (must inherit from base type)
echo "--- AC-002: Response must inherit from base type ---"
violations=$(rg -n 'public (sealed )?record Response\b' -g '*.cs' "$MODULE_DIR" \
  | rg -v 'Response\s*:' || true)
if [[ -z "$violations" ]]; then
  pass "AC-002: All Response records have a base type"
else
  fail "AC-002: Found Response records without base type"
  echo "$violations"
fi

# AC-003: No standalone Request records (must inherit from base type)
echo "--- AC-003: Request must inherit from base type ---"
violations=$(rg -n 'public record Request\b' -g '*.cs' "$MODULE_DIR" \
  | rg -v 'Request\s*:' || true)
if [[ -z "$violations" ]]; then
  pass "AC-003: All Request records have a base type"
else
  fail "AC-003: Found Request records without base type"
  echo "$violations"
fi

# AC-005: No Command with IFormFile directly
echo "--- AC-005: IFormFile must be wrapped in Request, not Command ---"
violations=$(rg -n 'sealed record Command\(' -g '*.cs' "$MODULE_DIR" \
  | xargs -I{} sh -c 'echo "{}" | rg -q "IFormFile" && echo "{}" || true' || true)
# Simpler: find sealed record Command lines that also have IFormFile in the same file
IFORM_FILE_VIOLATIONS=0
while IFS= read -r line; do
  file=$(echo "$line" | cut -d: -f1)
  if rg -q 'IFormFile' "$file" 2>/dev/null; then
    # Check if IFormFile is in Command line or Request line
    cmd_line=$(rg -n 'sealed record Command\(' "$file" 2>/dev/null || true)
    if echo "$cmd_line" | rg -q 'IFormFile'; then
      echo "  $cmd_line"
      IFORM_FILE_VIOLATIONS=$((IFORM_FILE_VIOLATIONS + 1))
    fi
  fi
done < <(rg -l 'sealed record Command\(' -g '*.cs' "$MODULE_DIR" 2>/dev/null || true)

if [[ "$IFORM_FILE_VIOLATIONS" -eq 0 ]]; then
  pass "AC-005: No Command carries IFormFile directly"
else
  fail "AC-005: Found $IFORM_FILE_VIOLATIONS Command(s) with IFormFile"
fi

echo "---"
if [[ "$FAIL" -eq 1 ]]; then
  echo -e "${RED}Some convention checks FAILED.${NC}"
  exit 1
else
  echo -e "${GREEN}All convention checks PASSED.${NC}"
  exit 0
fi
