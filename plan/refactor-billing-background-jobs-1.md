---
goal: Fold pending-placement reconciliation into the existing Stripe webhook job and remove the standalone recurring reconciliation job, keeping the number of background jobs unchanged
version: 1.0
date_created: 2026-08-16
last_updated: 2026-08-16
owner: ReSys.Shop Platform Team
status: 'Planned'
tags: refactor, background-jobs, billing, stripe, reconciliation
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This plan removes the `OrderPlacementReconciliationJob` (a recurring Hangfire job added in
the Stripe webhook fix) and folds its reconciliation capability into the **existing**
per-event `ProcessStripeWebhookEventJob`. The result is **zero new background jobs**: the
reconciliation sweep runs opportunistically after every successfully routed webhook event,
bounded to a batch of 25 completed payments, re-sending the idempotent
`CompleteCheckoutForPaymentCommand` for each.

## Full Context

**Original incident (reported):** Stripe dashboard showed a successful payment that fired
`checkout.session.completed` + `payment_intent.succeeded` + `charge.succeeded` at the same
second, yet the payment/order statuses were not updated correctly and inventory was not
reduced. Root causes identified during analysis:

1. `payment_intent.succeeded` could not correlate to the `PaymentCapture` in the Checkout
   flow — only the session id (`cs_...`) was stored in `ResponseCode`, not the PaymentIntent
   id (`pi_...`), so the handler returned early.
2. Only `checkout.session.completed` placed the order and reduced inventory;
   `payment_intent.succeeded` never finalized the order (async payment methods).
3. `StockReservationService.ConsumeForOrderAsync` hard-failed with
   `NoActiveReservations` when the 15-minute reservation TTL lapsed before the webhook placed
   the order.
4. No reconciliation mechanism existed to rescue "payment Completed but order still Draft".

**Already applied (committed in the working tree, NOT part of this plan):**
- `ProcessStripeWebhookEventJob.cs`: `FindPaymentByIntentAsync` (lookup by
  `StripePaymentIntentId`, then `ResponseCode`, then metadata `payment_id` = `payment.Number`,
  which is written at intent creation by `StripeGateway` using
  `GatewayConstants.Metadata.PaymentIdKey`). `payment_intent.succeeded` now completes the
  payment and places the order via idempotent `CompleteCheckoutForPaymentCommand` before
  recording the event; throws on placement failure so Hangfire retries re-attempt placement.
  `checkout.session.completed` stores the `pi_...` id BEFORE the paid-guard and persists it on
  unpaid sessions. `payment_failed`/`payment_intent.canceled` use the metadata-aware lookup.
- `StockReservation.Service.Interface.cs` / `.Implementation.cs`: new
  `StockConsumeLine(VariantId, Quantity)` record; `ConsumeForOrderAsync(Guid orderId,
  IReadOnlyCollection<StockConsumeLine> lines, CancellationToken ct)` is now idempotent
  (Fulfilled reservations counted as consumed) and self-healing (shortfall re-reserved via
  `ReserveForVariantAsync` with `InventoryFeature.Storefront.StockReservations.TtlMinutesDefault`).
- `Ordering/Services/CheckoutPlacementService.cs`: `ValidateCheckoutPrerequisites` runs before
  stock consumption; passes line items to `ConsumeForOrderAsync`.
- `CompleteCheckoutForPaymentResponse` gained a `Placed` flag (true = actually placed,
  false = idempotent no-op).

**Current background-job landscape (after this plan):**
- `ProcessStripeWebhookEventJob` — per-event Hangfire job (enqueued by
  `StripeWebhook.CommandHandler` via `IBackgroundJobClient`); now also hosts the sweep.
- `ReservationExpiryJob` (Inventory, recurring `*/1 * * * *`) — pre-existing, unchanged.
- `CartExpiryJob` (Ordering, recurring) — pre-existing, unchanged.

**Key review findings driving this plan:**
- 🔴 Finding 2 (Performance): the standalone `OrderPlacementReconciliationJob` sweeps
  **all** `State == Completed` payments every 5 minutes on a column that has **no index**
  (unbounded full scan) and performs a MediatR round-trip per payment, mostly idempotent
  no-ops. It also adds a 3rd recurring job.
- 🟡 Finding 1 (Concurrency): `checkout.session.completed` and `payment_intent.succeeded` both
  send `CompleteCheckoutForPaymentCommand`. This is mitigated by existing `RowVersion`
  concurrency tokens on `PaymentCapture`, `StockItem`, and `StockReservation` (second
  concurrent save throws `DbUpdateConcurrencyException` → Hangfire retry → no-op) plus
  transactional `SaveChangesAsync`. No code change is required; the plan documents this.

**Verification baseline (pre-plan):** `dotnet build service/Api/tests/Module.UnitTests`
0 warnings/errors; `dotnet test service/Api/tests/Module.UnitTests` → 2722 total,
2717 passed, 4 failed (pre-existing and unrelated: 3 `OrderStatusValueConverter` NREs +
`ModuleIsolationTests` drift at 31 vs expected 3), 1 skipped.

## 1. Requirements & Constraints

- **REQ-001**: Do not add any new recurring background job. The reconciliation capability must
  reuse the existing per-event `ProcessStripeWebhookEventJob`; after this plan the number of
  recurring Hangfire jobs must be identical to the pre-change baseline (2: `ReservationExpiryJob`,
  `CartExpiryJob`) plus the per-event webhook job.
- **REQ-002**: Preserve ongoing resilience. The sweep must (a) rescue historical payments that
  are `Completed` with no placed order (the reported incident), (b) rescue payments whose order
  placement failed on all Hangfire webhook retries, and (c) remain idempotent (already-placed
  orders are no-ops).
- **REQ-003**: The sweep must be bounded to `ReconcileBatchSize = 25` payments per webhook event
  and must never fail the webhook event being processed (best-effort try/catch).
- **REQ-004**: Keep the already-applied webhook correlation/finalization fix intact
  (`FindPaymentByIntentAsync`, `pi_...` stored pre-paid-guard, `payment_intent.succeeded`
  placement, `CompleteCheckoutForPaymentResponse.Placed`).
- **REQ-005**: All repository guards must pass: 0 build warnings (`TreatWarningsAsErrors=true`),
  cross-module references at the `EXPECTED_BASELINE`, and feature conventions.
- **SEC-001**: No secrets or credentials in new code. Stripe `metadata` values are used only in
  parameterized EF Core queries (`EF.Functions`/`FirstOrDefaultAsync` predicates) — no SQL string
  concatenation.
- **SEC-002**: The sweep performs only internal MediatR placement — it must never trigger new
  outbound Stripe provider calls.
- **CON-001**: Modules communicate via MediatR `ISender` only. Billing must not query Ordering's
  `DbSet` directly. The sweep re-sends `CompleteCheckoutForPaymentCommand`
  (`Module.Ordering.Features.Storefront.CompleteCheckoutForPayment`) — the same whitelisted
  pattern already used by the webhook job (baseline counted at `scripts/check-cross-module-refs.sh`).
- **CON-002**: Domain operations return `Result` objects, not exceptions. The sweep uses
  `result.IsFailure` checks (per AGENTS.md rule 1).
- **CON-003**: `TreatWarningsAsErrors=true` globally — all new code must compile with zero warnings.
- **CON-004**: Per AGENTS.md rule 6, do NOT use `git stash`, `git restore`, `git checkout -- <path>`,
  `git revert`, or `git reset --hard`. File deletions are performed with `rm` (explicit intent) and
  verified with `git status`.
- **GUD-001**: Follow the existing partial-class background-job conventions
  (`ProcessStripeWebhookEventJob.*`, `ReservationExpiryJob.*`): main logic in the job class,
  structured source-generated loggers in a `*.Loggers.cs` partial.
- **GUD-002**: Reuse existing loggers where possible (`CannotPlaceOrder` EventId 5013,
  `OrderPlaced` EventId 5017). Add exactly two new loggers with EventIds 5027 and 5028
  (the highest existing is 5026).
- **GUD-003**: Run the exact verification commands from AGENTS.md (see TASK-011..TASK-014) and
  report raw output — evidence before assertions.
- **GUD-004**: The sweep must be placed AFTER `RouteEventAsync` succeeds and BEFORE
  `webhookEvent.State = WebhookEventState.Processed;` so a throwing handler prevents the sweep
  (event marked Failed, Hangfire retries) and a sweep failure never marks a processed event Failed.
- **PAT-001**: Idempotent re-send pattern — re-send `CompleteCheckoutForPaymentCommand` for each
  candidate; a non-Draft order returns `Placed = false` (no-op) and a Draft order is placed once.
- **PAT-002**: Bounded-sweep pattern — `Take(ReconcileBatchSize)` on the query plus a filtered
  index on `State` so the query stays bounded as the table grows.

## 2. Implementation Steps

### Implementation Phase 1 — Fold the sweep into the webhook job; remove the standalone recurring job

- GOAL-001: Eliminate the recurring `OrderPlacementReconciliationJob` and move its logic into the
  existing per-event `ProcessStripeWebhookEventJob`, netting zero new background jobs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete files: `service/Api/src/Module/Billing/Backgrounds/OrderPlacementReconciliationJob.cs`, `OrderPlacementReconciliationJob.Scheduler.cs`, `OrderPlacementReconciliationJob.Constants.cs`, `OrderPlacementReconciliationJob.Loggers.cs`, and `service/Api/tests/Module.UnitTests/Billing/Backgrounds/OrderPlacementReconciliationJobTests.cs`. Use `rm` (not `git restore`). Verify with `git status --short` that exactly these 5 files are listed as deleted. | |  |
| TASK-002 | In `service/Api/src/Module/Billing/Paying.Extension.cs` remove the two lines added by the previous fix: `services.AddScoped<OrderPlacementReconciliationJob>();` (currently line 77) and `services.AddHostedService<OrderPlacementReconciliationJobScheduler>();` (currently line 82). The `using Module.Billing.Backgrounds;` import is still required by `ProcessStripeWebhookEventJob`. | |  |
| TASK-003 | In `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs`: (1) add `private const int ReconcileBatchSize = 25;` at the top of the class body; (2) add a private method `ReconcilePendingPlacementsAsync(CancellationToken ct)` with the exact body below; (3) in `ExecuteAsync`, insert a best-effort call immediately after the `catch` block closing brace (currently line 85) and before `webhookEvent.State = WebhookEventState.Processed;` (currently line 87): `try { await ReconcilePendingPlacementsAsync(ct); } catch (Exception ex) { ProcessStripeWebhookEventJobLoggers.ReconcileSweepFailed(_logger, ex, ex.Message); }`. Method body: `var candidates = await _dbContext.Set<PaymentCapture>().Where(p => p.State == PaymentRecordState.Completed).OrderBy(p => p.CompletedAtUtc).Take(ReconcileBatchSize).Select(p => new { p.Id, p.OrderId }).ToListAsync(ct); if (candidates.Count == 0) return; var placed = 0; foreach (var payment in candidates) { var result = await _sender.Send(new CompleteCheckoutForPaymentCommand { CartId = payment.OrderId, PaymentId = payment.Id }, ct); if (result.IsFailure) { ProcessStripeWebhookEventJobLoggers.CannotPlaceOrder(_logger, payment.Id, result.Message); continue; } if (result.Value.Placed) { placed++; ProcessStripeWebhookEventJobLoggers.OrderPlaced(_logger, payment.Id); } } if (placed > 0) ProcessStripeWebhookEventJobLoggers.ReconcileSweepCompleted(_logger, candidates.Count, placed);` | |  |
| TASK-004 | In `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs` add two source-generated logger methods (EventIds 5027 and 5028 — highest existing is 5026): `[LoggerMessage(EventId = 5027, Level = LogLevel.Information, Message = "Pending-placement reconciliation sweep: checked {Total} completed payments, placed {Placed}")] public static partial void ReconcileSweepCompleted(ILogger logger, int Total, int Placed);` and `[LoggerMessage(EventId = 5028, Level = LogLevel.Warning, Message = "Pending-placement reconciliation sweep failed: {Message}")] public static partial void ReconcileSweepFailed(ILogger logger, Exception exception, string? Message);` | |  |
| TASK-005 | **CANCELLED** — `scripts/check-cross-module-refs.sh` was deleted from the working tree (not committed) during the cross-module-references relaxation (`AGENTS.md` rule #2: "no whitelist, no drift check — removed 2026-08-16"). The drift baseline is moot; do not recreate the script. | ✅ | 2026-08-16 |

### Implementation Phase 2 — Index + migration for the bounded sweep query

- GOAL-002: Ensure the sweep's `WHERE State == 'Completed'` query stays bounded as the table grows
  by adding a filtered index and an EF Core migration.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | In `service/Api/src/Module/Billing/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs`, append inside `Configure` (after the relationship definitions): `builder.HasIndex(x => x.State).HasDatabaseName("ix_payment_captures_state");` — a PLAIN index, no `.HasFilter(...)` (user decision: the filter-style config had conventional issues and was removed; the file's other filtered indexes were also removed in the working tree, so the migration drops them). | ✅ | 2026-08-16 |
| TASK-007 | Generate the migration with: `dotnet ef migrations add AddPaymentCompletedStateIndex --project service/Api/src/Migrations --startup-project service/Api/src/Api --context ApplicationDbContext --output-dir Migrations`. `dotnet ef` 10.0.9 is installed and `service/Api/src/Api/DesignTimeDbContextFactory.cs` provides the design-time context (no live DB required). | ✅ | 2026-08-16 |
| TASK-008 | Inspect the generated `service/Api/src/Migrations/Migrations/*_AddPaymentCompletedStateIndex.cs` (and confirm `ApplicationDbContextModelSnapshot.cs` updated). Verify it contains: `migrationBuilder.CreateIndex(name: "ix_payment_captures_state", schema: "payment", table: "payment_captures", column: "state");` — plus `DropIndex` for `ix_payment_captures_response_code`, `ix_payment_captures_stripe_session_id`, `ix_payment_captures_stripe_payment_intent_id` (their filtered configs were removed from the model). | ✅ | 2026-08-16 |

### Implementation Phase 3 — Tests

- GOAL-003: Port the 5 reconciliation tests onto the webhook job's sweep and adjust the 2 webhook
  tests whose `Times.Once` assertions now see a second send from the sweep.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | In `service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJobTests.cs` update FOUR tests to expect the sweep's extra send, changing `Times.Once` → `Times.Exactly(2)` with the comment `// Two sends: the handler places the order, then the post-route reconciliation sweep re-sends the idempotent command.`: `HandleCheckoutSessionCompleted_ShouldCompletePaymentAndStoreIntentId`, `HandleCheckoutSessionCompleted_Retry_FindsPaymentByStoredIntentId`, `HandlePaymentIntentSucceeded_PlacesOrder`, `HandlePaymentIntentSucceeded_AlreadyCompleted_StillPlacesOrder` (the two checkout.session.completed tests were not in the original plan but also leave the payment `Completed`, so the sweep re-sends for them too — all four `Verify` blocks are byte-identical, applied via a single `replaceAll`). | ✅ | 2026-08-16 |
| TASK-010 | Create `service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJob.ReconcileSweepTests.cs` — a new test class `ProcessStripeWebhookEventJobReconcileSweepTests` in namespace `Module.UnitTests.Payment.Backgrounds`, mirroring the existing fixture (InMemory `ApplicationDbContext` + `AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly]`, `Mock<ISender>` returning `Result<CompleteCheckoutForPaymentResponse>.Ok(new() { Placed = true })`, `Mock<ILogger<ProcessStripeWebhookEventJob>>`, `Mock<IStockReservationService>`). Use event `Type = "checkout.session.expired"` with `Session { Id = "cs_unknown_sweep" }` (matches no payment → handler no-ops) so ONLY the sweep sends the command — this makes send counts deterministic. Implement TEST-004..TEST-008 from Section 6. | ✅ | 2026-08-16 |

### Implementation Phase 4 — Verification

- GOAL-004: Prove the refactor compiles clean, all tests pass (modulo the 4 documented pre-existing
  failures), and the repository guards stay green.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Run `dotnet build service/Api/tests/Module.UnitTests` and confirm `Build succeeded. 0 Warning(s) 0 Error(s)`. | ✅ | 2026-08-16 |
| TASK-012 | Run `dotnet test service/Api/tests/Module.UnitTests` and confirm `total: 2721`, `failed: 3`, `succeeded: 2717`, `skipped: 1`, where the 3 failures are exactly the pre-existing `OrderStatusValueConverter` NRE tests. NOTE: the previous baseline had 4 failures — the 4th (`ModuleIsolationTests.ModuleTypes_ShouldNotCrossReferenceOtherModules`) was removed from the test file in the working tree during the cross-module relaxation, so it no longer fails (`ModuleIsolationTests.cs` now asserts Shared-forward-only, Result-returning handlers, and no Domain exception classes). NOTE: `--filter` returns "Zero tests ran" on this project — use the full suite only. | ✅ | 2026-08-16 |
| TASK-013 | **CANCELLED** — the drift-check script `scripts/check-cross-module-refs.sh` was removed from the working tree (see TASK-005). No baseline to verify. | ✅ | 2026-08-16 |
| TASK-014 | Run `bash scripts/check-feature-conventions.sh` and confirm `All convention checks PASSED.` | |  |

## 3. Alternatives

- **ALT-001**: One-shot backfill — keep the `OrderPlacementReconciliationJob` class but schedule it
  once (`BackgroundJob.Schedule`) at startup instead of recurring. Rejected: rescues historical
  incidents only; a future "all webhook retries failed" case stays broken until manual action, and
  it still introduces a job/timer concept.
- **ALT-002**: Drop reconciliation entirely and rely on the fix + Hangfire `[AutomaticRetry(Attempts = 3)]`.
  Rejected: leaves the reported historical incident and any all-retries-failed payment permanently
  broken with no automated rescue.
- **ALT-003**: Keep the standalone recurring `OrderPlacementReconciliationJob` (the status quo of the
  prior fix). Rejected: adds a 3rd recurring job, contradicts REQ-001, sweeps on a fixed timer rather
  than on the exact trigger that creates the gap (webhook events), and performs unbounded no-op
  round-trips without the State index.

## 4. Dependencies

- **DEP-001**: The already-applied webhook fix in the working tree — the sweep relies on
  `CompleteCheckoutForPaymentCommand` being idempotent (non-Draft order → `Placed = false` no-op),
  self-defending (refuses to place until the payment is `Completed`), and on the `Placed` flag.
- **DEP-002**: `dotnet ef` tools (10.0.9, verified installed) and `service/Api/src/Api/DesignTimeDbContextFactory.cs`
  for generating the migration in TASK-007.
- **DEP-003**: Hangfire recurring-job infrastructure — unchanged; this plan only removes one
  `AddHostedService` registration (`OrderPlacementReconciliationJobScheduler`).
- **DEP-004**: EF Core `RowVersion` concurrency tokens on `PaymentCapture`
  (`PaymentRecordConfiguration.cs:38`), `StockItem` (`StockItemConfiguration.cs:30`), and
  `StockReservation` (`StockReservation.Configuration.cs:25`) — the concurrency self-healing that
  makes the dual-placement path (Finding 1) resolve via `DbUpdateConcurrencyException` + Hangfire retry.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` — modified: add `ReconcileBatchSize` const, `ReconcilePendingPlacementsAsync`, and the post-route best-effort call.
- **FILE-002**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs` — modified: add `ReconcileSweepCompleted` (5027) and `ReconcileSweepFailed` (5028).
- **FILE-003**: `service/Api/src/Module/Billing/Paying.Extension.cs` — modified: remove the two `OrderPlacementReconciliationJob` registrations.
- **FILE-004**: `scripts/check-cross-module-refs.sh` — **deleted from working tree** (uncommitted); TASK-005/TASK-013 cancelled, do not recreate.
- **FILE-005**: `service/Api/src/Module/Billing/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs` — modified: plain (unfiltered) index on `State`; the previously configured `ResponseCode`/`StripeSessionId`/`StripePaymentIntentId` filtered indexes were removed from the file in the working tree.
- **FILE-006**: `service/Api/src/Migrations/Migrations/20260816122340_AddPaymentCompletedStateIndex.cs` (+ `.Designer.cs`) and `ApplicationDbContextModelSnapshot.cs` — new/modified: generated migration (drops the 3 filtered indexes, creates `ix_payment_captures_state`).
- **FILE-007**: `service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJobTests.cs` — modified: `Times.Once` → `Times.Exactly(2)` in 4 tests.
- **FILE-008**: `service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJob.ReconcileSweepTests.cs` — new: 5 sweep tests.
- **FILE-009**: `service/Api/src/Module/Billing/Backgrounds/OrderPlacementReconciliationJob.cs` — deleted.
- **FILE-010**: `service/Api/src/Module/Billing/Backgrounds/OrderPlacementReconciliationJob.Scheduler.cs` — deleted.
- **FILE-011**: `service/Api/src/Module/Billing/Backgrounds/OrderPlacementReconciliationJob.Constants.cs` — deleted.
- **FILE-012**: `service/Api/src/Module/Billing/Backgrounds/OrderPlacementReconciliationJob.Loggers.cs` — deleted.
- **FILE-013**: `service/Api/tests/Module.UnitTests/Billing/Backgrounds/OrderPlacementReconciliationJobTests.cs` — deleted.

## 6. Testing

- **TEST-001**: (updated, existing) `HandlePaymentIntentSucceeded_PlacesOrder` — asserts `CompleteCheckoutForPaymentCommand` sent `Times.Exactly(2)`.
- **TEST-002**: (updated, existing) `HandlePaymentIntentSucceeded_AlreadyCompleted_StillPlacesOrder` — asserts `Times.Exactly(2)` (retry safety preserved).
- **TEST-002b**: (updated, existing) `HandleCheckoutSessionCompleted_ShouldCompletePaymentAndStoreIntentId` and `HandleCheckoutSessionCompleted_Retry_FindsPaymentByStoredIntentId` — both also assert `Times.Exactly(2)` (same sweep re-send).
- **TEST-003**: (existing, must keep passing) `HandleCheckoutSessionCompleted_NotPaid_Skips` — asserts `Times.Never` (payment stays `Processing`, so the sweep finds no `Completed` payment).
- **TEST-004**: `ReconcileSweep_PlacesOrdersForCompletedPayments` — seed one `Completed` payment (no `ResponseCode`/`StripeSessionId`) + a Pending `WebhookEvent` (`checkout.session.expired`, `Session { Id = "cs_unknown_sweep" }`); after `ExecuteAsync` the event is `Processed` and the command was sent once with `CartId == payment.OrderId` and `PaymentId == payment.Id`.
- **TEST-005**: `ReconcileSweep_AlreadyPlacedOrder_ProcessesEvent` — sender returns `Placed = false`; `ExecuteAsync` completes, event `Processed`, no throw.
- **TEST-006**: `ReconcileSweep_PlacementFailure_DoesNotFailEvent` — sender returns `Result<CompleteCheckoutForPaymentResponse>.Failure(Error.BadRequest("test", "placement failed"))`; `ExecuteAsync` completes (event `Processed`), no throw.
- **TEST-007**: `ReconcileSweep_SkipsNonCompletedPayments` — seed one `Processing` payment; `ExecuteAsync` completes; sender `Times.Never`.
- **TEST-008**: `ReconcileSweep_IsBoundedToBatchSize` — seed 30 `Completed` payments with distinct ascending `CompletedAtUtc` (`DateTimeOffset.UtcNow.AddMinutes(i)`); `ExecuteAsync` completes; sender `Times.Exactly(25)`.

## 7. Risks & Assumptions

- **RISK-001**: The sweep adds one bounded query + up to 25 re-sends to every webhook event.
  Mitigated by the `State` index (TASK-006/007) and `Take(ReconcileBatchSize)`.
- **RISK-002**: Concurrent double placement (`checkout.session.completed` + `payment_intent.succeeded`
  processed by two Hangfire workers) — self-healing via `RowVersion` concurrency exceptions on
  `PaymentCapture`/`StockItem`/`StockReservation` + transactional `SaveChangesAsync` + Hangfire retry;
  residual risk is low and documented, no code change required (Finding 1).
- **RISK-003**: Migration generation (TASK-007) could fail if `dotnet ef` or the design-time factory
  is unavailable. Fallback: the model-level index (TASK-006) applies to any freshly created schema
  (the app supports `DatabaseInitialization:DropSchemas`), and the migration can be produced later.
- **RISK-004**: `Order` has no `RowVersion` token, so its row write is last-write-wins; double
  consumption is still prevented because order + stock writes share one transaction and the stock
  writes always conflict on overlap (same order's reservations).
- **ASSUMPTION-001**: Stripe delivers events per account but Hangfire default worker concurrency may
  still run two jobs for the same payment concurrently — accepted and self-healing per RISK-002.
- **ASSUMPTION-002**: The `State` column value for a completed payment is the string `"Completed"`
  (enum stored via `.HasConversion<string>()`), so the index matches the sweep's `WHERE` clause.
  NOTE: removing the `ResponseCode`/`StripeSessionId`/`StripePaymentIntentId` filtered indexes (with
  the migration) leaves the webhook correlation lookups (`FindPaymentByIntentAsync`, session lookup)
  unindexed — flagged to the user as a performance consideration, accepted for now.
- **ASSUMPTION-003**: Unit tests use the InMemory provider and do not exercise the migration/index;
  the migration is verified by inspection (TASK-008).
- **ASSUMPTION-004**: The 3 pre-existing test failures (`OrderStatusValueConverter` NREs — `CheckoutState_LegacyStrings_MapToNewNames`, `FulfillmentState_LegacyStrings_MapToEnum`, `PaymentState_LegacyStrings_MapToEnum`) are unrelated to this plan and remain out of scope. The former 4th failure (`ModuleIsolationTests` cross-module drift test) no longer exists in the working tree.
- **ASSUMPTION-005**: The migration's `Down` restores the 3 filtered indexes (it re-creates them with
  their `filter:` clauses); a rollback therefore does not lose the webhook-lookup indexes.

## 8. Related Specifications / Further Reading

- [AGENTS.md](../../AGENTS.md) — Non-Negotiable Rules (Result objects, MediatR-only cross-module flow, vertical slices, warnings-as-errors, no git stash/restore) and the Verification command list.
- [docs/codebase/ARCHITECTURE.md](../../docs/codebase/ARCHITECTURE.md) — modular monolith architecture, layer responsibilities.
- [scripts/check-feature-conventions.sh](../../scripts/check-feature-conventions.sh) — AC-001/002/003/005/006 drift checks.
- [scripts/check-cross-module-refs.sh](../../scripts/check-cross-module-refs.sh) — removed from the working tree 2026-08-16 (whitelist/drift check retired per `AGENTS.md` rule #2).
- [ProcessStripeWebhookEventJob.cs](../../service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs) — the modified job (handlers at lines 128-431, `ExecuteAsync` at 47-88).
- [CompleteCheckoutForPayment.cs](../../service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs) — idempotent placement handler the sweep relies on.
- [StockReservation.Service.Implementation.cs](../../service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs) — idempotent `ConsumeForOrderAsync` (lines 285-360).
- [DatabaseInitializer.cs](../../service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs) — runs `MigrateAsync` at startup (migration applies on deploy).