---
goal: Replace EncryptedDictionaryConverter with inline IEncryptionService
version: 1.0
date_created: 2026-07-11
last_updated: 2026-07-11
owner: Payment Module Team
status: Planned
tags: refactor, payment, encryption, architecture
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Replace the `EncryptedDictionaryConverter` (EF Core `ValueConverter` with static factory) with explicit `IEncryptionService` calls in domain extension methods and feature handlers. This eliminates the fragile static-factory pattern, makes encryption/decryption testable with direct DI, and removes the `BuildServiceProvider()` anti-pattern from startup.

## 1. Requirements & Constraints

- **REQ-001**: `PaymentMethod.Settings` changes from `Dictionary<string, string>` to `string?` (raw encrypted JSON).
- **REQ-002**: Domain extension methods `GetSettingsDecrypted(IEncryptionService)` and `SetSettingsEncrypted(Dictionary<string, string>, IEncryptionService)` handle encrypt/decrypt inline.
- **REQ-003**: `EncryptedDictionaryConverter` removed from `PaymentMethodConfiguration` — `Settings` column mapped as plain `text`.
- **REQ-004**: `EncryptedDictionaryConverter.Configure()` and associated `BuildServiceProvider()` call removed from `Payment.Extension.cs`.
- **REQ-005**: Feature handlers inject `IEncryptionService` and use domain extension methods to read/write `Settings`.
- **REQ-006**: API request/response models retain `Dictionary<string, string>?` — mapping layer handles encrypt/decrypt.
- **REQ-007**: All existing tests pass with updated pattern. New tests verify explicit encrypt/decrypt roundtrip.
- **CON-001**: `TreatWarningsAsErrors=true` — zero warnings after refactor.
- **CON-002**: `IEncryptionService` remains `string Encrypt(string)` / `string Decrypt(string)` — no signature changes.
- **CON-003**: `AesEncryptionService` constructor takes raw `string encryptionKey` — no `IOptions<>` dependency (already done).

## 2. Implementation Steps

### Implementation Phase 1: Domain Entity + Extensions

- GOAL-001: Change PaymentMethod.Settings type and add inline encrypt/decrypt extension methods

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Change `PaymentMethod.Settings` from `Dictionary<string, string>` to `string?` (stores encrypted JSON) | | |
| TASK-002 | Add domain extension method `SetSettingsEncrypted(Dictionary<string, string> settings, IEncryptionService)` — JSON-serializes, encrypts, stores in `Settings` string | | |
| TASK-003 | Add domain extension method `GetSettingsDecrypted(IEncryptionService)` — decrypts `Settings` string, JSON-deserializes, returns `Dictionary<string, string>?` | | |
| TASK-004 | Update `PaymentMethodExtensions.Create()` — accept `Dictionary<string, string>?` and `IEncryptionService`; encrypt before storing | | |
| TASK-005 | Update `PaymentMethodExtensions.Update()` — accept `Dictionary<string, string>?` and `IEncryptionService`; encrypt before storing | | |

### Implementation Phase 2: EF Core Configuration

- GOAL-002: Remove EncryptedDictionaryConverter from configuration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Update `PaymentMethodConfiguration` — change `Settings` from `.HasConversion<EncryptedDictionaryConverter>().HasColumnType("jsonb")` to plain `.HasColumnType("text")` | | |
| TASK-007 | Remove `using Shared.Persistence.Converters;` from `PaymentMethodConfiguration.cs` if no longer needed | | |

### Implementation Phase 3: Feature Handlers + Mappings

- GOAL-003: Update all consumers of Settings to use IEncryptionService inline

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Inject `IEncryptionService` into `CreatePaymentMethod.CommandHandler` — encrypt settings before save, decrypt for response | | |
| TASK-009 | Inject `IEncryptionService` into `UpdatePaymentMethod.CommandHandler` — encrypt settings before update, decrypt for response | | |
| TASK-010 | Update `PaymentMethod.Mapping.Domain.cs` — `MapToDomain` accepts `IEncryptionService`, encrypts settings during mapping | | |
| TASK-011 | Update `PaymentMethod.Mapping.Model.cs` — `MapToDetail`/`MapToListItem` accept `IEncryptionService`, decrypt settings during mapping | | |
| TASK-012 | Update `GetPaymentMethodById.CommandHandler` and `GetPagedPaymentMethods.CommandHandler` — inject `IEncryptionService`, decrypt settings for response | | |
| TASK-013 | Update `ActivatePaymentMethod`, `DeactivatePaymentMethod`, `DeletePaymentMethod` — inject `IEncryptionService` if they read settings (check current code) | | |

### Implementation Phase 4: Startup + Cleanup

- GOAL-004: Remove EncryptedDictionaryConverter wiring and clean up

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Remove `EncryptedDictionaryConverter.Configure(...)` call from `Payment.Extension.cs` | | |
| TASK-015 | Remove `using Shared.Persistence.Converters;` from `Payment.Extension.cs` if no longer needed | | |
| TASK-016 | Remove `EncryptedDictionaryConverter.cs` from `Shared/Persistence/Converters/` OR keep it unused (decide: if no other consumers, delete it) | | |
| TASK-017 | Regenerate EF Core migration: `dotnet ef migrations add UseInlineEncryptionForSettings` | | |

### Implementation Phase 5: Tests

- GOAL-005: Update tests to use inline IEncryptionService pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Update `AesEncryptionService.Tests.cs` — no changes needed (interface unchanged) | | |
| TASK-019 | Rewrite `EncryptedDictionaryConverter.Tests.cs` — rename to `InlineEncryptionTests.cs`, test `SetSettingsEncrypted`/`GetSettingsDecrypted` roundtrip (no converter needed) | | |
| TASK-020 | Update `PaymentMethodSettingsEncryptionTests.cs` — create `PaymentMethod`, call `SetSettingsEncrypted(service)`, save, reload, call `GetSettingsDecrypted(service)`, verify roundtrip | | |
| TASK-021 | Update `PaymentMethod.Extensions.Tests.cs` — `Create()` and `Update()` now require `IEncryptionService` parameter; update call sites | | |
| TASK-022 | Update `CreatePaymentMethodTests.cs` — mock `IEncryptionService`, verify handler calls encrypt/decrypt | | |
| TASK-023 | Run full test suite: `dotnet test` — verify zero new failures | | |

## 3. Alternatives

- **ALT-001**: Keep `EncryptedDictionaryConverter` but fix the DI pattern by using a custom `Microsoft.EntityFrameworkCore.Infrastructure.IModelCustomizer` or `IValueConverterSelector` to inject `IEncryptionService` at model-building time. Rejected: more complex, still fragile timing.
- **ALT-002**: Use ASP.NET Core `IDataProtector` instead of custom `IEncryptionService`. Rejected: `IDataProtector` binds data to a machine/user profile, preventing key rotation across deployments. AES-256 gives ops full control over key management.

## 4. Dependencies

- **DEP-001**: `AesEncryptionService` (existing, in `Shared.Operational.Security.Encryption`) — no changes needed.
- **DEP-002**: `EncryptionHelper` (existing, in `Shared.Operational.Storages.Helpers`) — no changes needed.
- **DEP-003**: `IEncryptionService` interface (existing) — no changes needed.

## 5. Files

| File | Action | Description |
|------|--------|-------------|
| `service/Api/src/Module/Payment/Domain/PaymentMethods/PaymentMethod.cs` | MODIFY | Change `Settings` type from `Dictionary<string, string>` to `string?` |
| `service/Api/src/Module/Payment/Domain/PaymentMethods/PaymentMethod.Extensions.cs` | MODIFY | Add `SetSettingsEncrypted`/`GetSettingsDecrypted`; update `Create`/`Update` sigs |
| `service/Api/src/Module/Payment/Persistence/Configurations/PaymentMethods/PaymentMethodConfiguration.cs` | MODIFY | Remove `EncryptedDictionaryConverter`, map `Settings` as plain `text` |
| `service/Api/src/Module/Payment/Payment.Extension.cs` | MODIFY | Remove `EncryptedDictionaryConverter.Configure()` + `BuildServiceProvider()` |
| `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Create/CreatePaymentMethod.cs` | MODIFY | Inject `IEncryptionService`, encrypt settings before save |
| `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Update/UpdatePaymentMethod.cs` | MODIFY | Inject `IEncryptionService`, encrypt settings before update |
| `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Get/ById/GetPaymentMethodById.cs` | MODIFY | Inject `IEncryptionService`, decrypt settings for response |
| `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Get/Paged/GetPagedPaymentMethods.cs` | MODIFY | Inject `IEncryptionService`, decrypt settings for response |
| `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Domain.cs` | MODIFY | Pass `IEncryptionService` through mappings |
| `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Model.cs` | MODIFY | Pass `IEncryptionService` through mappings |
| `service/Api/src/Shared/Persistence/Converters/EncryptedDictionaryConverter.cs` | DELETE | No longer used |
| `service/Api/tests/Module.UnitTests/Payment/Persistence/PaymentMethodSettingsEncryptionTests.cs` | MODIFY | Use inline encrypt/decrypt pattern |
| `service/Api/tests/Shared.UnitTests/Persistence/Converters/EncryptedDictionaryConverter.Tests.cs` | MODIFY | Rewrite as inline encryption tests |
| `service/Api/tests/Module.UnitTests/Payment/Domain/PaymentMethods/PaymentMethod.Extensions.Tests.cs` | MODIFY | Update `Create`/`Update` call sites |

## 6. Testing

- **TEST-001**: `SetSettingsEncrypted` followed by `GetSettingsDecrypted` returns original dictionary — proves inline roundtrip.
- **TEST-002**: Empty/null dictionary → `SetSettingsEncrypted` → `GetSettingsDecrypted` returns empty dictionary.
- **TEST-003**: DB roundtrip: entity + `SetSettingsEncrypted` → save → reload → `GetSettingsDecrypted` — all keys preserved.
- **TEST-004**: Encrypted column value in DB is not human-readable (does not contain original dictionary keys/values).
- **TEST-005**: `CreatePaymentMethod` handler encrypts request settings before saving.
- **TEST-006**: `GetPaymentMethodById` handler decrypts settings in response.
- **TEST-007**: Architecture: `EncryptedDictionaryConverter` is not referenced anywhere in the codebase (grep-verified).

## 7. Risks & Assumptions

- **RISK-001**: Feature handlers that currently read `paymentMethod.Settings` directly (without going through mapping) need to be updated to call `paymentMethod.GetSettingsDecrypted(encryptionService)` instead. Risk: missing a call site → runtime NRE on settings access.
- **RISK-002**: Request models currently type `Settings` as `Dictionary<string, string>?` — this stays unchanged (API contract). Only the entity storage type changes.
- **ASSUMPTION-001**: No other code outside the Payment module references `EncryptedDictionaryConverter`. Confirmed by grep — only `PaymentMethodConfiguration` and `Payment.Extension.cs` use it.
- **ASSUMPTION-002**: `SettingsEncryptionKey` is properly configured in `GatewayProviders` section and `IEncryptionService` is registered before any feature handler resolves it.

## 8. Related Specifications / Further Reading

- `spec/design-payment-gateway-abstraction.md` — parent spec for this refactor
- `service/Api/src/Shared/Operational/Security/Encryption/IEncryptionService.cs` — encryption interface
- `service/Api/src/Shared/Operational/Storages/Helpers/EncryptionHelper.cs` — AES stream helper
