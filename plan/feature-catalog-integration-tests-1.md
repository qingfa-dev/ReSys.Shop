---
goal: Add comprehensive integration tests for the Catalog module (Admin + Storefront)
version: 1.0
date_created: 2026-07-04
last_updated: 2026-07-04
status: Planned
tags: feature, testing, catalog, integration-tests
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create integration tests for the Catalog module covering both Admin (back-office CRUD) and Storefront (public read) endpoints using the existing test infrastructure (PostgreSQL Testcontainers, Respawn, WebApplicationFactory, xUnit v3).

## 1. Requirements & Constraints

- **REQ-001**: All tests use the existing `ApiIntegrationTestBase` base class with `ApiFixture` injection
- **REQ-002**: Admin endpoints require JWT Bearer auth via `PostAsAdminRawAsync()` / `DeleteAsAdminRawAsync()`
- **REQ-003**: Storefront endpoints use unauthenticated `HttpClient` requests
- **REQ-004**: Response assertions use `ReadApiResponseAsync()` + `DeserializeValue<T>()` or `ReadAsResultAsync<T>()` / `ReadAsPagedResultAsync<T>()`
- **REQ-005**: Tests follow existing directory convention: `Scenarios/Catalog/{Admin|Store}/<Resource>/<Operation>/<Name>.IntegrationTests.cs`
- **CON-001**: `CatalogSchema.Name` must be added to Respawn's `SchemasToInclude` for DB reset between tests
- **CON-002**: Test request bodies use anonymous objects (not typed model classes)
- **CON-003**: All test files import response DTOs from `Module.Catalog.Features.*.Shared.Models`
- **CON-004**: Docker/Podman required for PostgreSQL Testcontainer
- **GUD-001**: Each test method tests one scenario (valid create, duplicate, missing fields, auth failure)
- **PAT-001**: Primary constructor pattern: `class Foo(ApiFixture fixture) : ApiIntegrationTestBase(fixture)`

## 2. Implementation Steps

### Implementation Phase 0: Infrastructure

- GOAL-001: Add CatalogSchema to Respawn configuration enabling database reset for Catalog tests

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `CatalogSchema.Name` to `SchemasToInclude` in `ApiFixture.cs` | | |
| TASK-002 | Verify existing tests still pass after schema addition | | |

### Implementation Phase 1: Admin - Option Types & Option Values

- GOAL-002: Test CRUD operations for option types and their option values

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | CreateOptionType tests (valid, duplicate name 409, missing name 422, no auth 401) | | |
| TASK-004 | GetAllOptionTypes tests (returns list, empty when none exist) | | |
| TASK-005 | GetOptionTypeById tests (found, not found 404) | | |
| TASK-006 | UpdateOptionType tests (valid update, not found 404) | | |
| TASK-007 | DeleteOptionType tests (valid delete, not found 404) | | |
| TASK-008 | CreateOptionValue tests (valid, missing name 422, no auth) | | |
| TASK-009 | GetAllOptionValues tests (returns values for option type, empty for unknown type) | | |
| TASK-010 | GetOptionValueById tests (found, not found) | | |
| TASK-011 | UpdateOptionValue tests (valid, not found) | | |
| TASK-012 | DeleteOptionValue tests (valid, not found) | | |

### Implementation Phase 2: Admin - Taxonomies & Taxons

- GOAL-003: Test CRUD operations for taxonomies, taxons, taxon tree, reposition, and restore

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | CreateTaxonomy tests (valid, duplicate name 409, missing name 422, no auth) | | |
| TASK-014 | GetAllTaxonomies tests (returns list, verify auto-created root taxon) | | |
| TASK-015 | GetTaxonomyById tests (found with taxons, not found 404) | | |
| TASK-016 | UpdateTaxonomy tests (valid update, not found 404) | | |
| TASK-017 | DeleteTaxonomy tests (soft delete, not found, no auth) | | |
| TASK-018 | RestoreTaxonomy tests (restore deleted, not found) | | |
| TASK-019 | CreateTaxon tests (valid with/without parent, missing name 422, no auth) | | |
| TASK-020 | GetAllTaxons tests (returns list for taxonomy, empty for missing taxonomy) | | |
| TASK-021 | GetTaxonById tests (found with parent/children data, not found 404) | | |
| TASK-022 | GetTaxonTree tests (returns tree structure with nested taxons) | | |
| TASK-023 | UpdateTaxon tests (valid, not found) | | |
| TASK-024 | DeleteTaxon tests (soft delete, not found) | | |
| TASK-025 | RestoreTaxon tests (restore deleted) | | |
| TASK-026 | RepositionTaxon tests (move up/down/to-root) | | |

### Implementation Phase 3: Admin - Products (CRUD + Status)

- GOAL-004: Test product CRUD, activation, discontinuation, and listing

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | CreateProduct tests (valid, missing name 422, duplicate slug 409, no auth) | | |
| TASK-028 | GetAllProducts tests (paged list, empty, pagination, status filter) | | |
| TASK-029 | GetProductById tests (found with variants, not found 404) | | |
| TASK-030 | UpdateProduct tests (valid update, not found 404) | | |
| TASK-031 | DeleteProduct tests (soft delete, not found, no auth) | | |
| TASK-032 | ActivateProduct tests (draft→active, already active, not found) | | |
| TASK-033 | DiscontinueProduct tests (active→archived, already archived, not found) | | |

### Implementation Phase 4: Admin - Variants, Prices & Option Values

- GOAL-005: Test variant management, price operations, and option-value assignments

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-034 | AddVariant tests (valid, missing sku 422, no auth) | | |
| TASK-035 | GetAllVariants tests (returns list for product, empty for no variants) | | |
| TASK-036 | GetVariantById tests (found, not found 404) | | |
| TASK-037 | UpdateVariant tests (valid update, not found) | | |
| TASK-038 | DeleteVariant tests (valid, not found, no auth) | | |
| TASK-039 | SetPrice tests (valid upsert, missing amount 422, not found) | | |
| TASK-040 | ListPrices tests (returns prices for variant, empty) | | |
| TASK-041 | RemovePrice tests (valid, not found) | | |
| TASK-042 | GetVariantOptionValues tests (returns status for each option value) | | |
| TASK-043 | AssignOptionValues tests (valid assignment, empty list) | | |
| TASK-044 | RevokeOptionValues tests (valid, not found) | | |

### Implementation Phase 5: Admin - Images, Classifications, Product Option Types

- GOAL-006: Test image upload, classifications, and product-option-type assignments

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-045 | UploadVariantImage tests (valid upload, missing file 422, no auth) | | |
| TASK-046 | GetAllVariantImages tests (returns list, empty for variant without images) | | |
| TASK-047 | GetVariantImageById tests (found, not found 404) | | |
| TASK-048 | UpdateVariantImage tests (update alt/position/type, not found) | | |
| TASK-049 | DeleteVariantImage tests (valid, not found) | | |
| TASK-050 | DownloadVariantImage tests (valid download, not found 404) | | |
| TASK-051 | GetProductClassifications tests (returns assigned taxons, empty) | | |
| TASK-052 | AssignClassifications tests (valid, not found) | | |
| TASK-053 | RevokeClassifications tests (valid, not found) | | |
| TASK-054 | GetProductOptionTypes tests (returns assigned option types) | | |
| TASK-055 | AssignProductOptionTypes tests (valid assignment) | | |
| TASK-056 | RevokeProductOptionTypes tests (valid, not found) | | |

### Implementation Phase 6: Storefront - Products

- GOAL-007: Test storefront product listing, detail, availability, related, similar, and search-by-image

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-057 | GetProductDetail tests (found by slug, not found 404, draft product not visible) | | |
| TASK-058 | ListProducts tests (returns active products, pagination, text search, color/size/price filters) | | |
| TASK-059 | GetProductAvailability tests (returns style matrix, not found) | | |
| TASK-060 | GetRelatedProducts tests (returns related by shared taxon, empty for no relation) | | |
| TASK-061 | GetSimilarProducts tests (returns visually similar, empty if no embeddings) | | |
| TASK-062 | SearchByImage tests (returns results/success, handles empty upload) | | |

### Implementation Phase 7: Storefront - Taxons, Taxonomies, OptionTypes, Images

- GOAL-008: Test storefront taxonomy navigation, option types listing, and image display

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-063 | GetAllTaxons tests (returns flat list, filterable by depth/taxonomy) | | |
| TASK-064 | GetProductsByTaxon tests (returns paginated products, empty for unknown taxon, includes sub-taxon products via nested set) | | |
| TASK-065 | GetTaxonomyTree tests (returns nested tree for mega-menu) | | |
| TASK-066 | GetAllOptionTypes tests (returns option types with values for filter facets) | | |
| TASK-067 | GetImage tests (returns image inline with correct content type, not found 404) | | |

## 3. Alternatives

- **ALT-001**: Single monolithic test file per resource — rejected; separate files per operation match existing convention and allow targeted test runs
- **ALT-002**: Typed request model classes instead of anonymous objects — rejected; anonymous objects are simpler, reduce maintenance, and match existing Location/Identity test patterns
- **ALT-003**: Separate test fixture for Catalog — rejected; adding `CatalogSchema` to existing fixture is simpler and all domain tests should share the same container

## 4. Dependencies

- **DEP-001**: PostgreSQL via Testcontainers (pgvector/pgvector:pg17) — already configured in `ApiFixture`
- **DEP-002**: `Microsoft.AspNetCore.Mvc.Testing` — already in `Api.Tests.csproj`
- **DEP-003**: Catalog data seeders must exist for some tests to run against pre-populated data (e.g., duplicate detection tests)
- **DEP-004**: Podman must be running for `podman.sock` detection; Docker must be running for `/var/run/docker.sock`

## 5. Files

- **FILE-001**: Modify `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs` — add `CatalogSchema.Name` to Respawn `SchemasToInclude`
- **FILE-002**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/Create/CreateOptionType.IntegrationTests.cs`
- **FILE-003**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/GetAll/GetAllOptionTypes.IntegrationTests.cs`
- **FILE-004**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/GetById/GetOptionTypeById.IntegrationTests.cs`
- **FILE-005**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/Update/UpdateOptionType.IntegrationTests.cs`
- **FILE-006**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/Delete/DeleteOptionType.IntegrationTests.cs`
- **FILE-007**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/OptionValues/Create/CreateOptionValue.IntegrationTests.cs`
- **FILE-008**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/OptionValues/GetAll/GetAllOptionValues.IntegrationTests.cs`
- **FILE-009**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/OptionValues/GetById/GetOptionValueById.IntegrationTests.cs`
- **FILE-010**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/OptionValues/Update/UpdateOptionValue.IntegrationTests.cs`
- **FILE-011**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/OptionTypes/OptionValues/Delete/DeleteOptionValue.IntegrationTests.cs`
- **FILE-012**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Create/CreateTaxonomy.IntegrationTests.cs`
- **FILE-013**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/GetAll/GetAllTaxonomies.IntegrationTests.cs`
- **FILE-014**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/GetById/GetTaxonomyById.IntegrationTests.cs`
- **FILE-015**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Update/UpdateTaxonomy.IntegrationTests.cs`
- **FILE-016**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Delete/DeleteTaxonomy.IntegrationTests.cs`
- **FILE-017**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Restore/RestoreTaxonomy.IntegrationTests.cs`
- **FILE-018**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/Create/CreateTaxon.IntegrationTests.cs`
- **FILE-019**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/GetAll/GetAllTaxons.IntegrationTests.cs`
- **FILE-020**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/GetById/GetTaxonById.IntegrationTests.cs`
- **FILE-021**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/GetTree/GetTaxonTree.IntegrationTests.cs`
- **FILE-022**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/Update/UpdateTaxon.IntegrationTests.cs`
- **FILE-023**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/Delete/DeleteTaxon.IntegrationTests.cs`
- **FILE-024**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/Restore/RestoreTaxon.IntegrationTests.cs`
- **FILE-025**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Taxonomies/Taxons/Reposition/RepositionTaxon.IntegrationTests.cs`
- **FILE-026**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Create/CreateProduct.IntegrationTests.cs`
- **FILE-027**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/GetAll/GetAllProducts.IntegrationTests.cs`
- **FILE-028**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/GetById/GetProductById.IntegrationTests.cs`
- **FILE-029**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Update/UpdateProduct.IntegrationTests.cs`
- **FILE-030**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Delete/DeleteProduct.IntegrationTests.cs`
- **FILE-031**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Activate/ActivateProduct.IntegrationTests.cs`
- **FILE-032**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Discontinue/DiscontinueProduct.IntegrationTests.cs`
- **FILE-033**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Add/AddVariant.IntegrationTests.cs`
- **FILE-034**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/GetAll/GetAllVariants.IntegrationTests.cs`
- **FILE-035**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/GetById/GetVariantById.IntegrationTests.cs`
- **FILE-036**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Update/UpdateVariant.IntegrationTests.cs`
- **FILE-037**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Delete/DeleteVariant.IntegrationTests.cs`
- **FILE-038**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Prices/Set/SetPrice.IntegrationTests.cs`
- **FILE-039**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Prices/List/ListPrices.IntegrationTests.cs`
- **FILE-040**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Prices/Remove/RemovePrice.IntegrationTests.cs`
- **FILE-041**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.IntegrationTests.cs`
- **FILE-042**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/OptionValues/Assign/AssignVariantOptionValues.IntegrationTests.cs`
- **FILE-043**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/OptionValues/Revoke/RevokeVariantOptionValues.IntegrationTests.cs`
- **FILE-044**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/Upload/UploadVariantImage.IntegrationTests.cs`
- **FILE-045**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/GetAll/GetAllVariantImages.IntegrationTests.cs`
- **FILE-046**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/GetById/GetVariantImageById.IntegrationTests.cs`
- **FILE-047**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/Update/UpdateVariantImage.IntegrationTests.cs`
- **FILE-048**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/Delete/DeleteVariantImage.IntegrationTests.cs`
- **FILE-049**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Variants/Images/Download/DownloadVariantImage.IntegrationTests.cs`
- **FILE-050**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Classifications/Get/GetProductClassifications.IntegrationTests.cs`
- **FILE-051**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Classifications/Assign/AssignProductClassifications.IntegrationTests.cs`
- **FILE-052**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/Classifications/Revoke/RevokeProductClassifications.IntegrationTests.cs`
- **FILE-053**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/OptionTypes/Get/GetProductOptionTypes.IntegrationTests.cs`
- **FILE-054**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/OptionTypes/Assign/AssignProductOptionTypes.IntegrationTests.cs`
- **FILE-055**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Admin/Products/OptionTypes/Revoke/RevokeProductOptionTypes.IntegrationTests.cs`
- **FILE-056**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/GetDetail/GetProductDetail.IntegrationTests.cs`
- **FILE-057**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs`
- **FILE-058**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/Availability/GetProductAvailability.IntegrationTests.cs`
- **FILE-059**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/Related/GetRelatedProducts.IntegrationTests.cs`
- **FILE-060**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/Similar/GetSimilarProducts.IntegrationTests.cs`
- **FILE-061**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/SearchByImage/SearchByImage.IntegrationTests.cs`
- **FILE-062**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Taxons/GetAll/GetAllTaxons.IntegrationTests.cs`
- **FILE-063**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Taxons/GetProducts/GetTaxonProducts.IntegrationTests.cs`
- **FILE-064**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Taxonomies/GetTree/GetTaxonomyTree.IntegrationTests.cs`
- **FILE-065**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/OptionTypes/GetAll/GetAllOptionTypes.IntegrationTests.cs`
- **FILE-066**: Create `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Images/GetImage/GetImage.IntegrationTests.cs`

## 6. Testing

- **TEST-001**: Verify `CatalogSchema.Name` in Respawn — after task TASK-001, confirm test data is reset between fixtures
- **TEST-002**: Run all Catalog integration tests with `dotnet test --filter "FullyQualifiedName~Catalog"`
- **TEST-003**: Run full test suite to verify no regression: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj`
- **TEST-004**: Verify test isolation — each test starts with clean database state

## 7. Risks & Assumptions

- **RISK-001**: Podman/Docker socket detection may fail on some CI/CD environments — `ConfigureContainerRuntime()` handles this but may need explicit `DOCKER_HOST` env var
- **RISK-002**: Image upload tests (TASK-045, TASK-050) depend on storage being configured and accessible — currently disabled in test config; may need stubbed storage or mock verification
- **RISK-003**: SearchByImage (TASK-062) is a scaffold that doesn't integrate with a real inference service — test can only verify the endpoint returns a valid HTTP response, not meaningful search results
- **RISK-004**: Storefront tests require seeded test data for meaningful assertions — some tests may need setup phases that create products/taxons via Admin endpoints first
- **ASSUMPTION-001**: All Catalog data seeders run within the existing `RunSeedersAsync()` flow in `ApiFixture`
- **ASSUMPTION-002**: Admin JWT token from `AuthTokenHelper.GenerateAdminToken()` grants all Catalog permissions
- **ASSUMPTION-003**: The `ApiResponse.DeserializeValue<T>()` method correctly deserializes the `"value"` property from `Result<T>` JSON envelope

## 8. Related Specifications / Further Reading

- [API Test Infrastructure docs](service/Api/tests/Api.Tests/Infrastructure/)
- Catalog Admin route definitions: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs`
- Catalog Storefront route definitions: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`
- Existing integration test examples: `service/Api/tests/Api.Tests/Scenarios/Location/Admin/Countries/Create/CreateCountry.IntegrationTests.cs`
