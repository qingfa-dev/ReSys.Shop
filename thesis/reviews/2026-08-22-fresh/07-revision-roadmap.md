# Revision Roadmap (prioritized)

> Generated from the 2026-08-22 fresh full panel review. Maps each finding to a concrete fix in the `.typ` source. After edits, run `typst compile main.typ` to rebuild `main.pdf`.

---

## TIER 1 — Must Fix (closes the 3 DA CRITICALs; blocks acceptance)

### R1. Reconcile the title-scope mismatch (DA-C1, EIC W1, R2 W2, R3 W1)
The title promises "Recommendation and Image-Based Product Search" but no recommender system exists. The revised text clarifies this in Scope and Conclusion, but the title itself remains unchanged.

**Files to modify:**
- `info.typ` → change English title from "BUILDING A FASHION E-COMMERCE APPLICATION WITH RECOMMENDATION AND IMAGE-BASED PRODUCT SEARCH" to "BUILDING A FASHION E-COMMERCE APPLICATION WITH VISUAL SEARCH AND IMAGE-BASED PRODUCT DISCOVERY"
- `info.typ` → update Vietnamese title accordingly
- `chapters/part1/ch1-introduction.typ` → verify Scope section no longer mentions "embedding-based recommendations" without qualification
- `chapters/part3/ch4-conclusion.typ` → verify title is not referenced by its old form

**Acceptance criteria:** Title no longer promises "Recommendation" without implementation; or recommendation capability is implemented and described.

---

### R2. Soften the DINOv2 comparison claim (DA-C2, R1 W1)
Fashion-CLIP's 0.40% mAP lead over DINOv2 ViT-S/14 is within one standard deviation of 3-fold CV. The thesis should soften from "outperformed" to "achieved highest mean mAP" without claiming statistical superiority.

**Files to modify:**
- `chapters/part2/ch3-evaluation/05-retrieval-performance.typ` → §3.5 RQ1 answer: change "Fashion-CLIP outperformed all five other models" to "Fashion-CLIP achieved the highest mean mAP across all models, though the gap to DINOv2 ViT-S/14 (0.40%) is within measurement uncertainty for 3-fold CV"
- `chapters/part2/ch3-evaluation/07-model-comparison.typ` → §3.7.3: soften any claims of Fashion-CLIP "leading" or "outperforming" without qualification
- `chapters/part3/ch4-conclusion.typ` → RQ1 answer: consistent softening

**Acceptance criteria:** No claim of statistically significant superiority over DINOv2 without a formal test.

---

### R3. Add RAM measurement (DA-C3, R1 W2)
One of five efficiency metrics (RAM) is reported as "N/A" for all six models. The thesis acknowledges psutil issues but provides no alternative.

**Files to modify:**
- `chapters/part2/ch3-evaluation/06-efficiency-metrics.typ` → Table 67: replace "N/A" with approximate RAM values using an alternative measurement method
- `chapters/part2/ch3-evaluation/06-efficiency-metrics.typ` → §3.6 text: explain the alternative method used and report approximate ranges

**Acceptance criteria:** All six models have reported RAM values in Tables 66–67.

---

## TIER 2 — Should Fix (credibility / completeness)

### S1. Add recent visual search references (R2 W1)
The related work cites DeepFashion (2016), FashionIQ (2021), and Fashion-CLIP (2022) but misses more recent (2023–2025) visual search systems.

**File:** `chapters/part2/ch1-background/f6/01-academic.typ`
- Add 2–3 references to recent visual search systems (e.g., Google multi-modal search, Amazon visual search, Pinterest Lens)
- Note they are out of scope if relevant

### S2. Add ethical considerations (R3 W2)
No discussion of algorithmic bias, privacy, or environmental impact of visual search in fashion e-commerce.

**File:** `chapters/part3/ch4-conclusion.typ`
- Add a brief "Ethical Considerations" subsection in Limitations (§4.3)
- Even one paragraph acknowledging these issues is appropriate

---

## TIER 3 — Optional polish

- **S3** (DA-m2): Rank Future Work items by effort/impact in §4.4.
- **S4** (R1 W3): Clarify mAP definition in Table 65 to "Query-averaged Mean Average Precision."
- **S5** (R2 W3): Add dataset bias discussion in §4.3 (single Indian platform, photography conventions).

---

## Verification before submission

1. `grep -rniE "recommendation" info.typ chapters/` → verify title no longer promises "Recommendation" without qualification
2. `grep -rniE "outperformed|outperform" chapters/` → verify DINOv2 claim is softened
3. `grep -rniE "N/A" chapters/part2/ch3-evaluation/06-efficiency-metrics.typ` → verify RAM values are reported
4. `typst compile main.typ` succeeds; `main.pdf` rebuilt
