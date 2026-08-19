# Thesis Review — Part 2, Chapter 2, Sections 2.1–2.2

**Scope of this file:** printed pages 30–77 (2.1 Requirements Specification, 2.2 System Modeling)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. "88 functional requirements across nine business modules" doesn't match what's actually documented

**Location:** §2.1 opening paragraph (p.30)

> "The platform delivers **88 functional requirements across nine business modules**... Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, Location, and Dashboard."

**Problem:** I extracted every requirement ID from §2.1.1's tables (Tables 10–17) and counted them directly:

| Module | Prefix | Count |
|--------|--------|-------|
| Catalog | CAT-FR | 22 |
| Identity | IDN-FR | 16 |
| Inventory | INV-FR | 12 |
| Ordering | ORD-FR | 14 |
| Payment | PAY-FR | 10 |
| Shipping | SHP-FR | 6 |
| Profile | PRF-FR | 3 |
| Location | LOC-FR | 4 |
| **Total** | | **87** |

That's **87, not 88**, and it covers **eight** modules, not nine. **Dashboard**, the ninth module named in the opening sentence, has zero functional requirements anywhere in the thesis. I searched the full 171-page document: "Dashboard" appears exactly twice total, once in this sentence and once elsewhere as an unrelated folder-count entry (a Vue admin panel structure listing), never as a requirements table, never with a `DSH-FR-XX` ID.

I also checked each module's ID sequence (CAT-FR-01 through 22, IDN-FR-01 through 16, etc.) for gaps that might explain the off-by-one, there are none; every module's numbering is fully sequential with no skipped IDs. So this isn't a stray typo from a deleted requirement, it's a genuine mismatch between the summary claim and the actual content.

**Fix:** Either add the missing Dashboard requirements (if that module genuinely exists in the implementation and was just never written up here), or, more likely, drop "Dashboard" from the module list and correct "88" to "87":
> "The platform delivers 87 functional requirements across eight business modules: Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, and Location."

---

## 🟠 REWRITE — 2. The "11 embedding models" overclaim resurfaces a fourth time

**Location:** Table 19, Feature Classification (§2.1.3, p.40), "Model Benchmark System" row

> "Secondary Contribution: Systematic benchmarking of retrieval accuracy and latency across **11 embedding models**, providing model selection guidelines for deployment."

**Problem:** This is the same overclaim flagged twice already in Chapter 1 (§1.3.4.1 vs. §1.6.3) and once in Part 1 (§V Research Methodology): only **four** representative models were formally benchmarked with accuracy/latency tables; eleven is the total number *supported by the framework*, not the number actually evaluated. Because this table explicitly frames it as a thesis "contribution," the imprecision matters more here than elsewhere, a reader could reasonably expect eleven full result sets and only find four.

**Fix:**
> "Secondary Contribution: Systematic benchmarking of retrieval accuracy and latency across four representative embedding models (selected from eleven supported by the framework), providing model selection guidelines for deployment."

---

## 🟢 KEEP — 3. "26 use cases" claim — verified exact

**Location:** §2.2 opening (p.40): *"Three actors interact with the platform across 26 use cases..."*

I extracted every distinct `UC-ADM-*`, `UC-STR-*`, and `UC-SYS-*` identifier used across §2.2.3–2.2.5 and counted them directly: **15 Administrator use cases + 9 Customer/Storefront use cases + 2 System use cases = 26.** This matches exactly. **No action needed**, this is a case where a precise-sounding number actually checks out.

---

## 🟢 KEEP — 4. Requirements traceability is internally consistent

I cross-referenced every functional-requirement ID cited inside a use case's "Requirements" field (73 distinct IDs across §2.2.3–2.2.5) against the 87 IDs actually defined in §2.1.1's tables. **Every single reference resolves correctly**, there are no orphan citations to requirements that don't exist. This is a genuinely well-maintained piece of traceability across ~50 pages and a lot of tables; it's worth knowing this part of the chapter doesn't need rework.

I also checked a couple of specific numeric claims that appear in more than one place for consistency:
- JWT access-token lifetime: "15-minute JWT access token" (IDN-FR-02) matches "JWT access tokens expire after 15 minutes" (NFR-02a). Consistent.
- Inventory reservation timeout: "expire after 15-minute inactivity" (§2.2.1.3, System actor) matches "Unconfirmed checkout inventory holds expire after 15 minutes of inactivity" (NFR-05d). Consistent.

---

## 🟢 KEEP — 5. AI-writing pattern check

Same clean result as the earlier chapters. I scanned this ~2,500-line section for the standard LLM tells (leverage, delve, seamless, testament, "it is important to note," etc.) and found none. All 26 em dashes present are the structural "Use Case ID — Name" table-header convention used consistently throughout (e.g., "UC-ADM-PROD — Manage Products"), not stray prose em dashes. The use-case scenario writing (numbered main flows, alternatives, exceptions) is dense, specific, and consistent in structure across all 26 specifications, this reads as carefully hand-built content, not generated filler. **No action needed.**

---

## 🟡 Optional note — 6. "Support" actors aren't mentioned in the actor count

**Location:** §2.2.1 says three actors interact with the platform (Customer, Administrator, System), but several individual use case tables list a fourth field, "Support", naming external systems the use case depends on: ML Service (UC-STR-SRC, UC-ADM-IMG, UC-SYS-EMB), Payment Gateway (UC-ADM-PAY, UC-STR-CHK, UC-STR-PAY, UC-SYS-MNT), Email Service and Google OAuth (UC-STR-AUT).

This isn't wrong, "Support" actors are a standard UML convention for secondary/external systems a use case interacts with, distinct from the primary actors who initiate use cases. But since §2.2.1 doesn't mention this distinction explicitly, a reader could be briefly confused about why a fourth actor type appears inside individual use case tables after being told there are only three. A one-sentence clarification in §2.2.1 (e.g., "Individual use cases may additionally reference supporting external systems such as the ML sidecar, payment gateway, or OAuth provider") would close the gap. Low priority, not a factual error.

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available, so this isn't a clearance.

**Diagram content (Figures 6–32):** I read the figure captions and cross-checked their claims against the surrounding text, but I did not visually inspect the rendered use-case diagrams themselves. If you want those checked (e.g., for consistency between the diagram actors/relationships and the table text), let me know and I'll rasterize and review the actual figures.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | "88 FRs / nine modules" vs. actual 87 FRs / eight modules (Dashboard is a phantom module) | High | Correct |
| 2 | "11 embedding models" overclaim, 4th occurrence | Medium | Rewrite |
| 3 | "26 use cases" claim | — | Keep, verified exact |
| 4 | Requirements traceability (73 references, 87 definitions) | — | Keep, fully consistent |
| 5 | Prose / AI-writing check | — | Keep, clean |
| 6 | "Support" actors not mentioned in actor count | Low | Optional clarification |

---

*Next: send Section 2.3 (System Architecture & Design) and/or 2.4 (Implementation) when you're ready, and I'll do the same pass.*
