# Process

How the harness-driven codebase is kept healthy over time.

## Core Sections (Required)

### 1) Doc-Gardening

Every document in `docs/codebase/` and `.harness/` must reflect current reality. A doc that lies is worse than no doc.

- **Cadence**: quarterly review (aligned with `.harness/quality.yml` `review_cadence`). Review all `last_reviewed` dates in `quality.yml` — any domain older than 90 days triggers a spot-check.
- **Trigger events**: after any architectural change (new module, new Shared/ pillar, new cross-cutting contract), update `domains.yml`, `ARCHITECTURE.md`, and `AGENTS.md` within the same PR.
- **Staleness linter**: `dotnet build` verifies all projects referenced in `domains.yml` exist; `python .harness/scripts/verify-harness.py` cross-checks AGENTS.md links, knowledge.yml paths, and domains.yml LOC counts.
- **Who**: the agent that makes a structural change is responsible for updating the affected docs. Reviewers check for doc updates in PR template.

### 2) Garbage Collection (GC)

Without periodic GC, agent-generated code drifts. GC targets three categories:

| Category | What to look for | Frequency | Action |
|----------|-----------------|-----------|--------|
| **Pattern drift** | Files that don't match the vertical-slice naming (e.g. single `CreateProduct.cs` without split files) | Quarterly | Refactor to split or add to `quality.yml` gap list |
| **Empty directories** | `find service/Api/src/Module -type d -empty` — any newly empty trees introduced by refactoring | Monthly | Remove or document intent in a feature plan |
| **Accumulated helpers** | Duplicate `string.IsNullOrEmpty` guards, duplicate `Result` factory methods, copy-pasted Loggers classes | Quarterly | Extract into `Shared/` abstractions |

**GC sweep command**:
```bash
python .harness/scripts/verify-harness.py  # identifies drift
find service/Api/src/Module -type d -empty  # empty directories
rg "TODO\(|FIXME\(" --glob "*.cs" | grep -v test | sort  # stale temporal markers
```

### 3) Feedback Encoding

When a bug is found, the fix must also harden the harness so the same class of bug cannot recur. This is the **backprop protocol**:

1. **Trace the cause** — is it a missing convention, a violated rule, or a missing test?
2. **Encode the invariant** — if it's new, add to `principles.yml` or `enforcement.yml`. If it's violated, add a lint rule or test.
3. **Document the gap** — add to `quality.yml` gap list for the affected domain.
4. **Examples**:
   - Bug: handler returns `Ok` for an already-existing slug → Add `Conflict` to `Result.Method.cs` and document in `principles.yml#result-not-exceptions` example.
   - Bug: `Nullable` warning escaping build → Ensure `TreatWarningsAsErrors=true` and add Roslyn analyzer rule.
   - Bug: two modules importing from each other → This is permitted; prefer MediatR `ISender` for cross-module behavior and enable `ValidateVerticalSliceIsolation` only if modules are ever split into separate projects.

### 4) Escalation Boundaries

Defines what decisions require human judgment vs. what agents resolve autonomously.
These match `.harness/config.yml` `escalation` block.

| Level | Scope | Examples |
|-------|-------|----------|
| **Autonomous** (agent decides) | Single-domain changes, doc corrections, dependency bumps, refactoring, review responses | Rename a handler, extract a helper, update a NuGet version |
| **Notify** (agent acts, flags for human awareness) | Cross-domain changes, new dependencies, performance-sensitive paths, harness config changes | Change an `ISender` contract, add a new NuGet package, modify `Program.cs` |
| **Human required** (agent proposes, human approves) | Public API changes, security changes, architectural changes, feature deprecation | New module, new auth provider, change to JWT signing, delete a module |

### 5) Merge Philosophy

- **Short-lived PRs**: target <1 day open. If blocked, create a follow-up ticket and merge what's safe.
- **Pre-merge gates**: PR template checklist + CI workflow (`dotnet build`, all unit tests, lint).
- **Follow-ups**: bugs found post-merge become tickets in `plan/`, not reverted PRs.
- **Prerequisite**: Level 2+ maturity (automated enforcement in place, test coverage catches regressions, agents can generate follow-up fixes). Without these, relaxed merge gates are reckless.

### 6) Agent Review (Level 3+ target)

At Level 3 maturity, agent-to-agent review should be the primary quality gate:
- **L1–2**: Humans review everything (current state)
- **L3**: Agents pre-review, humans spot-check
- **L4**: Agent-to-agent review, humans only for escalations

Current repo is at **Level 1 (Map)**, approaching **Level 2 (Rules)**. Agent review is not yet appropriate.

### 7) Evidence

- `.harness/config.yml:33-49` — escalation boundaries
- `.harness/quality.yml:18` — `review_cadence: quarterly`
- `.harness/enforcement.yml:48-61` — file limits (GC trigger)
- `.harness/scripts/verify-harness.py` — staleness detection
- `.github/PULL_REQUEST_TEMPLATE.md` — merge gates
- `.github/workflows/ci.yml` — CI pipeline
- `AGENTS.md:66-73` — known issues (triggers for backprop)
- `plan/data-README-consolidation-modules-1.md` — active plan (doc-gardening work)
