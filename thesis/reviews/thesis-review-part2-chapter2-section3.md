# Thesis Review — Part 2, Chapter 2, Section 2.3

**Scope of this file:** printed pages 77–94 (2.3 System Architecture & Design: System Overview, Domain-Driven Design, C4 Architecture, Database Design, API Design, Security Design)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. "Nine bounded contexts" contradicts "eight" stated everywhere else, including two sentences earlier

**Location:** §2.3.1, System Overview (p.77)

> "ReSys.Shop comprises three services... and **eight** bounded contexts using Domain-Driven Design with MediatR dispatch between modules.
> ...
> Internally, the backend is partitioned into **nine** bounded contexts, each owning a dedicated database schema."

**Problem:** These two sentences are three lines apart in the same subsection and contradict each other directly. The correct number is **eight**:
- Table 47 (right after the "nine" sentence) lists exactly 8 rows: Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location.
- §2.3.2 repeats "eight bounded contexts" three separate times.
- Figure 33's own caption says "eight business contexts."
- Table 48 in §2.3.2.1 also lists exactly 8 contexts.

So "nine" is an isolated, one-off error surrounded on all sides by "eight." This one is a simple, high-confidence fix.

**Fix:** Change "partitioned into nine bounded contexts" to "partitioned into eight bounded contexts."

---

## 🔴 CORRECT — 2. Two references to a "Section 2.1.5" that doesn't exist

**Locations:**
- §2.3.4.3, pgvector Integration (p.89): *"...with IVFFlat as a fallback for local environments (see Section 2.1.5 for index detail)."*
- §2.4 area (p.95, just past this section's boundary but worth flagging since it's the same broken reference): *"...sub-second CBIR queries (see Section 2.3.4 for index detail and Section 2.1.5..."*

**Problem:** Section 2.1 in this thesis only goes up to §2.1.3 (Feature Classification). There is no §2.1.4 or §2.1.5 anywhere in the document. This is the same category of error as the phantom "Chapter 6" reference found in the appendix during the earlier full-document pass, a cross-reference to a section that either got renumbered or deleted during editing and was never updated at the citing end. It appears twice, so it's not a one-off typo.

**Fix:** Find whatever content this was meant to point to (likely the HNSW/IVFFlat comparison in §1.4.3–1.4.4, or the benchmark protocol in §3.4.3) and correct both references to the actual section number.

---

## 🟠 REWRITE / RECONCILE — 3. PostgreSQL version is "17" everywhere except Chapter 3's hardware table, which says "16"

**Location:** this section states "PostgreSQL 17" three times (§2.3.1 Table 46, §2.3.3.2, §2.3.4 opening), and it recurs elsewhere in the thesis (Chapter 1 §1.5.4, Chapter 2 §2.4, Appendix D), seven occurrences total.

**Problem:** Chapter 3's Table 66 ("Hardware Environment," the actual benchmark setup) states: *"Database: PostgreSQL 16, pgvector 0.7.0."* Every other mention of the database version in the thesis, including this architecture section, says PostgreSQL 17. This isn't necessarily wrong, it's plausible the production/architecture design targets 17 while the benchmark environment happened to run on 16 during development, but as written there's no explanation for the discrepancy, and a reader has no way to tell whether it's an intentional version difference or a typo in one of the two places.

**Fix:** Either confirm the benchmark genuinely ran on PostgreSQL 16 and add a one-line note explaining why (e.g., "the benchmark environment used PostgreSQL 16 during initial development; the production schema targets PostgreSQL 17"), or, if it's simply a typo, correct Table 66 to say 17 so all eight mentions agree.

---

## 🟠 REWRITE — 4. Permission-string format is described two different ways

**Location:** §2.3.6.2, Dynamic Authorization (p.93)

> "Permissions use the format **Domain.Category.Resource.Action**:
> ```
> catalog.products.create
> catalog.variants.delete
> identity.roles.manage
> ```"

**Problem:** Two things don't line up here:
1. Earlier in the thesis (NFR-02b, §2.3.6.2's own neighboring text conventions elsewhere), the permission format is described as **`domain:category:action`**, colon-separated, three parts.
2. Here it's described as **`Domain.Category.Resource.Action`**, dot-separated, **four** parts, but the actual example strings immediately below it (`catalog.products.create`, `identity.roles.manage`) are dot-separated and only have **three** segments each (domain.resource.action), there's no separate "Category" segment visible in any example.

So the stated template doesn't match its own examples, and the separator character (colon vs. dot) is inconsistent with how the same concept is described elsewhere in the thesis. This is a small thing individually, but permission-string format is exactly the kind of implementation detail a committee member (or a future maintainer of the actual codebase) will try to verify against the code, and right now the document doesn't give a single consistent answer.

**Fix:** Pick one format and use it everywhere. Given the code examples are the ground truth (they presumably come directly from your implementation), the fix is likely to change the prose descriptions elsewhere in the thesis to `domain.resource.action` (dot-separated, three parts) and drop "Category" from the template sentence here.

---

## 🟢 KEEP — 5. "Approximately 262 Carter endpoints" — verified exact

**Location:** §2.3.5, API Design (p.91) and Table 50

I summed the "N" column in Table 50 directly: Catalog 80 + Identity 37 + Ordering 35 + Inventory 32 + Profile 27 + Location 18 + Payment 17 + Shipping 15 + Dashboard 1 = **262**, matching the stated "approximately 262" exactly. **No action needed.**

---

## 🟢 KEEP — 6. "Eleven inter-module contract DTOs" — verified exact

**Location:** §2.3.5.2 (p.91)

I counted the DTOs listed: 4 for Inventory (ReserveCartStock, ReleaseCartStockReservations, ConsumeCartStockReservations, CheckVariantAvailability) + 3 for Ordering (GetCartForCheckout, GetCartForShipping, AdvanceCheckoutState) + 2 for Payment (GetPaymentForCheckout, MarkPaymentPaid) + 2 for Catalog (GetVariantDiscontinuedStatuses, GetVariantWeights) = **11**. Matches exactly. **No action needed.**

---

## 🟢 KEEP — 7. Dashboard module clarified, not a phantom feature

**Note tying back to the previous review file (2.1–2.2):** that earlier pass flagged "Dashboard" as a module named in the FR summary count but with zero functional requirements documented anywhere. This section's Table 50 shows Dashboard *does* exist as a real feature, with exactly 1 API endpoint ("Aggregated metrics: sales, inventory, catalog, activity"). So Dashboard isn't fabricated, it's a genuine, minimal part of the system. The correction still stands as written in the earlier file (§2.1's "88 requirements / nine modules" opening line needs fixing), but it's worth knowing Dashboard itself is real; it's specifically the *functional requirements table* for it that's missing, not the feature.

---

## 🟢 KEEP — 8. AI-writing pattern check

Same clean result as every prior chapter. No LLM-tell vocabulary, no stray em dashes in prose (zero found in this section at all). The architecture description is dense with specific implementation detail (actual C# code snippets, actual permission strings, actual table/column names), which is a strong positive signal, this reads like it was written by someone who actually built the system, not generated at a distance from it.

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

**Figures 33–39 (diagrams):** captions cross-checked against the surrounding text; the rendered diagrams themselves weren't visually inspected. Say the word if you want those rasterized and reviewed directly.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | "Nine bounded contexts" contradicts "eight" stated 5+ times nearby | High | Correct |
| 2 | Phantom "Section 2.1.5" cross-reference, appears twice | High | Correct |
| 3 | PostgreSQL 17 (7 mentions) vs. PostgreSQL 16 (Chapter 3 hardware table) | Medium | Reconcile |
| 4 | Permission-string format stated two different ways | Medium | Rewrite for consistency |
| 5 | "262 Carter endpoints" | — | Keep, verified exact |
| 6 | "Eleven inter-module DTOs" | — | Keep, verified exact |
| 7 | Dashboard module is real, just missing an FR table (see prior file) | — | Keep, context note |
| 8 | Prose / AI-writing check | — | Keep, clean |

---

*Next: send Section 2.4 (Implementation) when you're ready, and I'll do the same pass on it.*
