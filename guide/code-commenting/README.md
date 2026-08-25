# Code Commenting Standard

> **Version 3.0** · Language-agnostic · CC-BY-4.0
>
> A structured, machine-parseable commenting system for teams and AI coding agents.
> Backed by Clean Code (Martin 2008), MIT CommLab, Conventional Comments, TSDoc,
> the ETH Zurich AGENTbench study (2026), and the Semantic Density Principle (arXiv:2604.07502).

---

## Table of Contents

1. [Philosophy](#1-philosophy)
2. [Formatting Conventions](#2-formatting-conventions)
3. [Label Decision Tree](#3-label-decision-tree)
4. [Label Categories](#4-label-categories)
   - [CAT-1 Validation and Checks](#cat-1-validation-and-checks)
   - [CAT-2 Object Operations](#cat-2-object-operations)
   - [CAT-3 Processing Logic](#cat-3-processing-logic)
   - [CAT-4 Events and Business Rules](#cat-4-events-and-business-rules)
   - [CAT-5 Flow Control and Coordination](#cat-5-flow-control-and-coordination)
   - [CAT-6 Resource Management](#cat-6-resource-management)
   - [CAT-7 Error Handling and Recovery](#cat-7-error-handling-and-recovery)
   - [CAT-8 Integration and Communication](#cat-8-integration-and-communication)
   - [CAT-9 Observability and Debugging](#cat-9-observability-and-debugging)
   - [CAT-10 AI and Agent Annotations ⭐ New in v3.0](#cat-10-ai-and-agent-annotations)
5. [Temporal Markers](#5-temporal-markers)
6. [Conventional Comments Extension](#6-conventional-comments-extension)
7. [Documentation Comment Standards](#7-documentation-comment-standards)
8. [Anti-Patterns](#8-anti-patterns)
9. [Best Practices Quick Reference](#9-best-practices-quick-reference)
10. [Context-Specific Guidelines](#10-context-specific-guidelines)
11. [Adoption Roadmap](#11-adoption-roadmap)
12. [References](#12-references)

---

## 1. Philosophy

Code is now read by **two audiences**: human developers and AI coding agents. This standard serves both.

| Principle | Source | Statement |
|-----------|--------|-----------|
| **P1** | Martin 2008 | Comments explain **WHY**, never **WHAT**. Self-documenting code is the primary mechanism. |
| **P2** | MIT CommLab | Four mechanisms, in order: **(1) naming → (2) structure → (3) context → (4) comments**. |
| **P3** | TechTarget 2024 | Most value: edge-case workarounds, performance trade-offs, historical context, complex business rules. |
| **P4** | VoiceType 2025 | When code is expressive enough, comments focus exclusively on reasoning behind decisions. |
| **P5** | Conventional Comments | Structured labels make intent clear, comments machine-parseable, and tooling integration possible. |
| **P6** ⭐ | arXiv 2026 | **Semantic Density Principle** — every comment token must earn its place. Verbosity hurts agents. |
| **P7** ⭐ | Osmani 2025 | Labels like `Contract:`, `Invariant:`, and `Assume:` are operational context for AI agents. |

---

## 2. Formatting Conventions

| Rule | Requirement |
|------|-------------|
| **F1** | Comments on their own line — never trailing a code statement (except inline data literals, see F6). |
| **F2** | Begin comment text with a **capitalised word** — treat it as a sentence. |
| **F3** | Max **100 characters** per line (80 for legacy codebases). |
| **F4** | Exactly **one space** between delimiter (`//`, `#`, `--`) and the label. |
| **F5** | Align comment with the **indentation** of the code block it describes. |
| **F6** | Inline alignment of trailing comments IS acceptable for data literals (RGB tuples, enum tables). |
| **F7** | Write in **English** by default. |
| **F8** | **One label, one action.** Never join two actions with "and". |
| **F9** ⭐ | For CAT-10 agent annotations use `KEY=VALUE` form for reliable machine parsing. |
| **F10** ⭐ | Use **imperative-mood verbs**: "Filter expired items" not "Expired item filtering". |

---

## 3. Label Decision Tree

```
Is this a PUBLIC API surface?
  YES → Use DocCommentStandards (TSDoc / XML-doc / GoDoc / Rustdoc / Javadoc)
  NO  ↓

Is this a time-sensitive or WIP marker?
  YES → Use TemporalMarker (TODO / FIXME / HACK / TEMP / DEADLINE / DEPRECATED / BREAKING / PERF)
  NO  ↓

Is this primarily for an AI/coding agent?
  YES → CAT-10 (Contract / Invariant / Assume / AgentHint / AgentSkip / Boundary / Context)
  NO  ↓

Validation or checking?              → CAT-1 (Validate / Check / Guard / Verify / Assert)
Creating, mutating, or deleting?     → CAT-2 (Create / Assign / Update / Add / Remove / Clone / Merge / Initialize / Reset)
Computation or transformation?       → CAT-3 (Compute / Transform / Parse / Format / Filter / Generate / Normalize / Aggregate / Sort / Explain)
Domain event or business rule?       → CAT-4 (Enforce / Raise / Trigger / Notify / Handle / Subscribe / Policy)
Async flow or rate control?          → CAT-5 (Await / Retry / Skip / Fallback / Batch / Throttle / Defer / Circuit)
Resource acquisition or cleanup?     → CAT-6 (Acquire / Release / Lock / Cache / Purge / Pool / Dispose)
Exception handling or rollback?      → CAT-7 (Catch / Recover / Compensate / Degrade / Escalate / Suppress)
External integration boundary?       → CAT-8 (Call / Send / Receive / Publish / Map / Serialize / Deserialize / Webhook)
Logging, tracing, or metrics?        → CAT-9 (Log / Trace / Monitor / Audit / Profile / Debug)
```

---

## 4. Label Categories

### CAT-1 Validation and Checks

> Preconditions, input validation, and conditional assertions. Use liberally around business-rule enforcement. For security-sensitive code, **always name the threat being mitigated**.

| Label | When to use |
|-------|-------------|
| `Validate:` | Input or data must meet a format or business-rule requirement. |
| `Check:` | Assert a condition before proceeding — null checks, existence, permissions. |
| `Guard:` | Defensive boundary check to prevent runtime errors. Name the threat. |
| `Verify:` | Confirm post-condition or system state after an operation. |
| `Assert:` | Development-time invariant; stripped or silent in production. |

```cs
// Validate: Email format matches RFC 5322 and domain whitelist
if (!Regex.IsMatch(email, pattern) || !IsAllowedDomain(email))
    throw new InvalidEmailException(email);

// Guard: Prevent SSRF by rejecting non-allowlisted hostnames
if (!ALLOWED_HOSTS.Contains(new Uri(target).Host))
    throw new ForbiddenHostError(target);

// Verify: Transaction committed successfully before releasing lock
Debug.Assert(transaction.Status == TransactionStatus.Committed);
```

```python
# Validate: Amount is positive and within single-transaction limit
if not (0 < amount <= MAX_TRANSACTION_AMOUNT):
    raise ValueError(f"Amount {amount} outside allowed range")
```

**Anti-pattern:**
```cs
// ❌ Check: check the order
// ✅ Check: Order total exceeds zero before payment gateway call
```

---

### CAT-2 Object Operations

> Object lifecycle — creation, mutation, deletion, and merging.

| Label | When to use |
|-------|-------------|
| `Create:` | Instantiate a new domain object or value type. |
| `Assign:` | Set a property value; include the business reason. |
| `Update:` | Modify an existing object's state. |
| `Add:` | Insert an element into a collection. |
| `Remove:` | Delete from a collection or mark for soft-delete. |
| `Clone:` | Produce an independent copy. Specify depth (deep/shallow). |
| `Merge:` | Combine data from multiple sources with defined conflict resolution. |
| `Initialize:` | Populate defaults or calculated values during object setup. |
| `Reset:` ⭐ | Return an object to a known default state without full re-creation. |

```cs
// Assign: Priority based on customer tier — premium orders fulfil first
order.Priority = customer.Tier == Tier.Premium ? Priority.High : Priority.Normal;

// Remove: Expired items from cart before checkout to prevent stale pricing
cart.Items.RemoveAll(item => item.ExpiresAt < DateTime.UtcNow);

// Clone: User preferences as deep copy for rollback snapshot
var backupPrefs = user.Preferences.DeepClone();
```

---

### CAT-3 Processing Logic

> Computations, data transformations, aggregations, and formatting. **Use verbs, not nouns.**

| Label | When to use |
|-------|-------------|
| `Compute:` | Derive a calculated value — totals, scores, rates. State the formula. |
| `Transform:` | Convert between representations. Name both sides. |
| `Parse:` | Extract structured data from raw text, bytes, or environment input. |
| `Format:` | Structure data for display, serialisation, or logging output. |
| `Filter:` | Remove unwanted elements based on a predicate. |
| `Generate:` | Produce derived data — IDs, hashes, tokens, reference codes. |
| `Normalize:` | Standardise format or encoding. |
| `Aggregate:` | Combine multiple values into a summary metric. |
| `Sort:` | Order data by a business-meaningful criterion. State the criterion. |
| `Explain:` ⭐ | Describe algorithmic or mathematical reasoning behind a non-obvious choice. |

```cs
// Compute: Order total inclusive of regional tax rate (subtotal × (1 + taxRate))
order.Total = order.Subtotal * (1 + taxRate);

// Transform: CustomerDto → Customer domain entity via AutoMapper profile
var customer = _mapper.Map<Customer>(customerDto);

// Filter: Active subscriptions only — excludes cancelled and trial-expired
var active = subscriptions.Where(s => s.IsActive).ToList();
```

```python
# Explain: Using insertion sort here because n is always <= 8 in practice;
#          it outperforms quicksort on tiny arrays due to cache locality.
insertion_sort(items)
```

---

### CAT-4 Events and Business Rules

> Domain events, policy enforcement, and workflow triggers. `Raise:` = internal domain event; `Trigger:` = external workflow.

| Label | When to use |
|-------|-------------|
| `Enforce:` | Apply a business invariant or policy that must not be violated. |
| `Raise:` | Emit an internal domain event for the current aggregate. |
| `Trigger:` | Initiate an external workflow, saga, or background process. |
| `Notify:` | Dispatch a notification to users, admins, or external systems. |
| `Handle:` | Process an inbound domain or integration event inside a handler. |
| `Subscribe:` | Register a listener for domain or integration events. |
| `Policy:` ⭐ | Reference a named business or compliance policy for grep-based audits. |

```cs
// Enforce: Minimum order amount policy (configured per region in appsettings)
if (order.Total < _settings.MinimumOrderAmount)
    throw new BusinessRuleViolationException(...);

// Policy: GDPR Article 17 — right to erasure; anonymise, do not delete
customer.Anonymize();

// Raise: InventoryReservedEvent so warehouse handler picks up line items
order.RaiseEvent(new InventoryReservedEvent(order.Items));
```

---

### CAT-5 Flow Control and Coordination

> Async operations, retry policies, short-circuits, batching. Essential for microservice codebases.

| Label | When to use |
|-------|-------------|
| `Await:` | Wait on async operation; document timeout or cancellation semantics. |
| `Retry:` | Re-attempt after transient failure with a defined policy. |
| `Skip:` | Bypass a code path under a well-defined condition. |
| `Fallback:` | Use an alternative when the primary fails. |
| `Batch:` | Group operations for efficiency or rate compliance. |
| `Throttle:` | Limit operation rate. State the limit explicitly. |
| `Defer:` | Postpone an operation — queue, scheduler, or hook. |
| `Continue:` | Explicitly proceed to next loop iteration for readability. |
| `Break:` | Exit a loop or pipeline early on a well-defined condition. |
| `Circuit:` ⭐ | Open a circuit breaker; name the threshold and state. |

```cs
// Await: External payment API — 30 s timeout, CancellationToken from caller
var response = await _httpClient.PostAsync(endpoint, payload, cts.Token);

// Retry: DB query on transient SQL exception — exponential backoff, max 3 attempts
var result = await _retryPolicy.ExecuteAsync(() => _db.QueryAsync(sql, parameters));

// Fallback: Serve cached pricing if catalogue service unavailable (stale < 15 min)
var prices = await _catalogue.GetPricesAsync() ?? _cache.GetPrices();

// Circuit: Open after 5 consecutive failures — 30 s recovery window
await _circuitBreaker.ExecuteAsync(() => _service.CallAsync(request));
```

---

### CAT-6 Resource Management

> Connections, locks, cache entries, and cleanup. Critical for preventing leaks in long-running services.

| Label | When to use |
|-------|-------------|
| `Acquire:` | Obtain a scarce resource — connection, lock, semaphore. |
| `Release:` | Explicitly free a resource outside using/try-finally. |
| `Lock:` | Establish exclusive access to a shared in-process resource. |
| `Cache:` | Store a computed value for fast retrieval. State TTL and invalidation strategy. |
| `Purge:` | Remove stale or invalid cache/store entries. |
| `Pool:` | Borrow or return from a managed resource pool. |
| `Dispose:` | Explicitly release managed and unmanaged resources. |

```cs
// Acquire: Distributed lock to prevent concurrent inventory mutation (TTL 2 min)
using var dLock = await _lockProvider.AcquireAsync($"inv-{sku}", TimeSpan.FromMinutes(2));

// Cache: Exchange rates valid 15 min — FX API rate-limit is 100 calls/h
_cache.Set(cacheKey, rates, TimeSpan.FromMinutes(15));

// Purge: Session tokens older than 24 h from Redis — session policy compliance
await _sessionStore.PurgeExpiredAsync(olderThan: TimeSpan.FromHours(24));
```

---

### CAT-7 Error Handling and Recovery

> Exception handling, graceful degradation, rollback. **Label each catch block individually.**

| Label | When to use |
|-------|-------------|
| `Catch:` | Handle a specific, named exception type with intention. |
| `Recover:` | Attempt to restore a valid system state after an error. |
| `Compensate:` | Roll back side-effects of a failed operation (saga compensation). |
| `Degrade:` | Reduce to a safe subset of functionality while core features continue. |
| `Escalate:` | Re-throw or forward an error to a higher-level handler. |
| `Suppress:` | Intentionally swallow a non-critical exception. **Always document why.** |

```cs
// Catch: Payment gateway timeout — queue for async retry via outbox
catch (PaymentTimeoutException ex)
{
    _logger.LogWarning("Timeout for order {Id}", order.Id);
    await _retryQueue.EnqueueAsync(order.Id);
}

// Compensate: Release reserved inventory on payment failure (saga step 3 rollback)
await _inventory.ReleaseReservationAsync(order.Items);

// Suppress: Optional telemetry failure must not block order flow (best-effort only)
catch (TelemetryException) { /* intentionally empty */ }
```

---

### CAT-8 Integration and Communication

> External API calls, message publishing, and data mapping. Always include **system name** and **contract version**.

| Label | When to use |
|-------|-------------|
| `Call:` | Invoke an external service or internal remote procedure. |
| `Send:` | Transmit a command or document to an external system. |
| `Receive:` | Ingest data arriving from an external source. |
| `Publish:` | Broadcast an event onto a message bus for fan-out consumers. |
| `Map:` | Convert between different data models at a boundary layer. |
| `Serialize:` | Convert to a transmittable or storable format. State the target format. |
| `Deserialize:` | Reconstruct a typed object from raw transmitted data. |
| `Webhook:` | Handle and validate an inbound webhook request. |

```cs
// Call: Identity provider (Okta) v2 API to validate bearer token
var identity = await _idp.ValidateTokenAsync(bearerToken);

// Publish: OrderStatusChangedEvent — consumed by notifications and analytics
await _bus.PublishAsync(new OrderStatusChangedEvent(order.Id, newStatus));

// Webhook: Verify HMAC-SHA256 signature before processing payload — prevents replay
if (!_webhookValidator.IsValid(req.Headers, req.Body))
    return Unauthorized("Invalid signature");
```

---

### CAT-9 Observability and Debugging

> Logging, distributed tracing, metrics, and audit. **Prefer structured logging** (key-value pairs) over string interpolation.

| Label | When to use |
|-------|-------------|
| `Log:` | Record a business event or diagnostic fact for troubleshooting. |
| `Trace:` | Open a distributed trace span for cross-service correlation. |
| `Monitor:` | Emit a metric counter, gauge, or histogram for dashboards. |
| `Audit:` | Record a tamper-evident entry for compliance or security review. |
| `Profile:` | Bracket a performance-sensitive section for measurement. |
| `Debug:` | Temporary diagnostic output — **must be removed before merging to main.** Use with `TEMP`. |

```cs
// Log: Order processed — structured fields for Kibana dashboard query
_logger.LogInformation(
    "Order {OrderId} processed for {CustomerId} — total {Total}",
    order.Id, order.CustomerId, order.Total);

// Audit: PCI-DSS 3.3 compliant log — card data never written to disk
_audit.Log(new AuditEntry { UserId = order.CustomerId, Action = "OrderPayment", ... });

// Profile: Cross-region pricing query — P99 target < 200 ms
using var timer = _profiler.StartTimer("pricing-query");
```

---

### CAT-10 AI and Agent Annotations

> **New in v3.0.** Structured annotations consumed by AI coding agents, static analysers, and formal verification tools. Based on the **Semantic Density Principle** and Design-by-Contract conventions.
>
> **Key finding:** An ETH Zurich AGENTbench study (2026) showed that human-curated, concise annotations outperform verbose LLM-generated context files. Every token must earn its place.

Use `KEY=VALUE` form so parsers can extract values without natural-language interpretation.

| Label | When to use | Format |
|-------|-------------|--------|
| `Contract:` | Formal pre/post-condition and exception contract at function entry. | `pre=COND, post=COND, throws=EX` |
| `Invariant:` | Property that must always be true for a class or data structure. | `CONDITION [; CONDITION]` |
| `Assume:` | Assumption the code makes that is NOT enforced with a guard. | `CONDITION — REASON` |
| `AgentHint:` | Explicit guidance to AI coding agents on how to edit or extend a block. | `INSTRUCTION [; do NOT: ANTIPATTERN]` |
| `AgentSkip:` | Block that AI agents MUST NOT auto-refactor or rewrite. | `REASON` |
| `Boundary:` | Architectural boundary agents must respect. Prevents cross-layer leaks. | `LAYER → LAYER — REASON` |
| `Context:` | Background context not in the code but needed for correct interpretation. | `DESCRIPTION — see REF` |

```cs
// Invariant: Total >= 0; Status is never null; Items.Count >= 1 when Status == Placed
public class Order { }

// Contract: pre=order!=null && order.Total>0, post=receipt.TransactionId!=null,
//           throws=PaymentTimeoutException|InsufficientFundsException
public async Task<PaymentReceipt> ProcessPaymentAsync(Order order) { }

// Boundary: Domain → Infrastructure — do not import EF Core types above this line
public interface IOrderRepository { }

// AgentHint: Add new payment methods to the switch below, never use a default branch;
//            do NOT inline currency conversion — call CurrencyService.ConvertAsync()
switch (method) { ... }

// AgentSkip: Hand-tuned SIMD vectorisation; auto-refactoring breaks alignment guarantees
unsafe void ProcessBatchSIMD(float* src, float* dst, int length) { }
```

```python
# Assume: input_df has no NaN values — caller guarantees upstream cleaning pipeline
def compute_percentiles(input_df: pd.DataFrame) -> pd.Series: ...

# Context: Implements ISO 4217 rounding rules for JPY (0 decimal places);
#          see ADR-042 and https://www.iso.org/standard/64758.html
rounded = round(amount, currency.decimal_places)
```

---

## 5. Temporal Markers

> Format: `MARKER(owner, ISO-date): reason — ticket reference`
> IDEs such as IntelliJ IDEA and VS Code aggregate these automatically.

| Marker | When to use | Example |
|--------|-------------|---------|
| `TODO` | Planned work not yet complete. Include owner and deadline. | `// TODO(jane.smith, 2026-06-01): Replace with PaymentService v2 — TICKET-1234` |
| `FIXME` | Known defect that needs correction before the next release. | `// FIXME(ops-team, 2026-04-15): Memory leak on scroll listener — TICKET-5678` |
| `HACK` | Temporary or suboptimal workaround that must be revisited. | `// HACK(platform, 2026-03-01): Workaround for rate-limit bug in SDK v3.1 — TICKET-9999` |
| `TEMP` | Short-lived code for a specific investigation or deploy window. | `// TEMP: Debug logging active — remove before merge` |
| `DEADLINE` | Hard expiry date attached to a code path or feature toggle. | `// DEADLINE(2026-06-15): Remove feature flag after full rollout — TICKET-9012` |
| `DEPRECATED` | API or method scheduled for removal. Align with `@deprecated` tags. | `// DEPRECATED(2026-01-01): Use OrderService.ProcessV2 instead` |
| `BREAKING` ⭐ | Change that breaks backward compatibility. | `// BREAKING(2026-07-01): Return type changes from string to UUID — bump major semver` |
| `PERF` ⭐ | Performance bottleneck with measured data. Include measurement and target. | `// PERF(john.doe, 2026-05-01): P99=340 ms — optimise before GA — TICKET-2345` |

---

## 6. Conventional Comments Extension

For **code-review comments**, the [Conventional Comments](https://conventionalcomments.org/) standard adds machine-parseable decoration qualifiers.

**Format:** `label (decorations?): subject [discussion?]`

**Review Labels:**

| Label | Meaning |
|-------|---------|
| `praise:` | Positive, constructive recognition. |
| `nitpick:` | Minor non-blocking style preference. |
| `suggestion:` | Concrete, actionable improvement. |
| `issue:` | Problem that **must** be addressed. |
| `question:` | Genuine request for clarification. |
| `thought:` | Exploratory idea without pressure. |
| `chore:` | Housekeeping unrelated to logic. |
| `security:` ⭐ | Security risk requiring immediate attention. |

**Decorations:**

| Decoration | Meaning |
|------------|---------|
| `non-blocking` | Does not block PR approval. |
| `blocking` | Must be resolved before merge. |
| `if-minor` | Fix only if change is small. |
| `security` | Has security implications. |
| `performance` | Has performance implications. |
| `agent-safe` ⭐ | Safe for AI agent auto-fix. |
| `agent-skip` ⭐ | Do NOT delegate to AI agent. |

**Examples:**
```
suggestion (security, blocking): Validate webhook signature before deserialising the body
— an unsigned payload could trigger unintended state changes.

issue (agent-skip, blocking): Hand-tuned SIMD path — do not delegate this refactor to an AI agent.

nitpick (non-blocking): Prefer Filter: label here for searchability.
```

---

## 7. Documentation Comment Standards

Public API surfaces — classes, interfaces, public methods — **MUST** carry machine-readable doc comments. Inline labels (CAT-1 to CAT-10) annotate implementation logic; doc-comment blocks annotate contracts consumed by tooling and IDEs.

### TypeScript — TSDoc

```typescript
/**
 * Calculates the arithmetic mean of two operands.
 *
 * @param x - First input number.
 * @param y - Second input number.
 * @returns The arithmetic mean of `x` and `y`.
 * @throws {@link RangeError} if either operand is NaN.
 *
 * @example
 * ```ts
 * getAverage(4, 6); // returns 5
 * ```
 */
const getAverage = (x: number, y: number): number => (x + y) / 2;
```

Required tags: `@param` `@returns` `@throws` `@remarks`

### C# — XML Documentation Comments

```cs
/// <summary>
/// Processes payment for a confirmed order.
/// </summary>
/// <param name="order">The order to charge. Must not be null.</param>
/// <returns>A payment receipt containing transaction ID.</returns>
/// <exception cref="PaymentTimeoutException">
/// Thrown when the gateway does not respond within 30 seconds.
/// </exception>
/// <exception cref="InsufficientFundsException">
/// Thrown when the customer's payment method is declined.
/// </exception>
public async Task<PaymentReceipt> ProcessPaymentAsync(Order order) { }
```

Required tags: `<summary>` `<param>` `<returns>` `<exception>`

### Python — Google Docstring Style

```python
def process_payment(order: Order) -> PaymentReceipt:
    """Process payment for a confirmed order.

    Args:
        order: The order entity to charge. Must have a positive total.

    Returns:
        PaymentReceipt containing the gateway transaction ID.

    Raises:
        PaymentTimeoutException: Gateway did not respond in 30 s.
        InsufficientFundsException: Payment method was declined.
    """
```

Required sections: `Args` `Returns` `Raises`

### Rust — Rustdoc

```rust
/// Processes payment for a confirmed order.
///
/// # Errors
///
/// Returns [`PaymentError::Timeout`] if the gateway does not respond within 30 s.
/// Returns [`PaymentError::Declined`] if the payment method is declined.
///
/// # Examples
///
/// ```rust
/// let receipt = process_payment(&order).await?;
/// assert!(!receipt.transaction_id.is_empty());
/// ```
pub async fn process_payment(order: &Order) -> Result<PaymentReceipt, PaymentError> { }
```

Required sections: `# Examples` `# Errors` (for `Result`-returning fns) `# Safety` (for `unsafe` fns)

### Go — GoDoc

```go
// ProcessPayment charges the provided order via the configured payment gateway.
// It returns a PaymentReceipt containing the transaction ID, or an error if
// the gateway times out (after 30 s) or the payment method is declined.
func ProcessPayment(ctx context.Context, order *Order) (*PaymentReceipt, error) { }
```

GoDoc extracts the first sentence as the summary. Document parameters and returns in prose.

### Java — Javadoc

```java
/**
 * Processes payment for a confirmed order.
 *
 * @param order the order to charge; must not be {@code null}
 * @return a {@link PaymentReceipt} containing the transaction ID
 * @throws PaymentTimeoutException if the gateway does not respond within 30 s
 * @throws InsufficientFundsException if the payment method is declined
 * @since 2.0
 */
public CompletableFuture<PaymentReceipt> processPayment(Order order) { }
```

Required tags: `@param` `@return` `@throws`

---

## 8. Anti-Patterns

| ID | Name | Bad | Good |
|----|------|-----|------|
| AP-1 | **Redundancy** | `// Assign: Set order ID to 123` | `// Assign: Order ID from payment gateway response` |
| AP-2 | **Vagueness** | `// Process: Handle the order` | `// Validate: Order meets minimum purchase and stock requirements` |
| AP-3 | **Over-commenting** | Comment on every trivial line | Comment only where naming and structure cannot carry the intent |
| AP-4 | **Inconsistent style** | Mixed casing, missing colons | Consistent `Label: Capitalised sentence.` throughout |
| AP-5 | **Stale comments** | Comment describes old behaviour the code has outgrown | Update comments immediately when refactoring |
| AP-6 | **Commented-out code** | Dead code left in comments | Use version control (git). Dead code is not documentation. |
| AP-7 ⭐ | **Verbose agent context** | Multi-line wall of text in `AgentHint:` | One constraint per line; concise and factual |
| AP-8 ⭐ | **Passive voice** | `// Filter: Expired sessions are removed` | `// Filter: Remove expired sessions from cache` |

### AP-7 Expanded: Verbose Agent Context

An ETH Zurich study (AGENTbench, 2026) found verbose, LLM-generated context annotations reduced agent success rates by ~3% and increased costs by 20%. Keep agent annotations surgical:

```python
# ❌ AgentHint: This is a complex function that processes orders. It first validates
#               the input, then processes data, then saves to the database. Be careful
#               when modifying it because it has many side effects...

# ✅ AgentHint: do NOT add DB calls here — use OrderRepository injected in __init__;
#              validation lives in OrderValidator, not here
```

---

## 9. Best Practices Quick Reference

| ID | Practice | Authority |
|----|----------|-----------|
| BP-1 | Comment the **WHY**, never the WHAT. | Martin 2008 |
| BP-2 | One label, one action — no "and". | Conventional Comments |
| BP-3 | Capitalise first word after the colon. | DEV Community 2024 |
| BP-4 | Align comment with its code block's indentation. | TDS Jan 2025 |
| BP-5 | Max 100 characters per comment line. | Google / Airbnb style guides |
| BP-6 | Use comments sparingly — naming and structure first. | MIT CommLab |
| BP-7 | Write the comment while writing the code — never retroactively. | daily.dev 2025 |
| BP-8 | Update comments immediately when refactoring. | Martin 2008 |
| BP-9 | Always comment external integration boundaries (Call / Send / Receive). | TechTarget 2024 |
| BP-10 | Use IDE-standard tags (TSDoc / XML-doc / GoDoc) for all public API surfaces. | VoiceType 2025 |
| BP-11 | Establish a team commenting vocabulary and enforce it in code review. | OpenReplay |
| BP-12 | Prefer verbs over nouns: "Filter expired sessions" not "Expired session filter". | TDS Jan 2025 |
| BP-13 ⭐ | **Semantic Density:** every comment token must earn its place. Remove filler words. | arXiv 2026 |
| BP-14 ⭐ | Prefer CAT-10 annotations for agent-consumed boundaries; keep them human-curated. | ETH AGENTbench 2026 |
| BP-15 ⭐ | When code will be edited by AI agents, add `AgentHint:` and `Boundary:` at decision points. | Osmani 2025 |
| BP-16 ⭐ | Use `Context:` to link external specs, ADRs, or tickets rather than embedding URLs arbitrarily. | Medium 2025 |

---

## 10. Context-Specific Guidelines

| Context | Guidance |
|---------|----------|
| **Complex Business Logic** | Use `Enforce:` and `Validate:` liberally. Add `Policy:` references to compliance frameworks. |
| **External Integration Points** | Pair `Call:`, `Send:`, `Receive:` with the system name and contract version. |
| **Performance-Critical Code** | Use `Cache:`, `Compute:`, `Profile:`. Include the measured P99. Add `PERF` marker for known issues. |
| **Error-Prone Operations** | Bracket with `Guard:`, `Catch:`, `Compensate:` to make failure modes explicit. |
| **Async and Concurrent Code** | Use `Await:`, `Lock:`, `Throttle:` consistently. Add `Circuit:` for resilience patterns. |
| **Security-Sensitive Code** | Always name the threat: "SQL injection", "SSRF", "replay attack". |
| **AI-Agent-Edited Code** ⭐ | Add `Contract:`, `Invariant:`, `Boundary:` at module entry points. Use `AgentSkip:` for hand-tuned sections. Keep concise (see AP-7). |
| **Library and SDK Code** ⭐ | Use `BREAKING` marker on any public surface change. All public APIs require doc-comment coverage. |

---

## 11. Adoption Roadmap

### Phase 1 — Foundation (Week 1–2)
- [ ] Agree on team commenting vocabulary; add to style guide.
- [ ] Create IDE snippets for most common labels (`Validate:`, `Guard:`, `Catch:`, `Call:`, `Log:`).
- [ ] Add label consistency check to code-review checklist.
- [ ] Apply to all new code and critical business-logic paths first.
- [ ] Identify code paths regularly edited by AI agents; add `Contract:` and `Boundary:` annotations.

### Phase 2 — Adoption (Month 1–2)
- [ ] Apply during routine refactoring — no big-bang rewrites.
- [ ] Include in PR review feedback as a non-blocking nitpick.
- [ ] Configure linting rules for `TODO`/`FIXME`/`BREAKING` format enforcement.
- [ ] Enable TSDoc, XML-doc, or GoDoc linting on public interfaces.
- [ ] Audit CAT-10 agent annotations for verbosity (AP-7) during PRs.

### Phase 3 — Optimisation (Month 2–3)
- [ ] Introduce grep / AST tooling to audit label usage across repos.
- [ ] Define domain-specific extension labels for your bounded context.
- [ ] Integrate doc-comment tags with API documentation generation pipeline.
- [ ] Measure adoption via code-review metrics and static analysis reports.
- [ ] Run agent-task success benchmarks (pre/post annotation) to validate CAT-10 quality.
- [ ] Configure Conventional Comments decorations (`agent-safe`, `agent-skip`) in your PR tooling.

---

## 12. References

| ID | Source | Relevance |
|----|--------|-----------|
| R1 | Martin, Robert C. *Clean Code* (2008) | Foundation: Chapter 4 defines good vs. bad comments. |
| R2 | [Conventional Comments](https://conventionalcomments.org/) | Machine-parseable label format for review comments. |
| R3 | [MIT CommLab](https://mitcommlab.mit.edu/broad/commkit/coding-and-comment-style/) | Four-mechanism hierarchy: naming → structure → context → comments. |
| R4 | [TechTarget 2024](https://www.techtarget.com/searchsoftwarequality/tip/Code-comment-best-practices-every-developer-should-know) | Five guiding principles; always comment integration boundaries. |
| R5 | [TSDoc](https://tsdoc.org/) | TypeScript documentation comment spec and parser. |
| R6 | [TypeScript JSDoc Reference](https://www.typescriptlang.org/docs/handbook/jsdoc-supported-types.html) | JSDoc subset supported by the TypeScript compiler. |
| R7 | [DEV Community 2024](https://dev.to/moh_moh701/c-clean-code-commenting-conventions-4abj) | C# XML-doc formatting conventions and anti-patterns. |
| R8 | [TDS Jan 2025](https://towardsdatascience.com/the-art-of-writing-efficient-code-comments-692213ed71b1/) | Verb-over-noun best practice; inline alignment for data literals. |
| R9 | [VoiceType 2025](https://voicetype.com/blog/code-commenting-best-practices) | Structured TODO format with owner, date, and ticket reference. |
| R10 | [daily.dev 2025](https://daily.dev/blog/10-code-commenting-best-practices-for-developers) | Write comments during development, not after; avoid redundancy. |
| R11 | [OpenReplay](https://blog.openreplay.com/dos-and-donts-of-commenting-code/) | Tag markers as structural annotations; vocabulary consistency. |
| R12 | [API Extractor](https://api-extractor.com/pages/tsdoc/doc_comment_syntax/) | TSDoc comment anatomy — summary, @remarks, @param, @returns. |
| R13 ⭐ | [arXiv:2604.07502 (2026)](https://arxiv.org/abs/2604.07502) | Semantic Density Principle; software engineering conventions for the agentic era. |
| R14 ⭐ | ETH Zurich AGENTbench (2026) | Human-curated annotations outperform LLM-generated context files. |
| R15 ⭐ | [Osmani 2025](https://medium.com/@addyosmani/my-llm-coding-workflow-going-into-2026-52fe1681325e) | AI coding workflow; using comments as operational context for agents. |

---

*This standard is maintained as both a human-readable README and a machine-readable [`CommentingRules.xml`](./CommentingRules.xml). The XML is the authoritative source; this README is generated from it.*

*License: [CC-BY-4.0](https://creativecommons.org/licenses/by/4.0/)*