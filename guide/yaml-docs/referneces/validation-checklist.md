# Validation Checklist — YAML Documentation Standard v2.0

Use when auditing existing YAML docs or before outputting a new one.

---

## Structural Validity

- [ ] Banner comment present at the top of the file
- [ ] Root keys present: `kind`, `id`, `name`, `version`, `status`, `schema_version`
- [ ] `meta` is the first named section (after root keys)
- [ ] `file_structure` is the last section
- [ ] YAML is well-formed (no duplicate keys, valid indentation)
- [ ] All multi-line code blocks use `|` (literal block scalar)

## Required Keys

- [ ] `meta` present with: `name`, `description`, `category`, `stability`, `dependencies`
- [ ] `abstractions` present with at least one `categories` entry and one `items` entry
- [ ] `file_structure` present as a literal block scalar (`|`) showing the directory tree

## ID Consistency

- [ ] All IDs are unique within the document (no two `P1`s, `A1`s, etc.)
- [ ] IDs follow naming convention (`P`, `F`, `A`, `B`, `AP`, `S`, `TS`, `M`, `G`, `R`)
- [ ] Category `order` values are sequential integers starting at `1`
- [ ] Feature `type` is one of: `core | extension | integration | utility`
- [ ] Abstraction `type` is one of the allowed values (see `naming-conventions.md`)
- [ ] Anti-pattern `severity` is one of: `critical | high | medium | low`

## Content Quality

- [ ] Every abstraction has `path`, `description`, and `purpose`
- [ ] All `path` values are relative from the Shared root
- [ ] All `description` values are active voice, present tense
- [ ] Anti-patterns each have both `avoid` and `better_approach`
- [ ] Scenarios each have `context` and `pattern`
- [ ] Test strategies each have `objective`

## YAML Encoding

- [ ] Strings containing `:` or `#` are quoted
- [ ] No bare `{` or `}` in unquoted strings
- [ ] Code blocks use `|` block scalar, not inline strings
- [ ] Boolean-like words (`yes`, `no`, `true`, `false`, `on`, `off`) are quoted when used as strings
- [ ] Version strings are quoted: `"1.0.0"` not `1.0.0`

---

## Anti-patterns to Flag in Audit Output

| Finding                                        | Severity    | Audit message format                                                    |
| ---------------------------------------------- | ----------- | ----------------------------------------------------------------------- |
| Missing `purpose` key                          | `[ERROR]`   | Missing required key 'purpose' in abstraction {id}                     |
| Missing `description` key                      | `[ERROR]`   | Missing required key 'description' in {element} {id}                   |
| No `severity` on anti_pattern                  | `[WARNING]` | anti_pattern {id} has no severity — defaulting to medium               |
| Scenario missing `benefits`                    | `[INFO]`    | scenario {id} has no benefits — consider adding                        |
| Stale `path` (non-existent file)               | `[WARNING]` | path in abstraction {id} may be stale — verify against filesystem      |
| Duplicate ID                                   | `[ERROR]`   | Duplicate ID "{id}" — IDs must be unique within document               |
| Empty `dependencies` without explanation       | `[INFO]`    | dependencies is empty — confirm module has no external deps             |
| `file_structure` not last section              | `[WARNING]` | file_structure should be the last section in the document              |
| Missing banner comment                         | `[INFO]`    | No banner comment — consider adding for tooling                        |
| Unquoted version string                        | `[WARNING]` | Version "{v}" should be quoted to prevent YAML float parsing           |
| Inline string used for code block              | `[WARNING]` | Code in {element} {id} should use literal block scalar (|)             |

---

## Severity Guide for Audit Reports

```
[ERROR]   — Document is invalid or will cause parser/agent failures. Must fix.
[WARNING] — Document is valid but may mislead consumers or agents. Should fix.
[INFO]    — Improvement opportunity. Consider fixing.
```

---

## Quick Audit Checklist (one-pass review)

Read the document top to bottom checking:

1. **Root** — has all 6 required keys? (`kind`, `id`, `name`, `version`, `status`, `schema_version`)
2. **Meta** — has 5 required keys? (`name`, `description`, `category`, `stability`, `dependencies`)
3. **Abstractions** — every item has `path` + `description` + `purpose`?
4. **Anti-patterns** — every item has `severity` + `avoid` + `better_approach`?
5. **Scenarios** — every item has `context` + `pattern`?
6. **File structure** — matches what you'd expect from the module?
7. **IDs** — scan for duplicates (gaps are fine, duplicates are errors)
8. **YAML validity** — run through a YAML linter or `python -c "import yaml; yaml.safe_load(open('README.yaml'))"`
