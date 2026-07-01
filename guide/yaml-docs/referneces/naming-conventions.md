# Naming Conventions — YAML Documentation Standard v2.0

---

## ID Patterns

All IDs must be unique within their document. Use these prefixes consistently:

| Element                      | Prefix | Format     | Examples         |
| ---------------------------- | ------ | ---------- | ---------------- |
| Principle                    | `P`    | `P[1-99]`  | `P1`, `P2`, `P12`|
| Feature                      | `F`    | `F[1-99]`  | `F1`, `F3`, `F10`|
| Abstraction (primary cat.)   | `A`    | `A[1-99]`  | `A1`, `A7`       |
| Abstraction (secondary cat.) | `B`    | `B[1-99]`  | `B1`, `B4`       |
| Abstraction (tertiary cat.)  | `C`    | `C[1-99]`  | `C1`, `C2`       |
| AntiPattern                  | `AP`   | `AP[1-99]` | `AP1`, `AP4`     |
| Scenario                     | `S`    | `S[1-99]`  | `S1`, `S3`       |
| Test Strategy                | `TS`   | `TS[1-99]` | `TS1`, `TS4`     |
| Mechanism                    | `M`    | `M[1-99]`  | `M1`, `M3`       |
| Explanation                  | `E`    | `E[1-99]`  | `E1`, `E2`       |
| Guide                        | `G`    | `G[1-99]`  | `G1`, `G2`       |
| Reference                    | `R`    | `R[1-99]`  | `R1`, `R5`       |
| Agent Rule                   | `AR`   | `AR[1-99]` | `AR1`, `AR3`     |
| Pattern (agent)              | `PP`   | `PP[1-99]` | `PP1`, `PP2`     |

**Rule:** Never reuse an ID even if an element is deleted. Assign the next unused number.

---

## Attribute Value Enumerations

### `status` (root key)
| Value        | Meaning                                             |
| ------------ | --------------------------------------------------- |
| `stable`     | Production-ready, no breaking changes expected      |
| `beta`       | API may change, use with caution                    |
| `deprecated` | Scheduled for removal; see `replacement` key        |

### `type` (feature)
| Value         | Meaning                                   |
| ------------- | ----------------------------------------- |
| `core`        | Primary capability; always present        |
| `extension`   | Optional add-on behavior                  |
| `integration` | Connects to external system / framework   |
| `utility`     | Helper functionality                      |

### `type` (abstraction)
| Value        | Meaning                             |
| ------------ | ----------------------------------- |
| `interface`  | Contract definition                 |
| `class`      | Concrete implementation             |
| `abstract`   | Abstract base class                 |
| `record`     | Immutable value type                |
| `enum`       | Enumeration                         |
| `function`   | Standalone function / static method |
| `component`  | UI or framework component           |
| `middleware` | Request pipeline middleware         |
| `handler`    | Command / event / request handler   |
| `extension`  | Extension methods class             |

### `severity` (anti_pattern)
| Value      | Impact                                    | Code-review treatment          |
| ---------- | ----------------------------------------- | ------------------------------ |
| `critical` | Data loss, security flaw, system crash    | `issue (blocking)`             |
| `high`     | Significant correctness / performance harm| `issue (blocking)`             |
| `medium`   | Maintainability or consistency harm       | `suggestion (non-blocking)`    |
| `low`      | Minor style or readability issue          | `nitpick (non-blocking)`       |

### `direction` (boundary — AgentContext)
| Value         | Meaning                                                    |
| ------------- | ---------------------------------------------------------- |
| `blocked`     | Agent must never create this dependency                    |
| `allowed`     | Agent may freely create this dependency                    |
| `conditional` | Agent may create only under conditions stated in `reason`  |

### `order` (category)
Integer starting at `1`, ascending. Categories are rendered in order.

---

## File Naming Conventions

| File              | Location          | Naming         |
| ----------------- | ----------------- | -------------- |
| Module docs       | Next to sources   | `README.yaml`  |
| AI agent context  | Repository root   | `AGENTS.yaml`  |
| Service docs      | Service root      | `README.yaml`  |
| Legacy (coexist)  | Same folder       | `README.md`    |

---

## YAML String Conventions

### When to use which block scalar
| Content type               | Style  | Reason                                      |
| -------------------------- | ------ | ------------------------------------------- |
| Code snippets              | `\|`   | Literal block — preserves all newlines      |
| Prose / paragraph text     | `>`    | Folded block — wraps long lines cleanly     |
| Short one-line strings     | inline | No block scalar needed                      |
| Strings with `:` or `#`    | `"`    | Quote to avoid YAML parse ambiguity         |

### Descriptions
- Present tense, active voice: `Provides composable entity behaviors`
- One or two sentences for `description` fields under elements
- Multi-paragraph prose allowed as folded `>` scalars

### Purpose
- Answer "why you'd reach for this" — decision-making value
- Start with a verb phrase: `Enables...`, `Prevents...`, `Centralizes...`

### Code Snippets
- Keep to 3–15 lines per `example` or `code_snippet`
- Use actual code from the project where possible
- Annotate with inline `# comments` or `// comments` for non-obvious lines
- No XML-escaping needed — YAML handles all characters natively

### Anti-pattern examples
Use `"❌ bad_code_example()"` and `"✅ good_code_example()"` as inline strings.

---

## Cross-Reference Linking

Reference elements in code comments using anchor syntax:

```csharp
// See Domain/README.yaml#A1 — IEntity interface contract
public interface IEntity { }
```

In AGENTS.yaml link modules:

```yaml
building_blocks:
  modules:
    - name: Domain
      doc: Domain/README.yaml
```

In tooling, query by key:

```python
import yaml
with open("README.yaml") as f:
    doc = yaml.safe_load(f)
features = [(f["id"], f["name"]) for f in doc.get("features", [])]
```

---

## Category Naming Patterns

Use descriptive, noun-phrase names that group by purpose:

| ❌ Avoid    | ✅ Prefer                      |
| ----------- | ------------------------------ |
| `Group 1`   | `Core DDD Abstractions`        |
| `Stuff`     | `Cross-Cutting Behaviors`      |
| `Other`     | `Utility and Extension Types`  |
| `Misc`      | `Infrastructure Adapters`      |

Standard category names by module type:

| Module type     | Primary category          | Secondary category              |
| --------------- | ------------------------- | ------------------------------- |
| DDD Domain      | `Core DDD`                | `Specialized Behaviors`         |
| Result / Models | `Result Types`            | `Functional Operators`          |
| Concerns        | `Core Concerns`           | `Utility Concerns`              |
| Auth / Security | `Authentication`          | `Authorization`                 |
| Persistence     | `Repository Contracts`    | `Query Helpers`                 |
| Messaging       | `Commands`                | `Events` · `Handlers`           |
| API             | `Endpoints`               | `Middleware` · `Filters`        |
