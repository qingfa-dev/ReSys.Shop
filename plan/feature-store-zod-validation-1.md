---
goal: Add user-friendly Zod validation messages and correct error-display plumbing (inline field Messages vs toasts) across the Store SPA.
version: 1.0
date_created: 2026-08-12
last_updated: 2026-08-12
owner: Store SPA team
status: 'Planned'
tags: [feature, validation, zod, vee-validate, store]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Store SPA validates user input with Zod schemas bridged into vee-validate via
`toFormValidator`. Today every form-facing schema relies on Zod's default English
messages (`Required`, `Too small`), the interceptor drops backend `metadata` so
API validation errors can never map back to a field, stores collapse the backend
`errors[]` array into a single string, and error display is inconsistent: some
forms toast, some show one generic inline Message, and 5xx can double-toast.

This plan makes validation messages friendly and deterministic, then routes each
error class to the correct surface — Zod/field errors inline under the input via
PrimeVue `Message`, non-field API errors via toast, and auth-form API errors via
the existing inline `apiError` Message.

## 1. Requirements & Constraints

- **REQ-001**: Every user-facing form schema must use friendly, human-readable Zod messages instead of Zod defaults.
- **REQ-002**: Zod field validation errors must render inline directly under the owning input using PrimeVue `<Message severity="error" size="small" variant="simple">`.
- **REQ-003**: Backend field-level validation errors (422 with `metadata.propertyName`) must map back to the matching form field and render inline under that input.
- **REQ-004**: Non-field API errors must render as a toast (4xx → warn, 5xx → error) with no duplicate toast from the global 5xx interceptor.
- **REQ-005**: Auth forms (login, register, forgot, reset, change-password) must keep the existing inline `apiError` Message for generic API failures.
- **REQ-006**: Manual-validation forms (Profile, AddressBook) must show per-field inline messages for Zod failures instead of a single aggregate toast.
- **SEC-001**: Never surface raw stack traces, internal error codes, or server internals to end users; always map to friendly messages.
- **SEC-002**: Backend `metadata.propertyName` values are read only for field mapping; never interpolated into HTML — Vue text interpolation only.
- **CON-001**: vee-validate + `toFormValidator` remains the form bridge. Do not migrate forms to `@primevue/forms`.
- **CON-002**: Comments follow the Store AGENTS.md standard (`// Label: Sentence.` in script; `<!-- Section: Title — purpose -->` in template).
- **CON-003**: Warnings-as-errors applies; `pnpm run build-only` and `pnpm run lint` must pass with zero warnings.
- **CON-004**: PrimeVue components are auto-imported via `PrimeVueResolver`; only `Label` is imported explicitly. The new `FieldMessage.vue` needs no explicit import in consumers.
- **GUD-001**: Keep all friendly message strings in one shared catalog (`shared/validations/messages.ts`) so wording is consistent and greppable.
- **GUD-002**: Field errors render inline; only non-field or global API errors toast. Do not toast Zod field errors.
- **GUD-003**: Reuse the existing `useApiErrorHandler` for toasts; do not introduce new toast plumbing.
- **PAT-001**: Keep the existing `defineField` → `:invalid="!!errors.x"` → sibling Message pattern, centralized via a `FieldMessage` component.
- **PAT-002**: Stores expose `errors: ApiError[]` alongside the `error: string`; views map field errors with `setFieldError(field, message)`.

## 2. Implementation Steps

### Implementation Phase 1: Zod friendly message catalog

- GOAL-001: Establish a shared message catalog and apply friendly messages to all form-facing schemas.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Store/src/shared/validations/messages.ts` exporting `zodMessages` (catalog): `required(field)`, `email`, `minLength(field,n)`, `maxLength(field,n)`, `usernamePattern`, `passwordRules`, `passwordsMatch`, `acceptTerms`, `mustBePositive`. Each returns a plain string; keep lines under 100 chars. | |  |
| TASK-002 | Update `app/Store/src/features/identity/validations/auth.ts`: apply `zodMessages` to `LoginFormSchema`, `RegisterFormSchema`, `ForgotPasswordSchema`, `ResetPasswordSchema`, `ChangePasswordSchema`. Add shared `ResetPasswordFormSchema` and `ChangePasswordFormSchema` (base schema + `confirmPassword` + refine `passwordsMatch`) so the two views stop duplicating local schemas. | |  |
| TASK-003 | Update `app/Store/src/features/profile/validations/profile.ts` `UpdateProfileRequestSchema` and `app/Store/src/features/profile/validations/address.ts` `AddressInputSchema` with `zodMessages` on all rules (used by ProfileView / AddressBookView manual `safeParse`). | |  |

### Implementation Phase 2: Reusable field error component

- GOAL-002: Extract the inline field-error markup into one reusable `FieldMessage` component and migrate the vee-validate auth forms.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `app/Store/src/shared/components/FieldMessage.vue`. Props: `error?: string | null`. Template: `<Message v-if="error" severity="error" size="small" variant="simple">{{ error }}</Message>`. No explicit imports needed (Message auto-imported). Include `<!-- Section: Field Error — inline validation or API field message -->`. | |  |
| TASK-005 | Migrate `app/Store/src/features/identity/views/LoginView.vue`: replace the two `<Message v-if="errors.x">` blocks with `<FieldMessage :error="errors.credential" />` and `<FieldMessage :error="errors.password" />`. Keep the `apiError` Message as-is. | |  |
| TASK-006 | Migrate `app/Store/src/features/identity/views/RegisterView.vue`: replace per-field Messages with `<FieldMessage :error="errors.firstName" />`, `errors.lastName`, `errors.email`, `errors.password`, `errors.confirmPassword`, and the terms `useField` error. Remove the local `ResetPasswordFormSchema`-style duplication (none here); use shared `RegisterFormSchema`. | |  |
| TASK-007 | Migrate `app/Store/src/features/identity/views/ForgotPasswordView.vue`, `app/Store/src/features/identity/views/ResetPasswordView.vue`, and `app/Store/src/features/profile/views/ChangePasswordView.vue` to `<FieldMessage>`. In ResetPasswordView and ChangePasswordView delete the local `ResetPasswordFormSchema`/`ChangePasswordFormSchema` definitions and import the new shared ones from `validations/auth.ts`. | |  |

### Implementation Phase 3: API error field mapping

- GOAL-003: Propagate backend field context end-to-end so 422 validation errors map back to inputs, and surface generic API errors once.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Update `app/Store/src/shared/api/interceptors/error.ts` `extractErrors`: read `metadata` from each backend error; set `field` from `metadata.propertyName` or `metadata.Field` (string, snake/pascal tolerant). Extend the mapped object to include `field`. | |  |
| TASK-009 | Update `app/Store/src/shared/composables/useApiErrorHandler.ts`: add `applyFieldErrors(errors: ApiError[], setFieldError: (field: string, message: string) => void): ApiError[]` that maps `{field,message}` into `setFieldError` and returns the remaining (field-less) errors. Add a guard in `handleError` to skip the toast for `HttpError` with `statusCode >= 500` (the interceptor already toasted it). | |  |
| TASK-010 | Update `app/Store/src/features/identity/stores/authStore.ts`: add `const errors = ref<ApiError[]>([])`; on failure assign `errors.value = result.errors ?? []` in `login`, `register`, `changePassword`, `forgotPassword`, `resetPassword`. Return `errors` from the store. Keep `error` string behavior unchanged. | |  |

### Implementation Phase 4: Correct error surfaces in auth forms

- GOAL-004: Wire the auth forms so API field errors appear inline under the owning input and generic API errors use the inline `apiError` Message.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Update `app/Store/src/features/identity/views/LoginView.vue`: on failed `auth.login`, call `applyFieldErrors(auth.errors, (f, m) => setFieldError(f, m))`; if the returned list is non-empty join their messages into `apiError`, else keep `auth.error`. Requires adding `setFieldError` from `useForm`. | |  |
| TASK-012 | Update `app/Store/src/features/identity/views/RegisterView.vue` identically for `auth.register` (fields: `firstName`, `lastName`, `email`, `password`). | |  |
| TASK-013 | Update `ForgotPasswordView.vue` (field `email`), `ResetPasswordView.vue` (fields `newPassword`), `ChangePasswordView.vue` (fields `currentPassword`, `newPassword`) with the same `applyFieldErrors` + `setFieldError` pattern. | |  |

### Implementation Phase 5: Manual-validation forms

- GOAL-005: Give Profile and AddressBook per-field inline Zod messages instead of a single aggregate toast/message.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Update `app/Store/src/features/profile/views/ProfileView.vue`: replace the single `notify.error('Enter both first and last name')` with `UpdateProfileRequestSchema.safeParse` → map `error.issues` into per-field `ref<string|null>` state rendered as `<FieldMessage>` under each input; keep `notify.error(profileStore.error)` for API failures. | |  |
| TASK-015 | Update `app/Store/src/features/profile/views/AddressBookView.vue`: replace `formError` single Message with per-field refs from `AddressInputSchema.safeParse` rendered as `<FieldMessage>`; keep the API-failure Message/toast for non-field errors, and map `addressStore.errors` via `applyFieldErrors` when present. | |  |

### Implementation Phase 6: Tests and verification

- GOAL-006: Prove friendly messages and the correct surfaces with automated tests, then run the full verification suite.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Update `app/Store/src/features/identity/validations/__tests__/zodResolver.spec.ts`: assert friendly messages (e.g. login empty credential → "Email or username is required", register short password → min-length message) and update any register payload to the current `firstName`/`lastName` shape. | |  |
| TASK-017 | Create `app/Store/src/shared/components/__tests__/FieldMessage.spec.ts`: renders Message when `error` is a string, renders nothing when null/empty. | |  |
| TASK-018 | Update `app/Store/src/shared/api/interceptors/__tests__/error.spec.ts`: add a case asserting `metadata.propertyName` populates `ApiError.field`. | |  |
| TASK-019 | Update `app/Store/src/features/identity/stores/__tests__/authStore.spec.ts`: assert `errors` ref is populated on register/login failure. | |  |
| TASK-020 | Update view specs (`LoginView.spec.ts`, `RegisterView.spec.ts`, `ProfileView.spec.ts`, `AddressBookView.spec.ts`, `ChangePasswordView.spec.ts`) for the new friendly messages and inline surfaces. | |  |
| TASK-021 | Run `pnpm run build-only`, `pnpm run lint`, and `npx vitest run identity profile` in `app/Store`. All must pass (only the three documented pre-existing catalog failures may remain). | |  |

## 3. Alternatives

- **ALT-001**: Global `z.setErrorMap(...)` override — rejected: it silently changes API-response schemas too (they must keep exact contract errors) and is implicit; per-schema `.message()` is explicit, greppable, and testable.
- **ALT-002**: Migrate forms to `@primevue/forms` + `zodResolver` — rejected: vee-validate owns all five production forms; `@primevue/forms` has zero component usage and only a regression test.
- **ALT-003**: Server-driven field errors only (drop client-side Zod messages) — rejected: instant client feedback is still required and Zod messages are the first line of defense.
- **ALT-004**: Toast every error, including field errors — rejected: field errors belong under the input (REQ-002); toasting them is noisy and hides which field failed.

## 4. Dependencies

- **DEP-001**: vee-validate + `@vee-validate/zod` (existing) — `toFormValidator` bridge.
- **DEP-002**: zod (existing, v4) — schema messages and `safeParse` issues.
- **DEP-003**: PrimeVue `Message` (auto-imported) — inline field error rendering.
- **DEP-004**: `shared/api` client + interceptor + `HttpError` — API error envelope to `ApiError.field` mapping.
- **DEP-005**: `useApiErrorHandler` (existing) — `applyFieldErrors` + guarded toast.
- **DEP-006**: Backend `Result` envelope — 422 validation failures carry `metadata.propertyName` (see `service/Api/src/Shared/Application/Mappings/ValidationResultMapper.cs`).

## 5. Files

- **FILE-001**: `app/Store/src/shared/validations/messages.ts` (new) — `zodMessages` catalog.
- **FILE-002**: `app/Store/src/shared/components/FieldMessage.vue` (new) — inline field error Message.
- **FILE-003**: `app/Store/src/features/identity/validations/auth.ts` — friendly messages + shared reset/change form schemas.
- **FILE-004**: `app/Store/src/features/profile/validations/profile.ts` — friendly messages.
- **FILE-005**: `app/Store/src/features/profile/validations/address.ts` — friendly messages.
- **FILE-006**: `app/Store/src/shared/api/interceptors/error.ts` — `metadata` → `ApiError.field`.
- **FILE-007**: `app/Store/src/shared/composables/useApiErrorHandler.ts` — `applyFieldErrors` + 5xx toast guard.
- **FILE-008**: `app/Store/src/features/identity/stores/authStore.ts` — `errors` ref.
- **FILE-009**: `app/Store/src/features/identity/views/LoginView.vue` — FieldMessage + API field mapping.
- **FILE-010**: `app/Store/src/features/identity/views/RegisterView.vue` — FieldMessage + API field mapping.
- **FILE-011**: `app/Store/src/features/identity/views/ForgotPasswordView.vue` — FieldMessage + API field mapping.
- **FILE-012**: `app/Store/src/features/identity/views/ResetPasswordView.vue` — FieldMessage + shared schema.
- **FILE-013**: `app/Store/src/features/profile/views/ChangePasswordView.vue` — FieldMessage + shared schema.
- **FILE-014**: `app/Store/src/features/profile/views/ProfileView.vue` — per-field Zod messages.
- **FILE-015**: `app/Store/src/features/profile/views/AddressBookView.vue` — per-field Zod/API messages.
- **FILE-016**: `app/Store/src/shared/components/__tests__/FieldMessage.spec.ts` (new).
- **FILE-017**: `app/Store/src/features/identity/validations/__tests__/zodResolver.spec.ts`.
- **FILE-018**: `app/Store/src/shared/api/interceptors/__tests__/error.spec.ts`.
- **FILE-019**: `app/Store/src/features/identity/stores/__tests__/authStore.spec.ts`.
- **FILE-020**: `app/Store/src/features/identity/views/__tests__/LoginView.spec.ts`, `RegisterView.spec.ts`.
- **FILE-021**: `app/Store/src/features/profile/views/__tests__/ProfileView.spec.ts`, `AddressBookView.spec.ts`, `ChangePasswordView.spec.ts`.

## 6. Testing

- **TEST-001**: `zodResolver.spec.ts` — friendly login/register messages, register payload shape.
- **TEST-002**: `FieldMessage.spec.ts` — renders message text; renders nothing when `error` is null/empty.
- **TEST-003**: `error.spec.ts` — `metadata.propertyName` → `ApiError.field`; `metadata.Field` fallback.
- **TEST-004**: `authStore.spec.ts` — `errors` populated on login/register failure, empty on success.
- **TEST-005**: `LoginView.spec.ts` / `RegisterView.spec.ts` — API field errors surface under inputs via `setFieldError`; generic errors keep inline `apiError`.
- **TEST-006**: `ProfileView.spec.ts` / `AddressBookView.spec.ts` — per-field inline messages on Zod failure.
- **TEST-007**: Manual regression — `pnpm run build-only`, `pnpm run lint`, `npx vitest run identity profile`.

## 7. Risks & Assumptions

- **RISK-001**: Backend `metadata` key casing may vary (`propertyName` vs `Field`). Interceptor reads both keys (TASK-008).
- **RISK-002**: Existing view tests assert old default messages / message counts; they must be updated in the same phase as the schema change (TASK-020).
- **RISK-003**: 5xx double-toast regression. Mitigated by the `handleError` guard for `statusCode >= 500` (TASK-009) and the existing interceptor 5xx toast.
- **RISK-004**: `FieldMessage` moves Message into a child component; tests that query `[data-pc-name="message"]` still match since `Message` renders unchanged.
- **RISK-005**: The two pre-existing flaky submit tests (`LoginView.spec.ts`, `RegisterView.spec.ts` "valid submit") and three catalog failures are unrelated; do not attempt to fix them in this plan.
- **ASSUMPTION-001**: Backend 422 validation failures include `metadata.propertyName` (per `ValidationResultMapper`).
- **ASSUMPTION-002**: vee-validate remains the form bridge; `@primevue/forms` is not adopted.
- **ASSUMPTION-003**: Auth forms prefer inline errors; non-auth action flows prefer toasts — the split defined in REQ-004/REQ-005.

## 8. Related Specifications / Further Reading

- [Store SPA AGENTS.md — comment standard](app/Store/AGENTS.md)
- [Zod v4 error messages](https://zod.dev/ERROR_HANDLING)
- [vee-validate Zod integration (@vee-validate/zod)](https://vee-validate.logaretm.com/v4/integrations/zod-schema-validation/)
- [PrimeVue Message component](https://primevue.org/message/)
- [Backend Result envelope — Error metadata](service/Api/src/Shared/Application/Mappings/ValidationResultMapper.cs)
