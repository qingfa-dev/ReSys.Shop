# Page-Cut Map — 20–30 Page Reduction (Section-by-Section, Page-by-Page)

Anchored to the prior review's page ranges for the 183-page `main.pdf`:
§2.2 pp. 39–72 · §2.4.5 pp. 98–122 · Appendix D pp. 153+.
Use-case inventory **verified from source** (25 total = 14 Admin + 9 Customer + 2 System),
not from prior review arithmetic (which wrongly said "26" and "12 of 15").

---

## A. §2.2 Use Cases — pp. 39–72 → target −8 to −10 pages

### A.1 Verified inventory (from `chapters/part2/ch2-design/02-use-cases/`)
| # | Actor | UC ID | Name | Keep/Compress |
|---|-------|-------|------|--------------|
| 1 | Admin | UC-ADM-PROD | Manage Products | **KEEP full** |
| 2 | Admin | UC-ADM-VAR | Manage Variants | Compress |
| 3 | Admin | UC-ADM-IMG | Manage Images | Compress |
| 4 | Admin | UC-ADM-TAX | Manage Taxons | Compress |
| 5 | Admin | UC-ADM-OPT | Manage Options | Compress |
| 6 | Admin | UC-ADM-ORD | Manage Orders | Compress |
| 7 | Admin | UC-ADM-ORD-ITEMS | Manage Order Details | Compress |
| 8 | Admin | UC-ADM-PAY | Manage Payments | Compress |
| 9 | Admin | UC-ADM-LOC | Manage Locations | Compress |
| 10 | Admin | UC-ADM-STK | Manage Stock | Compress |
| 11 | Admin | UC-ADM-USR | Manage Users | Compress |
| 12 | Admin | UC-ADM-ROL | Manage Roles | Compress |
| 13 | Admin | UC-ADM-SHP | Manage Shipping | Compress |
| 14 | Admin | UC-ADM-REF | Manage Refunds | Compress |
| 15 | Customer | UC-STR-AUT | Authentication | Compress |
| 16 | Customer | UC-STR-BRW | Browse Catalogue | Compress |
| 17 | Customer | UC-STR-CRT | Shopping Cart | Compress |
| 18 | Customer | UC-STR-CHK | Checkout | **KEEP full** |
| 19 | Customer | UC-STR-OHI | Order History | Compress |
| 20 | Customer | UC-STR-PAY | Payment | Compress |
| 21 | Customer | UC-STR-PRF | Profile | Compress |
| 22 | Customer | UC-STR-SES | Session | Compress |
| 23 | Customer | UC-STR-SRC | Visual Search | **KEEP full** |
| 24 | System | UC-SYS-EMB | Embedding Operations | **KEEP full** |
| 25 | System | UC-SYS-MNT | System Maintenance | Compress |

**Totals:** 14 Admin · 9 Customer · 2 System · **25 total** (not 26).

### A.2 Keep-in-full (why)
- **UC-ADM-PROD** — representative admin CRUD workflow (create/update/archive, status transitions, audit logging).
- **UC-STR-SRC** — the central CBIR contribution (upload → embedding → vector search → threshold → results).
- **UC-STR-CHK** — complex multi-step transactional checkout (address/delivery/payment).
- **UC-SYS-EMB** — automated background/ML processing, connects directly to the ML contribution.

### A.3 Compression table (for the 21 compressed use cases)
Convert each detailed spec to one compact row:
`Actor | UC ID | Name | Goal | Trigger | Related FRs`
Preserve a short unique-business-rule note where one genuinely exists (e.g., UC-ADM-STK reservation logic; UC-STR-PAY payment-state transitions). Do not force uniform sentence length.

### A.4 Deletion/compression map (pp. 39–72)
| Subsection | Pages | Remove | Redundant because | Survives in | Cross-ref | Est. saving | Risk |
|---|---|---|---|---|---|---|---|
| Repetitive admin UC detail blocks (VAR/IMG/TAX/OPT/ORD/ORD-ITEMS/PAY/LOC/STK/USR/ROL/SHP/REF) | ~9–11 | Full spec prose (actor/goal/pre/post/scenario/alt/exc flows) for each | All follow UC-ADM-PROD pattern; business flow duplicated in §2.4.5 | Compact table rows + surviving FR IDs | Table in §2.2; note "impl. in §2.4.5" | ~5–6 pp | Low |
| Repetitive customer UC detail blocks (AUT/BRW/CRT/OHI/PAY/PRF/SES) | ~6–8 | Full spec prose | Follow UC-STR-CHK/SRC patterns | Compact table rows | Table | ~3–4 pp | Low |
| System UC detail (UC-SYS-MNT) | ~1–2 | Full spec prose | Routine maintenance flow | Compact row | Table | ~1 pp | Low |
| Use-case overview/summary duplicate tables | ~1–2 | Merge `03-use-case-overview.typ` + `03-use-case-summary.typ` duplication | Same 25 UCs listed twice | One consolidated table | — | ~1–2 pp | Low |

**A.5 Traceability check (required):** every compressed UC still maps to its functional-requirement IDs in the compact table; UC IDs preserved verbatim; no FR loses its UC link.

**A.6 Target result:** ~33 pp → ~23–25 pp (saving ~8–10).

---

## B. §2.4.5 Frontend Applications — pp. 98–122 → target −8 to −10 pages

### B.1 Section-by-section map (from `04-implementations/05-frontend-ux/`)
| File/Feature | Keep | Compress | Merge | Remove | Reason |
|---|---|---|---|---|---|
| f0 frontend architecture | KEEP | | | | Component boundaries, API-client, state mgmt, decisions |
| f1 Visual Search | KEEP (full) | | | | Central CBIR implementation evidence |
| f3 Checkout | KEEP (one detailed example) | | | | Multi-step state + transactional behaviour |
| f6 Product Management | KEEP (one admin example) | | | | Demonstrates the normal admin pattern |
| f2 Catalogue/Cart | | COMPRESS | | | Ordinary storefront flow; reduce screenshots |
| f4 Order/Auth/Payment | | COMPRESS | | | Ordinary flows; keep auth/security integration note |
| f5 Profile | | COMPRESS | | | Routine |
| f7 Order/Payment (admin) | | COMPRESS | | | Routine admin; fold into matrix |
| f8 Inventory | | COMPRESS | | | Keep stock/reservation distinct behaviour in matrix |
| f9 User/Shipping (admin) | | COMPRESS | | | Routine; matrix |
| f10 System Processes | | COMPRESS | | | Keep unique process/background behaviour in matrix |

### B.2 Replacement structure (proposed new §2.4.5 outline)
1. Frontend Architecture (keep)
2. Storefront — Visual Search (full)
3. Storefront — Checkout (one detailed example)
4. Administration — Product Management (one detailed example)
5. Remaining storefront screens — compact matrix + one paragraph each (at most one screenshot proving a non-obvious fact)
6. Remaining admin modules — compact matrix (Module | Vue Component(s) | Main API | Distinctive Behaviour)

### B.3 Cross-reference rule
> "The functional behaviour of this feature is specified in §2.2.x; this section focuses on its frontend implementation."
Do not re-explain business flows already in §2.2.

### B.4 Expected saving
~20–24 pp → ~11–13 pp (~8–10 saved).

### B.5 Integrity check (required)
No requirement or research contribution disappears because its frontend explanation was removed; Visual Search + Checkout + auth/security + ML-interaction evidence all remain.

---

## C. Appendix D Database Schema — pp. 153+ → target −5 to −10 pages

### C.1 Keep detailed
Product · Image Embedding / Product Image Embedding · Order · Payment Capture · Stock Item · Stock Reservation (verify exact names in `backmatter/appendices/d-database.typ`). Preserve: aggregate boundaries, vector(512), model isolation, indexing (HNSW/IVFFlat), concurrency, idempotency, state transitions, inventory reservation, transaction integrity.

### C.2 Compress
Ordinary CRUD/reference tables → `Table | Purpose | Key Relationships | Important Columns/Constraints` (do not list every routine timestamp/ID/audit/metadata field). Do NOT delete the vector-dimensionality/model-isolation explanation.

### C.3 Proposed structure
1. Schema overview & ownership
2. Core aggregates (Product, Image Embedding, Order, Payment Capture, Stock Item, Stock Reservation) — detailed
3. Reference/CRUD tables — compact summary table
4. pgvector integration (vector(512), model_name isolation, per-model dims, HNSW/IVFFlat) — detailed

### C.4 Expected saving
~30 pp → ~20–23 pp (~5–10 saved).

### C.5 Technical integrity audit (required)
Vector dimension · model isolation · HNSW/IVFFlat · schema ownership · cross-schema references · concurrency all preserved and unaltered.

---

## D. Totals
| Section | Before | After | Saving |
|---|---|---|---|
| §2.2 | ~33 | ~23–25 | −8 to −10 |
| §2.4.5 | ~20–24 | ~11–13 | −8 to −10 |
| Appendix D | ~30 | ~20–23 | −5 to −10 |
| **Total** | **183** | **~155–163** | **−21 to −30** |

Target **met** (20–30 pp). Do not exceed ~30 pp without a separate non-destructive-review justification.

---

## E. Fixed technical issues to apply during these cuts
1. **Statistical overclaim (CRITICAL):** Fashion-CLIP ±2SD lower bound (0.9216) OVERLAPS all other models' upper bounds (incl. both CNNs 0.9229/0.9246) — it does NOT "exceed" them. Reword §3.5/§3.7 accordingly.
2. **Efficiency ratio error (MAJOR):** §3.6 "42.6 ms is 2.2× faster than DINOv2 (126.3 ms)" — actual 126.3/42.6 = 2.96 ≈ **3.0×**, not 2.2×. Also the "2.1× higher than the two CLIP variants' lower range" throughput claim is mislabeled (21.4/4.0=5.4×, 21.4/11.9=1.8×; 2.1× = 21.4/10.2, i.e. vs DINOv2/ResNet).
3. **RQ3 (MAJOR):** reword "independent scaling and fault isolation were achieved" unless experimentally demonstrated.
4. **Terminology (MAJOR):** "production-ready" → "production-oriented"/"deployment-feasible" unless justified.
5. **RAM (MODERATE):** reconcile Ch3 ranges vs Appendix A "N/A"; standardize the not-instrumented disclosure.
