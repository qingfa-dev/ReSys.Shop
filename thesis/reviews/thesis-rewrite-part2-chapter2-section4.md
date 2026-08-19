# Thesis Rewrite — Part 2, Chapter 2, Section 2.4

Five edits here, plus one important non-edit: this is the section that actually resolves the "11 models" question everywhere else, so it's worth understanding that before you touch anything.

All edits trace back to `thesis-review-part2-chapter2-section4.md` and the master fix list.

---

## No edit needed, but read this first — Table 55's model registry is the source of truth for every "11 models" fix elsewhere

Table 55 (§2.4.4.1) lists exactly six models: `fashion_clip`, `clip_vit_b16`, `openclip-vit-b-32`, `efficientnet_b0`, `resnet50`, `dinov2_vits14`. This table doesn't need editing, it's correct and it's the reason every other "eleven models" mention across Part 1, Chapter 1, and §2.1 got corrected to "four representative models, selected from six" in the earlier rewrite files. If you haven't applied those yet, this is the table to check your corrected numbers against.

---

## Edit 1 — §2.4.4.3, CBIR endpoint URL, version 1 of 2 (p.101)

**BEFORE:**
> 1. Client Validation (Vue 3). Validates format (JPEG, PNG, WebP) and size (≤ 10 MB). Dispatches multipart form to POST **/api/admin/catalog/storefront/search-by-image**.

**AFTER:**
> 1. Client Validation (Vue 3). Validates format (JPEG, PNG, WebP) and size (≤ 10 MB). Dispatches multipart form to POST **/api/catalog/storefront/search-by-image**.

**Why:** the original path has both "admin" and "storefront" segments, which shouldn't be possible under your own stated convention (a route is one surface or the other). Visual search is a customer-facing feature (UC-STR-SRC), so "storefront" is right and "admin" is the stray segment. This version now matches the declared `/api/{module}/{surface}/{resource}` convention exactly: module = catalog, surface = storefront, resource = search-by-image.

---

## Edit 2 — §2.4.5.1, CBIR endpoint URL, version 2 of 2 (p.102)

**BEFORE:**
> For visual search, the repository extends the base with a multipart upload method: `async searchByImage(file: File): Promise<Result<Product[]>>` dispatching POST **/api/storefront/search-by-image** with Content-Type: multipart/form-data.

**AFTER:**
> For visual search, the repository extends the base with a multipart upload method: `async searchByImage(file: File): Promise<Result<Product[]>>` dispatching POST **/api/catalog/storefront/search-by-image** with Content-Type: multipart/form-data.

**Why:** this was missing the `catalog` module segment entirely and, combined with Edit 1, gave the same feature two different, mutually inconsistent URLs three pages apart. Both now read identically and match the stated convention.

**One more thing worth doing:** before finalizing either edit, check your actual Carter route definitions in the codebase, since the other worked examples in this section (`/api/storefront/payment/methods`, `/api/storefront/payment/create-intent`) put "storefront" *first*, the opposite order from what §2.3.5.2 declares. If it turns out "surface-then-module" is what's actually implemented, the fix should go the other way: correct §2.3.5.2's stated convention to match the real routes, and use `/api/storefront/catalog/search-by-image` here instead. Either way, the goal is for the convention statement and every worked example to agree with each other and with the real code.

---

## Edit 3 — §2.4.5, cross-reference points to the wrong subsection (p.102)

**BEFORE:**
> This section presents the implemented interfaces organized by the 26 use cases defined in **Section 2.2.2**.

**AFTER:**
> This section presents the implemented interfaces organized by the 26 use cases defined in **Section 2.2**.

**Why:** §2.2.2 is "Functional Decomposition" (the work-breakdown structure), not where the use cases themselves are defined. The 26 use cases are actually spread across §2.2.3–2.2.5. Pointing to the whole of §2.2 is simpler and accurate; if you want to be more precise, "Sections 2.2.3–2.2.5" also works.

---

## Edit 4 — §2.4.5.2.1, "four-state" vs. "five states," same subsection (p.102)

**BEFORE (opening line):**
> The visual search interface implements a **four-state UI model**:
> [Table 58: Empty, Upload, Loading, Results, four rows]

**BEFORE (closing line, after Table 58):**
> The **five** visual search states are illustrated below.

**AFTER (closing line only, opening line and table unchanged):**
> The **four** visual search states are illustrated below.

**Why:** the opening sentence and Table 58 both agree on four states, only the closing sentence says five, and there's no fifth state described or shown anywhere in this subsection. The simplest correct fix is matching "five" to the four that are actually documented. If you do have a fifth state in mind that got cut during editing (an Error state seems like the natural candidate, given UC-STR-SRC's alternative flows for invalid formats and ML-service failures), the better fix is adding that row to Table 58 instead, in which case both the opening line and the table need updating together, not just the closing sentence.

---

## Edit 5 — §2.4.5.2, storefront use case count undercounts by one (p.102)

**BEFORE:**
> The customer storefront implements **eight use cases** covering product discovery, purchasing, and account management.

**AFTER:**
> The customer storefront implements **nine use cases** covering product discovery, purchasing, and account management.

**Why:** the eight subsections that follow this sentence (§2.4.5.2.1–2.4.5.2.8) actually document nine distinct use case IDs: SRC, BRW, CRT, CHK, OHI, AUT, SES, PAY, and PRF (Profile Management, in §2.4.5.2.8), matching the full storefront set from §2.2.4 exactly. The content was already complete, only the summary count was off by one.

---

## Edit 6 — Table 51, pgvector version mismatch (p.94)

**BEFORE:**
> ORM and Database: EF Core 10.0.9, Npgsql 10.0.2, **pgvector 0.3.2**

**AFTER:**
> ORM and Database: EF Core 10.0.9, Npgsql 10.0.2, **pgvector 0.7.0**

**Why:** Table 51 is the thesis's dedicated "pinned version specifications" table, so it's the one most likely to get treated as authoritative if someone tries to reproduce your setup. Chapter 3's test environment and hardware descriptions both say pgvector 0.7.0, and that's the only version mentioned anywhere else in the thesis. Double-check this against your actual `Directory.Packages.props` or `pg_extension` output before finalizing, but 0.7.0 is very likely the correct value here.

---

## What wasn't touched

§2.4.1.1 (containerized resources), §2.4.2 (Vertical Slice Architecture core), §2.4.3 (Data Persistence, aside from Edits already covered under §2.3), §2.4.4.1–2.4.4.2 (Model Management and Embedding Pipeline), §2.4.5.3 (Administrative Interfaces, the "fifteen administrative use cases" claim was verified exact) all held up under review. The 262-endpoint and 11-DTO counts checked out exactly and needed no changes.

---

Ready for Chapter 3 (Testing and Evaluation) next whenever you want to continue, that's the chapter carrying the biggest unresolved item (Table 67/68 vs. Appendix A), so it'll take more careful handling than a simple find-and-replace.
