---
goal: Create EFCore Migration YAML Documentation
version: 1.0
date_created: 2026-07-02
status: 'Completed'
tags: feature, documentation, efcore, migrations, yaml-docs
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Create structured YAML documentation for the Api.Migrations project following the Shared YAML Documentation Standard v3.0. Two files will be produced: README.yaml documenting the Migrations module architecture, and GUIDE.yaml with step-by-step instructions for creating new EF Core migrations using `dotnet ef` CLI commands.

## 1. Requirements & Constraints

- **REQ-001**: README.yaml MUST follow the Shared YAML Documentation Standard v3.0 (kind: BuildingBlockModule)
- **REQ-002**: GUIDE.yaml MUST follow the Shared YAML Documentation Standard v3.0 (kind: BuildingBlockModule) with the `guides` section as its primary content
- **REQ-003**: All 20 HARD RULES from SKILL.md MUST pass validation
- **REQ-004**: All abstraction items MUST have id, name, type, path, description, purpose
- **REQ-005**: All path values MUST be relative from the Migrations module root (start with ./)
- **REQ-006**: version strings MUST be quoted semver ("1.0.0")
- **REQ-007**: Code blocks MUST use | (literal block scalar)
- **CON-001**: files MUST be placed in `/service/Api/src/Migrations/`
- **CON-002**: No external files beyond the Api.Migrations project scope can be referenced
- **CON-003**: Must document the existing DesignTimeDbContextFactory, existing migration (InitialCreate), and ModelSnapshot
- **GUD-001**: Follow naming conventions from `guide/yaml-docs/referneces/naming-conventions.md`
- **GUD-002**: Use active voice, present tense for all descriptions
- **GUD-003**: purpose fields must start with a verb

## 2. Implementation Steps

### Implementation Phase 1: README.yaml

- GOAL-001: Create README.yaml documenting the Api.Migrations module with meta, abstractions, technical, anti-patterns, usage, guides, and file_structure sections.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Write root keys (kind, id, name, version, status, schema_version) with kind=BuildingBlockModule | ✅ | 2026-07-02 |
| TASK-002 | Write meta section with name, description, category, stability, dependencies (EF Core Design, Npgsql, EFCore.NamingConventions) | ✅ | 2026-07-02 |
| TASK-003 | Write principles section (P1: Design-Time Isolation, P2: Single Source of Truth for Schema) | ✅ | 2026-07-02 |
| TASK-004 | Write features section (F1: Design-Time Factory, F2: Migration Generation, F3: Model Snapshot) | ✅ | 2026-07-02 |
| TASK-005 | Write abstractions section with categories: Core Abstractions (A1: DesignTimeDbContextFactory), Generated Artifacts (A2: Migration, A3: ModelSnapshot) | ✅ | 2026-07-02 |
| TASK-006 | Write technical section (M1: Migration Generation Pipeline, M2: Model Snapshot Diffing) | ✅ | 2026-07-02 |
| TASK-007 | Write anti_patterns section (AP1: Manual SQL in migrations, AP2: Skipping model snapshot) | ✅ | 2026-07-02 |
| TASK-008 | Write usage section (S1: Creating a migration, S2: Reverting a migration) | ✅ | 2026-07-02 |
| TASK-009 | Write testing section (TS1: Migration idempotency verification) | ✅ | 2026-07-02 |
| TASK-010 | Write file_structure section with ASCII tree of the Migrations project | ✅ | 2026-07-02 |

### Implementation Phase 2: GUIDE.yaml

- GOAL-002: Create GUIDE.yaml with detailed step-by-step instructions for creating, reverting, and applying EF Core migrations.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Write root keys (kind=BuildingBlockModule, id=migrations-guide) | ✅ | 2026-07-02 |
| TASK-012 | Write meta section explaining this is a standalone migration guide | ✅ | 2026-07-02 |
| TASK-013 | Write guides section with G1: "Add a New Migration" (steps: detect entity changes, run dotnet ef migrations add, review generated code, update model snapshot) | ✅ | 2026-07-02 |
| TASK-014 | Write guides section with G2: "Revert and Remove a Migration" (steps: rollback, remove migration file) | ✅ | 2026-07-02 |
| TASK-015 | Write guides section with G3: "Apply Migrations at Startup" (steps: configure, run, verify) | ✅ | 2026-07-02 |
| TASK-016 | Write anti_patterns section for common migration pitfalls | ✅ | 2026-07-02 |
| TASK-017 | Write file_structure section | ✅ | 2026-07-02 |

### Implementation Phase 3: Validation

- GOAL-003: Validate both YAML files against the 20 HARD RULES and validation checklist.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Run validation checklist on README.yaml | ✅ | 2026-07-02 |
| TASK-019 | Run validation checklist on GUIDE.yaml | ✅ | 2026-07-02 |
| TASK-020 | Verify all abstractions have correct path values | ✅ | 2026-07-02 |
| TASK-021 | Verify YAML is well-formed (no duplicate keys, valid indentation) | ✅ | 2026-07-02 |

## 3. Alternatives

- **ALT-001**: Single README.md (markdown) file — rejected because it doesn't follow the standard yaml-docs format required for machine-parseable documentation.
- **ALT-002**: Embed migration instructions inside README.yaml's guides section — rejected because the user explicitly requested a separate GUIDE.yaml file.
- **ALT-003**: Use RepositoryModule kind instead of BuildingBlockModule — rejected because the Migrations project is a utility tooling module, not a data access/persistence layer; the template and standard for BuildingBlockModule fits better.

## 4. Dependencies

- **DEP-001**: guide/yaml-docs/ — Shared YAML Documentation Standard v3.0 (SKILL.md, element-reference.md, naming-conventions.md, validation-checklist.md, templates/module.yaml)
- **DEP-002**: service/Api/src/Migrations/Api.Migrations.csproj — target project structure
- **DEP-003**: service/Api/src/Migrations/DesignTimeDbContextFactory.cs — design-time factory to document
- **DEP-004**: service/Api/src/Shared/Operational/Persistence/ — shared persistence layer context

## 5. Files

- **FILE-001**: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Migrations/README.yaml` — New YAML documentation for the Migrations module
- **FILE-002**: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Migrations/GUIDE.yaml` — New YAML guide for creating EF Core migrations
- **FILE-003**: `/home/qingfa/Repos/ReSys.Shop/plan/feature-ef-migration-yaml-docs-1.md` — This implementation plan

## 6. Testing

- **TEST-001**: Run YAML linter: `python3 -c "import yaml; yaml.safe_load(open('service/Api/src/Migrations/README.yaml'))"`
- **TEST-002**: Run YAML linter: `python3 -c "import yaml; yaml.safe_load(open('service/Api/src/Migrations/GUIDE.yaml'))"`
- **TEST-003**: Verify all abstraction paths reference existing files in the Migrations directory
- **TEST-004**: Verify all ID prefixes follow naming-conventions.md (P, F, A, B, AP, S, TS, M, G)

## 7. Risks & Assumptions

- **RISK-001**: YAML parsing may fail if HARD RULES about quoting strings with `:` or `#` are violated — mitigated by following element-reference.md strictly.
- **RISK-002**: Migration file names (timestamps) will change as new migrations are added — acceptable; GUIDE.yaml instructs how to name them generically.
- **ASSUMPTION-001**: The user has PyYAML installed for validation (python3 -c "import yaml").
- **ASSUMPTION-002**: The Migrations project structure (DesignTimeDbContextFactory, Migrations/ subfolder) will remain stable.
- **ASSUMPTION-003**: The `dotnet ef` CLI tool is installed and available for developers following the guide.

## 8. Related Specifications / Further Reading

- [guide/yaml-docs/SKILL.md](file:///home/qingfa/Repos/ReSys.Shop/guide/yaml-docs/SKILL.md)
- [guide/yaml-docs/referneces/element-reference.md](file:///home/qingfa/Repos/ReSys.Shop/guide/yaml-docs/referneces/element-reference.md)
- [guide/yaml-docs/referneces/naming-conventions.md](file:///home/qingfa/Repos/ReSys.Shop/guide/yaml-docs/referneces/naming-conventions.md)
- [guide/yaml-docs/referneces/validation-checklist.md](file:///home/qingfa/Repos/ReSys.Shop/guide/yaml-docs/referneces/validation-checklist.md)
- [guide/yaml-docs/templates/module.yaml](file:///home/qingfa/Repos/ReSys.Shop/guide/yaml-docs/templates/module.yaml)
