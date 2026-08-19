# Thesis Review — Part 2, Chapter 2, Section 2.4

**Scope of this file:** printed pages 94–107 (2.4 Implementation: Technology Stack, Vertical Slice Architecture Core, Data Persistence, ML Sidecar and CBIR Search, Frontend Applications)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. The actual model registry has six models, not eleven — this resolves the "11 models" question raised in every earlier chapter

**Location:** Table 55, §2.4.4.1 Model Management (p.98)

> "**Six models span four architectures**, selected from a decorator-based registry on first inference:
> fashion_clip, clip_vit_b16, openclip-vit-b-32, efficientnet_b0, resnet50, dinov2_vits14."

**Why this matters:** Every prior chapter review in this series flagged the recurring claim that the framework "supports eleven models" (Part 1 §V, Chapter 1 §1.3.4.1/§1.6.3, Chapter 2 §2.1.3 Table 19), always framed as "four representative models selected from the eleven supported." I treated that as an overclaim before because Chapter 3's benchmark only reports four models in detail. **This table is the actual, concrete, code-level model registry, the ground truth for what the system supports, and it lists six, not eleven.**

So the "eleven" figure doesn't match any real artifact in the thesis: not the benchmark's four reported models, and now confirmably not the implementation's six registered models either. This changes my earlier recommendation: this isn't just an overclaim to soften, it's a number that should be corrected to match reality everywhere it appears.

**Fix:** Go through every occurrence of "eleven models" (there are at least four across Parts 1–2) and change it to reflect the real number, six, unless there's a separate, larger benchmark-only model list documented somewhere that I haven't seen yet (in which case, that list needs to be shown explicitly, e.g. as an appendix table, so the "eleven" claim has something to point to). As it stands, the number appears to have no basis anywhere in the actual thesis content.

---

## 🔴 CORRECT — 2. Visual search UI: "four-state model" vs. "five states," in the same subsection

**Location:** §2.4.5.2.1, Visual Search (p.102)

> "The visual search interface implements a **four-state UI model**:" [Table 58 lists exactly four rows: Empty, Upload, Loading, Results]
> ...
> "The **five** visual search states are illustrated below."

**Problem:** The subsection opens by declaring four states and Table 58 backs that up with exactly four rows. Two paragraphs later, the closing sentence says five. This is a same-page, same-subsection contradiction, likely a leftover from an earlier draft where a fifth state (perhaps an "Error" state, given the CBIR flow elsewhere mentions ML-service-unavailable and invalid-format error paths) was removed from the table but not from this sentence.

**Fix:** Either add the missing fifth state to Table 58 if it's meant to exist (an Error state seems like the obvious candidate given UC-STR-SRC's alternative flows A1/A4 in §2.2.4.4), or change "five" to "four" to match the table as it stands.

---

## 🔴 CORRECT — 3. The CBIR search endpoint is given three different, mutually inconsistent URLs

**Locations:**
- §2.4.4.3, step 1 (p.101): *"Dispatches multipart form to POST **/api/admin/catalog/storefront/search-by-image**."*
- §2.4.5.1 (p.102): *"...dispatching POST **/api/storefront/search-by-image**..."*
- Declared convention, §2.3.5.2 (p.91): *"Endpoints follow the convention **/api/{module}/{surface}/{resource}**, where surface is storefront or admin."*

**Problem:** These are three different paths for what is described as the same feature (uploading an image to search visually):
1. The first version has **four** path segments and includes both "admin" and "storefront" simultaneously, which shouldn't be possible under the stated convention (a route is either admin-surface or storefront-surface, not both).
2. The second version has only **two** segments, it's missing the `{module}` segment (`catalog`) entirely.
3. Neither matches the declared three-segment pattern (`/api/catalog/storefront/search-by-image` would be the version that actually fits the rule).

This also isn't isolated to just this one endpoint: the other concrete example routes shown in §2.4.5.2 (`GET /api/storefront/payment/methods`, `POST /api/storefront/payment/create-intent`) all put "storefront" as the **first** segment, which is the reverse ordering from the declared convention (`{module}/{surface}/{resource}` says module comes first, surface second). So the mismatch between the stated rule and the worked examples looks systemic, not a one-off typo.

**Fix:** Decide which ordering is actually implemented in the codebase (check your Carter route definitions directly), then make every stated example and every prose description of the convention consistent with it. Given the actual code is the source of truth here, this is a quick grep-and-fix in the thesis text once you confirm the real route pattern.

---

## 🟠 REWRITE — 4. pgvector version disagreement

**Location:** Table 51, §2.4.1 Technology Stack (p.94)

> "ORM and Database: EF Core 10.0.9, Npgsql 10.0.2, **pgvector 0.3.2**"

**Problem:** This is the thesis's dedicated "pinned version specifications" table, presumably the most authoritative version listing in the document. But Chapter 3's test environment and hardware descriptions both say **pgvector 0.7.0** (§3.2 test setup and Table 66 Hardware Environment). Table 51 is the only place in the thesis that says 0.3.2. Given it's the "pinned versions" table, this is the one most likely to get copied verbatim by anyone trying to reproduce your setup, so it's worth getting right. This sits alongside the PostgreSQL 16-vs-17 mismatch already flagged in the §2.3 review, both appear to stem from the same technology-stack table not being kept in sync with what Chapter 3 actually describes running.

**Fix:** Confirm the actual pgvector version used (check your `Directory.Packages.props` or actual `pg_extension` output) and make Table 51 and Chapter 3 agree.

---

## 🟠 REWRITE — 5. "Eight use cases" undercounts the storefront coverage actually documented

**Location:** §2.4.5.2, Storefront Interfaces (p.102)

> "The customer storefront implements **eight use cases** covering product discovery, purchasing, and account management."

**Problem:** The subsections that follow (§2.4.5.2.1 through §2.4.5.2.8) document exactly **nine** distinct storefront use case IDs: UC-STR-SRC, BRW, CRT, CHK, OHI, AUT, SES, PAY, and PRF (Profile Management, §2.4.5.2.8). All nine match the full storefront use case set defined back in §2.2.4. This is a simple off-by-one in the summary sentence, the content itself is complete and correctly covers all nine, it's just the introductory count that's wrong.

By contrast, I checked the equivalent claim for the admin side, "the administration dashboard implements fifteen administrative use cases" (§2.4.5.3), by counting IDs across all its subsections (5+2+2+2+2+1+1 = 15), and that one is exactly right.

**Fix:** Change "eight use cases" to "nine use cases" in §2.4.5.2's opening sentence.

---

## 🟡 CUT / CORRECT — 6. Minor cross-reference imprecision

**Location:** §2.4.5, opening (p.102): *"...organized by the 26 use cases defined in **Section 2.2.2**."*

**Problem:** §2.2.2 is "Functional Decomposition" (the work-breakdown structure), not where the 26 use cases are individually defined, those are spread across §2.2.3 (Administrator), §2.2.4 (Customer), and §2.2.5 (System). Minor, but worth fixing since it's a specific section pointer a reader might actually follow.

**Fix:** Change "Section 2.2.2" to "Section 2.2" (or "Sections 2.2.3–2.2.5" if you want to be precise).

---

## 🟢 KEEP — 7. "Six containerized resources" reconciles correctly

**Location:** §2.4.1.1 (p.95): *"The platform defines six containerized resources with startup dependencies..."*

This resolves the ambiguity I flagged as a minor note in the §2.3 review (where §2.3.3.2 said "six standalone deployable processes" but only listed five bullet points). Here it's explicit: PostgreSQL, Redis, the Python ML sidecar, and the .NET API are named individually (4), plus the two Vue SPAs (Store and Admin) make 6. Consistent with the earlier section once you count the two SPAs separately. **No further action needed**, this was already resolved by reading further into the document.

---

## 🟢 KEEP — 8. "Fifteen administrative use cases" — verified exact

I counted the use case IDs across all seven admin subsections (§2.4.5.3.1–2.4.5.3.7): Product Management (5: PROD, VAR, IMG, TAX, OPT) + Order Management (2: ORD, ORD-ITEMS) + Payment Management (2: PAY, PAY-METHOD) + Inventory Management (2: STK, LOC) + User/Role Administration (2: USR, ROL) + Shipping Configuration (1: SHP) + Reference Data (1: REF) = **15**, matching exactly. **No action needed.**

---

## 🟢 KEEP — 9. AI-writing pattern check

Same clean result as every chapter so far. No LLM-tell vocabulary, zero stray em dashes in this ~700-line section. The implementation walkthrough includes real code snippets, real response JSON, and specific UI-state tables, this reads as authored directly from the working system, which is exactly what you'd expect from an implementation chapter written by the person who built it.

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

**Figure 41 (CBIR search sequence diagram):** caption cross-checked against the six-stage flow described in prose; the rendered figure itself wasn't visually inspected.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | Model registry (Table 55) shows 6 models, not 11 — resolves the recurring "11 models" question | High | Correct everywhere "eleven models" appears |
| 2 | "Four-state" vs. "five states" contradiction, same subsection | High | Correct |
| 3 | CBIR endpoint given 3 different, inconsistent URLs | High | Correct, verify against actual routes |
| 4 | pgvector 0.3.2 (Table 51) vs. 0.7.0 (Chapter 3) | Medium | Reconcile |
| 5 | "Eight use cases" undercounts the 9 actually documented | Medium | Rewrite |
| 6 | "Section 2.2.2" cross-reference should be "Section 2.2" | Low | Correct |
| 7 | "Six containerized resources" | — | Keep, reconciles correctly |
| 8 | "Fifteen administrative use cases" | — | Keep, verified exact |
| 9 | Prose / AI-writing check | — | Keep, clean |

---

*This closes out Chapter 2 (2.1 through 2.4). Send Chapter 3 (Testing and Evaluation) when you're ready and I'll do the same pass, note that Chapter 3 is where several of the numbers flagged in earlier chapters (the 5.4%/7.7%/model-count figures) actually live, so it's worth treating as the source of truth once we get there and back-propagating any final corrections.*
