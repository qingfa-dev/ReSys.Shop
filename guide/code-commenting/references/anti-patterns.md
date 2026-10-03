# Anti-Patterns Reference

Quick lookup for what NOT to do. Claude reads this file when reviewing or auditing
comments to identify violations.

## AP-1 — Redundancy
Comment restates what the code already says.

```cs
// ❌ Assign: Set order ID to 123
order.Id = 123;

// ✅ Assign: Order ID from payment gateway response — gateway is source of truth
order.Id = paymentResponse.OrderId;
```

**Test:** Would the comment be meaningful if you deleted the code line? If no → redundant.

---

## AP-2 — Vagueness
Label is present but subject gives no useful information.

```cs
// ❌ Process: Handle the order
// ✅ Validate: Order meets minimum purchase and in-stock requirements before payment
```

**Test:** Can you grep for this label and understand the intent without reading the code? If no → too vague.

---

## AP-3 — Over-commenting
Every trivial line carries a comment, drowning signal in noise.

```cs
// ❌
// Create: New list
var items = new List<Item>();
// Add: First item
items.Add(item1);

// ✅ (single comment where it earns its place)
// Create: Cart pre-populated with customer's saved items for one-click checkout
var items = new List<Item>(customer.SavedItems);
```

**Test:** Would a competent developer need this comment to understand the code? If no → remove it.

---

## AP-4 — Inconsistent Style
Mixed capitalisation, missing colons, or inconsistent label usage.

```
// ❌
// validate email format
// CHECK: user permissions
// Generate ID

// ✅
// Validate: Email format and domain allowlist
// Check: User permissions for resource access
// Generate: Unique transaction ID
```

**Fix:** Add label linting to CI. Use IDE snippets to enforce casing.

---

## AP-5 — Stale Comments
Comment describes old behaviour the code has since evolved beyond.

```cs
// ❌ Create: Simple product entity   ← code is no longer simple
var product = await ProductFactory.CreateWithInventoryTracking(sku, name, price, supplier, warehouseLocation);

// ✅ Create: Product with full inventory-management metadata for warehouse sync
var product = await ProductFactory.CreateWithInventoryTracking(sku, name, price, supplier, warehouseLocation);
```

**Fix:** Treat stale comments as bugs. Update immediately when refactoring (BP-8).

---

## AP-6 — Commented-Out Code
Dead code left in comments misleads readers.

```python
# ❌
# old_price = item.base_price * 1.1
# discount = calculate_legacy_discount(item)
price = pricing_service.get_current_price(item.sku)
```

**Fix:** Use `git` for history. Delete dead code; it is not documentation.

---

## AP-7 — Verbose Agent Context (NEW in v3.0)
Bloated CAT-10 annotations that attempt to describe everything.

ETH Zurich AGENTbench (2026): verbose annotations reduce agent success by ~3%, increase costs by 20%.

```python
# ❌ AgentHint: This is a complex function that does many things. First it validates
#               the input, then it processes the data, then it saves to the database.
#               Be careful when modifying it because it is used in many places and
#               has many side effects that are hard to predict...

# ✅ AgentHint: do NOT add DB calls here — use OrderRepository injected in __init__;
#              validation lives in OrderValidator, not here
```

**Rule of thumb:** If your AgentHint exceeds 2 lines, split the function instead.

---

## AP-8 — Passive Voice (NEW in v3.0)
Passive voice obscures the agent/actor and weakens the imperative intent.

```
// ❌ Filter: Expired sessions are removed from the cache
// ✅ Filter: Remove expired sessions from cache to prevent stale auth

// ❌ Validate: Input is checked for SQL injection characters
// ✅ Validate: Reject input containing SQL injection characters before query construction
```

**Rule:** Start the comment body with an imperative verb (Remove, Reject, Compute, Apply...).

---

## Severity Guide

| Anti-Pattern | Severity in Code Review |
|-------------|------------------------|
| AP-1 Redundancy | `nitpick (non-blocking)` |
| AP-2 Vagueness | `suggestion (non-blocking)` |
| AP-3 Over-commenting | `nitpick (non-blocking)` |
| AP-4 Inconsistent style | `issue (blocking)` — breaks tooling |
| AP-5 Stale comments | `issue (blocking)` — misleads maintainers |
| AP-6 Commented-out code | `issue (blocking)` — pollutes codebase |
| AP-7 Verbose agent context | `suggestion (non-blocking)` |
| AP-8 Passive voice | `nitpick (non-blocking)` |