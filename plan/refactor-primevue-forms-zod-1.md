---
goal: Replace Custom Form Components with @primevue/forms Directly
version: 2.0
date_created: 2026-07-31
last_updated: 2026-07-31
owner: Admin SPA team
status: Completed
tags: [refactor, forms, primevue, zod]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-bright_green)

Delete both custom `FormField.vue` and `FormSection.vue`. Replace all form
sections in 6 detail views with direct `@primevue/forms` `<Form>` + inline
Card + inline field labels. Use `zodResolver` from `@primevue/forms/resolvers/zod`
for declarative validation. Each view renders its own `<Card>` → `<Form>` structure
with the section title as a header div, then fields with `name` prop on
PrimeVue components.

## 1. Requirements & Constraints

- **REQ-001**: Delete `FormField.vue` and `FormSection.vue`
- **REQ-002**: Remove both from barrel `src/shared/components/form/index.ts`
- **REQ-003**: Delete `src/shared/components/form/` directory (or leave empty barrel for future)
- **REQ-004**: All 6 detail views render inline `<Card>` + `<Form>` + labeled fields directly
- **REQ-005**: Every field uses Zod resolver via `zodResolver(schema)` passed to `<Form :resolver>`
- **REQ-006**: Error display uses `$form.{name}?.errors?.[0]?.message`
- **REQ-007**: Remove all `fieldErrors` refs and manual `safeParse` from view scripts
- **REQ-008**: On submit, `FormSubmitEvent` provides `values: { valid: boolean, states, errors }`
- **CON-001**: 570 existing tests must pass
- **CON-002**: Build must pass (`pnpm run build-only` in app/Admin)
- **CON-003**: Lint must pass with 0 new violations
- **GUD-001**: Field pattern: `<div class="flex flex-col gap-1"><label class="font-medium">Label <span v-if class="text-red-500">*</span></label><InputText name="field" fluid /><small v-if="$form.field?.invalid" class="text-red-500">{{ $form.field?.errors?.[0]?.message }}</small></div>`
- **PAT-001**: `<Form v-slot="$form" :resolver :initialValues class="flex flex-col gap-4" @submit="onSubmit">`

## 2. Implementation Steps

### Implementation Phase 1: Delete custom components & barrel

- GOAL-001: Remove FormField.vue, FormSection.vue, and their barrel exports

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `app/Admin/src/shared/components/form/FormField.vue` | | |
| TASK-002 | Delete `app/Admin/src/shared/components/form/FormSection.vue` | | |
| TASK-003 | Update `app/Admin/src/shared/components/form/index.ts` — remove both FormField and FormSection exports (keep empty barrel or remove entirely) | | |

### Implementation Phase 2: Refactor all 6 detail views

- GOAL-002: Replace custom FormSection/FormField with inline Card + Form + labeled fields + zodResolver

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Refactor `CountryDetail.vue`: remove `import { FormSection, FormField } from '@form'`, add `import { Form } from '@primevue/forms'` + `import { zodResolver } from '@primevue/forms/resolvers/zod'`. Replace `<FormSection title="Country Details">...<FormField...></FormSection>` with inline `<Card>` + `<Form :resolver="countryResolver" :initialValues="form" v-slot="$form" class="flex flex-col gap-4" @submit="onSubmit">` + labeled fields. Remove `fieldErrors` ref. Update `onSave` to use `FormSubmitEvent`. | | |
| TASK-005 | Refactor `StateDetail.vue`: same pattern. Replace FormSection/FormField with inline Card + Form + labeled fields. Use `stateSchema` as resolver. | | |
| TASK-006 | Refactor `ProductDetail.vue`: 6 tabs. Replace inner FormSections with Card + Form per tab. Replace each FormField with labeled div. ~17 fields. Use `productSchema` as resolver. | | |
| TASK-007 | Refactor `OptionTypeDetail.vue`: 2 tabs. Replace FormSection in tab 0 with Card + Form + labeled fields (4 fields). Use `optionTypeSchema` resolver. | | |
| TASK-008 | Refactor `TaxonomyDetail.vue`: flat layout, no tabs. Replace FormSection with Card + Form + labeled fields (3 fields). Use `taxonomySchema` resolver. | | |
| TASK-009 | Refactor `TaxonDetail.vue`: 5 tabs. Replace FormSections per tab with Card + Form + labeled fields (~17 fields). Use `taxonSchema` resolver. | | |

### Implementation Phase 3: Verify & cleanup

- GOAL-003: Build, test, lint verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Run `pnpm run build-only` (0 errors) | | |
| TASK-011 | Run `pnpm run test:unit -- run` (570 pass) | | |
| TASK-012 | Run `pnpm run lint` (0 new violations) | | |
| TASK-013 | Grep for remaining `FormField` and `FormSection` references in app/Admin/src — must be zero | | |

## 3. Alternatives

- **ALT-001**: Keep FormSection as a thin wrapper around Card+Form — adds an abstraction layer vs. direct composition; user prefers direct usage
- **ALT-002**: Keep FormField for label consistency — we inline labels in each view instead for maximum flexibility

## 4. Dependencies

- **DEP-001**: `@primevue/forms` ^5.0.0 — installed
- **DEP-002**: `@primevue/forms/resolvers/zod` — available via `zodResolver` export
- **DEP-003**: `zod` — all schemas already defined in `validations/` per module

## 5. Files

### Deleted
- **FILE-001**: `app/Admin/src/shared/components/form/FormField.vue`
- **FILE-002**: `app/Admin/src/shared/components/form/FormSection.vue`
- **FILE-003**: `app/Admin/src/shared/components/form/index.ts` (emptied)

### Modified
- **FILE-004**: `app/Admin/src/features/location/views/CountryDetail.vue`
- **FILE-005**: `app/Admin/src/features/location/views/StateDetail.vue`
- **FILE-006**: `app/Admin/src/features/catalog/views/ProductDetail.vue`
- **FILE-007**: `app/Admin/src/features/catalog/views/OptionTypeDetail.vue`
- **FILE-008**: `app/Admin/src/features/catalog/views/TaxonomyDetail.vue`
- **FILE-009**: `app/Admin/src/features/catalog/views/TaxonDetail.vue`

## 6. Testing

- **TEST-001**: All 570 existing unit tests pass
- **TEST-002**: Build with 0 errors
- **TEST-003**: Zero remaining references to FormField/FormSection in source
- **TEST-004**: Manual smoke test — CountryDetail form creates/edits with Zod validation

## 7. Risks & Assumptions

- **RISK-001**: `zodResolver` may not handle all edge cases (nullable Date fields, optional strings) — verify with ProductDetail's complex schema first
- **ASSUMPTION-001**: `@primevue/forms` Form component supports `<Form v-slot="$form">` pattern with all PrimeVue input components via `name` prop — verified from docs
- **ASSUMPTION-002**: `FormSubmitEvent` provides structured `values` object matching form field names

## 8. Related Specifications / Further Reading

- [PrimeVue Forms Documentation](https://primevue.dev/forms/)
- [Zod Resolver Import](https://primevue.dev/forms/#resolvers)
