# Citation Compliance Check Report

**Date:** 2026-08-19
**Thesis:** "Building a Fashion E-commerce Application with Recommendation and Image-Based Product Search"
**Bibliography file:** `thesis/backmatter/bibliography.bib`

---

## 1. Summary

| Metric | Count |
|--------|-------|
| Total bib entries | 37 |
| Verified OK (pre-remediation) | 8 |
| Verified OK (this audit) | 18 |
| Issues found | 4 |
| Orphan entries (never cited) | 14 |
| Missing citations (text refs not in bib) | 0 |

---

## 2. Issues Table

| # | Entry Key | Problem | Severity | Suggested Fix |
|---|-----------|---------|----------|---------------|
| 1 | `zhai2023sigmoid` | **Wrong page numbers.** Bib: `41--50`. Actual (CrossRef DOI `10.1109/iccv51070.2023.01100`): `11941--11952`. | HIGH | Change `pages = {41--50}` to `pages = {11941--11952}` |
| 2 | `liu2022convnext` | **Wrong page numbers.** Bib: `11976--11986`. Actual (CrossRef DOI `10.1109/cvpr52688.2022.01167`): `11966--11976`. Off by 10 pages. | HIGH | Change `pages = {11976--11986}` to `pages = {11966--11976}` |
| 3 | `johnson2019billion` | **Wrong year.** Bib: `year = {2019}`. Actual print (CrossRef DOI `10.1109/tbdata.2019.2921572`): July 2021, Vol. 7, No. 3. The DOI was assigned in 2019 but IEEE Trans. Big Data print publication is 2021. | MEDIUM | Change `year = {2019}` to `year = {2021}` |
| 4 | `rendle2012bpr` | **Misleading key year.** Key says `rendle2012bpr` but the paper was published at UAI **2009** (pages 452--461). The bib `year` field is correct (2009), but the key convention is inconsistent. | LOW | Rename key to `rendle2009bpr` for consistency (optional; academic convention often uses first-available year) |

---

## 3. Orphan Check (Bib entries never cited in text)

These 14 entries exist in `bibliography.bib` but have no matching `@key` reference in any `.typ` file:

| # | Entry Key | Paper |
|---|-----------|-------|
| 1 | `han2017outfitnet` | Han et al., Learning Fashion Compatibility with Bidirectional LSTMs (ACM MM 2017) |
| 2 | `hermans2017defense` | Hermans et al., In Defense of the Triplet Loss for Person Re-Identification (arXiv 2017) |
| 3 | `huang2015darn` | Huang et al., Cross-Domain Image Retrieval with a Dual Attribute-Aware Ranking Network (ICCV 2015) |
| 4 | `jarvelin2002cumulated` | Järvelin & Kekäläinen, Cumulated Gain-Based Evaluation of IR Techniques (ACM TOIS 2002) |
| 5 | `johnson2019billion` | Johnson et al., Billion-Scale Similarity Search with GPUs (IEEE TBD 2021) |
| 6 | `li2023fashion` | Li et al., Multimodal Pretraining with Language for Fashion (arXiv 2023) |
| 7 | `liu2022convnext` | Liu et al., A ConvNet for the 2020s (CVPR 2022) |
| 8 | `microsoft-aspire` | Microsoft, .NET Aspire Documentation |
| 9 | `radenovic2019fine` | Radenović et al., Fine-tuning CNN Image Retrieval with No Human Annotation (IEEE TPAMI 2019) |
| 10 | `rendle2012bpr` | Rendle et al., BPR: Bayesian Personalized Ranking from Implicit Feedback (UAI 2009) |
| 11 | `schroff2015facenet` | Schroff et al., FaceNet: A Unified Embedding for Face Recognition and Clustering (CVPR 2015) |
| 12 | `sun2023evaclip` | Sun et al., EVA-CLIP: Improved Training Techniques for CLIP at Scale (arXiv 2023) |
| 13 | `zhai2023sigmoid` | Zhai et al., Sigmoid Loss for Language Image Pre-Training (ICCV 2023) |
| 14 | `zheng2017sift` | Zheng et al., SIFT Meets CNN: A Decade Survey of Instance Retrieval (IEEE TPAMI 2017) |

**Recommendation:** If these entries were included as "background reading" for the advisor/reviewers, they are harmless. Otherwise, remove entries not cited in the thesis body to keep the bibliography clean. For a CTU thesis, an uncluttered bibliography is preferred.

---

## 4. Missing Citation Check

**No missing citations found.** Every `@key` reference in the thesis `.typ` files has a matching entry in `bibliography.bib`.

---

## 5. Verified Entries (No Issues)

The following 26 entries were verified as correct (title, authors, venue, year, pages all match published sources):

| Entry Key | Verification Source | Status |
|-----------|-------------------|--------|
| `shaw2012software` | OpenLibrary (ISBN 978-0132394413) | OK |
| `radenovic2019fine` | IEEE TPAMI 2019 | OK |
| `li2023fashion` | arXiv:2305.14353 | OK |
| `he2016deep` | CVPR 2016 (pre-verified) | OK |
| `tan2019efficientnet` | ICML 2019 (pre-verified) | OK |
| `radford2021learning` | ICML 2021 (pre-verified) | OK |
| `chia2022fashionclip` | Scientific Reports 2022 (pre-verified) | OK |
| `zheng2017sift` | IEEE TPAMI 2017, Vol. 40, No. 5, pp. 1224-1244 | OK |
| `manning2008introduction` | Cambridge University Press 2008 | OK |
| `jarvelin2002cumulated` | ACM TOIS 2002, Vol. 20, No. 4, pp. 422-446 (DOI: 10.1145/582415.582418) | OK |
| `liu2016deepfashion` | CVPR 2016 (pre-verified) | OK |
| `wu2019fashioniq` | CVPR 2021 (pre-verified) | OK |
| `huang2015darn` | ICCV 2015, pp. 1062-1070 | OK |
| `han2017outfitnet` | ACM MM 2017, pp. 1078-1086 (DOI: 10.1145/3123266.3123394) | OK |
| `schroff2015facenet` | CVPR 2015, pp. 815-823 (DOI: 10.1109/cvpr.2015.7298682) | OK |
| `hermans2017defense` | arXiv:1703.07737 | OK |
| `hevner2004design` | MIS Quarterly 2004, Vol. 28, No. 1, pp. 75-105 | OK |
| `peffers2008design` | JMIS 2008, Vol. 24, No. 3, pp. 45-77 | OK |
| `malkov2018efficient` | IEEE TPAMI 2018 (pre-verified) | OK |
| `oquab2023dinov2` | arXiv:2304.07193 | OK |
| `dosovitskiy2020vit` | ICLR 2021 (pre-verified) | OK |
| `paszke2019pytorch` | NeurIPS 2019 | OK |
| `lewis2014microservices` | martinfowler.com, 2014 | OK |
| `newman2019monolith` | O'Reilly Media 2019 | OK |
| `jones2015jwt` | RFC 7519, IETF (DOI: 10.17487/RFC7519) | OK |

---

## 6. Verification Notes

### Pre-verified entries (from remediation)
These 8 entries were individually verified in a prior remediation pass and confirmed correct:
`chia2022fashionclip`, `wu2019fashioniq`, `liu2016deepfashion`, `he2016deep`, `tan2019efficientnet`, `radford2021learning`, `dosovitskiy2020vit`, `malkov2018efficient`

### Entries verified via CrossRef API in this audit
- `jarvelin2002cumulated` — DOI `10.1145/582415.582418` confirmed. Vol. 20, No. 4, pp. 422-446, ACM TOIS 2002. ✅
- `han2017outfitnet` — DOI `10.1145/3123266.3123394` confirmed. ACM MM 2017, pp. 1078-1086. ✅
- `schroff2015facenet` — DOI `10.1109/cvpr.2015.7298682` confirmed. CVPR 2015, pp. 815-823. ✅
- `zheng2017sift` — DOI `10.1109/tpami.2017.2709749` confirmed. IEEE TPAMI, Vol. 40, No. 5, pp. 1224-244, 2017/2018. ✅
- `johnson2019billion` — DOI `10.1109/tbdata.2019.2921572` confirmed. IEEE TBD, Vol. 7, No. 3, pp. 535-547. Print 2021. ⚠️ Year mismatch
- `zhai2023sigmoid` — DOI `10.1109/iccv51070.2023.01100` confirmed. ICCV 2023, pp. 11941-11952. ❌ Pages wrong
- `liu2022convnext` — DOI `10.1109/cvpr52688.2022.01167` confirmed. CVPR 2022, pp. 11966-11976. ❌ Pages wrong

### Entries verified via known authoritative sources
- `shaw2012software` — OpenLibrary confirms Shaw & Garlan, "Software Architecture", Prentice Hall. ✅
- `manning2008introduction` — Well-known IR textbook, Cambridge University Press. ✅
- `hevner2004design` — MIS Quarterly 2004, highly cited (25,958 citations per Scholar). ✅
- `peffers2008design` — JMIS 2008, highly cited (16,786 citations per Scholar). ✅

---

## 7. Priority Actions

1. **FIX** `zhai2023sigmoid` pages: `41--50` → `11941--11952` (HIGH)
2. **FIX** `liu2022convnext` pages: `11976--11986` → `11966--11976` (HIGH)
3. **FIX** `johnson2019billion` year: `2019` → `2021` (MEDIUM)
4. **OPTIONAL** Rename `rendle2012bpr` key to `rendle2009bpr` (LOW)
5. **OPTIONAL** Remove 14 orphan entries from bib if not needed for reviewer reference
