#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
STORE="$ROOT/app/Store/src"
fail=0
check() {
  local label="$1"; shift
  if "$@" >/dev/null 2>&1; then
    echo "FAIL: $label"
    fail=1
  fi
}
check "dark: variants" rg -n 'dark:' "$STORE" --glob '*.vue' --glob '*.scss' --glob '*.css' --glob '*.ts'
check "dark-mode runtime" rg -n 'app-dark|prefers-color-scheme|useTheme|resys_theme' "$STORE"
check "raw Tailwind palette" rg -n 'class="[^"]*(green|red|gray|white|black|slate|zinc|neutral|stone|blue|yellow|sky|indigo|violet|purple|pink|rose|orange|lime|cyan|fuchsia|amber|emerald|teal)([-0-9/]|\b)' "$STORE" --glob '*.vue'
check "hex color literals in .vue" rg -n '#[0-9a-fA-F]{3}(?:[0-9a-fA-F]{3})?\b' "$STORE" --glob '*.vue'
check "non-canonical text/border tokens" rg -n -e 'text-surface-[1-9][0-9]*' -e 'border-surface-(100|300)' "$STORE" --glob '*.vue'
if [ "$fail" -eq 0 ]; then echo "Light-mode token audit OK"; else echo "Light-mode token audit FAILED"; fi
exit $fail
