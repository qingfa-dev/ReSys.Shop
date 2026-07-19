---
goal: Apply Code Commenting Standard v3.0 to all 252 CQRS handler files across all 9 modules
version: 1.0
date_created: 2026-07-19
owner: Engineering Standards
status: 'Completed'
tags: feature, commenting, csharp, handlers, cqrs, standards
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Apply the structured Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to all 252 CQRS handler files in `service/Api/src/Module/`. Coverage audit reveals:

| Metric | Count | Coverage |
|--------|-------|----------|
| Handle() XML doc (`/// <summary>`) | 252/252 | **100%** ✅ |
| Class-level XML doc (`/// <summary>`) | 173/252 | **69%** — 79 missing |
| Inline structured labels (`// Label:`) | 187/252 | **74%** — 65 missing |
| `/// <inheritdoc />` instead of explicit `<summary>` | 4/252 | upgrade to explicit |

Target: 100% on all three dimensions across all modules. Each comment must explain WHY, never WHAT; use imperative verbs; follow the Semantic Density Principle (arXiv:2604.07502).

## 1. Requirements & Constraints

- **REQ-001**: All 252 handler classes must have `/// <summary>` on the handler class declaration
- **REQ-002**: All 252 Handle() methods already have `/// <summary>` — verify none use `/// <inheritdoc />` (upgrade 4 files)
- **REQ-003**: Add structured inline labels (CAT-1 through CAT-9) in Handle() method bodies where currently absent
- **REQ-004**: CAT-10 agent annotations must use `KEY=VALUE` form for machine parsing
- **REQ-005**: `dotnet build` must pass with TreatWarningsAsErrors=true after all changes
- **REQ-006**: Max line length 100 characters per F3 rule
- **REQ-007**: One label, one action — never join two actions with "and" (F8)
- **REQ-008**: Comments on their own line — never trailing a code statement (F1 exception for inline data literals)
- **CON-001**: Do NOT modify test files in `service/Api/tests/`
- **CON-002**: Do NOT modify `.Endpoint.cs`, `.Response.cs`, `.Request.cs`, `.Validator.cs` files — handlers only
- **PAT-001**: Follow existing inline label patterns from well-commented modules (Location=100%, Shipping=100%, Payment=100%)
- **PAT-002**: Use `// Validate:` for input guards, `// Load:` for data fetching, `// Call:` for external service calls, `// Cache:` for cache operations, `// Transform:` for Mapster mapping, `// Log:` for logging, `// Catch:` for exception handling
- **GUD-001**: Use `/// <summary>Brief description of handler purpose.</summary>` format for class XML doc
- **GUD-002**: Imperative verbs in inline labels: "Load product by ID" not "Product loading by ID"

## 2. Implementation Steps

### Implementation Phase 1: Identity Module — Add Inline Labels (27 files)

- GOAL-001: Add structured inline labels (CAT-1 through CAT-9) to 27 Identity handler files that are missing them. Add class-level `/// <summary>` to 19 Identity handler classes missing it.

| Task | File | ClassDoc | InlineLabels |
|------|------|----------|--------------|
| TASK-001 | `Features/Admin/Permissions/Get/GetPermissions.cs` | Add class doc | Add inline labels |
| TASK-002 | `Features/Admin/Roles/Create/CreateRole.cs` | Add class doc | Add inline labels |
| TASK-003 | `Features/Admin/Roles/Delete/DeleteRole.cs` | — | Verify existing inline labels |
| TASK-004 | `Features/Admin/Roles/Get/ById/GetRoleById.cs` | Add class doc | Add inline labels |
| TASK-005 | `Features/Admin/Roles/Get/PagedOrAll/GetRolesPagedOrAll.cs` | Add class doc | Add inline labels |
| TASK-006 | `Features/Admin/Roles/Update/UpdateRole.cs` | — | Add inline labels |
| TASK-007 | `Features/Admin/Users/Create/CreateUser.cs` | — | Add inline labels |
| TASK-008 | `Features/Admin/Users/Delete/DeleteUser.cs` | — | Add inline labels |
| TASK-009 | `Features/Admin/Users/GetById/GetUserById.cs` | Add class doc | Add inline labels |
| TASK-010 | `Features/Admin/Users/GetPagedOrAll/GetUsersPagedOrAll.cs` | Add class doc | Add inline labels |
| TASK-011 | `Features/Admin/Users/Permissions/Assign/AssignUserPermissions.cs` | — | Add inline labels |
| TASK-012 | `Features/Admin/Users/Permissions/Revoke/RevokeUserPermissions.cs` | — | Add inline labels |
| TASK-013 | `Features/Admin/Users/Permissions/Sync/SyncUserPermissions.cs` | — | Add inline labels |
| TASK-014 | `Features/Admin/Users/Roles/Assign/AssignUserRoles.cs` | — | Add inline labels |
| TASK-015 | `Features/Admin/Users/Roles/Get/GetUserRoles.cs` | Add class doc | Add inline labels |
| TASK-016 | `Features/Admin/Users/Roles/Revoke/RevokeUserRoles.cs` | — | Add inline labels |
| TASK-017 | `Features/Admin/Users/Roles/Sync/SyncUserRoles.cs` | — | Add inline labels |
| TASK-018 | `Features/Admin/Users/Status/ToggleUserStatus.cs` | — | Add inline labels |
| TASK-019 | `Features/Admin/Users/Update/UpdateUser.cs` | — | Add inline labels |
| TASK-020 | `Features/Store/Auth/Logout/Logout.cs` | Add class doc | Add inline labels |
| TASK-021 | `Features/Store/Auth/Register/EmailRegister.cs` | Add class doc | Add inline labels |
| TASK-022 | `Features/Store/Emails/Change/ChangeEmail.cs` | Add class doc | Add inline labels |
| TASK-023 | `Features/Store/Emails/Confirm/ConfirmEmail.cs` | Add class doc | Add inline labels |
| TASK-024 | `Features/Store/Emails/Resend/ResendEmailVerification.cs` | Add class doc | Add inline labels |
| TASK-025 | `Features/Store/Passwords/Change/ChangePassword.cs` | Add class doc | Add inline labels |
| TASK-026 | `Features/Store/Passwords/Forgot/RequestPasswordReset.cs` | Add class doc | Add inline labels |
| TASK-027 | `Features/Store/Passwords/Reset/ResetPassword.cs` | Add class doc | Add inline labels |

### Implementation Phase 2: Catalog Module — Add Class Doc + Inline Labels (50 + 11 gaps)

- GOAL-002: Add class-level `/// <summary>` to 50 Catalog handler classes missing it. Add inline labels to 11 Catalog handlers missing them.

| Task | File | Work |
|------|------|------|
| TASK-028 | `Features/Admin/OptionTypes/Create/CreateOptionType.cs` | Add class doc |
| TASK-029 | `Features/Admin/OptionTypes/Delete/DeleteOptionType.cs` | Add class doc |
| TASK-030 | `Features/Admin/OptionTypes/Get/ById/GetOptionTypeById.cs` | Add class doc |
| TASK-031 | `Features/Admin/OptionTypes/Get/Paged/GetOptionTypesPaged.cs` | Add class doc |
| TASK-032 | `Features/Admin/OptionTypes/OptionValues/Create/CreateOptionValue.cs` | Add class doc |
| TASK-033 | `Features/Admin/OptionTypes/OptionValues/Delete/DeleteOptionValue.cs` | Add class doc |
| TASK-034 | `Features/Admin/OptionTypes/OptionValues/Get/ById/GetOptionValueById.cs` | Add class doc |
| TASK-035 | `Features/Admin/OptionTypes/OptionValues/Get/Paged/GetOptionValuesPaged.cs` | Add class doc |
| TASK-036 | `Features/Admin/OptionTypes/OptionValues/Update/UpdateOptionValue.cs` | Add class doc |
| TASK-037 | `Features/Admin/OptionTypes/Update/UpdateOptionType.cs` | Add class doc |
| TASK-038 | `Features/Admin/Products/Classifications/Assign/AssignProductClassifications.cs` | Add class doc |
| TASK-039 | `Features/Admin/Products/Classifications/Get/GetProductClassifications.cs` | Add class doc |
| TASK-040 | `Features/Admin/Products/Classifications/Revoke/RevokeProductClassifications.cs` | Add class doc |
| TASK-041 | `Features/Admin/Products/Classifications/Sync/SyncProductClassifications.cs` | Add class doc |
| TASK-042 | `Features/Admin/Products/OptionTypes/Assign/AssignProductOptionTypes.cs` | Add class doc |
| TASK-043 | `Features/Admin/Products/OptionTypes/Get/GetProductOptionTypes.cs` | Add class doc |
| TASK-044 | `Features/Admin/Products/OptionTypes/Revoke/RevokeProductOptionTypes.cs` | Add class doc |
| TASK-045 | `Features/Admin/Products/OptionTypes/Sync/SyncProductOptionTypes.cs` | Add class doc |
| TASK-046 | `Features/Admin/Products/Variants/OptionValues/Assign/AssignVariantOptionValues.cs` | Add class doc |
| TASK-047 | `Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.cs` | Add class doc + inline labels |
| TASK-048 | `Features/Admin/Products/Variants/OptionValues/Revoke/RevokeVariantOptionValues.cs` | Add class doc |
| TASK-049 | `Features/Admin/Products/Variants/OptionValues/Sync/SyncVariantOptionValues.cs` | Add class doc |
| TASK-050 | `Features/Admin/Taxonomies/Create/CreateTaxonomy.cs` | Add class doc |
| TASK-051 | `Features/Admin/Taxonomies/Delete/DeleteTaxonomy.cs` | Add class doc |
| TASK-052 | `Features/Admin/Taxonomies/Get/ById/GetTaxonomyById.cs` | Add class doc |
| TASK-053 | `Features/Admin/Taxonomies/Get/Paged/GetTaxonomiesPaged.cs` | Add class doc |
| TASK-054 | `Features/Admin/Taxonomies/Restore/RestoreTaxonomy.cs` | Add class doc |
| TASK-055 | `Features/Admin/Taxonomies/Taxons/Create/CreateTaxon.cs` | Add class doc |
| TASK-056 | `Features/Admin/Taxonomies/Taxons/Delete/DeleteTaxon.cs` | Add class doc |
| TASK-057 | `Features/Admin/Taxonomies/Taxons/Get/ById/GetTaxonById.cs` | Add class doc + inline labels |
| TASK-058 | `Features/Admin/Taxonomies/Taxons/Get/Paged/GetTaxonsAllOrPaged.cs` | Add class doc + inline labels |
| TASK-059 | `Features/Admin/Taxonomies/Taxons/Get/Tree/GetTaxonTree.cs` | Add inline labels |
| TASK-060 | `Features/Admin/Taxonomies/Taxons/Reposition/RepositionTaxon.cs` | Add class doc |
| TASK-061 | `Features/Admin/Taxonomies/Taxons/Restore/RestoreTaxon.cs` | Add class doc |
| TASK-062 | `Features/Admin/Taxonomies/Taxons/Rules/Create/CreateTaxonRule.cs` | Add class doc |
| TASK-063 | `Features/Admin/Taxonomies/Taxons/Rules/Delete/DeleteTaxonRule.cs` | Add class doc |
| TASK-064 | `Features/Admin/Taxonomies/Taxons/Rules/Get/GetTaxonRules.cs` | Add class doc + inline labels |
| TASK-065 | `Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.cs` | Add class doc |
| TASK-066 | `Features/Admin/Taxonomies/Taxons/Rules/Update/UpdateTaxonRule.cs` | Add class doc |
| TASK-067 | `Features/Admin/Taxonomies/Taxons/Update/UpdateTaxon.cs` | Add class doc |
| TASK-068 | `Features/Admin/Taxonomies/Update/UpdateTaxonomy.cs` | Add class doc |
| TASK-069 | `Features/Storefront/Images/Get/Image/GetImage.cs` | Add class doc + inline labels |
| TASK-070 | `Features/Storefront/OptionTypes/Get/All/GetAllOptionTypes.cs` | Upgrade `/// <inheritdoc />` to explicit `/// <summary>`; add inline labels |
| TASK-071 | `Features/Storefront/Products/Get/List/ListProducts.cs` | Add class doc + inline labels |
| TASK-072 | `Features/Storefront/Products/Get/Related/GetRelatedProducts.cs` | Upgrade `/// <inheritdoc />` to explicit `/// <summary>` |
| TASK-073 | `Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs` | Add class doc |
| TASK-074 | `Features/Storefront/Products/SearchByImage/SearchByImage.cs` | Add class doc + inline labels |
| TASK-075 | `Features/Storefront/Taxons/Get/All/GetAllTaxons.cs` | Upgrade `/// <inheritdoc />` to explicit `/// <summary>`; add inline labels |
| TASK-076 | `Features/Storefront/Taxons/Get/Products/GetProducts.cs` | Add class doc |

### Implementation Phase 3: Profile Module — Add Inline Labels (16 files)

- GOAL-003: Add structured inline labels to 16 Profile handler files missing them. Add class-level `/// <summary>` to 4 Profile handler classes missing it.

| Task | File | Work |
|------|------|------|
| TASK-077 | `Features/Admin/Profiles/Get/PagedOrAll/GetUserProfilesPagedOrAll.cs` | Add class doc |
| TASK-078 | `Features/Store/Addresses/Create/CreateAddress.cs` | Add inline labels |
| TASK-079 | `Features/Store/Addresses/Delete/DeleteAddress.cs` | Add inline labels |
| TASK-080 | `Features/Store/Addresses/Get/ById/GetAddressById.cs` | Add inline labels |
| TASK-081 | `Features/Store/Addresses/Get/PagedOrAll/GetAddresses.cs` | Add inline labels |
| TASK-082 | `Features/Store/Addresses/Update/UpdateAddress.cs` | Add inline labels |
| TASK-083 | `Features/Store/NotificationPreferences/Get/GetNotificationPreferences.cs` | Add inline labels |
| TASK-084 | `Features/Store/NotificationPreferences/Update/UpdateNotificationPreferences.cs` | Add inline labels |
| TASK-085 | `Features/Store/Profiles/Create/CreateProfile.cs` | Add class doc |
| TASK-086 | `Features/Store/Profiles/Delete/DeleteProfile.cs` | Add inline labels |
| TASK-087 | `Features/Store/Profiles/Get/Detail/GetProfile.cs` | Add inline labels |
| TASK-088 | `Features/Store/Profiles/Update/UpdateProfile.cs` | Add inline labels |
| TASK-089 | `Features/Store/Wishlists/AddItem/AddWishlistItem.cs` | Add inline labels |
| TASK-090 | `Features/Store/Wishlists/Create/CreateWishlist.cs` | Add inline labels |
| TASK-091 | `Features/Store/Wishlists/Delete/DeleteWishlist.cs` | Add inline labels |
| TASK-092 | `Features/Store/Wishlists/GetById/GetWishlistById.cs` | Add inline labels |
| TASK-093 | `Features/Store/Wishlists/Get/GetWishlists.cs` | Add inline labels |
| TASK-094 | `Features/Store/Wishlists/RemoveItem/RemoveWishlistItem.cs` | Add inline labels |
| TASK-095 | `Features/Store/Wishlists/Update/UpdateWishlist.cs` | Add inline labels |

### Implementation Phase 4: Inventory Module — Add Inline Labels (9 files)

- GOAL-004: Add inline labels to 9 Inventory handler files missing them. Add class-level `/// <summary>` to 5 Inventory handler classes missing it.

| Task | File | Work |
|------|------|------|
| TASK-096 | `Features/Admin/StockItems/GetAll/GetAllStockItems.cs` | Add inline labels |
| TASK-097 | `Features/Admin/StockItems/GetById/GetStockItemById.cs` | Add inline labels |
| TASK-098 | `Features/Admin/StockItems/Restock/RestockStockItem.cs` | Add inline labels |
| TASK-099 | `Features/Admin/StockItems/Summary/GetStockSummary.cs` | Add inline labels |
| TASK-100 | `Features/Admin/StockLocations/Delete/DeleteStockLocation.cs` | Add inline labels |
| TASK-101 | `Features/Admin/StockLocations/GetById/GetStockLocationById.cs` | Add inline labels |
| TASK-102 | `Features/Admin/StockLocations/GetPaged/GetPagedStockLocations.cs` | Add inline labels |
| TASK-103 | `Features/Admin/StockLocations/SetDefault/SetDefaultStockLocation.cs` | Add inline labels |
| TASK-104 | `Features/Admin/StockTransfers/GetById/GetStockTransferById.cs` | Add inline labels |

### Implementation Phase 5: Remaining Gaps — Ordering, Dashboard, Catalog Storefront (5 files)

- GOAL-005: Address remaining gaps — 1 Ordering handler (class doc), 1 Dashboard handler (inline labels), 4 Catalog Storefront handlers (upgrade `/// <inheritdoc />` to explicit inline labels)

| Task | File | Work |
|------|------|------|
| TASK-105 | `Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.cs` | Add class doc + inline labels |
| TASK-106 | `Dashboard/Features/Admin/Get/GetDashboard.cs` | Add inline labels |
| TASK-107 | `Catalog/Features/Storefront/Products/Get/Detail/GetProductDetail.cs` | Upgrade `/// <inheritdoc />` to explicit `/// <summary>` |
| TASK-108 | `Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs` | Add class doc |
| TASK-109 | `Catalog/Features/Storefront/Taxonomies/Get/Tree/GetTree.cs` | Add inline labels |

### Implementation Phase 6: Verification

- GOAL-006: Full build and test verification after all changes

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-110 | `dotnet build` — verify 0 warnings, 0 errors | | |
| TASK-111 | `dotnet test service/Api/tests/Module.UnitTests` — verify all 2510+ tests pass | | |
| TASK-112 | Verify all 4 `/// <inheritdoc />` instances upgraded to explicit `/// <summary>` | | |

## 3. Alternatives

- **ALT-001**: Skip handlers that already have inline labels — rejected because the user explicitly asked for ALL handlers including those already covered
- **ALT-002**: Auto-generate all labels with an LLM — rejected because per the Semantic Density Principle (arXiv:2604.07502), human-curated annotations outperform LLM-generated ones
- **ALT-003**: Process by file size (largest first) — rejected; module-based batching is more deterministic and allows per-module verification

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` — authoritative standard defining all label categories
- **DEP-002**: `guide/code-commenting/README.md` — human-readable reference for label selection
- **DEP-003**: `dotnet build` with TreatWarningsAsErrors=true — must pass after each phase

## 5. Files

- **FILE-001** to **FILE-027**: 27 Identity handler files (see TASK-001 to TASK-027)
- **FILE-028** to **FILE-076**: 49 Catalog handler files (see TASK-028 to TASK-076)
- **FILE-077** to **FILE-095**: 19 Profile handler files (see TASK-077 to TASK-095)
- **FILE-096** to **FILE-104**: 9 Inventory handler files (see TASK-096 to TASK-104)
- **FILE-105** to **FILE-109**: 5 remaining handler files (see TASK-105 to TASK-109)

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings (TreatWarningsAsErrors=true)
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests --no-build` — all tests pass
- **TEST-003**: Verify 4 files using `/// <inheritdoc />` on Handle() changed to explicit `/// <summary>`: `GetAllOptionTypes.cs`, `GetProductDetail.cs`, `GetRelatedProducts.cs`, `GetAllTaxons.cs`
- **TEST-004**: Grep audit — verify no Handle() method remains without `/// <summary>` in 5-line window

## 7. Risks & Assumptions

- **RISK-001**: Adding class `/// <summary>` to handlers with nested private classes may need careful placement above the outermost class declaration
- **RISK-002**: Inline labels must describe WHY, not WHAT — a risk of vague labels (AP-2) if the label body restates the code. Mitigation: follow imperative-verb format: "Load user by ID" not "User loading"
- **ASSUMPTION-001**: All 252 handler files follow the same vertical-slice pattern: `{Action}.cs` containing a nested `Handler` class implementing `IRequestHandler<,>`
- **ASSUMPTION-002**: Handle() method bodies already have sufficient structure to add meaningful CAT-1/CAT-3/CAT-8 labels that explain intent rather than restate code

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — authoritative standard (CAT-1 through CAT-10)
- `guide/code-commenting/README.md` — human-readable overview with label decision tree
- `guide/code-commenting/SKILL.md` — code-commenting skill with application workflow
- `guide/code-commenting/references/label-quick-reference.md` — full label table reference
- `plan/feature-code-commenting-api-services-1.md` — companion plan for service files
- `plan/feature-code-commenting-benchmarks-1.md` — companion plan for Python benchmarks
