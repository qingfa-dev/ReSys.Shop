#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLAN="$ROOT/plan/refactor-store-primevue-overhaul-1.md"
STORE="$ROOT/app/Store/src"
fail=0
total=0
while IFS=$'\t' read -r comp file; do
  [ -z "$comp" ] && continue
  total=$((total + 1))
  if ! grep -qE "<${comp}([ />]|$)" "$STORE/$file" 2>/dev/null; then
    echo "MISSING: <$comp> not found in $STORE/$file"
    fail=1
  fi
done < <(grep -E '^\| [A-Z][A-Za-z]+ \| [^|]+ \| via ' "$PLAN" | awk -F'|' '{gsub(/[ ]+/,"",$2); gsub(/[ ]+/,"",$3); if ($2 != "" && $3 != "") print $2 "\t" $3}')
if [ "$fail" -eq 0 ]; then echo "PrimeVue coverage OK: $total matrix rows verified."; else echo "PrimeVue coverage FAILED."; fi
exit $fail
