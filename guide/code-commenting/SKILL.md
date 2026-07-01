---
name: code-commenting
description: >
  Apply, audit, and teach the structured Code Commenting Standard v3.0 —
  a language-agnostic, machine-parseable labelling system for code comments
  that serves both human developers and AI coding agents.

  Use this skill whenever the user asks to:
  - Add, improve, fix, or review code comments in ANY language
  - Audit a codebase or file for comment quality, consistency, or anti-patterns
  - Explain what a label like Validate:, Contract:, AgentHint:, or FIXME means
  - Write doc comments (TSDoc, XML-doc, Google Docstring, Rustdoc, GoDoc, Javadoc)
  - Annotate code for AI agent consumption (Contract:, Invariant:, Boundary:, AgentHint:)
  - Generate BREAKING, PERF, DEADLINE, or other temporal markers
  - Enforce commenting standards in a PR review or style guide
  - Understand the difference between inline labels (CAT-1 to CAT-10) and doc comments
  - Answer questions about commenting best practices or Clean Code principles

  Trigger aggressively: if the user pastes code and asks "can you clean this up",
  "add some comments", "review this", or "annotate this for our AI agent",
  consult this skill immediately.
---

# Code Commenting Standard v3.0

A structured, machine-parseable commenting system for human developers and AI coding agents.

**Machine-readable source:** `CommentingRules.xml`
**Human-readable source:** `README.md`

---

## Core Workflow

When a user asks to comment or audit code, follow this sequence:

### Step 1 — Identify the task type

| User intent | Action |
|-------------|--------|
| "Add comments to this code" | Apply labels from the decision tree below |
| "Review / audit my comments" | Check against anti-patterns (load `references/anti-patterns.md`) |
| "Write doc comments for this function" | Apply DocCommentStandards for their language |
| "Annotate for our AI agent / Claude Code" | Apply CAT-10 labels |
| "Explain label X" | Look up label in the label table below |
| "What temporal marker should I use?" | Use the Temporal Markers table |

### Step 2 — Identify the target language

Select the appropriate comment delimiter:

| Language | Delimiter | Doc standard |
|----------|-----------|--------------|
| TypeScript / JavaScript | `//` `/* */` | TSDoc |
| C# | `//` `/// ` | XML Documentation Comments |
| Python | `#` | Google Docstring Style |
| Rust | `//` `///` | Rustdoc |
| Go | `//` | GoDoc |
| Java | `//` `/** */` | Javadoc |
| SQL | `--` | Inline convention (no standard) |
| Shell / YAML / TOML | `#` | Inline convention |

### Step 3 — Choose labels using the decision tree

```
PUBLIC API surface?
  YES → Use DocCommentStandard for their language. Stop.
  NO  ↓

Time-sensitive or work-in-progress?
  YES → TemporalMarker: TODO / FIXME / HACK / TEMP / DEADLINE / DEPRECATED / BREAKING / PERF

Primarily for AI/agent consumption?
  YES → CAT-10: Contract / Invariant / Assume / AgentHint / AgentSkip / Boundary / Context

Validation / checking?         → CAT-1
Creating / mutating / deleting? → CAT-2
Computing / transforming?      → CAT-3
Domain event / business rule?  → CAT-4
Async flow / rate control?     → CAT-5
Resource acquisition / cleanup? → CAT-6
Exception / rollback?          → CAT-7
External integration boundary? → CAT-8
Logging / tracing / metrics?   → CAT-9
```

For the full label list, load `references/label-quick-reference.md`.

---

## Label Format Rules

```
// Label: Capitalised body sentence in imperative mood. Max 100 chars.
```

- **One label per comment.** Never join two actions with "and".
- **Imperative verb** in the body: "Filter expired sessions" not "Expired sessions are filtered".
- **CAT-10 agent labels** use KEY=VALUE form: `pre=x>0, post=result>0, throws=ArgumentException`.
- **TemporalMarkers** include owner and date: `TODO(owner, YYYY-MM-DD): reason — TICKET`.

---

## Comment Application Guide (inline)

### For each code block, ask:

1. **Is naming and structure already clear?** → No comment needed.
2. **Is the WHY non-obvious?** → Add a label comment.
3. **Is this a public API?** → Add a doc-comment block.
4. **Will an AI agent edit this?** → Add CAT-10 annotations.
5. **Is this temporary or in-progress?** → Add a TemporalMarker.

### Minimum viable annotations per file type

| File type | Minimum required |
|-----------|-----------------|
| Domain service / business logic | `Enforce:` on every business rule; `Validate:` on all inputs |
| External integration adapter | `Call:` / `Send:` / `Receive:` on every integration boundary |
| Repository / data access | `Cache:` with TTL; `Acquire:` on connections |
| Event handler | `Handle:` at function entry; `Raise:` on outgoing events |
| Public library / SDK | Full doc-comment on every public surface; `BREAKING` on all breaking changes |
| AI-agent-edited module | `Contract:` on entry functions; `Boundary:` at layer edges; `AgentSkip:` on hand-tuned blocks |

---

## CAT-10 Agent Annotations — Special Guidance

This category is **new in v3.0** and deserves extra attention when the user is annotating
code for AI coding agents (Claude Code, GitHub Copilot, Cursor, etc.).

### When to use each CAT-10 label

```
Contract:  → At the TOP of any non-trivial function. Tells the agent the invariants it must preserve.
Invariant: → At class/struct definition level. The agent must never violate these.
Assume:    → Near any logic that relies on an unchecked precondition.
AgentHint: → At complex branching points, switch statements, algorithm entry points.
AgentSkip: → Before hand-tuned, SIMD, legal, compliance-critical, or opaque blocks.
Boundary:  → At every architectural layer boundary (Domain/Infrastructure, API/Service, etc.)
Context:   → Near any formula, calculation, or policy with an external reference (ADR, ISO, ticket).
```

### Anti-pattern to avoid (AP-7 — Verbose Agent Context)

ETH Zurich AGENTbench (2026): verbose LLM-generated annotations **reduce** agent success by ~3% and
**increase** inference costs by 20%. Keep CAT-10 comments surgical:

```python
# ❌ Bloated
# AgentHint: This function is very complex and does many important things including
#             validation, transformation, and persistence. You should be very careful
#             when modifying it...

# ✅ Surgical
# AgentHint: do NOT add persistence here — use OrderRepository injected in __init__;
#            do NOT inline currency conversion — call CurrencyService.convert()
```

---

## DocComment Standards Summary

Load `references/label-quick-reference.md` for full table.

| Language | Block opener | Required tags |
|----------|-------------|---------------|
| TypeScript | `/** ... */` | `@param` `@returns` `@throws` `@remarks` |
| C# | `/// ` per line | `<summary>` `<param>` `<returns>` `<exception>` |
| Python | `"""..."""` | `Args:` `Returns:` `Raises:` |
| Rust | `/// ` per line | `# Examples` `# Errors` (for Result) `# Safety` (for unsafe) |
| Go | `// FuncName ...` | First sentence = summary; prose for params/returns |
| Java | `/** ... */` | `@param` `@return` `@throws` |

---

## Anti-Pattern Checklist

When auditing comments, check for these violations (full details in `references/anti-patterns.md`):

| Check | Violation |
|-------|-----------|
| Does the comment restate what the code says? | AP-1 Redundancy |
| Does the label body tell you nothing new? | AP-2 Vagueness |
| Is every trivial line commented? | AP-3 Over-commenting |
| Is capitalisation or colon formatting inconsistent? | AP-4 Inconsistent style |
| Does the comment match what the code actually does? | AP-5 Stale comment |
| Is there commented-out code? | AP-6 Dead code |
| Does the AgentHint exceed 2 lines? | AP-7 Verbose agent context |
| Does the body use passive voice? | AP-8 Passive voice |

---

## Output Format Guidelines

When generating comments for the user:

1. **Always show the full comment + the code line it annotates** — never output comments in isolation.
2. **For multi-label suggestions**, show them as a diff or annotated block, not a plain list.
3. **For doc-comment generation**, emit the full block in the language's native format.
4. **For audit output**, report violations as `suggestion (non-blocking):` or `issue (blocking):` in Conventional Comments format.
5. **For CAT-10 annotations**, always explain in one sentence WHY that specific annotation was chosen.

### Example audit output format

```
Audit results for OrderService.cs (3 issues, 2 suggestions):

issue (blocking): Line 47 — AP-5 Stale comment. Comment says "Simple entity creation"
  but code calls CreateWithInventoryTracking with 5 parameters.
  Fix: Update to "Create: Product with full inventory-management metadata"

suggestion (non-blocking): Line 82 — AP-8 Passive voice.
  Current:  // Filter: Expired items are removed
  Improved: // Filter: Remove expired items before checkout to prevent stale pricing

suggestion (non-blocking): Line 103 — CAT-10 missing for agent-edited module.
  Recommend adding: // Contract: pre=order!=null && order.Total>0, throws=PaymentTimeoutException
```

---

## Reference Files

| File | When to load |
|------|--------------|
| `references/label-quick-reference.md` | Full label table needed; user is unsure which label to use |
| `references/anti-patterns.md` | Auditing comments; checking for violations |
| `CommentingRules.xml` | Full authoritative standard; formal spec needed; edge-case resolution |
| `README.md` | User wants a human-readable overview to share with their team |