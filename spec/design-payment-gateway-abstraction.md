---
title: Payment Gateway Abstraction & Provider-Agnostic Architecture
version: 1.2
date_created: 2026-07-11
last_updated: 2026-07-11
owner: Payment Module Team
tags: design, payment, gateway, refactor, abstraction, encryption
---

# Payment Gateway Abstraction & Provider-Agnostic Architecture

## 1. Purpose & Scope

Refactor `IPaymentGatewayActionProvider`, `GatewayOptions`, `PaymentGatewayResponse`, and `PaymentMethod` to form a provider-agnostic gateway abstraction. The current design leaks Stripe-specific concepts (PaymentIntent, SetupIntent, webhook types) into the domain and feature handlers, and couples `PaymentMethod.ProviderType` to free-form strings with no mapping to concrete gateway implementations. All hardcoded string values must be consolidated into nested constants classes under `GatewayConstants`.

**Scope:**
- `IPaymentGatewayActionProvider` — generalize to support any payment provider: Authorize, Capture, Purchase, Void, Refund, CreateSetupIntent, GetPaymentStatus
- `GatewayOptions` — decouple from `PaymentRecord`; become a `record` with zero domain-type references
- `PaymentGatewayResponse` — simplify flat structure; add `Provider`, `SetupIntentClientSecret`, `Properties` bag; remove nested `Params`/`Options`/`AvsResult`/`CvvResult` dictionaries
- `PaymentMethod` — replace `ProviderType` (free-form string) with `ProviderKey` validated against `IGatewayRegistry`. **Add `Settings` dictionary for encrypted provider-specific values** (API key overrides, merchant IDs). Keep `Preferences` as plain JSONB for non-sensitive behavioral config. Persist `WebhookEnabled` flag (not secrets) to DB.
- `PaymentRecord` — add `ProviderKey` snapshot; remove `Order` navigation property
- `StripeGateway` — eliminate global `StripeConfiguration.ApiKey`; use per-request `RequestOptions.ApiKey` from `GatewayProviders:stripe` config section (not from PaymentMethod.Settings — that's an admin override)
- `CreateSetupIntent` — move into `IPaymentGatewayActionProvider.CreateSetupIntentAsync()`
- DI registration — multi-provider routing via `IGatewayRegistry` (not binary Bogus/Stripe toggle)
- Cross-module coupling — Payment and Ordering communicate through MediatR only
- **Encryption infrastructure** — `IEncryptionService` (wraps existing `EncryptionHelper`) + `EncryptedDictionaryConverter` for the `PaymentMethod.Settings` column
- **Configuration layering** — `appsettings.json` for deployment secrets (API keys, webhook secrets); `PaymentMethod.Settings` for admin-managed per-method overrides (encrypted at rest in DB)

**Out of scope:**
- Partial refund implementation (separate plan)
- Stripe SDK version upgrade
- Migration scripts for existing `PaymentMethod.ProviderType` value migration
- Azure Key Vault / HashiCorp Vault integration

## 2. Definitions

| Term | Definition |
|------|-----------|
| Provider | A payment service provider (Stripe, PayPal, Bogus) |
| Gateway | The abstract interface + concrete implementation that communicates with a provider |
| ProviderKey | A registered string identifier linking `PaymentMethod` to a concrete gateway via `IGatewayRegistry` |
| SetupIntent | A provider operation to tokenize a payment method for future use (no payment amount) |
| PaymentIntent | A provider operation representing a single payment transaction with an amount |
| WebhookEvent | A provider-originated asynchronous notification (e.g., `payment_intent.succeeded`) |
| AutoCapture | Whether the provider captures immediately (purchase) or authorizes first (authorize + later capture) |
| IdempotencyKey | A unique key per transaction to prevent duplicate processing at the provider |
| GatewayRegistry | A DI-managed singleton mapping `ProviderKey` → `IPaymentGatewayActionProvider` |
| Settings | Encrypted provider-specific configuration per PaymentMethod (e.g., custom API key override, merchant account ID). Stored encrypted in DB column `Settings`. |
| Preferences | Plain-text non-sensitive behavioral config (e.g., `tracking_url`, `statement_descriptor`). Existing JSONB column. |
| Deployment Secret | API key, webhook signing secret — environment-level value from `appsettings.json` or env vars. Never in DB. |

### Configuration Layering

```
┌─────────────────────────────────────────────────────────────┐
│  Layer              │  Storage       │  Sensitivity          │
├─────────────────────────────────────────────────────────────┤
│  appsettings.json   │  Env vars /    │  HIGH — deployment    │
│  GatewayProviders:  │  config files  │  secrets (API keys,   │
│    {providerKey}:   │                │  webhook secrets)     │
│    SecretKey        │                │                       │
│    WebhookSecret    │                │                       │
│    PublishableKey   │                │                       │
├─────────────────────────────────────────────────────────────┤
│  PaymentMethod      │  DB column     │  MEDIUM — admin-      │
│  .Settings (enc)    │  (encrypted    │  managed per-method   │
│                     │   JSONB)       │  overrides (merchant  │
│                     │                │  ID, endpoint URL)    │
├─────────────────────────────────────────────────────────────┤
│  PaymentMethod      │  DB column     │  LOW — non-sensitive  │
│  .Preferences (raw) │  (JSONB)       │  behavioral config    │
│                     │                │  (tracking_url, etc.) │
└─────────────────────────────────────────────────────────────┘
```

## 3. Requirements, Constraints & Guidelines

### Configuration & Encryption

- **REQ-CFG-001**: `PaymentMethod` must have a `Settings` property (`Dictionary<string, string>`) stored encrypted in the DB column as JSONB.
- **REQ-CFG-002**: `PaymentMethod` must retain the existing `Preferences` property (`Dictionary<string, string>`) stored as plain JSONB (no encryption).
- **REQ-CFG-003**: `PaymentMethod` must have a `WebhookEnabled` boolean toggle (admin managed, persisted to DB). `WebhookUrl` and `WebhookSecret` must NOT be stored on the entity — they come from `GatewayProviders:{providerKey}` config section.
- **REQ-CFG-004**: `appsettings.json` must have a `GatewayProviders` section with one sub-section per registered `ProviderKey`. Each sub-section holds: `SecretKey`, `WebhookSecret`, `PublishableKey`, `Enabled` flag.
- **REQ-CFG-005**: Encryption key for `PaymentMethod.Settings` must come from `GatewayProviders:SettingsEncryptionKey` in appsettings. Minimum length 32 characters. Missing key at startup with non-empty Settings must log critical warning.
- **REQ-CFG-006**: An `IEncryptionService` interface must be defined (`string Encrypt(string plaintext)`, `string Decrypt(string ciphertext)`). Implementation `AesEncryptionService` wraps the existing `EncryptionHelper` from `Shared.Operational.Storages.Helpers`.
- **REQ-CFG-007**: An EF Core `EncryptedDictionaryConverter : ValueConverter<Dictionary<string, string>, string>` must handle JSON serialization + AES encryption on write, and decryption + deserialization on read. The converter uses a `IEncryptionService` resolved via static factory set at startup.

### Constant Consolidation

- **REQ-CST-001**: All hardcoded string and numeric values defined in `GatewayConstants` with nested static classes (see section 4.6).
- **REQ-CST-002**: No feature handler, gateway implementation, or domain class may use literal strings or magic numbers where a `GatewayConstants` member exists.
- **REQ-CST-003**: Provider-specific constants (Stripe event type strings, etc.) must live in their respective nested sub-classes under `GatewayConstants`.

### Provider Abstraction

- **REQ-GEN-001**: `IPaymentGatewayActionProvider` must declare: `ProviderKey { get; }`, `AutoCapture { get; }`, `SourceRequired { get; }`, `Supports(object?)`, and lifecycle methods.
- **REQ-GEN-002**: Every gateway method accepts `GatewayOptions` and returns `Result<PaymentGatewayResponse>`. No exceptions thrown from gateway methods.
- **REQ-GEN-003**: `PaymentGatewayResponse` includes `Provider`, `SetupIntentClientSecret`, `PaymentStatus`, flat AVS/CVV strings, and `Properties` bag.
- **REQ-GEN-004**: Gateway implementations must not set global static state. `StripeConfiguration.ApiKey` must never be set. `RequestOptions.ApiKey` from config per call.
- **REQ-GEN-005**: `GatewayOptions` must be a `record` with zero references to domain entities.
- **REQ-GEN-006**: `CreateSetupIntentAsync` must be on `IPaymentGatewayActionProvider`.

### Provider Routing

- **REQ-RTE-001**: `IGatewayRegistry.GetGateway(key)` returns `Result<IPaymentGatewayActionProvider>`.
- **REQ-RTE-002**: `PaymentMethod.ProviderKey` validated against `IGatewayRegistry.IsRegistered()` on create/update.
- **REQ-RTE-003**: Binary Bogus/Stripe toggle replaced with unconditional gateway registration + per-provider `Enabled` config flag.

### Webhook Abstraction

- **REQ-WEB-001**: `IWebhookHandler` interface with `string Provider { get; }`, `string[] SupportedEventTypes { get; }`, `Task<Result> HandleAsync(...)`.
- **REQ-WEB-002**: `WebhookSecret` resolved from `GatewayProviders:{providerKey}:WebhookSecret` config, not from `PaymentMethod`.
- **REQ-WEB-003**: `StripeWebhookService` renamed to `StripeWebhookHandler`, implements `IWebhookHandler`.

### PaymentMethod Entity

- **REQ-PMT-001**: `ProviderKey` replaces `ProviderType`. Max length from `GatewayConstants.Constraints.MaxProviderKeyLength`.
- **REQ-PMT-002**: Remove `IsAvailableFor(Order order)`. Replace with `IsAvailableFor(string channel)`.
- **REQ-PMT-003**: `WebhookEnabled` persisted to DB. `WebhookUrl`, `WebhookSecret` moved to `GatewayProviders` config.
- **REQ-PMT-004**: `Settings` dictionary encrypted in DB. `Preferences` dictionary plain JSONB.

### Cross-Module Decoupling

- **REQ-CRS-001**: Payment module: zero `using Module.Ordering.*`. `PaymentRecord.Order` nav removed.
- **REQ-CRS-002**: Ordering module: zero `using Module.Payment.*`. Uses MediatR commands.
- **REQ-CRS-003**: `OrderConfiguration.HasMany(p => p.Payments)` removed from Ordering module.

### Security

- **SEC-001**: `StripeConfiguration.ApiKey` never set globally; per-request `RequestOptions.ApiKey`.
- **SEC-002**: Gateway secrets never logged, serialized, or exposed in error messages.
- **SEC-003**: At startup, log critical warning if any `PaymentMethod` has `WebhookEnabled = true` and `GatewayProviders:{ProviderKey}:WebhookSecret` is null/empty.
- **SEC-004**: `PaymentMethod.Settings` encryption key must be ≥ 32 chars. Validated at startup via `GatewayProviders:SettingsEncryptionKey`.

### Constraints

- **CON-001**: All feature handlers inject `IGatewayRegistry` instead of `IPaymentGatewayActionProvider`.
- **CON-002**: `PaymentGatewayResponse` constructor parameter `parmas` → `properties`.
- **CON-003**: `IdempotencyKey` prefix `"spree-"` → `GatewayConstants.Idempotency.Prefix`.
- **CON-004**: `PaymentRecordConfiguration` maps `CaptureEventCreated`, `RefundedAmount`, `ProviderKey`.
- **CON-005**: `PaymentMethodConfiguration` maps `ProviderKey`, `WebhookEnabled`, `Settings` (encrypted), `Preferences` (plain); removes `ProviderType` mapping.

### Guidelines

- **GUD-001**: Each concrete gateway lives in `Infrastructure/Gateways/{ProviderName}/`.
- **GUD-002**: Gateway operations throw zero exceptions. All failures return `Result` with typed `Error`.
- **GUD-003**: Idempotency keys generated in feature handlers, passed through `GatewayOptions`.

## 4. Interfaces & Data Contracts

### 4.1 `IEncryptionService`

```csharp
namespace Shared.Operational.Security.Encryption;

/// <summary>Encrypts/decrypts individual string values. Wraps the existing EncryptionHelper.</summary>
public interface IEncryptionService
{
    /// <summary>Encrypts a plaintext string and returns a Base64-encoded ciphertext string.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypts a Base64-encoded ciphertext string and returns the original plaintext.</summary>
    string Decrypt(string ciphertext);
}
```

### 4.2 `AesEncryptionService` (implementation)

```csharp
namespace Shared.Operational.Security.Encryption;

using Shared.Operational.Storages.Helpers;
using System.Security.Cryptography;
using System.Text;

/// <summary>AES-256-CBC string encryption wrapping the existing EncryptionHelper.</summary>
public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IOptions<GatewayProvidersOptions> options)
    {
        var rawKey = options.Value.SettingsEncryptionKey
            ?? throw new InvalidOperationException("SettingsEncryptionKey is not configured.");
        _key = Encoding.UTF8.GetBytes(rawKey);
    }

    public string Encrypt(string plaintext)
    {
        using var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(plaintext));
        using var cipherStream = EncryptionHelper.EncryptAsync(plainStream, _key).GetAwaiter().GetResult();
        using var reader = new StreamReader(cipherStream);
        var bytes = ((MemoryStream)cipherStream).ToArray();
        return Convert.ToBase64String(bytes);
    }

    public string Decrypt(string ciphertext)
    {
        var bytes = Convert.FromBase64String(ciphertext);
        using var cipherStream = new MemoryStream(bytes);
        using var plainStream = EncryptionHelper.DecryptAsync(cipherStream, _key).GetAwaiter().GetResult();
        using var reader = new StreamReader(plainStream);
        return reader.ReadToEnd();
    }
}
```

### 4.3 `EncryptedDictionaryConverter` (EF Core value converter)

```csharp
namespace Shared.Persistence.Converters;

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Encrypts a Dictionary<string, string> when writing to DB, decrypts when reading.
/// Uses IEncryptionService resolved via a static factory set at application startup.
/// </summary>
public sealed class EncryptedDictionaryConverter : ValueConverter<Dictionary<string, string>, string>
{
    private static Func<IEncryptionService>? _encryptionServiceFactory;

    /// <summary>Must be called at startup to register the encryption service factory.</summary>
    public static void Configure(Func<IEncryptionService> factory)
    {
        _encryptionServiceFactory = factory;
    }

    public EncryptedDictionaryConverter()
        : base(
            convertToProviderExpression: dict => EncryptDictionary(dict),
            convertFromProviderExpression: encrypted => DecryptDictionary(encrypted))
    {
    }

    private static string EncryptDictionary(Dictionary<string, string> dict)
    {
        var json = JsonSerializer.Serialize(dict);
        return GetService().Encrypt(json);
    }

    private static Dictionary<string, string> DecryptDictionary(string encrypted)
    {
        var json = GetService().Decrypt(encrypted);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    private static IEncryptionService GetService()
    {
        return _encryptionServiceFactory?.Invoke()
            ?? throw new InvalidOperationException(
                "EncryptedDictionaryConverter is not configured. Call Configure() at startup.");
    }
}
```

### 4.4 `GatewayProvidersOptions` (appsettings binding)

```csharp
namespace Module.Payment.Infrastructure;

/// <summary>Binds to GatewayProviders config section.</summary>
public sealed class GatewayProvidersOptions
{
    public const string SectionName = "GatewayProviders";

    /// <summary>Encryption key for PaymentMethod.Settings column. Must be >= 32 chars.</summary>
    public string? SettingsEncryptionKey { get; set; }
}

/// <summary>Per-provider configuration from appsettings.</summary>
public sealed class ProviderOptions
{
    public bool Enabled { get; set; } = true;
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
```

### 4.5 `appsettings.json` — GatewayProviders section

```json
{
  "GatewayProviders": {
    "SettingsEncryptionKey": "",
    "stripe": {
      "Enabled": false,
      "SecretKey": "",
      "WebhookSecret": "",
      "PublishableKey": ""
    },
    "bogus": {
      "Enabled": false,
      "SecretKey": "",
      "WebhookSecret": "",
      "PublishableKey": ""
    }
  }
}
```

### 4.6 `appsettings.Development.json` — GatewayProviders section

```json
{
  "GatewayProviders": {
    "SettingsEncryptionKey": "dev-encryption-key-32-chars-len!",
    "stripe": {
      "Enabled": false,
      "SecretKey": "",
      "WebhookSecret": "",
      "PublishableKey": ""
    },
    "bogus": {
      "Enabled": true,
      "SecretKey": "",
      "WebhookSecret": "",
      "PublishableKey": ""
    }
  }
}
```

### 4.7 `IPaymentGatewayActionProvider`

```csharp
namespace Module.Payment.Domain.Gateways;

public interface IPaymentGatewayActionProvider
{
    string ProviderKey { get; }
    bool AutoCapture { get; }
    bool SourceRequired { get; }
    bool Supports(object? source);

    Task<Result<PaymentGatewayResponse>> AuthorizeAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> CaptureAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> VoidAsync(
        string? responseCode, object? source, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> RefundAsync(
        decimal amount, string? responseCode, GatewayOptions options, CancellationToken ct = default);

    Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct = default);

    Task<string> GetPaymentStatusAsync(
        string responseCode, CancellationToken ct = default);
}
```

### 4.8 `GatewayOptions`

```csharp
namespace Module.Payment.Domain.Gateways;

public sealed record GatewayOptions
{
    public static string Currency => GatewayConstants.Currency.Usd;

    public required string Email { get; init; }
    public required string Customer { get; init; }
    public string? CustomerId { get; init; }
    public string? Ip { get; init; }
    public required string OrderId { get; init; }
    public required string PaymentId { get; init; }
    public required string IdempotencyKey { get; init; }
    public string? StatementDescriptorSuffix { get; init; }
    public decimal Shipping { get; init; }
    public decimal Tax { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public Dictionary<string, object?>? BillingAddress { get; init; }
    public Dictionary<string, object?>? ShippingAddress { get; init; }
    public Dictionary<string, object?>? ProviderSpecific { get; init; }
}
```

### 4.9 `PaymentGatewayResponse`

```csharp
namespace Module.Payment.Domain.Gateways;

public sealed record PaymentGatewayResponse
{
    public bool Success { get; }
    public string Message { get; }
    public string Provider { get; }
    public string? Authorization { get; }
    public string? SetupIntentClientSecret { get; }
    public string? PaymentStatus { get; }
    public string? AvsResultCode { get; }
    public string? CvvResultCode { get; }
    public string? CvvResultMessage { get; }
    public Dictionary<string, object?> Properties { get; }

    public PaymentGatewayResponse(
        bool success,
        string message,
        string provider,
        string? authorization = null,
        string? setupIntentClientSecret = null,
        string? paymentStatus = null,
        Dictionary<string, object?>? properties = null,
        string? avsResultCode = null,
        string? cvvResultCode = null,
        string? cvvResultMessage = null);
}
```

### 4.10 `IGatewayRegistry`

```csharp
namespace Module.Payment.Domain.Gateways;

public interface IGatewayRegistry
{
    Result<IPaymentGatewayActionProvider> GetGateway(string providerKey);
    bool IsRegistered(string providerKey);
    IReadOnlyCollection<string> RegisteredProviders { get; }
}
```

### 4.11 `IWebhookHandler`

```csharp
namespace Module.Payment.Domain.Gateways;

public interface IWebhookHandler
{
    string Provider { get; }
    string[] SupportedEventTypes { get; }
    Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default);
}
```

### 4.12 `GatewayConstants`

```csharp
namespace Module.Payment.Domain.Gateways;

public static class GatewayConstants
{
    public static class Constraints
    {
        public const int MaxProviderKeyLength = 50;
        public const int MaxWebhookUrlLength = 500;
        public const int MaxWebhookSecretLength = 500;
        public const int MaxPaymentNumberLength = 50;
        public const int MaxResponseCodeLength = 255;
        public const int MaxAvsResponseLength = 255;
        public const int MaxCvvCodeLength = 10;
        public const int MaxCvvMessageLength = 255;
        public const int MaxSourceTypeLength = 100;
        public const int MaxIntentClientSecretLength = 500;
        public const int MaxSettingsEncryptionKeyLength = 32;
        public const int Precision = 18;
        public const int Scale = 2;
    }

    public static class Providers
    {
        public const string Stripe = "stripe";
        public const string Bogus = "bogus";
    }

    public static class Currency
    {
        public const string Usd = "USD";
    }

    public static class Amount
    {
        public const long CentsMultiplier = 100;
    }

    public static class PaymentNumber
    {
        public const string Prefix = "PAY-";
        public const string DateFormat = "yyyyMMdd";
        public const string Format = $"{Prefix}{{{DateFormat}}}-";
    }

    public static class Idempotency
    {
        public const string Prefix = "shop-";
        public static string ForPayment(string paymentNumber) => $"{Prefix}{paymentNumber}";
    }

    public static class Metadata
    {
        public const string OrderIdKey = "order_id";
        public const string PaymentIdKey = "payment_id";
        public const string PaymentMethodIdKey = "payment_method_id";
    }

    public static class Configuration
    {
        public const string SectionName = "GatewayProviders";
        public const string SettingsEncryptionKey = "SettingsEncryptionKey";
    }

    public static class Stripe
    {
        public static class ConfirmationMethod
        {
            public const string Manual = "manual";
        }

        public static class CaptureMethod
        {
            public const string Automatic = "automatic";
            public const string Manual = "manual";
        }

        public static class IntentStatus
        {
            public const string Succeeded = "succeeded";
            public const string RequiresCapture = "requires_capture";
        }
    }

    public static class Bogus
    {
        public static class TestCards
        {
            public const string Success = "4242424242424242";
            public const string Declined = "4000000000000002";
            public const string InsufficientFunds = "4000000000009995";
        }

        public const string SetupIntentSecretPrefix = "pi_setup_fake_";
    }

    public static class Webhook
    {
        public static class Headers
        {
            public const string StripeSignature = "Stripe-Signature";
        }

        public static class Messages
        {
            public const string MissingSignature = "Missing Stripe-Signature header.";
            public const string InvalidSignature = "Invalid Stripe webhook signature.";
            public const string InvalidPayload = "Invalid Stripe webhook payload.";
        }
    }

    public static class WebhookEvents
    {
        public static class Stripe
        {
            public const string PaymentIntentSucceeded = "payment_intent.succeeded";
            public const string PaymentIntentPaymentFailed = "payment_intent.payment_failed";
            public const string ChargeRefunded = "charge.refunded";
            public const string ChargeDisputeCreated = "charge.dispute.created";
        }
    }

    public static class ResponseMessages
    {
        public const string PaymentCaptured = "Payment captured.";
        public const string Authorized = "Authorized.";
        public const string Captured = "Captured.";
        public const string Voided = "Voided.";
        public const string Refunded = "Refunded.";
    }

    public static class ErrorCodes
    {
        public static class Stripe
        {
            public const string CaptureMissingIntent = "Stripe.Capture.MissingIntent";
            public const string CreditMissingIntent = "Stripe.Credit.MissingIntent";
            public const string CancelMissingIntent = "Stripe.Cancel.MissingIntent";
            public const string UnknownError = "Stripe.UnknownError";
        }

        public static class Bogus
        {
            public const string CardDeclined = "Bogus.CardDeclined";
            public const string InsufficientFunds = "Bogus.InsufficientFunds";
            public const string UnknownCard = "Bogus.UnknownCard";
        }
    }
}
```

### 4.13 `PaymentMethod` (updated)

```csharp
namespace Module.Payment.Domain.PaymentMethods;

public sealed partial class PaymentMethod : Entity, IAuditable, IParameterizable, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public bool AutoCapture { get; set; } = false;
    public DisplayOn DisplayOn { get; set; } = DisplayOn.Both;
    public int Position { get; set; } = 0;
    public string? Presentation { get; set; }

    /// <summary>Non-sensitive behavioral config. Stored as plain JSONB.</summary>
    public Dictionary<string, string> Preferences { get; set; } = [];

    /// <summary>Provider-specific settings (e.g., merchant ID, endpoint override). Encrypted at rest.</summary>
    public Dictionary<string, string> Settings { get; set; } = [];

    /// <summary>Whether this payment method accepts webhooks. Secrets come from GatewayProviders config.</summary>
    public bool WebhookEnabled { get; set; }
}

public enum DisplayOn { Both, Frontend, Backend }
```

### 4.14 `PaymentRecord` (updated)

```csharp
namespace Module.Payment.Domain.Payments;

public sealed partial class PaymentRecord : Entity, IAuditable
{
    public string Number { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentRecordState State { get; set; } = PaymentRecordState.Checkout;
    public string? ResponseCode { get; set; }
    public string? IntentClientSecret { get; set; }
    public bool CaptureEventCreated { get; set; }
    public decimal RefundedAmount { get; set; }

    public string? AvsResponse { get; set; }
    public string? CvvResponseCode { get; set; }
    public string? CvvResponseMessage { get; set; }

    public Guid PaymentMethodId { get; set; }
    public Guid OrderId { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }

    /// <summary>Snapshot of ProviderKey used at transaction time.</summary>
    public string ProviderKey { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; } = null!;
}
```

### 4.15 `PaymentMethodConfiguration` (updated)

```csharp
builder.ToTable(PaymentSchema.TableNames.PaymentMethods, PaymentSchema.Name);
builder.HasKey(x => x.Id);
builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
builder.Property(x => x.Code).HasMaxLength(50);
builder.Property(x => x.Description).HasMaxLength(1000);
builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(GatewayConstants.Constraints.MaxProviderKeyLength);
builder.Property(x => x.Active).IsRequired().HasDefaultValue(true);
builder.Property(x => x.AutoCapture).IsRequired().HasDefaultValue(false);
builder.Property(x => x.DisplayOn).IsRequired().HasConversion<string>().HasDefaultValue(DisplayOn.Both);
builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);
builder.Property(x => x.Presentation);
builder.Property(x => x.WebhookEnabled).HasDefaultValue(false);

// Preferences: plain JSONB (existing)
builder.Property(x => x.Preferences)
    .HasConversion<DictionaryValueConverter<string, string>>()
    .HasColumnType("jsonb");

// Settings: encrypted JSONB (new)
builder.Property(x => x.Settings)
    .HasConversion<EncryptedDictionaryConverter>()
    .HasColumnType("jsonb");
```

### 4.16 `PaymentRecordConfiguration` (updated)

```csharp
builder.ToTable(PaymentSchema.TableNames.PaymentRecords, PaymentSchema.Name);
builder.HasKey(x => x.Id);
builder.Property(x => x.Number).IsRequired().HasMaxLength(GatewayConstants.Constraints.MaxPaymentNumberLength);
builder.Property(x => x.Amount).HasPrecision(GatewayConstants.Constraints.Precision, GatewayConstants.Constraints.Scale);
builder.Property(x => x.State).IsRequired().HasConversion<string>().HasDefaultValue(PaymentRecordState.Checkout);
builder.Property(x => x.ResponseCode).HasMaxLength(GatewayConstants.Constraints.MaxResponseCodeLength);
builder.Property(x => x.AvsResponse).HasMaxLength(GatewayConstants.Constraints.MaxAvsResponseLength);
builder.Property(x => x.CvvResponseCode).HasMaxLength(GatewayConstants.Constraints.MaxCvvCodeLength);
builder.Property(x => x.CvvResponseMessage).HasMaxLength(GatewayConstants.Constraints.MaxCvvMessageLength);
builder.Property(x => x.IntentClientSecret).HasMaxLength(GatewayConstants.Constraints.MaxIntentClientSecretLength);
builder.Property(x => x.CaptureEventCreated);
builder.Property(x => x.RefundedAmount).HasPrecision(GatewayConstants.Constraints.Precision, GatewayConstants.Constraints.Scale);
builder.Property(x => x.PaymentMethodId);
builder.Property(x => x.OrderId);
builder.Property(x => x.SourceId);
builder.Property(x => x.SourceType).HasMaxLength(GatewayConstants.Constraints.MaxSourceTypeLength);
builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(GatewayConstants.Constraints.MaxProviderKeyLength);

builder.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
builder.HasOne(x => x.PaymentMethod).WithMany(pm => pm.Payments).HasForeignKey(x => x.PaymentMethodId).OnDelete(DeleteBehavior.SetNull);
```

### 4.17 `StripeOptions` (updated — reads from GatewayProviders)

```csharp
namespace Module.Payment.Infrastructure.Gateways.Stripe;

/// <summary>
/// Binds to GatewayProviders:stripe section.
/// SecretKey, WebhookSecret, PublishableKey come from deployment config.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = $"{GatewayConstants.Configuration.SectionName}:{GatewayConstants.Providers.Stripe}";

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
```

### 4.18 `BogusOptions` (updated — reads from GatewayProviders)

```csharp
namespace Module.Payment.Infrastructure.Gateways.Bogus;

public sealed class BogusOptions
{
    public const string SectionName = $"{GatewayConstants.Configuration.SectionName}:{GatewayConstants.Providers.Bogus}";

    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? PublishableKey { get; set; }
}
```

### 4.19 `StripeGateway` (updated — per-request ApiKey)

```csharp
namespace Module.Payment.Infrastructure.Gateways.Stripe;

public sealed class StripeGateway : Gateway
{
    private readonly IOptions<StripeOptions> _options;

    public override string ProviderKey => GatewayConstants.Providers.Stripe;
    public override bool AutoCapture => true;
    public override bool SourceRequired => true;
    public override bool Supports(object? source) => source is string;

    public StripeGateway(IOptions<StripeOptions> options)
    {
        _options = options;
        // NEVER: StripeConfiguration.ApiKey = _options.Value.SecretKey;
    }

    private RequestOptions BuildRequestOptions(GatewayOptions opt) => new()
    {
        ApiKey = _options.Value.SecretKey,
        IdempotencyKey = opt.IdempotencyKey
    };

    public override async Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: true);
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
            var status = intent.Status == GatewayConstants.Stripe.IntentStatus.Succeeded
                ? GatewayConstants.ResponseMessages.PaymentCaptured
                : $"Status: {intent.Status}";
            return new PaymentGatewayResponse(true, status, GatewayConstants.Providers.Stripe,
                authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    // ... other methods follow same pattern
}
```

### 4.20 Gateway Result Classes (using `GatewayConstants.ErrorCodes`)

```csharp
// StripeGateway.Result.cs
public static class StripeGatewayResult
{
    public static class Errors
    {
        public static Error CaptureMissingIntent => Error.Validation(
            GatewayConstants.ErrorCodes.Stripe.CaptureMissingIntent, "PaymentIntent ID required.");
        public static Error CreditMissingIntent => Error.Validation(
            GatewayConstants.ErrorCodes.Stripe.CreditMissingIntent, "PaymentIntent ID required.");
        public static Error CancelMissingIntent => Error.Validation(
            GatewayConstants.ErrorCodes.Stripe.CancelMissingIntent, "PaymentIntent ID required.");
    }
}

// BogusGateway.Result.cs
public static class BogusGatewayResult
{
    public static class Errors
    {
        public static Error CardDeclined => Error.BadRequest(
            GatewayConstants.ErrorCodes.Bogus.CardDeclined, "Card was declined by issuer.");
        public static Error InsufficientFunds => Error.BadRequest(
            GatewayConstants.ErrorCodes.Bogus.InsufficientFunds, "Insufficient funds on the card.");
        public static Error UnknownCard => Error.BadRequest(
            GatewayConstants.ErrorCodes.Bogus.UnknownCard, "Unknown test card number.");
    }
}
```

## 5. Acceptance Criteria

- **AC-001**: Given `ProviderKey = GatewayConstants.Providers.Stripe`, When `IGatewayRegistry.GetGateway(key)` is called, Then returns `Result.Ok(StripeGateway)`.
- **AC-002**: Given `ProviderKey = GatewayConstants.Providers.Bogus`, When `IGatewayRegistry.GetGateway(key)` is called, Then returns `Result.Ok(BogusGateway)`.
- **AC-003**: Given an unregistered `ProviderKey`, When `GetGateway(key)` is called, Then returns `Result.Failure(Error.NotFound)`.
- **AC-004**: Given any gateway method fails, When called, Then a `Result` with typed `Error` is returned and no exception is thrown.
- **AC-005**: Given `CreateSetupIntentAsync` on `StripeGateway`, When called, Then `SetupIntentService.CreateAsync` is invoked and response has non-null `SetupIntentClientSecret`.
- **AC-006**: Given `CreateSetupIntentAsync` on `BogusGateway`, When called, Then response has `SetupIntentClientSecret` prefixed with `GatewayConstants.Bogus.SetupIntentSecretPrefix`.
- **AC-007**: Given `StripeGateway` executes any method, Then `StripeConfiguration.ApiKey` is never set; `RequestOptions.ApiKey` is used per call.
- **AC-008**: Given `PaymentMethod` with `WebhookEnabled = true` and `GatewayProviders:stripe:WebhookSecret` is null/empty, When application starts, Then critical log warning emitted.
- **AC-009**: Given `GatewayOptions` is constructed, Then zero references to `PaymentRecord` or `Order` types exist.
- **AC-010**: Given webhook payload arrives, Then correct `IWebhookHandler` is resolved by provider and processes event.
- **AC-011**: Given `PaymentRecord` needs `Order` data, Then loaded via MediatR (no nav property on PaymentRecord).
- **AC-012**: Given `CancelOrder` in Ordering module, Then `VoidOrderPaymentsCommand` sent via `ISender`.
- **AC-013**: Given `CreatePaymentIntent` handler, Then calls `IGatewayRegistry.GetGateway(payment.ProviderKey)`.
- **AC-014**: Given `PaymentRecordConfiguration`, Then `CaptureEventCreated`, `RefundedAmount`, `ProviderKey` are mapped.
- **AC-015**: Given `PaymentMethodConfiguration`, Then `ProviderKey`, `WebhookEnabled`, `Settings` (encrypted), `Preferences` (plain) are mapped; `ProviderType` is not mapped.
- **AC-016**: Given `PaymentGatewayResponse` constructor, Then parameter is named `properties` not `parmas`.
- **AC-017**: Given any Payment module file, Then no hardcoded string duplicates a `GatewayConstants` member.
- **AC-018**: Given `PaymentMethod.Settings` contains `{"merchant_id": "acct_123"}`, When persisted, Then DB column value is not human-readable (Base64 ciphertext).
- **AC-019**: Given encrypted DB column value, When loaded by EF Core, Then `PaymentMethod.Settings` is decrypted to the original `Dictionary<string, string>`.
- **AC-020**: Given `GatewayProviders:SettingsEncryptionKey` is missing or < 32 chars, When `AesEncryptionService` is constructed, Then `InvalidOperationException` is thrown at startup.
- **AC-021**: Given `EncryptedDictionaryConverter.Configure()` is not called at startup, Then first read/write throws `InvalidOperationException` at runtime.
- **AC-022**: Given `PaymentMethod.Settings` decrypted, When passed to gateway, Then gateway can read provider-specific keys (e.g., Stripe can read `"stripe_account"` for Connect).
- **AC-023**: Given `BogusGateway`, When `Settings` dictionary is empty, Then no decryption errors occur — empty dict is valid.

## 6. Test Automation Strategy

### Test Levels
- **Unit**: Each gateway method tested with mocked HTTP (Stripe) / hardcoded responses (Bogus). `IEncryptionService` tested for roundtrip (Encrypt → Decrypt). `EncryptedDictionaryConverter` tested for Null/Empty/Populated dictionary roundtrip. `IGatewayRegistry` tested for found/not-found branches. Validators tested with valid/invalid input.
- **Integration**: `EncryptedDictionaryConverter` verified with real `IEncryptionService` and DB. Webhook handler routing verified with sample payloads.
- **End-to-End**: `CreateIntent` → Confirm → Webhook → state transition.

### Frameworks
- MSTest + FluentAssertions + Moq (existing pattern in `Module.UnitTests`)

### Test Data Management
- `BogusGateway` provides deterministic test card numbers from `GatewayConstants.Bogus.TestCards`.
- `StripeGateway` integration tests use Stripe test key from environment variable (never committed).
- Encryption tests use a known 32-char test key.

### CI/CD Integration
- Unit tests: `dotnet test service/Api/tests/Module.UnitTests` (no Docker required).
- Shared unit tests: `dotnet test service/Api/tests/Shared.UnitTests` for `IEncryptionService`.
- Integration tests: `dotnet test` with `Category=Integration` (requires Docker).

### Coverage Requirements
- `IEncryptionService` / `AesEncryptionService`: 100% coverage (Encrypt, Decrypt, roundtrip, invalid input).
- `EncryptedDictionaryConverter`: 100% branch coverage (empty, populated, null-equivalent).
- `IPaymentGatewayActionProvider`: 100% of interface methods across both Stripe and Bogus.
- `IGatewayRegistry`: 100% branch coverage.

## 7. Rationale & Context

**Encrypted Settings vs. AppSettings:**
`appsettings.json` holds deployment-level secrets (Stripe API keys, webhook secrets) that differ per environment and should never be stored in the database. `PaymentMethod.Settings` holds per-payment-method admin-managed values (e.g., a specific Stripe Connect account ID, a custom PayPal merchant ID, an endpoint URL override). These are provider configuration an admin sets per payment method, and they must be encrypted at rest because they could include sensitive identifiers (though not auth secrets). Using AES-256-CBC via the existing `EncryptionHelper` provides consistent encryption across the codebase.

**Why not IDataProtector:**
The codebase already has a working `EncryptionHelper` for AES-CBC stream encryption. Adding a simple string wrapper avoids introducing a new dependency. The encryption key comes from configuration, giving ops full control over key management.

**Separation from Preferences:**
`Preferences` is existing plain-text JSONB for non-sensitive behavioral config (`tracking_url`, feature toggles). `Settings` is new and encrypted. Keeping them separate avoids accidentally exposing secrets when Preferences are logged or serialized to API responses.

**Static Converter Factory:**
EF Core `ValueConverter` instances are created per property mapping and don't support DI. The `EncryptedDictionaryConverter.Configure()` static method pattern is a pragmatic approach: set the factory at startup, and all converter instances share it.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe API — payment processing, setup intents, webhooks. Production/test mode via secret key.

### Third-Party Services
- **SVC-001**: Stripe.net SDK — Stripe API client. No version change required.

### Infrastructure Dependencies
- **INF-001**: PostgreSQL — schema migration adds `ProviderKey`, `WebhookEnabled`, `Settings` (JSONB, encrypted content) columns; renames `ProviderType` to `ProviderKey`.
- **INF-002**: Encryption key from `GatewayProviders:SettingsEncryptionKey` — must be provisioned per environment.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 — no change.
- **PLT-002**: MediatR — cross-module communication. Already in use.
- **PLT-003**: `System.Security.Cryptography` — for AES. Already available in .NET runtime.
- **PLT-004**: `System.Text.Json` — for JSONB serialization in `EncryptedDictionaryConverter`. Already available.

## 9. Examples & Edge Cases

### 9.1 DI Registration in `Payment.Extension.cs`

```csharp
public static WebApplicationBuilder AddPaymentModule(this WebApplicationBuilder builder)
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    // Bind GatewayProviders config section
    services.Configure<GatewayProvidersOptions>(
        configuration.GetSection(GatewayConstants.Configuration.SectionName));

    // Per-provider options
    services.Configure<StripeOptions>(
        configuration.GetSection(StripeOptions.SectionName));
    services.Configure<BogusOptions>(
        configuration.GetSection(BogusOptions.SectionName));

    // Encryption service
    services.AddSingleton<IEncryptionService, AesEncryptionService>();

    // Configure the EF Core value converter with the encryption service
    EncryptedDictionaryConverter.Configure(() =>
        builder.Services.BuildServiceProvider().GetRequiredService<IEncryptionService>());

    // Gateway implementations
    services.AddScoped<StripeGateway>();
    services.AddScoped<BogusGateway>();

    // Registry
    services.AddSingleton<IGatewayRegistry>(sp =>
    {
        var registry = new GatewayRegistry();
        var stripeOpts = sp.GetRequiredService<IOptions<StripeOptions>>();
        if (stripeOpts.Value.Enabled)
            registry.Register(GatewayConstants.Providers.Stripe, sp.GetRequiredService<StripeGateway>);

        var bogusOpts = sp.GetRequiredService<IOptions<BogusOptions>>();
        if (bogusOpts.Value.Enabled)
            registry.Register(GatewayConstants.Providers.Bogus, sp.GetRequiredService<BogusGateway>);

        return registry;
    });

    // Webhook handlers
    services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();
    builder.AddSeeder<PaymentMethodSeeder>();
    return builder;
}
```

### 9.2 StripeGateway — Per-Request ApiKey (no global state)

```csharp
public sealed class StripeGateway : Gateway
{
    private readonly StripeOptions _options;

    public StripeGateway(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        // REMOVED: StripeConfiguration.ApiKey = _options.SecretKey;
    }

    private RequestOptions BuildRequestOptions(GatewayOptions opt) => new()
    {
        ApiKey = _options.SecretKey,
        IdempotencyKey = opt.IdempotencyKey
    };

    public override async Task<Result<PaymentGatewayResponse>> PurchaseAsync(
        decimal amount, object? source, GatewayOptions options, CancellationToken ct)
    {
        try
        {
            var po = CreatePaymentIntentOptions(amount, source, options, autoCapture: true);
            var ro = BuildRequestOptions(options);
            var intent = await new PaymentIntentService().CreateAsync(po, ro, ct).ConfigureAwait(false);
            var succeeded = intent.Status == GatewayConstants.Stripe.IntentStatus.Succeeded;
            return new PaymentGatewayResponse(
                succeeded,
                succeeded ? GatewayConstants.ResponseMessages.PaymentCaptured : status,
                GatewayConstants.Providers.Stripe,
                authorization: intent.Id);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    public override async Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
        string? customerId, Dictionary<string, string>? metadata, CancellationToken ct)
    {
        try
        {
            var options = new SetupIntentCreateOptions { Metadata = metadata };
            var ro = new RequestOptions { ApiKey = _options.SecretKey };
            var intent = await new SetupIntentService().CreateAsync(options, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(
                true, "Setup intent created.", GatewayConstants.Providers.Stripe,
                setupIntentClientSecret: intent.ClientSecret);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }
}
```

### 9.3 Gateway Resolution with Settings

```csharp
public async Task<Result<Response>> Handle(Command command, CancellationToken ct)
{
    var paymentMethod = await dbContext.Set<PaymentMethod>()
        .FirstOrDefaultAsync(pm => pm.Id == payment.PaymentMethodId, ct);

    var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
    if (gatewayResult.IsFailure)
        return PaymentResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);

    var gateway = gatewayResult.Value;

    // Merge payment method Settings into ProviderSpecific for the gateway
    var options = new GatewayOptions { /* ... */ };
    if (paymentMethod.Settings.Count > 0)
    {
        options = options with
        {
            ProviderSpecific = paymentMethod.Settings
                .ToDictionary(kvp => (object?)kvp.Key, kvp => (object?)kvp.Value)
        };
    }

    var result = await payment.ProcessAsync(gateway, options, ct);
    // ...
}
```

### 9.4 BogusGateway SetupIntent

```csharp
public override Task<Result<PaymentGatewayResponse>> CreateSetupIntentAsync(
    string? customerId, Dictionary<string, string>? metadata, CancellationToken ct)
{
    var response = new PaymentGatewayResponse(
        success: true,
        message: "Bogus setup intent created.",
        provider: GatewayConstants.Providers.Bogus,
        setupIntentClientSecret: $"{GatewayConstants.Bogus.SetupIntentSecretPrefix}{Guid.NewGuid():N}");
    return Task.FromResult(Result.Ok(response));
}
```

### 9.5 PaymentMethod with Encrypted Settings (Usage in Admin Create/Update)

```csharp
// Admin creates a payment method with Stripe Connect account
public static Result<PaymentMethod> Create(
    string name, string? code, string providerKey,
    Dictionary<string, string>? settings = null,
    Dictionary<string, string>? preferences = null)
{
    var method = new PaymentMethod
    {
        Id = Guid.NewGuid(),
        Name = name,
        Code = code,
        ProviderKey = providerKey,  // validated against IGatewayRegistry
        Active = true,
        Settings = settings ?? [],
        Preferences = preferences ?? [],
        CreatedAtUtc = DateTimeOffset.UtcNow,
        CreatedBy = ICurrentUser
    };
    return method;
}
```

### 9.6 Edge Cases

| Case | Expected Behavior |
|------|------------------|
| `ProviderKey` unknown | `IGatewayRegistry.GetGateway` returns `Error.NotFound` |
| Webhook payload has no matching `IWebhookHandler.Provider` | Endpoint returns 404 |
| `StripeOptions.SecretKey` not configured | `OptionsValidationException` at startup (fail-fast) |
| `GatewayProviders:SettingsEncryptionKey` missing | `AesEncryptionService` throws at construction |
| `EncryptedDictionaryConverter` used before `Configure()` | Throws `InvalidOperationException` |
| `PaymentMethod.Settings` is empty `{}` | Encrypted and stored as ciphertext; decrypt returns empty dict |
| `PaymentMethod.Settings` was previously null in DB (legacy) | `EncryptedDictionaryConverter` decrypt returns empty dict for blank DB value |
| `BogusGateway` settings contain unknown keys | Ignored by Bogus (ProviderSpecific passed through) |
| `CreateSetupIntentAsync` called with null `customerId` on Stripe | Passes null to Stripe (guest setup intents accepted) |
| `PaymentRecord.ProviderKey` ≠ `PaymentMethod.ProviderKey` | Use `PaymentRecord.ProviderKey` (operational snapshot) |

## 10. Validation Criteria

- All existing unit tests pass with zero regressions after refactoring.
- `dotnet build` succeeds with zero warnings (`TreatWarningsAsErrors=true`).
- All existing integration tests pass.
- `IEncryptionService.Encrypt(plaintext)` followed by `Decrypt(ciphertext)` returns original plaintext.
- `EncryptedDictionaryConverter` roundtrip: `dict → encrypt → store → read → decrypt → dict` preserves all key/value pairs.
- `IGatewayRegistry` resolves correct provider for every `ProviderKey` in seed data.
- Zero occurrences of `StripeConfiguration.ApiKey` assignment in codebase (grep-verified).
- Payment module has zero `using Module.Ordering.*` directives.
- Ordering module has zero `using Module.Payment.*` directives.
- `PaymentGatewayResponse` has no `Params` or `Options` properties.
- `PaymentMethod` has `ProviderKey` not `ProviderType`; `Settings` (encrypted) and `Preferences` (plain) dictionaries; `WebhookEnabled` flag; no `WebhookUrl`/`WebhookSecret` properties.
- `PaymentRecordConfiguration` maps `CaptureEventCreated`, `RefundedAmount`, `ProviderKey`.
- `PaymentMethodConfiguration` maps `ProviderKey`, `WebhookEnabled`, `Settings` (encrypted), `Preferences` (plain); does not map `ProviderType`, `WebhookUrl`, `WebhookSecret`.
- `GatewayOptions` constructor accepts zero domain entity types.
- All five feature handlers (`CreatePaymentIntent`, `ConfirmPayment`, `CapturePayment`, `VoidPayment`, `RefundPayment`) inject `IGatewayRegistry` instead of `IPaymentGatewayActionProvider`.
- `CancelOrder`/`CancelOrderAdmin` (Ordering) send MediatR commands.
- No hardcoded string literal in gateway code duplicates a `GatewayConstants` value.
- `StripeGateway` takes `IOptions<StripeOptions>` from `GatewayProviders:stripe` section.
- `BogusGateway` takes `IOptions<BogusOptions>` from `GatewayProviders:bogus` section.
- `appsettings.json` has `GatewayProviders` section with `SettingsEncryptionKey`, and per-provider sub-sections.

## 11. Related Specifications / Further Reading

- `docs/superpowers/plans/2026-07-11-mvp-hardening-03-payment.md` — existing payment hardening plan (12 tasks)
- `service/Api/src/Module/Payment/README.yaml` — module readme with current architecture and anti-patterns
- `service/Api/src/Shared/Operational/Storages/Helpers/EncryptionHelper.cs` — existing AES-CBC encryption helper (reused)
- `.harness/principles.yml` — golden principles including module isolation (PRN-003, AP-3)
- `docs/security/secret-rotation.md` — secret management documentation
- Stripe SDK: https://stripe.com/docs/api/payment_intents
