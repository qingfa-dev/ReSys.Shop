# Thesis Rewrite — References List

Three edits, all bibliography entries. Nothing here depends on the unresolved Chapter 3 benchmark question, safe to apply now.

All edits trace back to `thesis-review-references.md` and the master fix list.

---

## Edit 1 — [6] Fashion-CLIP, fabricated title, venue, and co-author (p.124)

**BEFORE:**
> [6] A. Chia, S. Gieysztor, and others, "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge," in *Proceedings of the 45th International ACM SIGIR Conference on Research and Development in Information Retrieval (SIGIR)*, 2022.

**AFTER:**
> [6] P. J. Chia, G. Attanasio, F. Bianchi, S. Terragni, A. R. Magalhães, D. Goncalves, C. Greco, and J. Tagliabue, "Contrastive language and vision learning of general fashion concepts," *Scientific Reports*, vol. 12, article 18958, 2022. doi: 10.1038/s41598-022-23052-9

**Why:** the title, venue, and one co-author don't match the real paper. The actual work was published in *Scientific Reports* (Nature), not at SIGIR, under a different title, and "S. Gieysztor" isn't among the real authors. This reference is cited three times in Chapter 1 (§1.3.3.5, §1.3.4.4, §1.6.1), all three citing sentences were already corrected in the Chapter 1 rewrite file, so fixing this entry closes the loop on that.

---

## Edit 2 — [27] Fashion IQ, wrong author name, title, and venue (p.125)

**BEFORE:**
> [27] H. Wu, Y. Gao, X. Guo, Z. Al-Zahir, and others, "Fashion IQ: A New Dataset Towards Natural Language Guided Retrieval," in *Proceedings of the IEEE International Conference on Computer Vision (ICCV)*, 2019.

**AFTER:**
> [27] H. Wu, Y. Gao, X. Guo, Z. Al-Halah, S. Rennie, K. Grauman, and R. Feris, "Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback," in *Proceedings of the IEEE/CVF Conference on Computer Vision and Pattern Recognition (CVPR)*, 2021, pp. 11307–11317.

**Why:** the fourth author's name is wrong (the real co-author is Ziad Al-Halah, not "Al-Zahir"), the title is slightly off, and the venue/year are wrong, the paper was published at CVPR 2021, not ICCV 2019 (it existed as a 2019 arXiv preprint before that, which may be the source of the confusion).

---

## Edit 3 — [26] DeepFashion, missing co-author (p.125)

**BEFORE:**
> [26] Z. Liu, P. Luo, X. Wang, and X. Tang, "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations," in *Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition (CVPR)*, 2016, pp. 1096–1104.

**AFTER:**
> [26] Z. Liu, P. Luo, S. Qiu, X. Wang, and X. Tang, "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations," in *Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition (CVPR)*, 2016, pp. 1096–1104.

**Why:** the real author list has five names; "Shi Qiu" was missing from the thesis's version. Title, venue, year, and page range were all already correct, this is a one-name addition, nothing else changes.

---

## What wasn't touched

References [3], [4], [5], [10], and [12] were individually checked against the published record and confirmed exact, no changes. The remaining 18 references (documentation links, books, the JWT RFC, and a few academic sources not independently re-verified) weren't flagged during review, see `thesis-review-references.md` for the full breakdown of what was and wasn't checked, and why the unchecked ones were judged lower-risk.

---

## A note on scope, since this closes out the "safe to fix now" work

At this point, every file produced so far, Part 1, Chapter 1, Chapter 2 (all four sections), Part 3, and this References list, has been either fully rewritten or given surgical edits that don't depend on anything unresolved. The one piece still pending is Chapter 3's Table 67/68 vs. Appendix A reconciliation, which needs your decision (or a fresh benchmark run) before I can write a confident rewrite for it, Chapter 3 itself, Appendix A's tables, and the handful of downstream references in Part 3 and Table 70 that quote its numbers.

If you'd like, once you've resolved which benchmark numbers are authoritative, send me the answer (or the fresh results) and I'll do a final pass propagating the correct figures through every place they appear, Chapter 3, Part 3's Summary of Work, Table 70, and Figures 42–45, in one consolidated file, rather than you having to track down each mention yourself.

Otherwise, that's the full set of rewrites done. Let me know if you'd like anything revisited, or if you want a final consolidated "all edits applied" master document pulling every rewrite file into one.
