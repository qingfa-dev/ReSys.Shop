# Phase 6 Peer Review Report

**Thesis:** BUILDING A FASHION E-COMMERCE APPLICATION WITH RECOMMENDATION AND IMAGE-BASED PRODUCT SEARCH  
**Author:** Nguyen Thanh Phat (B2005853)  
**Advisor:** Dr. Tran Cong An  
**Institution:** Can Tho University (CTU), High-Quality Program  
**Review Date:** August 2026  
**Remediation Status:** 22 findings fixed, 3 citation corrections, 5 PNG charts regenerated, benchmark numbers reconciled (verification script: PASS)

---

## 1. Dimension Scores

### 1.1 Originality — 6.5/10

**Justification:** The thesis correctly identifies a genuine engineering gap: bridging Python ML research with .NET enterprise e-commerce stacks. The contribution is explicitly framed as "architectural, not algorithmic," which is honest and appropriate for a bachelor thesis. The four contribution differentiators (polyglot architecture, vector-native consistency, commodity hardware benchmarking, applied model comparison) are valid and well-articulated in `f6/03-contributions.typ`. The pgvector-acid-compliance angle is a practical insight.

However, the novelty is incremental rather than fundamental. The system integrates existing components (.NET, Vue, FastAPI, pgvector, pre-trained models) without introducing new algorithms, architectures, or theoretical frameworks. The "engineering gap" framing is somewhat generic — many integration projects could claim the same. The thesis would benefit from a clearer articulation of what is specifically novel about *this* integration versus, say, a Flask+Django equivalent. The research gap in `f6/01-academic.typ` and `f6/02-commercial.typ` is adequate but brief (7 and 20 lines respectively).

**Verdict:** Acceptable for a bachelor thesis; would need stronger novelty claims for a master's thesis or journal publication.

---

### 1.2 Methodological Rigor — 7.0/10

**Justification:** The benchmark design is sound and well-executed for its scope.

**Strengths:**
- 3-fold stratified cross-validation preserving category distribution (Section 3.4, `04-benchmark-protocol.typ:49`)
- Seven accuracy metrics at three depths (mAP, P@K, R@K for K=5,10,20) — comprehensive for a fashion retrieval benchmark
- Five efficiency metrics capturing different operational dimensions
- Exact cosine search for accuracy metrics isolates model quality from index effects (`04-benchmark-protocol.typ:49`)
- Statistical separation analysis via mean ± 2SD bounds (`05-retrieval-performance.typ:36`)
- Verification script confirms all numbers are internally consistent

**Weaknesses:**
- **No formal statistical significance tests.** The thesis relies on non-overlapping mean ± 2SD bounds for separation claims (`05-retrieval-performance.typ:36`), but does not report p-values, confidence intervals with formal statistical tests (e.g., paired t-test, Wilcoxon signed-rank), or effect sizes. The statement "statistically meaningful separation" (`05-retrieval-performance.typ:36`) is informal. With only 3 folds, formal testing has limited power, but should still be attempted or explicitly acknowledged as a limitation.
- **Category-only relevance is coarse.** The binary category-label ground truth treats all same-category products as equally relevant (`04-benchmark-protocol.typ:3`), which inflates mAP. The thesis acknowledges this limitation (`07-model-comparison.typ:32`) but does not quantify its impact — e.g., by comparing category-only vs. enriched-label results. The enriched-label mention in the conclusion (`ch4-conclusion.typ:37`) is introduced without data.
- **Single hardware configuration.** All measurements on one CPU (Intel i7-1165G7), limiting generalizability. This is acknowledged but could be mitigated by normalizing metrics against a baseline.
- **EfficientNet-B0 high variance.** The latency standard deviation of 26.6 ms (mean 37.8 ms, CV = 70%) suggests measurement instability (`06-efficiency-metrics.typ:22`). This is not discussed or investigated.

**Verdict:** Adequate for a bachelor thesis; the methodological gaps are acknowledged limitations rather than hidden flaws.

---

### 1.3 Evidence Sufficiency — 7.5/10

**Justification:** The thesis presents a complete evidence package: tables with mean ± SD for all metrics, chart visualizations for all major comparisons, a requirements traceability table, and 28 passing functional test cases.

**Strengths:**
- The traceability table (`ch4-conclusion.typ:57-102`) is excellent — every objective and RQ maps to a specific chapter section and finding
- All benchmark numbers are internally consistent (verified by `verify_remediation.py`)
- The three-table structure (accuracy, efficiency, combined) provides layered evidence
- The "Answer to RQ" format directly ties findings to research questions

**Weaknesses:**
- **No comparison with published benchmarks.** The thesis does not compare its mAP results against published results for the same models on standard benchmarks (e.g., DeepFashion, SOP). This makes it impossible to assess whether the measured performance is typical or anomalous for these models.
- **Limited analysis of failure modes.** The thesis reports aggregate metrics but does not analyze which product categories or image types the models fail on. A confusion analysis or per-category breakdown would strengthen the evidence.
- **RAM measurement gap.** The unreliable RAM data (`---` in tables, `06-efficiency-metrics.typ:36`) means memory trade-offs are only estimated ("~100 MB to >600 MB"). For a resource-efficiency thesis, this is a notable gap.
- **EfficientNet-B0 variance unexplained.** The 70% CV for latency is unusual and warrants investigation (e.g., thermal throttling, OS scheduling artifacts).

**Verdict:** Sufficient for the stated claims; would benefit from failure analysis and external benchmark comparison.

---

### 1.4 Argument Coherence — 8.0/10

**Justification:** The thesis follows a logical structure: problem → methodology → implementation → evaluation → conclusion. Each RQ is introduced in Chapter 1, answered in Chapter 3, and summarized in Chapter 4. The argument chain is:

1. Fashion search fails with text → CBIR solves this
2. Multiple embedding models exist → which is best for fashion?
3. Models differ in accuracy and speed → which trade-off is optimal?
4. Can this work in a .NET enterprise stack? → sidecar architecture demonstrates viability

This chain is coherent and each link is supported by evidence.

**Strengths:**
- The "Answer to RQ" format (`05-retrieval-performance.typ:38`, `06-efficiency-metrics.typ:37-38`, `07-model-comparison.typ:44`) directly addresses each question with specific numbers
- The five-finding synthesis (`07-model-comparison.typ:36-42`) effectively distills the evaluation into actionable insights
- The requirements traceability table closes the loop completely

**Weaknesses:**
- The "architectural, not algorithmic" framing (`ch1-introduction.typ:7`) could be more strongly integrated throughout — the abstract and conclusion occasionally drift into implications that go beyond the demonstrated contribution
- The deployment recommendations (`07-model-comparison.typ:26-28`) are well-reasoned but the evidence supporting them is limited to a single hardware profile

**Verdict:** Coherent and well-structured; the argument holds together.

---

### 1.5 Writing Quality — 6.0/10

**Justification:** The writing is clear, technically accurate, and appropriate for a Vietnamese undergraduate thesis. However, there are several issues.

**Strengths:**
- Technical terminology is consistently used and accurate (mAP, P@K, cosine similarity, IVFFlat, etc.)
- Figures and tables are well-labeled with clear captions
- The structure is logical with good use of headings and numbered lists
- The abstract is concise and data-rich (both EN and VI)

**Weaknesses:**
- **Inconsistent sentence construction.** Some sentences are long and complex (`05-retrieval-performance.typ:30`), others are terse (`05-retrieval-performance.typ:34`). This creates an uneven reading rhythm.
- **Redundancy.** The "Answer to RQ" conclusions repeat numbers that appear in adjacent tables. For example, `05-retrieval-performance.typ:38` restates all the P@K values already visible in `tbl-aggregate` on the same page.
- **Passive voice overuse.** Many sentences use passive constructions ("was measured," "were collected," "is recommended") where active voice would be clearer. This is a common pattern in Vietnamese-to-English academic writing.
- **Minor grammatical issues.** A few awkward constructions: "the evaluation detects large effects but may miss smaller differences" (`07-model-comparison.typ:32`) — "detects" is semantically odd here; "evaluates" or "is powered to detect" would be more precise.
- **The English is B1-B2 level.** This is appropriate for the author's proficiency and the thesis reads authentically — it does not sound AI-generated, which is a strength. However, some phrasing could be tightened without changing the author's voice.

**Verdict:** Adequate for a bachelor thesis. The writing conveys the content accurately and reads as genuinely student-authored.

---

## 2. Weighted Total

| Dimension | Weight | Score | Weighted |
|-----------|--------|-------|----------|
| Originality | 20% | 6.5 | 1.30 |
| Methodological Rigor | 25% | 7.0 | 1.75 |
| Evidence Sufficiency | 25% | 7.5 | 1.88 |
| Argument Coherence | 15% | 8.0 | 1.20 |
| Writing Quality | 15% | 6.0 | 0.90 |
| **Total** | **100%** | | **7.03** |

**Overall Score: 7.0/10** — Solid bachelor thesis with clear contributions and sound methodology. Meets CTU standards for an undergraduate thesis.

---

## 3. Strengths

1. **Benchmark numerical consistency.** All reported numbers (mAP, P@K, R@K, latency, throughput, storage, derived percentages) are internally consistent across abstract, tables, charts, and conclusion. The verification script confirms this. This is a non-trivial achievement after remediation.

2. **Clear problem framing.** The four-compound-inefficiency structure (`ch1-introduction.typ:11-19`) is memorable and well-argued. The "architectural, not algorithmic" contribution statement is honest and appropriately scoped.

3. **Excellent traceability.** The requirements traceability table (`ch4-conclusion.typ:57-102`) maps every objective and RQ to specific chapter sections and findings. This is above average for a bachelor thesis.

4. **Practical deployment recommendations.** The accuracy-efficiency trade-off analysis with concrete deployment guidance (`07-model-comparison.typ:26-28`) is genuinely useful for practitioners.

5. **Bilingual abstracts.** Both English and Vietnamese abstracts are data-rich and consistent in content.

6. **Comprehensive functional testing.** The 28 test cases covering visual search, ML pipeline, cart/checkout, and admin management (`03-testing-result.typ`) demonstrate end-to-end system functionality.

7. **Honest limitation acknowledgment.** The thesis does not oversell its findings — limitations are clearly stated in Sections 1.4 (Scope), 3.7 (Limitations), and Chapter 4.

---

## 4. Critical Issues (Must Fix Before Submission)

**C1. Missing statistical significance reporting.**
The thesis claims "statistically meaningful separation" (`05-retrieval-performance.typ:36`) based on non-overlapping mean ± 2SD bounds. This is an informal heuristic, not a statistical test. With 3 folds, formal testing has limited power, but the thesis should either:
- Report paired t-test or Wilcoxon signed-rank p-values for key model pairs (Fashion-CLIP vs CLIP-generic, Fashion-CLIP vs EfficientNet-B0), OR
- Explicitly state that with only 3 folds, formal significance testing is underpowered and the non-overlapping bounds are presented as indicative rather than conclusive.

**Location:** `05-retrieval-performance.typ:36`, `ch4-conclusion.typ:11`

**C2. Enriched-label evaluation claim without supporting data.**
The conclusion states: "The enriched-label evaluation reduces P@20 substantially (from ~0.90 under category-only labels to ~0.30 under category+colour+pattern labels)" (`ch4-conclusion.typ:37`). This claim appears nowhere else in the thesis — no table, no chart, no benchmark section presents enriched-label results. Either:
- Add the enriched-label results as an appendix or additional table in Chapter 3, OR
- Remove this claim from the conclusion if the data was not formally collected.

**Location:** `ch4-conclusion.typ:37`

---

## 5. Important Issues (Should Fix)

**I1. EfficientNet-B0 latency variance unexplained.**
The 26.6 ms standard deviation on a 37.8 ms mean (CV = 70%) is unusually high (`06-efficiency-metrics.typ:22`). This suggests measurement instability that could affect the reliability of the latency comparison. A brief discussion of possible causes (thermal throttling, OS scheduling, garbage collection) and mitigation (warming runs, median instead of mean) would strengthen the methodology.

**Location:** `06-efficiency-metrics.typ:22-23`

**I2. No external benchmark comparison.**
The thesis does not compare its mAP results against published benchmarks for the same models. For example, Fashion-CLIP's published performance on DeepFashion or SOP benchmarks would provide context for whether 0.9309 mAP is typical or anomalous for this model. This comparison would validate the experimental setup.

**Location:** `04-benchmark-protocol.typ`, `05-retrieval-performance.typ`

**I3. DINOv2 mentioned but not evaluated.**
The model architecture table (`04-benchmark-protocol.typ:16`) lists "DINOv2 ViT-S/14 (384-dim) -- framework-supported" but this model is never evaluated or discussed in the results. Either evaluate it and add results, or remove it from the model table to avoid confusion.

**Location:** `04-benchmark-protocol.typ:16`

**I4. RAM measurement gap.**
All RAM values in the efficiency table are dashes (`06-efficiency-metrics.typ:22-25`). The thesis acknowledges psutil unreliability but provides only rough estimates ("~100 MB to >600 MB"). For a thesis evaluating resource efficiency, this is a notable data gap. Consider using an alternative measurement method (e.g., `/proc/[pid]/status`, `time -v`, or Docker stats) or explicitly framing RAM as out of scope.

**Location:** `06-efficiency-metrics.typ:22-25`, `04-benchmark-protocol.typ:41`

---

## 6. Minor Issues (Nice to Have)

**M1. Redundant restatement of results.**
The "Answer to RQ" paragraphs (`05-retrieval-performance.typ:38`, `06-efficiency-metrics.typ:37-38`) repeat all numerical values already visible in adjacent tables. Consider tightening these to focus on interpretation rather than data restatement.

**M2. Commercial comparison table depth.**
The commercial systems comparison (`f6/02-commercial.typ:6-18`) is brief (4 products, 2 columns). Adding pricing model, API latency, or accuracy data from published sources would strengthen the gap analysis.

**M3. Passive voice overuse.**
Many sentences use passive constructions where active voice would be clearer. Examples: "was measured" → "we measured"; "were collected" → "the benchmark collected". This is a common pattern in Vietnamese-to-English academic writing and is acceptable for the author's proficiency level.

**M4. Bibliography balance.**
The bibliography (36 entries) has a good mix of books, journal articles, conference papers, and technical reports. However, several entries are arXiv preprints (e.g., `li2023fashion`, `hermans2017defense`, `oquab2023dinov2`, `sun2023evaclip`). For a bachelor thesis this is acceptable, but noting which have been peer-reviewed vs. preprint would be helpful.

**M5. Sentence-level precision.**
`07-model-comparison.typ:32`: "the evaluation detects large effects but may miss smaller differences" — "detects" is semantically odd; "is powered to detect" or "can detect" would be more precise.

---

## 7. Recommendations

1. **Address C1 (statistical significance) by adding a brief paragraph.** In `05-retrieval-performance.typ`, after the non-overlapping bounds analysis, add: "Note: With only 3 folds, formal significance tests (e.g., paired t-test) would have limited statistical power. The non-overlapping 95% confidence bounds are presented as indicative evidence of separation rather than formal hypothesis test results." This is honest and methodologically sound.

2. **Address C2 (enriched-label data) by either adding the data or removing the claim.** If enriched-label results were computed during benchmarking, add them as a table in Section 3.5 or as an appendix entry. If not, remove the sentence from `ch4-conclusion.typ:37` and note it as future work.

3. **Address I1 by adding a sentence on variance causes.** In `06-efficiency-metrics.typ`, after the efficiency table, note: "The high standard deviation for EfficientNet-B0 may reflect CPU frequency scaling or OS scheduling variability on the single-test hardware configuration."

4. **Address I3 by removing DINOv2 from the model table** if it was not evaluated, or by adding a brief discussion of why it was excluded despite being framework-supported.

5. **Consider adding per-category analysis** as a future work direction or appendix. Even a simple breakdown of mAP by category (Apparel, Accessories, Footwear, etc.) would provide useful failure-mode insight.

---

## 8. Assessment

**Verdict: Needs Minor Revision → Ready for Submission (after addressing C1 and C2)**

**Rationale:** The thesis is a solid bachelor-level contribution with sound methodology, internally consistent results, and clear argumentation. The two critical issues (statistical significance reporting and enriched-label claim) are fixable without structural changes. After addressing C1 and C2, the thesis meets CTU standards for a bachelor thesis in the High-Quality Program.

The remediation effort was successful — benchmark numbers are consistent, figures are regenerated, and citations are corrected. The writing reads authentically as a Vietnamese undergraduate student's work, which is appropriate for the context.

**Confidence:** High. The evidence base is complete and internally consistent. The weaknesses are typical of an undergraduate thesis and do not undermine the core contribution.
