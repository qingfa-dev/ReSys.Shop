# Cross-Module Namespace References Vio lating Module Isolation

## Summary

All 9 business modules share a single `Module.csproj` assembly. Despite the rule
"modules must not reference each other" and MediatR `ISender` being the intended
cross-module communication path, 32 files contain direct `using Module.X.Domain...`
statements referencing types from other modules. The `ValidateVerticalSliceIsolation`
build target only checks csproj `ProjectReference` (of which there are none) and
emits `<Warning>` (not `<Error>`), so these namespace-level violations pass the
build silently.

## Root Cause

All modules compile into one assembly (`Module.csproj`), so `using Module.X.Domain...`
statements compile without issue. The build target cannot detect namespace-level
references within a single project.

## Prevention

1. **`scripts/check-cross-module-refs.sh`** — drift check that counts cross-module
   `using` statements and fails if the count increases above the baseline.
   Run manually or in CI before merging.
2. **Baseline tracking**: The script tracks a numeric baseline (currently 32).
   When violations are removed, contributors decrease the baseline so the check
   prevents regressions.
3. **AGENTS.md rule #2**: Explicitly states "Modules must not reference each other"
   and that existing violations are being removed.
4. **PR template**: Checklist item verifies cross-module work uses `ISender`.

## Detection

- `bash scripts/check-cross-module-refs.sh` — exits 1 if violations exceed baseline
- Manual grep: `rg "using Module\.(Catalog|Identity|Inventory|Location|Ordering|Payment|Profile|Shipping|Dashboard)\." service/Api/src/Module/ -l`

## Evidence

- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — references Catalog, Inventory, Payment, Shipping, Profile domain types
- `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` — references Ordering domain types
- `Directory.Build.targets:42-53` — ValidateVerticalSliceIsolation target (warnings only, csproj-level)
- `docs/codebase/CONCERNS.md` — full cross-module reference inventory
