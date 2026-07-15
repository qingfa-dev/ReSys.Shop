# Feature Convention Enforcement — CI Infrastructure

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add automated CI checks that detect feature convention violations (inline-field commands, unbased responses, unbased requests, inline IFormFile) before PR merge.

**Architecture:** A single bash script `scripts/check-feature-conventions.sh` performs grep/ripgrep-based static analysis against the Module assembly. Each check maps to an acceptance criterion from the spec. The script exits non-zero on any violation. CI workflow is updated to run it after build.

**Tech Stack:** bash, ripgrep (`rg`), GitHub Actions YAML

## Global Constraints

- Warnings-as-errors global; any warning fails the build
- CI runs on PR/push for .NET, both Vue SPAs, Embedding service, and Benchmarks
- Convention checks live as a standalone script in `scripts/check-feature-conventions.sh`
- Each check returns the violating file:line or "PASS"
- Exit code 0 = all checks pass; exit code 1 = any check fails

---

## File Map

| File | Purpose | Action |
|---|---|---|
| `scripts/check-feature-conventions.sh` | Single bash script running all 5 convention checks | Create |
| `.github/workflows/ci.yml` | Add convention check step after build | Modify |

---

### Task 1: Create convention check script

**Files:**
- Create: `scripts/check-feature-conventions.sh`

- [ ] **Step 1: Write the convention check script**

```bash
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
violations=$(rg -n 'sealed record (Command|Query)\(' --include '*.cs' "$MODULE_DIR" \
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
violations=$(rg -n 'public (sealed )?record Response\b' --include '*.cs' "$MODULE_DIR" \
  | rg -v 'Response\s*:' || true)
if [[ -z "$violations" ]]; then
  pass "AC-002: All Response records have a base type"
else
  fail "AC-002: Found Response records without base type"
  echo "$violations"
fi

# AC-003: No standalone Request records (must inherit from base type)
echo "--- AC-003: Request must inherit from base type ---"
violations=$(rg -n 'public record Request\b' --include '*.cs' "$MODULE_DIR" \
  | rg -v 'Request\s*:' || true)
if [[ -z "$violations" ]]; then
  pass "AC-003: All Request records have a base type"
else
  fail "AC-003: Found Request records without base type"
  echo "$violations"
fi

# AC-005: No Command with IFormFile directly
echo "--- AC-005: IFormFile must be wrapped in Request, not Command ---"
violations=$(rg -n 'sealed record Command\(' --include '*.cs' "$MODULE_DIR" \
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
done < <(rg -l 'sealed record Command\(' --include '*.cs' "$MODULE_DIR" 2>/dev/null || true)

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
```

- [ ] **Step 2: Make script executable**

```bash
chmod +x scripts/check-feature-conventions.sh
```

- [ ] **Step 3: Run against current codebase to verify it catches violations**

```bash
bash scripts/check-feature-conventions.sh
```

Expected: AC-001, AC-002, AC-003, AC-005 all report FAIL with current violations listed.

- [ ] **Step 4: Commit**

```bash
git add scripts/check-feature-conventions.sh
git commit -m "ci: add feature convention enforcement script"

```

---

### Task 2: Add convention check to CI workflow

**Files:**
- Modify: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: `scripts/check-feature-conventions.sh` from Task 1

- [ ] **Step 1: Read existing CI file**

Read the current `.github/workflows/ci.yml` to find the right insertion point.

```bash
cat .github/workflows/ci.yml
```

Expected: Shows existing jobs. Find the .NET build/unit test step.

- [ ] **Step 2: Add convention check step after dotnet build, before dotnet test**

Insert after the `dotnet build` step (find exact indentation from the file; use 2-space YAML indent typical for this project):

```yaml
      - name: Check feature conventions
        run: bash scripts/check-feature-conventions.sh
```

This assumes the step already has `working-directory: service/Api` or runs from repo root. If the build step uses `working-directory`, the convention check must run from repo root (not from `service/Api/`).

**Important:** If the build step uses `working-directory: service/Api`, the convention check must be:

```yaml
      - name: Check feature conventions
        run: bash scripts/check-feature-conventions.sh
```

It runs from repo root, so adjust accordingly.

- [ ] **Step 3: Verify the YAML is valid**

```bash
python -c "import yaml; yaml.safe_load(open('.github/workflows/ci.yml')); print('YAML valid')"
```

- [ ] **Step 4: Commit**

```bash
git add .github/workflows/ci.yml
git commit -m "ci: add feature convention checks to CI pipeline"

```
