Short summary

This guide defines the recurring process patterns that keep the ReSys.Shop harness accurate as the codebase evolves: how to garden docs, run garbage collection on drift, encode feedback into rules, and decide when a human needs to be involved.

Key decisions
- Run the harness verifier (`.harness/scripts/verify-harness.py`) at least once per agent session that touches modules, domains, or docs.
- When a module is removed, the cleanup checklist is mandatory: source, tests, `.harness/*.yml`, `AGENTS.md`, `README.md`, `ARCHITECTURE.md`, and any active plans/specs.
- Bugs and review findings that recur must become either a new principle in `.harness/principles.yml` or an enforcement rule in `.harness/enforcement.yml`.
- Agents may act autonomously inside a single domain; cross-domain changes, new dependencies, security changes, and harness config changes require notification or human approval.

# Process Patterns

## 1) Doc-Gardening

Doc-gardening is the recurring cleanup of documentation drift. The goal is to keep `AGENTS.md`, `README.md`, `ARCHITECTURE.md`, and `.harness/` consistent with the actual code.

### When to garden

- After adding, renaming, or removing a module or significant directory.
- After a refactor that changes the public API shape, dependency direction, or layer boundaries.
- Before declaring a feature branch complete (see Verification checklist).
- Whenever the harness verifier reports drift.

### Gardening checklist

- [ ] Run `python .harness/scripts/verify-harness.py` and fix all failures.
- [ ] Confirm `AGENTS.md` still points to files that exist.
- [ ] Confirm `README.md` module count matches `.harness/domains.yml`.
- [ ] Confirm `docs/codebase/ARCHITECTURE.md` module table matches real directories.
- [ ] Confirm `.harness/domains.yml`, `principles.yml`, `enforcement.yml`, and `quality.yml` still describe the code accurately.
- [ ] If a doc is obsolete and not updated, delete it or move it to `plan/completed/` with a note.

## 2) Garbage Collection (GC)

GC removes pattern drift, duplicated helpers, and accumulated "AI slop" before it compounds. Without GC, agent-generated codebases tend to degrade fast enough to consume a significant fraction of engineering time.

### GC rules

1. **Deleted code, deleted docs** — when a module/service/feature is removed, all of these must be updated in the same change or a fast-follow PR:
   - Source and tests
   - `.harness/domains.yml`, `quality.yml`, `enforcement.yml`
   - `AGENTS.md`, `README.md`, `docs/codebase/ARCHITECTURE.md`
   - Active plans/specs that mention the removed component (mark completed, update, or archive)
2. **No orphaned references** — after removal, `grep` the repo for the old namespace/feature name. Migration `.Designer.cs` and `ApplicationDbContextModelSnapshot.cs` are allowed to retain historical entity references.
3. **Consolidate duplication** — if two helpers do the same thing, prefer the one that follows the harness rules and delete the other.
4. **Keep AGENTS.md a routing table** — if `AGENTS.md` grows past ~100 lines, move detail into `docs/codebase/` and link to it.

### GC cadence

- Per change: author runs the harness verifier.
- Per sprint/iteration: dedicated GC pass to scan for stale docs, duplicate helpers, and disabled enforcement (e.g., `ValidateVerticalSliceIsolation`).

## 3) Feedback Encoding

Feedback becomes durable only when it is encoded as a rule, a doc update, or a test. Ad-hoc fixes without encoding tend to regress.

### Encoding flow

1. **Bug or review finding** — e.g., "Module X references Module Y directly."
2. **Fix the immediate issue** — remove the direct reference, use `ISender.Send()`.
3. **Encode the prevention** —
   - If a principle would have prevented it: add/update `.harness/principles.yml`.
   - If tooling can catch it: add/update `.harness/enforcement.yml` and the verifier script.
   - If it is a known gap: add to `docs/codebase/CONCERNS.md` with a remediation item.
4. **Verify** — run the verifier and the relevant test suite.

### Examples

| Finding | Encoded as |
|---------|------------|
| Webhooks module removed but docs still list 9 modules | Updated `AGENTS.md`, `README.md`, `ARCHITECTURE.md`, `.harness/principles.yml`; added `verify-harness.py` check |
| Module cross-reference slipped into a PR | Re-enable `ValidateVerticalSliceIsolation` target (see CONCERNS.md) and fix violations |
| Hardcoded dev JWT secret | Added to `CONCERNS.md` and `AGENTS.md` Known Issues; remediation tracked in active plans |

## 4) Escalation Boundaries

These boundaries mirror `.harness/config.yml` but add concrete examples. The goal is to prevent both over-asking (slow) and under-asking (dangerous).

### Autonomous — agent decides and implements

- Single-domain changes that follow existing vertical-slice patterns.
- Doc corrections (typos, stale links, count updates).
- Dependency patch bumps that pass `dotnet build` and tests.
- Refactoring that does not change public API shape or cross-module contracts.
- Review response fixes inside the same domain.

### Notify — agent implements, human informed

- Cross-domain changes (even if mediated by `ISender`).
- New dependencies or package additions.
- Performance-sensitive changes (caching, query shapes, rate limits).
- Harness config changes (`.harness/*.yml`, verifier script).

### Human required — agent proposes, human decides

- Public API changes (route paths, DTO shapes, breaking contracts).
- Security changes (auth, secrets, permissions, CORS, rate limits).
- Architectural changes (new domains, layer reorganization, dependency direction).
- Feature deprecation or module removal.
- Changes to escalation boundaries themselves.

## 5) Verification Checklist

Before claiming a harness-aware change is complete:

- [ ] `python .harness/scripts/verify-harness.py` passes.
- [ ] `dotnet build` passes with zero warnings.
- [ ] Unit tests pass (`dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests`).
- [ ] If the change touches a frontend app, `pnpm run lint && pnpm run test:unit` passes.
- [ ] Docs updated if the change affects module count, boundaries, or conventions.

## Pointers

- `.harness/config.yml` — machine-readable escalation rules and harness metadata.
- `.harness/scripts/verify-harness.py` — automated drift detection.
- `docs/codebase/CONCERNS.md` — known tech debt and security risks.
- `docs/codebase/ARCHITECTURE.md` — domain boundaries and dependency rules.
- `AGENTS.md` — concise routing table for agents.
