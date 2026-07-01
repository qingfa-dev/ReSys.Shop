---
name: yaml-doc
description: >
  Generate, audit, and maintain structured YAML documentation files following
  the Shared YAML Documentation Standard v3.0. Covers README.yaml,
  AGENTS.yaml, and all module/service/domain/concern/repository/event/config
  doc variants.

  Trigger on ANY of these signals:
  - "document this", "create a README", "write the docs", "add docs"
  - "make this agent-friendly", "AGENTS.yaml", "agent context"
  - User pastes code, a file tree, or a class listing without asking a question
  - "what should AGENTS.yaml say about X"
  - "audit my docs", "review this README.yaml", "what's missing"
  - "convert this README", "port this XML doc to YAML"
  - Any request for a template, scaffold, or boilerplate for documentation

  NEVER trigger on: questions about YAML syntax in general, debugging YAML
  parsing errors unrelated to this schema, or requests to document infrastructure
  config files (docker-compose, k8s manifests, etc.).
---

# Shared YAML Documentation Standard v3.0

Machine-parseable, human-readable documentation for every layer of a codebase.
Consumable by AI coding agents, IDEs, CI linters, and developers alike.

**References:**  `references/element-reference.md` · `references/naming-conventions.md` · `references/validation-checklist.md`
**Templates:**   `templates/` — one file per document kind

---

## HARD RULES — enforce on every document, no exceptions

```
RULE-01  Every document MUST have all six root keys: kind, id, name, version, status, schema_version
RULE-02  meta MUST be the FIRST named section after root keys
RULE-03  file_structure MUST be the LAST section in every document
RULE-04  Every abstraction item MUST have: id, name, type, path, description, purpose
RULE-05  Every anti_pattern MUST have: id, name, severity, avoid, better_approach
RULE-06  Every scenario MUST have: id, title, context, pattern
RULE-07  Every guide step MUST have: order, title, description
RULE-08  All IDs MUST be unique within the document — duplicates are parse errors
RULE-09  version strings MUST be quoted: "1.0.0" not 1.0.0
RULE-10  Code blocks MUST use | (literal block scalar) — never inline strings
RULE-11  Strings containing : or # MUST be quoted
RULE-12  Boolean-like words used as strings (yes/no/true/false/on/off) MUST be quoted
RULE-13  All path values MUST be relative from the module root (start with ./)
RULE-14  category order values MUST be sequential integers starting at 1
RULE-15  Abstraction ID prefixes MUST follow tier: A=cat-1, B=cat-2, C=cat-3, D=cat-4
RULE-16  severity MUST be one of: critical | high | medium | low
RULE-17  feature type MUST be one of: core | extension | integration | utility
RULE-18  abstraction type MUST be one of the 10 allowed values (see naming-conventions.md)
RULE-19  direction (boundary) MUST be one of: blocked | allowed | conditional
RULE-20  Documents with direction=conditional MUST include a reason key
```

---

## Document Kind → Template Map

| kind                  | Use for                                    | Template file                |
| --------------------- | ------------------------------------------ | ---------------------------- |
| `BuildingBlockModule` | Generic module, library, SDK, utility      | `templates/module.yaml`      |
| `ServiceModule`       | Microservice or bounded-context API        | `templates/service.yaml`     |
| `DomainModule`        | DDD domain layer (entities, aggregates)    | `templates/domain.yaml`      |
| `ConcernsModule`      | Cross-cutting behaviors (audit, soft-del)  | `templates/concerns.yaml`    |
| `RepositoryModule`    | Data access / persistence layer            | `templates/repository.yaml`  |
| `EventsModule`        | Domain/integration events + contracts      | `templates/events.yaml`      |
| `ConfigModule`        | App configuration and options binding      | `templates/config.yaml`      |
| `PipelineModule`      | Middleware, behaviors, pipeline components | `templates/pipeline.yaml`    |
| `AgentContext`        | AI agent rules, boundaries, patterns       | `templates/agents.yaml`      |

---

## Section Inclusion Rules

| Section          | `BuildingBlock` | `Service` | `Domain` | `Concerns` | `Repository` | `Events` | `Config` | `Pipeline` | `Agent` |
| ---------------- | :-------------: | :-------: | :------: | :--------: | :----------: | :------: | :------: | :--------: | :-----: |
| `meta`           | ✅ R            | ✅ R      | ✅ R     | ✅ R       | ✅ R         | ✅ R     | ✅ R     | ✅ R       | —       |
| `overview`       | —               | —         | ✅ R     | —          | —            | ⭐       | —        | —          | —       |
| `principles`     | ⭐              | ⭐        | ⭐       | ✅ R       | ⭐           | ⭐       | ⭐       | ⭐         | —       |
| `features`       | ⭐              | ⭐        | —        | ✅ R       | ⭐           | ⭐       | —        | ⭐         | —       |
| `abstractions`   | ✅ R            | ✅ R      | ✅ R     | ✅ R       | ✅ R         | ✅ R     | ✅ R     | ✅ R       | —       |
| `technical`      | ⭐              | ✅ R      | ⭐       | ⭐         | ⭐           | ⭐       | —        | ✅ R       | —       |
| `anti_patterns`  | ⭐              | ✅ R      | ✅ R     | ✅ R       | ✅ R         | ⭐       | ⭐       | ⭐         | —       |
| `usage`          | ⭐              | ✅ R      | ⭐       | ⭐         | ✅ R         | ✅ R     | ✅ R     | ⭐         | —       |
| `testing`        | ○               | ⭐        | ✅ R     | ⭐         | ✅ R         | ⭐       | —        | ⭐         | —       |
| `guides`         | ○               | ○         | ⭐       | —          | ⭐           | —        | ✅ R     | ○          | —       |
| `references`     | —               | —         | ⭐       | —          | —            | —        | —        | —          | —       |
| `file_structure` | ✅ R            | ✅ R      | ✅ R     | ✅ R       | ✅ R         | ✅ R     | ✅ R     | ✅ R       | —       |
| `project_info`   | —               | —         | —        | —          | —            | —        | —        | —          | ✅ R    |
| `building_blocks`| —               | —         | —        | —          | —            | —        | —        | —          | ⭐      |
| `agent_rules`    | —               | —         | —        | —          | —            | —        | —        | —          | ✅ R    |
| `boundaries`     | —               | —         | —        | —          | —            | —        | —        | —          | ✅ R    |
| `patterns`       | —               | —         | —        | —          | —            | —        | —        | —          | ✅ R    |
| `code_style`     | —               | —         | —        | —          | —            | —        | —        | —          | ⭐      |
| `testing_context`| —               | —         | —        | —          | —            | —        | —        | —          | ⭐      |
| `agent_skip_zones`| —              | —         | —        | —          | —            | —        | —        | —          | ○       |

**Key:** ✅ R = Required · ⭐ = Recommended · ○ = Optional · — = Do not include

---

## Workflow: Five Steps to a Valid Document

### Step 1 — Pick the kind

Use the table above. When in doubt: microservice → `ServiceModule`, pure DDD types → `DomainModule`, anything else → `BuildingBlockModule`.

### Step 2 — Gather (or infer) information

| Need to know              | How to get it                                          |
| ------------------------- | ------------------------------------------------------ |
| Module identity           | Ask if missing; infer from namespace/folder name       |
| Purpose                   | Read public API surface; first sentence of any README  |
| Dependencies              | Package manifest (`.csproj`, `package.json`, etc.)     |
| Components                | Class/interface listing from pasted code or file tree  |
| Usage patterns            | Tests, controller actions, consumer code               |
| Anti-patterns             | Code review comments, known bugs, issue history        |
| File layout               | Directory listing                                      |

**If the user pastes code or a file tree, extract everything — do NOT ask for what is already visible.**

### Step 3 — Select sections

Cross-reference the Section Inclusion Rules table. Omit sections marked `—` for the chosen `kind`. Never add sections not in the table.

### Step 4 — Write, following HARD RULES

Use the skeleton from `element-reference.md`. Apply all 20 HARD RULES to every field.
Key reminders:
- `description`: present tense, active voice, 1–2 sentences
- `purpose`: starts with a verb, answers "why you'd reach for this"
- `contract`: use `|` literal block, plain code comments only
- `example`: use `|` literal block, 3–15 lines, annotated

### Step 5 — Self-validate before output

Run the full checklist from `references/validation-checklist.md`. For audits, emit findings in this format:
```
[ERROR]   {rule-id}  {location}  {message}
[WARNING] {rule-id}  {location}  {message}
[INFO]    {rule-id}  {location}  {message}
```
Example: `[ERROR] RULE-04  abstractions[A3]  Missing required key 'purpose'`

---

## Quick-Reference: ID Prefixes

| Tier | Category position | ID prefix | Example IDs    |
| ---- | ----------------- | --------- | -------------- |
| 1    | order: 1          | `A`       | A1, A2, A3     |
| 2    | order: 2          | `B`       | B1, B2, B3     |
| 3    | order: 3          | `C`       | C1, C2, C3     |
| 4    | order: 4          | `D`       | D1, D2, D3     |
| —    | principles        | `P`       | P1, P2, P3     |
| —    | features          | `F`       | F1, F2, F3     |
| —    | anti_patterns     | `AP`      | AP1, AP2, AP3  |
| —    | usage.scenarios   | `S`       | S1, S2, S3     |
| —    | testing.strategies| `TS`      | TS1, TS2, TS3  |
| —    | technical.mechanisms | `M`    | M1, M2, M3     |
| —    | technical.explanations | `E`  | E1, E2, E3     |
| —    | guides            | `G`       | G1, G2, G3     |
| —    | references        | `R`       | R1, R2, R3     |
| —    | agent_rules       | `AR`      | AR1, AR2, AR3  |
| —    | patterns (agent)  | `PP`      | PP1, PP2, PP3  |

**Never reuse an ID after deletion. Always assign the next unused number.**

---

## Reference Files (load on demand)

| File                                 | Load when                                         |
| ------------------------------------ | ------------------------------------------------- |
| `references/element-reference.md`    | Need full key spec for any section                |
| `references/naming-conventions.md`   | Need ID patterns, enum values, text conventions   |
| `references/validation-checklist.md` | Auditing a document or pre-output self-check      |
| `templates/module.yaml`              | Generic module / library / SDK                    |
| `templates/service.yaml`             | Microservice or API                               |
| `templates/domain.yaml`              | DDD domain layer                                  |
| `templates/concerns.yaml`            | Cross-cutting concerns / behaviors                |
| `templates/repository.yaml`          | Data access / repository layer                    |
| `templates/events.yaml`              | Domain and integration events                     |
| `templates/config.yaml`              | Application configuration and options             |
| `templates/pipeline.yaml`            | Middleware, behaviors, pipeline components        |
| `templates/agents.yaml`              | AI agent context (AGENTS.yaml)                    |
