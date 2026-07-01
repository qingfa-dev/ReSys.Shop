# Element Reference — YAML Documentation Standard v3.0

Full key spec for every section. This is the authoritative reference.
Cross-reference HARD RULES in SKILL.md before writing any field.

---

## Root Keys (all documents except AgentContext)

```yaml
kind: BuildingBlockModule   # See kind table in SKILL.md
id: kebab-case-id           # RULE-01: unique, kebab-case
name: Namespace.ModuleName  # dot-notation canonical name
version: "1.0.0"            # RULE-09: semver, always quoted
status: stable              # stable | beta | deprecated
schema_version: "3.0"       # RULE-09: always quoted
```

---

## `meta` — Module Identity (REQUIRED, always first) [RULE-02]

```yaml
meta:
  name: Module Display Name
  description: >                          # RULE-10: folded block for prose
    Two to four sentences. What does this provide?
    Who consumes it? What problem does it solve?
  category: "Domain Logic"                # RULE-11: quote if contains special chars
  stability: Stable                       # Stable | Beta | Deprecated (display form)
  standards_alignment: "Evans DDD, SOLID" # optional; omit if not applicable
  owner: team-name-or-email               # optional; who maintains this
  deprecated_reason: "Use NewModule"      # only when status=deprecated
  dependencies:
    - name: PackageName
      version: "1.2.3"                    # RULE-09: quoted
      scope: runtime                      # runtime | dev | peer | optional
    - name: PackageName2
      version: ">=2.0.0 <3.0.0"
      scope: runtime
  # Empty dependencies list is valid — means no external deps
  dependencies: []
```

---

## `overview` — Domain Context (DomainModule only, REQUIRED there)

```yaml
overview:
  objective: >
    One paragraph: what business problem does this domain model?
    Who are the key actors? What invariants does it protect?
  bounded_context: ContextName            # optional; DDD bounded context name
  core_concepts:
    - name: Entity
      description: >
        Objects with a distinct identity that persists through state changes.
    - name: Value Object
      description: >
        Immutable objects defined entirely by their attributes, not identity.
    - name: Aggregate
      description: >
        Cluster of domain objects treated as a single transactional unit.
    - name: Business Rule
      description: >
        Encapsulated domain invariant; checked before state-changing operations.
    - name: Domain Event
      description: >
        Immutable record that something noteworthy happened in the domain.
```

---

## `principles` — Design Philosophies (see SKILL.md for when required)

```yaml
principles:
  - id: P1                                # RULE-08: unique
    name: Principle Name
    description: >                        # 1–2 sentences; what it states
      What this principle means in practice.
    rationale: >                          # why it matters; what breaks without it
      Why this decision was made; what would go wrong without it.
  - id: P2
    name: ...
    description: ...
    rationale: ...
```

---

## `features` — Capability Inventory (see SKILL.md for when required)

```yaml
features:
  - id: F1                                # RULE-08: unique
    name: Feature Name
    type: core                            # RULE-17: core|extension|integration|utility
    description: >
      What this feature provides; when you'd use it.
    api_highlight: "TypeName { key: type }"  # optional: key surface area in one line
  - id: F2
    name: Feature Name
    type: extension
    description: ...
    methods:                              # optional: use when feature = method family
      - name: "MethodName<T>(param: type) → ReturnType"
        description: What it does and when to call it.
      - name: "MethodName2(param: type) → ReturnType"
        description: ...
```

---

## `abstractions` — Component Inventory (REQUIRED in all non-Agent documents)

RULE-04: every item needs id, name, type, path, description, purpose.
RULE-15: ID prefix = tier of containing category (A=cat-1, B=cat-2, C=cat-3, D=cat-4).

```yaml
abstractions:
  - category: Primary Category Name      # noun phrase; see naming-conventions.md
    order: 1                             # RULE-14: sequential from 1
    description: >
      What groups these abstractions together. One sentence.
    items:
      - id: A1                           # RULE-15: A prefix for order=1 category
        name: IInterfaceName
        type: interface                  # RULE-18: see allowed values below
        path: ./Relative/Path/File.cs   # RULE-13: relative from module root
        description: >                  # RULE-04: 1–2 sentences, active voice
          What this abstraction does.
        purpose: >                      # RULE-04: starts with verb; decision value
          Enables / Provides / Prevents...
        contract: |                     # RULE-10: literal block; code comments only
          // Contract: pre=condition, post=condition, throws=ExceptionType
          // Invariant: description of invariant
          public interface IExample { Guid Id { get; } }
        top_use: >                      # optional: most common call-site in one line
          Most common use case.
        notes: >                        # optional: caveats, gotchas, thread safety
          Any important caveats or non-obvious behavior.
        example: |                      # RULE-10: literal block; 3–15 lines
          public class MyEntity : IExample {
            // implementation
          }
        methods:                        # optional: for class abstractions with API surface
          - name: "Method(param: type) → ReturnType"
            description: What it does.
          - name: "Method2(param: type) → ReturnType"
            description: ...
        related:                        # optional: cross-references within this doc
          - id: A2
            note: Implements this interface
          - id: B1
            note: Used together with this

      - id: A2
        name: ClassName
        type: class
        path: ./Relative/Path/File2.cs
        description: ...
        purpose: ...
        # ... additional optional fields

  - category: Secondary Category Name
    order: 2
    description: ...
    items:
      - id: B1                           # RULE-15: B prefix for order=2 category
        name: HelperClass
        type: class
        path: ./Helpers/HelperClass.cs
        description: ...
        purpose: ...

  # Tertiary category uses C prefix; quaternary uses D prefix
  - category: Tertiary Category Name
    order: 3
    description: ...
    items:
      - id: C1
        name: ...
        type: ...
        path: ...
        description: ...
        purpose: ...
```

### Allowed `type` values for abstractions [RULE-18]

| Value        | Use for                               |
| ------------ | ------------------------------------- |
| `interface`  | Contract / marker interface           |
| `class`      | Concrete implementation               |
| `abstract`   | Abstract base class                   |
| `record`     | Immutable value / DTO record type     |
| `enum`       | Enumeration type                      |
| `function`   | Standalone function / static method   |
| `component`  | UI or framework component             |
| `middleware` | ASP.NET / pipeline middleware         |
| `handler`    | Command / query / event handler       |
| `extension`  | Extension methods class               |

---

## `technical` — Internals and Mechanisms

```yaml
technical:
  mechanisms:
    - id: M1                             # RULE-08: unique, M prefix
      name: Mechanism Name
      scope: "Brief scope label"         # RULE-11: quote if contains special chars
      description: >
        Prose explanation of how this works.
      pattern: |                         # RULE-10: code / pseudocode
        // Illustrative code
        result = step1().then(step2);
      steps:                             # use steps OR pattern, not both
        - "Step one — what happens"
        - "Step two — what happens"
        - "Step three — what happens"

  explanations:                          # conceptual notes; used in DomainModule
    - id: E1                             # RULE-08: unique, E prefix
      name: Explanation Title
      prose: >
        Detailed explanation of a concept or design decision.
      key_insight: >
        One-sentence takeaway an agent or developer should remember.

  status_code_map:                       # ServiceModule only; optional
    - type: Success
      code: 200
    - type: Created
      code: 201
    - type: ValidationFailure
      code: 400
    - type: Unauthorized
      code: 401
    - type: Forbidden
      code: 403
    - type: NotFound
      code: 404
    - type: Conflict
      code: 409
    - type: UnprocessableEntity
      code: 422
    - type: InternalServerError
      code: 500
```

---

## `anti_patterns` — What NOT to Do [RULE-05]

```yaml
anti_patterns:
  - id: AP1                              # RULE-08: unique, AP prefix
    name: Anti-Pattern Name
    severity: critical                   # RULE-16: critical|high|medium|low
    avoid: >                             # RULE-05: what NOT to do
      One clear statement of the forbidden pattern.
    bad_example: |                       # RULE-10: literal block for multi-line code
      // ❌ Bad
      entity.IsDeleted = true;
    better_approach: >                   # RULE-05: what TO do instead
      One clear statement of the correct approach.
    good_example: |                      # optional but strongly recommended
      // ✅ Good
      SoftDeleteBehavior.Delete(entity);
    impact: >                            # optional: consequence if ignored
      Data inconsistency; interceptors will not fire; audit trail broken.
    related_rule: AR4                    # optional: cross-ref to agent_rule
```

---

## `usage` — Common Patterns and Scenarios [RULE-06]

```yaml
usage:
  scenarios:
    - id: S1                             # RULE-08: unique, S prefix
      title: Scenario Title
      context: >                         # RULE-06: when/why developer reaches for this
        When you need to... / Use this when...
      pattern: |                         # RULE-06 + RULE-10: 10–25 lines
        // Full working example
        var result = service.DoSomething(input);
        result.Match(
          onSuccess: v => Console.WriteLine(v),
          onFailure: e => Console.WriteLine(e.Message)
        );
      benefits:                          # optional; 2–4 bullet items
        - Consistent error response shapes
        - Automatic HTTP status code mapping
      caveats:                           # optional; known limitations
        - Does not handle streaming responses
```

---

## `testing` — Test Patterns [RULE-07 applies to steps within guides]

```yaml
testing:
  frameworks:                            # optional: declare test stack here
    - name: xUnit
      version: "2.x"
    - name: FluentAssertions
      version: "6.x"
  strategies:
    - id: TS1                            # RULE-08: unique, TS prefix
      title: Test Strategy Title
      objective: >
        What this test pattern verifies and why it matters.
      scope: unit                        # unit | integration | e2e | contract
      code_example: |                    # RULE-10: literal block
        [Fact]
        public void MethodName_Condition_Expected() {
          // Arrange
          // Act
          // Assert
        }
      notes: >                           # optional: setup requirements, shared fixtures
        Requires in-memory DbContext; do not use real database.
```

---

## `guides` — Step-by-Step Tutorials

```yaml
guides:
  - id: G1                               # RULE-08: unique, G prefix
    title: Guide Title
    objective: >
      What capability the developer gains after completing this guide.
    prerequisites:                       # optional: what must exist first
      - "Entity base class implemented (A2)"
      - "Repository interface registered in DI"
    steps:
      - order: 1                         # RULE-07: required; sequential from 1
        title: Step Title
        description: >                   # RULE-07: what to do
          Clear instruction for this step.
        code_snippet: |                  # optional; RULE-10: literal block
          // Code for this step
          public class MyEntity : Entity { }
      - order: 2
        title: Step Title
        description: ...
        code_snippet: |
          ...
    best_practices:                      # optional: 2–5 bullet items
      - Never expose a public parameterless constructor.
      - Use private or init-only setters for all properties.
    pitfalls:                            # optional: common mistakes in this guide
      - Forgetting to call base(id) in the constructor skips ID assignment.
```

---

## `references` — Literature (DomainModule, optional elsewhere)

```yaml
references:
  - id: R1                               # RULE-08: unique, R prefix
    author: "Evans, Eric"
    title: "Domain-Driven Design: Tackling Complexity in the Heart of Software"
    year: 2003
    relevance: >
      Foundational text; defines Entity, Value Object, Aggregate, Domain Event.
  - id: R2
    author: "Fowler, Martin"
    title: "Patterns of Enterprise Application Architecture"
    year: 2002
    relevance: "Repository pattern, Unit of Work, Layer Supertype."
```

---

## `file_structure` — Directory Layout (REQUIRED, always last) [RULE-03]

```yaml
file_structure: |
  ModuleName/
  ├── SubFolder/
  │   ├── File.cs              # Short description (≤60 chars)
  │   └── File2.cs             # Short description
  ├── RootFile.cs              # Short description
  └── README.yaml              # This documentation
```

Rules:
- Use ASCII tree (`├──`, `│`, `└──`)
- Annotate every non-obvious file with `# comment`
- Always include `README.yaml` as the last entry
- Omit test projects (they get their own README.yaml)

---

## AgentContext-specific keys (`agents.yaml` only)

```yaml
kind: AgentContext              # RULE-01 equivalent for agents
project: ProjectName
version: "1.0.0"                # RULE-09
schema_version: "3.0"           # RULE-09

project_info:                   # REQUIRED for AgentContext
  name: ProjectName
  version: "1.0.0"              # RULE-09
  description: >
    One sentence — what this project does.
  tech_stack: ".NET 10 | C# 13 | EF Core 9 | MediatR 12"
  repo: "https://github.com/org/repo"    # optional

building_blocks:                # RECOMMENDED
  path: src/Shared
  modules:
    - name: ModuleName
      doc: ModuleName/README.yaml
      description: One-line purpose.
      kind: DomainModule        # optional: kind of the referenced module

agent_rules:                    # REQUIRED for AgentContext [RULE-05 equivalent]
  - id: AR1                     # RULE-08: unique, AR prefix
    severity: critical          # RULE-16
    rule: >                     # imperative mood; one constraint per entry
      Never import Infrastructure namespaces into Domain layer.
    rationale: >                # optional: why this rule exists
      Domain must remain framework-agnostic for testability and portability.

boundaries:                     # REQUIRED for AgentContext
  - from: Domain
    to: Infrastructure
    direction: blocked          # RULE-19: blocked|allowed|conditional
    reason: >                   # RULE-20: required when direction=conditional
      Domain must remain framework-agnostic.
  - from: Application
    to: Infrastructure
    direction: conditional
    reason: >                   # RULE-20: required here
      Only via registered interfaces; never reference concrete EF types.

patterns:                       # REQUIRED for AgentContext
  - id: PP1                     # RULE-08: unique, PP prefix
    name: Pattern Name
    description: |              # RULE-10: literal block for multi-line
      What to do and how.
      Use specific type names, not vague descriptions.
    example: |                  # optional but strongly recommended
      // Concrete example of the pattern

code_style:                     # RECOMMENDED for AgentContext
  - File-scoped namespaces only (no block namespaces).
  - Primary constructors preferred for simple injection.
  - Use var when type is obvious from the right-hand side.
  - Prefer async/await over .Result or .Wait() calls.
  - Constants and enums in PascalCase; private fields in _camelCase.

testing_context:                # RECOMMENDED for AgentContext
  frameworks:
    - name: xUnit
      version: "2.x"
    - name: FluentAssertions
      version: "6.x"
    - name: NSubstitute
      version: "5.x"
  convention: >
    Test method names: MethodName_Condition_Expected.
    One Arrange/Act/Assert block per test. No logic in tests.
  scope: |
    Unit: pure domain logic, no I/O, no DB.
    Integration: in-memory EF Core, no external services.
    E2E: path/to/e2e — never modify without approval.

agent_skip_zones:               # OPTIONAL
  - path: path/to/file.cs
    reason: "Hand-tuned; see ADR-042"
  - pattern: "**/Migrations/**"
    reason: "EF Core migrations — use dotnet ef commands"
```
