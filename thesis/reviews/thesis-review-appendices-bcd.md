# Thesis Review — Appendices B, C, D

**Scope of this file:** printed pages 131–152 (Appendix B: Dataset Composition, Appendix C: Hardware Specifications, Appendix D: Database Schema, all 40 tables). This closes out the full-document pass.

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. "Variable Vector Dimensions" is contradicted by the actual schema, shown four separate times

**Location:** the claim is in §2.3.4.4 (Chapter 2, "Key Design Decisions"), but the contradiction is fully visible once you look at Appendix D's concrete schema, so it belongs in this file.

> §2.3.4.4: *"Variable Vector Dimensions: pgvector columns support per-model dimensionalities: 384 (DINOv2-S), 512 (Fashion-CLIP), 768 (DINOv2-B), 1280 (EfficientNet-B0), 2048 (ResNet-50)."*

versus, stated identically in **four different places**:

> §2.3.4.3: *"stores feature vectors in an embedding column defined as **vector(512)**."*
> §2.4.3.2: *"Embeddings are stored in a **vector(512)** column with model-aware discriminators."*
> Appendix D, Table 82: *"vector(512); IVFFlat cosine index."*
> Appendix D.9: *"Vector embeddings are stored in catalog.image_embeddings with column type **vector(512)**, made nullable in migration 20260804013350."*

**Why this is a hard contradiction, not just an inconsistency:** pgvector's `vector(N)` column type is fixed-width by definition, a column declared `vector(512)` can only physically store 512-dimensional vectors. It cannot also hold a DINOv2-S embedding (384 dimensions), a DINOv2-B embedding (768), an EfficientNet-B0 embedding (1280), or a ResNet-50 embedding (2048) in the same column. There's no truncation, padding, or per-model column strategy mentioned anywhere that would reconcile this. So the "Variable Vector Dimensions" bullet describes a capability the concrete, four-times-repeated schema doesn't actually have.

**Two possible explanations, both worth checking:**
1. The system only ever actually stores Fashion-CLIP embeddings (512-dim) in production, and the "variable dimensions" bullet describes an aspirational/future capability rather than what's implemented, in which case the bullet should say so explicitly.
2. There's a real per-model dimension-handling mechanism in the codebase (e.g., separate tables or columns per model) that simply wasn't captured correctly when this schema was documented, in which case Appendix D needs to show it.

**Fix:** Check the actual EF Core migration files and `IEntityTypeConfiguration<ImageEmbedding>` class directly. Either the "Variable Vector Dimensions" bullet needs to be removed/qualified, or the schema documentation (§2.3.4.3, §2.4.3.2, Table 82, D.9) needs to be corrected to show however dimension variability is actually handled.

---

## 🟠 REWRITE — 2. Appendix C's claim that Chapter 3 and Appendix A share "a single workstation" sharpens the earlier unresolved discrepancy

**Location:** Appendix C opening (p.133)

> "All benchmark results reported in Chapter 3 and Appendix A were collected on a single workstation."

**Why this matters here:** this is a direct, explicit assertion that Chapter 3's Table 67/68 and Appendix A's Tables 71–75 come from the **same** hardware, software environment, and (by implication) the same run. Every earlier file in this review series found that these two sets of numbers don't actually match (Table 67's mAP of 0.8788 vs. Appendix A.1's 0.9309, Table 68's latency figures vs. Appendix A.4's Table 74). This sentence removes the most charitable explanation for that mismatch, "maybe they ran on different hardware or at different times", and replaces it with an explicit claim that they're the same experiment. That makes the discrepancy harder to explain away, not easier: if it really was one workstation and one benchmark run, the two tables should be identical, and they aren't.

**Fix:** once the Table 67/68 vs. Appendix A reconciliation is done (see the Chapter 3 and Part 3 review files), come back to this sentence and either confirm it's accurate (single run, tables now match) or correct it to reflect however many actual runs were involved.

---

## 🟢 KEEP — 3. Software stack versions checked out as accurate for the stated timeframe

**Location:** Table 78, §C.2 (p.133): PyTorch 2.13.0, TorchVision 0.28.0, HuggingFace Transformers 5.14.1, OpenCLIP 3.3.0, NumPy 2.5.1

I checked PyTorch and Transformers against their actual release histories. **Both check out precisely**: PyTorch reached the 2.11–2.13 range by mid-2026 (confirmed via official release notes), and Hugging Face Transformers 5.14.1 was released July 16, 2026, exactly matching the version stated here. This is a good sign, these aren't generic-sounding placeholder version numbers, they're specific and turned out to be genuinely accurate, which suggests real care was taken pinning the actual environment used. **No action needed.**

---

## 🟠 REWRITE — 4. "Sequential" image selection paired with "preserves the natural category distribution" doesn't quite follow

**Location:** Appendix B.1 (p.131)

> "For the thesis evaluation, a controlled subset of 5,000 images was selected. Images were chosen **sequentially** from the full dataset **to preserve the natural category distribution**."

**Problem:** this is a methodological claim that doesn't obviously hold together. Selecting images sequentially (i.e., taking the first N records in file/index order) preserves the natural distribution *only if* the underlying dataset is already randomly shuffled. Many scraped e-commerce catalogue dumps are naturally grouped or sorted (by category, by upload batch, by product ID range), in which case sequential selection would *distort* the distribution toward whatever appears first in the listing, not preserve it. As written, the sentence asserts a conclusion ("preserves the distribution") without justifying the premise (that sequential order is representative).

This doesn't necessarily mean the actual 5,000-image sample is wrong, if the Kaggle dataset happens to already be shuffled, sequential selection would genuinely work, but the thesis doesn't say that, and a methodology-focused reader (exactly the kind of reader Chapter 3 and this appendix are written for) is likely to ask the question.

**Fix:** either confirm and state explicitly that the source dataset is pre-shuffled (making sequential selection valid), or switch to describing the actual selection method accurately if it was, for example, stratified random sampling instead of pure sequential order.

---

## 🟢 KEEP — 5. Bounded context count in Appendix D matches the corrected "eight," and reinforces PostgreSQL 17

**Location:** Appendix D opening (p.135)

> "The database uses **PostgreSQL 17** with pgvector via EF Core 10. Five migrations, 33 IEntityTypeConfiguration<T> classes across **eight** bounded contexts."

This is good confirming evidence for two things already flagged: it reaffirms "eight" bounded contexts (matching the correction made to the "nine" typo found in the Chapter 2 review), and it's the eighth mention of "PostgreSQL 17" in the thesis, further reinforcing that Chapter 3's Table 66 ("PostgreSQL 16") is the lone outlier that needs correcting, not the other way around. Appendix D.1 through D.8 map cleanly onto the eight named contexts (Catalog, Identity, Ordering, Payment, Inventory, Shipping, Profile, Location). **No action needed**, this section is consistent with the corrections already recommended elsewhere.

---

## 🟢 KEEP — 6. Inventory schema matches the described stock-reservation architecture

**Location:** Appendix D.5 (p.145–148), `stock_items`, `stock_reservations`, `stock_movements`, `stock_transfers`

These tables consistently reference "xmin concurrency" (PostgreSQL's built-in system column used for optimistic concurrency control), matching the row-versioning approach described in Chapter 2's architecture section. The reservation table (`stock_reservations`) includes an `expires_at_utc` auto-release timeout field, consistent with the 15-minute reservation timeout claim verified earlier in the Chapter 2 review. No inconsistencies found in this section. **No action needed.**

---

## 🟢 KEEP — 7. Phantom "Chapter 6" reference reappears, already flagged, noted here for completeness

**Location:** Appendix B.3 (p.132): *"Category + Colour labels... This is the primary relevance criterion used in Chapter 6."*

This is the same broken cross-reference already flagged in the very first full-document pass and again in the Chapter 3/Appendix A review (Appendix A.2's Table 72 caption uses nearly identical wording). It appears at least twice now across Appendices A and B. No new finding, just confirming it needs to be part of the same fix, most likely this should say "Chapter 3" throughout.

---

## Not checked in this pass

**Appendix D.1–D.8's remaining ~35 table definitions:** I spot-checked the Catalog and Inventory schemas in detail and skimmed the rest (Identity, Ordering, Payment, Shipping, Profile, Location) for structural consistency (foreign keys, cascade rules, naming conventions). Nothing else stood out as contradicting earlier chapters, but a full field-by-field audit against the actual EF Core migrations would be the only way to be fully certain, which is outside what I can verify from the thesis text alone.

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

---

## Summary table

| # | Item | Severity | Action |
|---|---|---|---|
| 1 | "Variable Vector Dimensions" claim contradicted by fixed `vector(512)` schema, shown 4x | High | Correct, verify against actual migrations |
| 2 | Appendix C's "single workstation" claim sharpens the unresolved Table 67/68 vs. Appendix A discrepancy | Medium | Resolve alongside Chapter 3 finding |
| 3 | Software stack versions (PyTorch 2.13.0, Transformers 5.14.1) | — | Keep, verified accurate |
| 4 | "Sequential" selection claimed to "preserve natural distribution" | Medium | Rewrite / clarify |
| 5 | Bounded context count (8) and PostgreSQL 17 reaffirmed | — | Keep, consistent with prior corrections |
| 6 | Inventory schema matches described architecture | — | Keep |
| 7 | Phantom "Chapter 6" reference, 2nd occurrence | — | Keep (fix alongside prior instance) |

---

*This completes the full-document review: Part 1, Chapter 1, Chapter 2 (2.1–2.4), Chapter 3, Part 3, the References list, and Appendices B–D are all covered across eight files now. The natural next step is a consolidated priority list pulling every 🔴 finding across all eight files into one fix-it-in-order checklist, since several items (the "eleven models" figure, the PostgreSQL version, the Table 67/68 vs. Appendix A reconciliation, "Chapter 6") recur across multiple files and are easiest to fix once, together. Let me know if you'd like that.*
