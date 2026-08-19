# Thesis Review — References List

**Scope of this file:** printed pages 124–126, all 28 numbered references. I checked every reference that names a specific academic paper (the ones most likely to have been recalled from memory rather than copied from a source) against the real published record. Documentation links (Microsoft, Vue, Redis, Hangfire, pgvector GitHub, martinfowler.com, jimmybogard.com) and well-known standards (RFC 7519) weren't individually re-verified, they're low-risk, easily checkable by URL, and not the kind of reference that gets hallucinated.

Legend: 🔴 CORRECT (factual error, must fix) · 🟠 REWRITE (minor inaccuracy) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — [6] Fashion-CLIP citation is fabricated

**As written in the thesis:**
> A. Chia, S. Gieysztor, and others, "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge," in *Proceedings of the 45th International ACM SIGIR Conference on Research and Development in Information Retrieval (SIGIR)*, 2022.

**What's actually real:** the Fashion-CLIP paper is Chia, Attanasio, Bianchi, Terragni, Magalhães, Goncalves, Greco, and Tagliabue, **"Contrastive language and vision learning of general fashion concepts,"** published in ***Scientific Reports*** (Nature), vol. 12, 2022. Wrong title, wrong venue (it was never at SIGIR), and "S. Gieysztor" isn't a real co-author on the actual paper.

**Corrected entry:**
> P. J. Chia, G. Attanasio, F. Bianchi, S. Terragni, A. R. Magalhães, D. Goncalves, C. Greco, and J. Tagliabue, "Contrastive language and vision learning of general fashion concepts," *Scientific Reports*, vol. 12, article 18958, 2022. doi: 10.1038/s41598-022-23052-9

This reference is cited three times in the thesis (§1.3.3.5, §1.3.4.4, §1.6.1), so all three citing locations should be double-checked once the entry is fixed.

---

## 🔴 CORRECT — [27] Fashion IQ citation has a wrong author name, title, and venue

**As written in the thesis:**
> H. Wu, Y. Gao, X. Guo, Z. Al-Zahir, and others, "Fashion IQ: A New Dataset Towards Natural Language Guided Retrieval," in *Proceedings of the IEEE International Conference on Computer Vision (ICCV)*, 2019.

**What's actually real:** the fourth author is **Ziad Al-Halah**, not "Al-Zahir." The actual title is **"Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback."** It began as a 2019 arXiv preprint and was formally published at **CVPR 2021**, not ICCV 2019. Full author list: Hui Wu, Yupeng Gao, Xiaoxiao Guo, Ziad Al-Halah, Steven Rennie, Kristen Grauman, Rogerio Feris.

**Corrected entry:**
> H. Wu, Y. Gao, X. Guo, Z. Al-Halah, S. Rennie, K. Grauman, and R. Feris, "Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback," in *Proceedings of the IEEE/CVF Conference on Computer Vision and Pattern Recognition (CVPR)*, 2021, pp. 11307–11317.

---

## 🟠 REWRITE — [26] DeepFashion citation drops a co-author

**As written in the thesis:**
> Z. Liu, P. Luo, X. Wang, and X. Tang, "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations," CVPR 2016, pp. 1096–1104.

**Problem:** the real author list is Ziwei Liu, Ping Luo, **Shi Qiu**, Xiaogang Wang, Xiaoou Tang, **five** authors, not four. "Shi Qiu" is missing entirely from the thesis's version. Everything else (title, venue, year, page range 1096–1104) is correct, this is a smaller error than [6] or [27], a dropped name rather than a fabricated title or venue, but still worth fixing since it's a factual inaccuracy in an otherwise-correct entry.

**Corrected entry:**
> Z. Liu, P. Luo, S. Qiu, X. Wang, and X. Tang, "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations," in *Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition (CVPR)*, 2016, pp. 1096–1104.

---

## 🟢 KEEP — Verified correct

I checked the following against the published record and confirmed title, authors, venue, year, and page numbers all match exactly:

- **[3]** He, Zhang, Ren, Sun, "Deep Residual Learning for Image Recognition," CVPR 2016, pp. 770–778. ✓
- **[4]** Tan and Le, "EfficientNet: Rethinking Model Scaling for Convolutional Neural Networks," ICML 2019, pp. 6105–6114. ✓
- **[5]** Radford et al., "Learning Transferable Visual Models From Natural Language Supervision," ICML 2021, pp. 8748–8763. ✓ ("A. Radford et al." is standard shorthand for a 12-author paper, appropriate here.)
- **[10]** Dosovitskiy et al., "An Image is Worth 16x16 Words: Transformers for Image Recognition at Scale," ICLR 2021. ✓ (also a many-author paper correctly abbreviated as "et al.")
- **[12]** Malkov and Yashunin, "Efficient and Robust Approximate Nearest Neighbor Search Using Hierarchical Navigable Small World Graphs," IEEE TPAMI, vol. 42, no. 4, pp. 824–836, 2018. ✓ (checked earlier in this review series)

No action needed on any of these five.

---

## Not individually re-verified

The remaining 18 references fall into categories I judged lower-risk and didn't spend search budget re-confirming one by one:

- **Documentation/product references** ([13] pgvector GitHub, [16] martinfowler.com, [17] jimmybogard.com, [19] ASP.NET Core docs, [20] EF Core docs, [21] Vue.js docs, [22] Redis docs, [24] Hangfire docs): these are URLs to living documentation sites, easy for you to click and confirm directly, and not the type of source that gets hallucinated (there's no "real" paper to misattribute).
- **Books** ([14] Shaw & Garlan, [15] Newman, [28] Manning/Raghavan/Schütze): well-known, widely-cited textbooks, low risk of fabrication.
- **Standards** ([25] RFC 7519 JWT): a formal, numbered IETF standard, trivially verifiable and low risk.
- **Dataset/tooling sources** ([7] Aggarwal Kaggle dataset, [23] PyTorch NeurIPS 2019): plausible and specific enough that I'd expect them to check out, but I didn't independently confirm.
- **Academic methodology papers** ([8] Hevner et al., [9] Peffers et al., [11] Oquab et al. DINOv2, [18] Greg Young CQRS): foundational/well-known works in their respective fields, lower risk than the fashion-specific ML papers (which is exactly where the two confirmed errors were found).

Given that both confirmed fabrications ([6], [27]) were in the same category (fashion-domain ML papers cited to support specific numeric claims), and the DeepFashion error ([26]) was in the same category too, that category is the one I'd prioritize if you want to extend this check yourself: any reference supporting a specific number or being used as authority for a specific technique in Chapters 1–3.

---

## Summary table

| Ref | Item | Severity | Action |
|---|---|---|---|
| [6] | Fashion-CLIP: wrong title, wrong venue, fabricated co-author | High | Correct, cited 3x in the thesis |
| [27] | Fashion IQ: wrong author name, wrong title, wrong venue | High | Correct |
| [26] | DeepFashion: missing co-author (Shi Qiu) | Low | Correct |
| [3],[4],[5],[10],[12] | Verified correct | — | Keep |
| Remaining 18 | Not individually re-verified (docs, books, standards, lower-risk categories) | — | Spot-check if time allows, prioritize fashion/ML papers |

---

*This closes the References list. Remaining unreviewed material: Appendix B (Dataset Composition), Appendix C (Hardware Specifications), and Appendix D (Database Schema). Given the pattern found throughout this review, numbers not matching between sections, I'd suggest those three next, then a final consolidated priority list pulling every 🔴 finding across all seven files into one fix-it-in-order checklist. Let me know which you'd like.*
