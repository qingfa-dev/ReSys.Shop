---
goal: Implement admin catalog Taxon Rules frontend API service
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, catalog, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement frontend API service layer for Taxon Rules (child entity under Taxons) in `app/Admin/src/features/catalog/`. 5 backend endpoints: CRUD + Sync. Follows child-entity patterns: types -> schemas -> mappers -> API -> inline component.

## 1. Requirements & Constraints

- **REQ-001**: All 5 backend endpoints must have corresponding frontend API methods
- **REQ-002**: API methods take taxonomyId + taxonId as parent ID parameters
- **REQ-003**: Zod validation for type, matchPolicy, value fields
- **REQ-004**: Inline component inside TaxonForm (no stand-alone pages)
- **REQ-005**: Sync endpoint accepts full replacement list (send all rules, server reconciles)
- **CON-001**: Follow existing OptionValue/Taxon child entity patterns exactly
- **CON-002**: Zero TypeScript errors
- **PAT-001**: Static API class wrapping apiClient
- **PAT-002**: Standalone form component with props parentId/entity + emits saved/cancelled

## 2. Implementation Steps

### Phase 1: Taxon Rules core

- GOAL-001: Implement complete Taxon Rules feature: types, schemas, mappers, API, inline component

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/taxon-rule.response.ts` — `TaxonRuleDetailResponse`: id, taxonId, type, matchPolicy, value; `TaxonRuleListItem`: id, type, matchPolicy, value; `SyncTaxonRulesResponse`: rules: TaxonRuleListItem[] | | |
| TASK-002 | Create `types/taxon-rule.request.ts` — `TaxonRuleRequest`: type: string, matchPolicy: string, value: string; `SyncRuleItem`: id?: string, type, matchPolicy, value; `SyncTaxonRulesRequest`: rules: SyncRuleItem[] | | |
| TASK-003 | Create `schemas/taxon-rule.fields.ts` — `TaxonRuleFields`: type (required string), matchPolicy (required string, one of All/Any), value (required string) | | |
| TASK-004 | Create `schemas/taxon-rule.forms.ts` — `TaxonRuleForms` with create()/update() schemas; export `TaxonRuleForm` | | |
| TASK-005 | Create `mappers/taxon-rule.mapper.ts` — `TaxonRuleFormMapper` with toCreate, toUpdate | | |
| TASK-006 | Create `api/taxon-rule.api.ts` — `TaxonRuleApi`: getMany(taxonomyId, taxonId) GET `/catalog/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`; create POST same; update(taxonomyId, taxonId, ruleId, data) PUT `.../rules/${ruleId}`; delete(taxonomyId, taxonId, ruleId) DELETE same; sync(taxonomyId, taxonId, data) POST `.../rules/sync` | | |
| TASK-007 | Create `components/TaxonRuleManager.vue` — inline component used in TaxonForm; DataTable of rules + Add button; slideover form for create/edit; fields: type (dropdown), matchPolicy (radio All/Any), value (text); sync on save | | |
| TASK-008 | Update `composables/useTaxonomy.ts` — add `ruleApi: TaxonRuleApi` to returned object | | |
| TASK-009 | Integrate TaxonRuleManager into TaxonForm.vue as a section | | |
| TASK-010 | Update all barrel exports: `types/index.ts`, `schemas/index.ts`, `mappers/index.ts`, `api/index.ts` | | |
| TASK-011 | Verify: `pnpm build` passes | | |

## 3. Alternatives

- **ALT-001**: Standalone Taxon Rules page — rejected: rules are always scoped to a taxon, no independent list page

## 4. Dependencies

- **DEP-001**: Existing `TaxonForm.vue` — integration target
- **DEP-002**: Backend route pattern: `/catalog/taxonomies/{taxonomyId}/taxons/{taxonId}/rules`

## 5. Files

- **FILE-001**: `types/taxon-rule.response.ts`, `types/taxon-rule.request.ts`
- **FILE-002**: `schemas/taxon-rule.fields.ts`, `schemas/taxon-rule.forms.ts`
- **FILE-003**: `mappers/taxon-rule.mapper.ts`
- **FILE-004**: `api/taxon-rule.api.ts`
- **FILE-005**: `components/TaxonRuleManager.vue`
- **FILE-006**: `composables/useTaxonomy.ts` (updated)
- **FILE-007**: `components/TaxonForm.vue` (updated)
- **FILE-008**: Barrel files (updated)

## 6. Testing

- **TEST-001**: `api/__tests__/taxon-rules.spec.ts` — mock apiClient, verify all 5 methods call correct URL/method/body

## 7. Risks & Assumptions

- **ASSUMPTION-001**: Backend `GetTaxonRules` returns `PagedResult<TaxonRuleDetailResponse>` — if returns plain `Result<List<...>>`, change API return type

## 8. Related Specifications / Further Reading

Backend: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/`
