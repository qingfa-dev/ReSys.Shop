#!/usr/bin/env bash
# scripts/check-feature-conventions.sh
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

# Helper: filter out lines with // EXCEPTION comment
no_exception() { rg -v '//\s*EXCEPTION' || true; }

# AC-001: No Command/Query inlines domain fields
echo "--- AC-001: Command/Query must wrap Request/Id/Slug/Parameters ---"
# Find all sealed record Command/Query lines
all=$(rg -n 'sealed record (Command|Query)\([^)]*\)\s*:' -g '*.cs' "$MODULE_DIR" \
  | rg ': I(Command|Query|PagedQuery)(<|$)' || true)
real_violations=""
while IFS= read -r line; do
  # Extract just the parameter list between parentheses
  params=$(echo "$line" | rg -o '\([^)]*\)' | head -1)
  # Allowed: single wrapping param (Request or Parameters)
  if echo "$params" | rg -q '^\(\s*(Querying)?(Request|Parameters)(\s+\w+)?\s*\)$'; then continue; fi
  # Allowed: single Guid param ending in Id
  if echo "$params" | rg -q '^\(\s*[a-zA-Z]+\s+\w*Id\s*\)$'; then continue; fi
  # Allowed: single string param
  if echo "$params" | rg -q '^\(\s*string\s+\w+\s*\)$'; then continue; fi
  # Allowed: Guid Id, Request/Parameters
  if echo "$params" | rg -q '^\(\s*[a-zA-Z]+\s+\w*Id\s*,\s*(Querying)?(Request|Parameters)(\s+\w+)?\s*\)$'; then continue; fi
  # Allowed: multiple Guid Ids (e.g. Guid TaxonomyId, Guid Id, Request)
  if echo "$params" | rg -q '^\(\s*[a-zA-Z]+\s+\w*Id\s*(\s*,\s*[a-zA-Z]+\s+\w*Id\s*)*(,\s*(Querying)?(Request|Parameters)(\s+\w+)?\s*)?\)$'; then continue; fi
  # Allowed: wrapping Request plus Parameters (e.g. Request Request, Parameters Parameters)
  if echo "$params" | rg -q '^\(\s*(Querying)?Request\s+\w+\s*,\s*(Querying)?Parameters\s+\w+\s*\)$'; then continue; fi
  # Allowed: single string param plus Parameters (e.g. string CartToken, Parameters Parameters)
  if echo "$params" | rg -q '^\(\s*string\s+\w+\s*,\s*(Querying)?Parameters\s+\w+\s*\)$'; then continue; fi
  real_violations+="$line"$'\n'
done < <(echo "$all" | no_exception)

if [[ -z "$real_violations" ]]; then
  pass "AC-001: All Commands/Queries follow allowed patterns"
else
  fail "AC-001: Found Commands/Queries with inlined fields"
  echo "$real_violations" | head -20
fi

# AC-002: No standalone Response records (must inherit from base type or have // EXCEPTION)
echo "--- AC-002: Response must inherit from base type ---"
violations=$(rg -n 'public (sealed )?record Response\b' -g '*.cs' "$MODULE_DIR" \
  | rg -v 'Response\s*:' || true)
# Filter out lines where the preceding line or same line has // EXCEPTION
real_violations=""
while IFS= read -r line; do
  file=$(echo "$line" | cut -d: -f1)
  lineno=$(echo "$line" | cut -d: -f2)
  # Check if this line or the line before has EXCEPTION
  if sed -n "${lineno}p" "$file" | rg -q '//\s*EXCEPTION'; then continue; fi
  if [ "$lineno" -gt 1 ] && sed -n "$((lineno-1))p" "$file" | rg -q '//\s*EXCEPTION'; then continue; fi
  real_violations+="$line"$'\n'
done < <(echo "$violations")
if [[ -z "$real_violations" ]]; then
  pass "AC-002: All Response records have a base type or exception comment"
else
  fail "AC-002: Found Response records without base type or exception"
  echo "$real_violations"
fi

# AC-003: No standalone Request records (must inherit from base type or have // EXCEPTION)
echo "--- AC-003: Request must inherit from base type ---"
violations=$(rg -n 'public record Request\b' -g '*.cs' "$MODULE_DIR" \
  | rg -v 'Request\s*:' || true)
real_violations=""
while IFS= read -r line; do
  file=$(echo "$line" | cut -d: -f1)
  lineno=$(echo "$line" | cut -d: -f2)
  if sed -n "${lineno}p" "$file" | rg -q '//\s*EXCEPTION'; then continue; fi
  if [ "$lineno" -gt 1 ] && sed -n "$((lineno-1))p" "$file" | rg -q '//\s*EXCEPTION'; then continue; fi
  real_violations+="$line"$'\n'
done < <(echo "$violations")
if [[ -z "$real_violations" ]]; then
  pass "AC-003: All Request records have a base type or exception comment"
else
  fail "AC-003: Found Request records without base type or exception"
  echo "$real_violations"
fi

# AC-005: No Command with IFormFile directly
echo "--- AC-005: IFormFile must be wrapped in Request, not Command ---"
IFORM_FILE_VIOLATIONS=0
while IFS= read -r line; do
  file=$(echo "$line" | cut -d: -f1)
  if rg -q 'IFormFile' "$file" 2>/dev/null; then
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
