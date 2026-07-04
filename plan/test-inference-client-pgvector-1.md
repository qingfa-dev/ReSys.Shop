---
goal: Unit test InferenceClient implementation, models, and pgvector embedding pipeline
version: 1.0
date_created: 2026-07-03
status: 'Completed'
tags: test, inference, embeddings, pgvector, http
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Add unit test coverage for the `InferenceClient` typed HttpClient implementation, its request/response DTOs, the config-driven DI registration, and the pgvector embedding data pipeline (converting inference responses to `ImageEmbedding` entities). The `InferenceClient` currently has zero test coverage and no callers — tests ensure correctness before consumers (e.g., `SearchByImage`) are built against it.

## 1. Requirements & Constraints

- **REQ-001**: `InferenceClient.CreateEmbeddingAsync` and `ListModelsAsync` must be unit-testable without a real HTTP service
- **REQ-002**: Tests must verify success, HTTP error, timeout, and network-failure paths using a mock `HttpMessageHandler`
- **REQ-003**: `EmbeddingRequest`, `EmbeddingResponse`, and `ModelMetadata` JSON serialization must match the `camelCase` naming policy used by the inference client
- **REQ-004**: The `List<float>` vector from `EmbeddingResponse` must correctly convert to `Pgvector.Vector` via `ImageEmbeddingMethod.Create()`
- **REQ-005**: Tests must follow the existing `Module.UnitTests` pattern (xUnit v3, FluentAssertions, `[Trait]` attributes, no base class)
- **REQ-006**: The pgvector `VectorValueConverter` tests must be verified as passing for the `Vector <-> JSON` round-trip
- **CON-001**: `InferenceClient` receives `HttpClient` via DI — tests must use `HttpClient` with a mock `DelegatingHandler` (no `WebApplicationFactory`)
- **CON-002**: `Result<T>` is a `readonly record struct` — tests verify `.IsSuccess`/`.Errors`/`.Value` patterns
- **CON-003**: The test file must live in `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/` mirroring the production source tree
- **PAT-001**: Follow the test pattern in `VariantImage.Method.Tests.cs` (standalone `[Fact]` methods, `DisplayName`, FluentAssertions)

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Create a mock `HttpMessageHandler` for the test and unit test `InferenceClient.CreateEmbeddingAsync` across all code paths

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create directory `.../Tests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/` and test file `ImageEmbedding.Inference.Tests.cs` | | |
| TASK-002 | Implement a `MockHttpMessageHandler` inner class that returns configurable `HttpResponseMessage` or throws on demand (for timeout/network-error simulation) | | |
| TASK-003 | Test `CreateEmbeddingAsync_Success_ReturnsEmbeddingResponse`: mock returns 200 + valid JSON, verify `Result.Value` matches expected `EmbeddingResponse` (vector, model version, dimension, metadata) | | |
| TASK-004 | Test `CreateEmbeddingAsync_NonSuccessStatusCode_ReturnsServiceError`: mock returns 500 + error body, verify `Result.IsSuccess` is false and errors contain `Inference.ServiceError` | | |
| TASK-005 | Test `CreateEmbeddingAsync_InvalidResponseBody_ReturnsInvalidResponse`: mock returns 200 + malformed JSON, verify errors contain `Inference.InvalidResponse` | | |
| TASK-006 | Test `CreateEmbeddingAsync_OperationCanceled_ReturnsRequestTimeout`: mock handler throws `OperationCanceledException`, verify errors contain `Inference.RequestTimeout` | | |
| TASK-007 | Test `CreateEmbeddingAsync_NetworkFailure_ReturnsCommunicationFailed`: mock handler throws `HttpRequestException`, verify errors contain `Inference.CommunicationFailed` | | |
| TASK-008 | Test `CreateEmbeddingAsync_NullResponseBody_ReturnsInvalidResponse`: mock returns 200 + null body via 204 No Content, verify errors contain `Inference.InvalidResponse` | | |

### Implementation Phase 2

- GOAL-002: Unit test `InferenceClient.ListModelsAsync` across all code paths

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Test `ListModelsAsync_Success_ReturnsModelList`: mock returns 200 + valid JSON array, verify `Result.Value` is a non-empty `List<ModelMetadata>` | | |
| TASK-010 | Test `ListModelsAsync_NonSuccessStatusCode_ReturnsServiceError`: mock returns 503, verify error handling | | |
| TASK-011 | Test `ListModelsAsync_OperationCanceled_ReturnsRequestTimeout`: same pattern as TASK-006 for `ListModelsAsync` | | |
| TASK-012 | Test `ListModelsAsync_NetworkFailure_ReturnsCommunicationFailed`: mock throws `HttpRequestException` | | |

### Implementation Phase 3

- GOAL-003: Unit test the JSON serialization contract of the inference DTOs (`EmbeddingRequest`, `EmbeddingResponse`, `ModelMetadata`)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create test file `ImageEmbedding.Inference.Models.Tests.cs` next to the existing inference tests | | |
| TASK-014 | Test `EmbeddingRequest_SerializesToCamelCase`: use `JsonSerializer.Serialize` with `JsonNamingPolicy.CamelCase`, verify `image_url` and `model` property names | | |
| TASK-015 | Test `EmbeddingRequest_DeserializesFromCamelCase`: verify round-trip | | |
| TASK-016 | Test `EmbeddingResponse_SerializesToCamelCase`: verify `vector`, `model_version`, `dimension`, `metadata` property names | | |
| TASK-017 | Test `EmbeddingResponse_WithMetadata_DeserializesCorrectly`: verify `Dictionary<string, object>` metadata round-trip | | |
| TASK-018 | Test `ModelMetadata_SerializesToCamelCase`: verify `id`, `name`, `dimension`, `description`, `isOnnx`, `tags` | | |

### Implementation Phase 4

- GOAL-004: Verify the DI registration works correctly and the pgvector data pipeline is sound

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Test `AddInferenceClient_RegistersTypedClient`: create `ServiceCollection`, call `AddInferenceClient`, build provider, resolve `IInferenceClient`, verify it is not null and is of type `InferenceClient` | | |
| TASK-020 | Verify `VectorValueConverter` tests pass: run `VectorValueConverter.Tests.cs` and `VectorConfiguration.Tests.cs` in `Shared.UnitTests` | | |
| TASK-021 | Verify `ImageEmbeddingMethod.Create` correctly converts `List<float>` to `Pgvector.Vector`: this test already exists in `ImageEmbedding.Method.Tests.cs` — confirm it passes | | |
| TASK-022 | Verify `ImageEmbedding.Validation.Tests.cs` passes: the validation tests already exist | | |
| TASK-023 | Run full `Module.UnitTests` and `Shared.UnitTests` test suites to confirm no regressions | | |

## 3. Alternatives

- **ALT-001**: Integration test via `WebApplicationFactory` with Testcontainers. Rejected because the inference service is a Python app not running in test containers; unit testing with mock `HttpMessageHandler` is faster, more reliable, and tests the same code paths.
- **ALT-002**: Use Moq to mock `IInferenceClient` instead of testing the implementation directly. Rejected — this would test the mock, not the real `InferenceClient` implementation; we need to verify JSON serialization, error handling, and status-code logic in the actual client.
- **ALT-003**: Test pgvector end-to-end with an integration test. Already covered by existing `VectorValueConverter` unit tests and `ImageEmbeddingConfiguration` tests.

## 4. Dependencies

- **DEP-001**: `Pgvector` NuGet package — already available via `Module.UnitTests` reference to `Module.csproj` → `Shared.csproj`
- **DEP-002**: `Microsoft.Extensions.DependencyInjection` — for DI registration test
- **DEP-003**: `System.Text.Json` — for DTO serialization tests
- **DEP-004**: No new NuGet packages required

## 5. Files

| File | Change |
|------|--------|
| `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Tests.cs` | **New** — unit tests for `InferenceClient.CreateEmbeddingAsync` and `ListModelsAsync` |
| `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Models.Tests.cs` | **New** — unit tests for DTO serialization |
| `service/Api/tests/Shared.UnitTests/Operational/Persistence/Configurations/Vectors/VectorValueConverter.Tests.cs` | **Verify** — run existing tests |
| `service/Api/tests/Shared.UnitTests/Operational/Persistence/Configurations/Vectors/VectorConfiguration.Tests.cs` | **Verify** — run existing tests |
| `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs` | **Verify** — run existing tests |
| `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Validation.Tests.cs` | **Verify** — run existing tests |

## 6. Testing

- **TEST-001**: `ImageEmbedding.Inference.Tests.cs` — 10 test methods covering all `CreateEmbeddingAsync` code paths (success, HTTP error, invalid response, timeout, network failure) and all `ListModelsAsync` code paths (success, HTTP error, timeout, network failure)
- **TEST-002**: `ImageEmbedding.Inference.Models.Tests.cs` — 6 test methods covering JSON property naming (camelCase), serialization round-trip, and metadata dictionaries
- **TEST-003**: Existing pgvector `VectorValueConverter` tests (4 tests) pass
- **TEST-004**: Existing `VectorConfiguration` tests (3 tests) pass
- **TEST-005**: Existing `ImageEmbeddingMethod` tests pass
- **TEST-006**: Existing `ImageEmbedding.Validation` tests pass

## 7. Risks & Assumptions

- **RISK-001**: The inference service API contract (endpoint paths `/embeddings` and `/models`, JSON shape of request/response) is assumed to match the DTOs. Any future API contract change will break tests — tests should be updated in lockstep.
- **ASSUMPTION-001**: `Result<T>` deserialization from JSON works correctly. The inference client expects the inference service to return a `Result<EmbeddingResponse>` JSON shape. If the inference service returns a raw `EmbeddingResponse` (not wrapped in a `Result<T>` envelope), the `DeserializeResultAsync` method will fail — this should be verified when integrating with the real service.
- **ASSUMPTION-002**: The `MockHttpMessageHandler` pattern (captured in the test) is the standard approach for testing typed HttpClients and will remain valid across .NET SDK updates.
- **ASSUMPTION-003**: The pgvector `VectorValueConverter` serializes vectors as JSON float arrays — this matches the `EmbeddingResponse.Vector` format (JSON array of floats).

## 8. Related Specifications / Further Reading

- [InferenceClient implementation](../service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/)
- [ImageEmbedding domain model](../service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/)
- [VectorValueConverter tests](../service/Api/tests/Shared.UnitTests/Operational/Persistence/Configurations/Vectors/VectorValueConverter.Tests.cs)
- [Existing test pattern example — VariantImage tests](../service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/VariantImage.Method.Tests.cs)
