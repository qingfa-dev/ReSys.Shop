# Thesis Review — Part 2, Chapter 1: Background and Related Work

**Scope of this file:** printed pages 6–29 (1.1 Fashion E-Commerce through 1.6.3 Contribution Differentiators)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. The "15 to 20% improvement" claim contradicts your own Chapter 3 results — and appears three times

**Locations:**
- §1.3.3.5 (p.16): *"The original paper reports a 15-to-20% improvement on fashion retrieval over general CLIP, **confirmed in the benchmark evaluation presented in Chapter 3.**"*
- §1.3.4.4 (p.18): *"Fashion-CLIP achieved the highest mAP among the evaluated models, with a **15 to 20 percent improvement** over general CLIP on fashion-specific queries, **confirmed through the systematic benchmark in Chapter 3** [6]."*
- §1.6.1 (p.28): *"The Fashion-CLIP work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images **improves retrieval by 15 to 20%** over the general model [6]."*

**Problem:** This is a real, checkable contradiction, not a matter of interpretation. Your own Chapter 3, Table 67 (§3.5) reports Fashion-CLIP's mAP advantage over CLIP-generic as **5.4%** (0.8788 vs 0.8341), and that 5.4% figure is repeated consistently throughout Chapter 3, the abstract, and the requirements-traceability matrix. Two of the three passages above explicitly claim the 15–20% figure is "confirmed" by that same Chapter 3 benchmark. It isn't; Chapter 3 shows less than a third of the claimed improvement.

I also checked the actual Fashion-CLIP paper (Chia et al., "Contrastive language and vision learning of general fashion concepts," *Scientific Reports*, 2022) for a "15–20%" figure. Their multi-modal retrieval table reports HITS@5 of 0.61 for FashionCLIP vs. 0.22 for CLIP on their internal test set, a much larger gap than 15–20%, and on a different metric (HITS@5, not mAP). I could not find a "15–20%" figure anywhere in that paper, so this number doesn't seem to trace back to a verifiable source either.

**Why this matters:** This is the exact pattern a plagiarism/hallucination reviewer flags hardest: a specific, repeated, precise-sounding statistic that (a) doesn't match the source it's attributed to and (b) contradicts your own measured results one chapter later. A committee member who reads Chapter 1 and then Chapter 3 back to back will catch this immediately, and it undermines confidence in every other number in the thesis.

**Fix:** Remove the "15 to 20 percent" and "confirmed in Chapter 3" language in all three places. Replace with your own real number, consistent with Chapter 3:
> "Fashion-CLIP achieved the highest mAP among the evaluated models, outperforming general CLIP by 5.4% under the category-only evaluation, as confirmed in Chapter 3 (§3.5)."

This is also tied to a separate reference problem, see item 3 below.

---

## 🔴 CORRECT — 2. EfficientNet-B0's accuracy trade-off is misstated

**Location:** §1.3.4.5 (p.18)

> "EfficientNet-B0 provides the fastest inference at 5.3 million parameters, trading off **3.4 percent lower mAP@10** with no text-to-image capability."

**Problem:** Chapter 3 (§3.5, and repeated at line ~6306 in the source PDF) consistently states EfficientNet-B0's mAP is **7.7% below** Fashion-CLIP's (0.8158 vs. 0.8788), not 3.4%. This is a second, separate instance of a specific percentage in Chapter 1 that doesn't match the number reported for the same comparison in Chapter 3.

**Fix:** Change "3.4 percent lower mAP@10" to "7.7 percent lower mAP" (or whatever the correct figure is once you've resolved the Table 67 vs. Appendix A discrepancy flagged in the earlier full-thesis review). Either way, make sure every place in the thesis that quotes this comparison uses the same number.

---

## 🔴 CORRECT — 3. Reference [6] (Fashion-CLIP citation) is fabricated

**Location:** cited at §1.3.3.5, §1.3.4.4, and §1.6.1; defined in the References list as:

> [6] A. Chia, S. Gieysztor, and others, "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge," in *Proceedings of the 45th International ACM SIGIR Conference...* 2022.

**Problem:** (carried over from the earlier full-document pass, repeating it here since Chapter 1 is where it's actually used three times) The real paper is Chia, Attanasio, Bianchi, Terragni, Magalhães, Goncalves, Greco, Tagliabue, **"Contrastive language and vision learning of general fashion concepts,"** *Scientific Reports* (Nature), vol. 12, 2022. Wrong title, wrong venue (not SIGIR), and "S. Gieysztor" is not a real co-author on that paper. Since this reference underpins three separate claims in this chapter, it needs to be fixed at the source.

**Fix:** Replace the bibliography entry with the correct citation:
> Chia, P. J., Attanasio, G., Bianchi, F., Terragni, S., Magalhães, A. R., Goncalves, D., Greco, C., & Tagliabue, J. (2022). Contrastive language and vision learning of general fashion concepts. *Scientific Reports*, 12, 18958. https://doi.org/10.1038/s41598-022-23052-9

---

## 🟠 REWRITE — 4. "Evaluates 11 models" overclaims scope again

**Location:** §1.6.3, Contribution 3 (p.29)

> "Commercial visual search runs on cloud TPU clusters. This thesis **evaluates 11 models** on consumer-grade hardware, establishing that production-quality visual search is achievable without specialised infrastructure..."

**Problem:** Same issue flagged in Part 1: elsewhere you're careful to say four representative models were formally benchmarked, "selected from the eleven supported by the framework" (§1.3.4.1 gets this right). This sentence drops the distinction again and claims all 11 were evaluated, which overstates what Chapter 3 actually demonstrates as a contribution.

**Fix:**
> "This thesis benchmarks four representative models, spanning CNN, ViT, and CLIP-based architectures, on consumer-grade hardware, out of eleven supported by the framework, establishing that production-quality visual search is achievable without specialised infrastructure."

---

## 🟠 REWRITE — 5. Unsupported precision threshold

**Location:** §1.2.3.2 (p.7)

> "For normalized fashion embeddings, scores above 0.70 generally correspond to strong visual similarity perceptible to human shoppers."

**Problem:** This is a specific, confident empirical claim (a 0.70 cosine-similarity threshold tied to human perception) with no citation and no reference to your own data. It reads as plausible domain knowledge, but as written it's an assertion, not a sourced or measured fact. If this number came from your own qualitative observation while building the system, say so explicitly; if it's a general claim about embedding models, it needs a citation.

**Fix:** Either cite a source for the 0.70 threshold, or soften it: "informal inspection of retrieval results during development suggested that scores above roughly 0.70 tended to correspond to..." Vague, hedged claims are much safer here than a precise, uncited number.

---

## 🟢 KEEP — 6. Technical facts and figures verified

I checked several of the more checkable factual claims in this chapter against public sources:

- **DeepFashion dataset size** (§1.6.1): "over 800,000 images" matches DeepFashion's published scale. Correct.
- **Pinterest Lens "600M+ monthly searches"** (Table 9, §1.6.2): matches Pinterest's own published figures. Correct.
- **Model parameter counts** (Tables 2–5): ResNet-50 (25.6M), ResNet-101 (44.5M), EfficientNet-B0 (5.3M, 1280-dim), EfficientNet-B4 (19.3M, 1792-dim), CLIP ViT-B/16 (~150M, 512-dim) all match published architecture specs within normal reporting variance. No action needed.

---

## 🟢 KEEP — 7. AI-writing pattern check

Same result as Part 1: no chronic LLM tells (leverage, delve, seamless, testament, "in today's landscape," triadic filler phrases). The explanatory passages (semantic gap, latent space, cosine similarity, CBIR pipeline) read as genuinely authored technical writing, specific and grounded in the actual system, not generic filler. **No action needed** here; the writing style is not the issue in this chapter, the numeric/citation accuracy is.

---

## Not checked in this pass

**Plagiarism:** No verbatim matches found in the passages I spot-checked against public sources, but I don't have an institutional plagiarism tool, so this isn't a clearance, just an absence of red flags in what I sampled.

**Sections 1.5 (Platform Architecture and Technology Stack):** mostly descriptive of your own implementation choices (modular monolith, .NET, Vue, Redis, Hangfire) and didn't contain checkable external claims or numbers, so I didn't flag anything there. Let me know if you want a line-by-line pass on it anyway.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | "15–20% improvement" claim, repeated 3x, contradicts Chapter 3's 5.4% | High | Correct (all 3 locations) |
| 2 | EfficientNet-B0 "3.4% lower mAP" contradicts Chapter 3's 7.7% | High | Correct |
| 3 | Reference [6] (Fashion-CLIP citation) is fabricated | High | Correct in bibliography |
| 4 | "Evaluates 11 models" overclaim | Medium | Rewrite |
| 5 | Uncited 0.70 cosine-similarity threshold | Low | Rewrite / cite |
| 6 | Model specs, dataset sizes, Pinterest stat | — | Keep, verified |
| 7 | Prose / AI-writing check | — | Keep, clean |

---

*Next: send "Chapter 2: Design and Implementation" (Part 2) when you're ready, and I'll do the same pass on it.*
