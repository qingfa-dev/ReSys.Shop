# Thesis Rewrite — Part 2, Chapter 1: Background and Related Work

Chapter 1 is 24 pages, and only five passages actually need changes, everything else already checked out clean in the earlier review. So instead of reproducing the whole chapter, this file gives you surgical before/after edits for each of the five spots, in page order, ready to drop into your `.typ` source.

All five trace back to `thesis-review-part2-chapter1-background.md` and the master fix list.

---

## Edit 1 — §1.2.3.2, uncited similarity threshold (p.7)

**BEFORE:**
> Cosine similarity produces values ranging from +1.0 (identical vector orientation) to 0.0 (orthogonal vectors) down to −1.0 (opposite orientations). For normalized fashion embeddings, **scores above 0.70 generally correspond to strong visual similarity perceptible to human shoppers**.

**AFTER:**
> Cosine similarity produces values ranging from +1.0 (identical vector orientation) to 0.0 (orthogonal vectors) down to −1.0 (opposite orientations). During development, informal inspection of retrieval results suggested that scores above roughly 0.70 tended to correspond to visually similar products; the platform uses this value as a configurable relevance threshold, detailed in §2.3.4.

**Why:** the original states a precise empirical threshold as settled fact with no citation and no reference to your own data. Reframing it as an observation from your own development work, which is what it actually is, makes the claim defensible instead of asserting a general finding about human perception you haven't measured.

---

## Edit 2 — §1.3.3.5, first "15 to 20%" occurrence (p.16)

**BEFORE:**
> Fashion-CLIP inherits the ViT-B/16 architecture, producing 512-dimensional embeddings. **The original paper reports a 15-to-20% improvement on fashion retrieval over general CLIP, confirmed in the benchmark evaluation presented in Chapter 3.**

**AFTER:**
> Fashion-CLIP inherits the ViT-B/16 architecture, producing 512-dimensional embeddings. The benchmark evaluation presented in Chapter 3 confirms Fashion-CLIP's advantage over general CLIP empirically, with a 5.4% higher mAP under the category-only evaluation scheme (§3.5).

**Why:** Chapter 3's own Table 67 reports a 5.4% mAP advantage, not 15–20%, and I couldn't find a 15–20% figure in the actual Fashion-CLIP paper either (their reported HITS@5 gap is much larger and uses a different metric entirely). Saying Chapter 3 "confirms" a number Chapter 3 doesn't actually contain is the specific problem here, so the rewrite makes the claim point to the real, verifiable number.

---

## Edit 3 — §1.3.4.4, second "15 to 20%" occurrence (p.18)

**BEFORE:**
> Fashion-CLIP further trains CLIP on over 700,000 fashion product images paired with detailed descriptions covering garment categories, fabric textures, style descriptors, and occasion labels [6]... First, retrieval quality: Fashion-CLIP achieved the highest mAP among the evaluated models, **with a 15 to 20 percent improvement over general CLIP on fashion-specific queries, confirmed through the systematic benchmark in Chapter 3** [6].

**AFTER:**
> Fashion-CLIP further trains CLIP on over 700,000 fashion product images paired with detailed descriptions covering garment categories, fabric textures, style descriptors, and occasion labels [6]... First, retrieval quality: Fashion-CLIP achieved the highest mAP among the evaluated models, **outperforming general CLIP by 5.4% under the systematic benchmark presented in Chapter 3** (§3.5) [6].

**Why:** same correction as Edit 2, this is the passage that actually drives the "Fashion-CLIP was selected as the primary model" decision in §1.3.4.5, so getting the real number here matters for the credibility of that choice, not just for consistency.

---

## Edit 4 — §1.3.4.5, EfficientNet-B0's accuracy trade-off (p.18)

**BEFORE:**
> EfficientNet-B0 provides the fastest inference at 5.3 million parameters, trading off **3.4 percent lower mAP@10** with no text-to-image capability.

**AFTER:**
> EfficientNet-B0 provides the fastest inference at 5.3 million parameters, trading off **7.7 percent lower mAP** with no text-to-image capability.

**Why:** Chapter 3's Table 67 puts this gap at 7.7% (0.8158 vs. 0.8788), not 3.4%. This is a separate error from the Fashion-CLIP-vs-CLIP number above, a different comparison, same underlying problem of a Chapter 1 figure not matching the Chapter 3 table it's describing.

---

## Edit 5 — §1.6.1, third "15 to 20%" occurrence (p.28)

**BEFORE:**
> The Fashion-CLIP work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images **improves retrieval by 15 to 20% over the general model** [6]. This thesis follows that approach, using pre-trained models without custom training, and extends the evaluation to additional architectures (ResNet, EfficientNet, DINOv2) for systematic comparison.

**AFTER:**
> The Fashion-CLIP work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images improves fashion retrieval quality over the general model [6], a finding this thesis's own benchmark corroborates with a 5.4% mAP improvement (§3.5). This thesis follows that approach, using pre-trained models without custom training, and extends the evaluation to additional architectures (ResNet, EfficientNet, DINOv2) for systematic comparison.

**Why:** third and last occurrence of the same inflated figure. Here I softened the claim about what "the original paper" reports (since I couldn't verify 15–20% appears there) and anchored the specific number to your own measured result instead, which you can stand behind because you computed it.

---

## Edit 6 — §1.6.3, "evaluates 11 models" (p.29)

**BEFORE:**
> 3. Commodity hardware benchmarking. Commercial visual search runs on cloud TPU clusters. **This thesis evaluates 11 models on consumer-grade hardware**, establishing that production-quality visual search is achievable without specialised infrastructure, lowering the barrier for small to medium e-commerce platforms.

**AFTER:**
> 3. Commodity hardware benchmarking. Commercial visual search runs on cloud TPU clusters. **This thesis benchmarks four representative models, spanning CNN, ViT, and CLIP-based architectures, on consumer-grade hardware**, establishing that production-quality visual search is achievable without specialised infrastructure, lowering the barrier for small to medium e-commerce platforms.

**Why:** same "eleven models" issue flagged across the thesis, resolved by Table 55's actual six-model registry. Framed here as "four representative models" rather than restating "eleven" or "six," since the actual contribution being claimed in this bullet is the benchmark itself, and four is the number of models the benchmark evaluates in depth. If you'd rather keep an explicit denominator, "four representative models, selected from six supported by the framework" also works and matches the phrasing used in the Part 1 rewrite.

---

## What wasn't touched

Everything else in Chapter 1 (§1.1 through §1.2, the ResNet/EfficientNet/ViT/DINOv2 architecture descriptions, §1.3.1 through §1.3.3.4, §1.4 Vector Databases, §1.5 Platform Architecture, and the rest of §1.6) checked out clean in the earlier review, verified model parameter counts, verified dataset statistics, verified Pinterest/DeepFashion figures, and no AI-writing red flags anywhere. No changes needed there.

One more thing worth doing at the source level, not a text edit: reference [6] in your bibliography itself is fabricated (wrong title, wrong venue, fabricated co-author), already covered with the exact corrected citation in `thesis-review-references.md`. Fixing the bibliography entry is a separate action from the six text edits above, but since [6] is cited in three of these five passages, it's worth doing both together.

---

Ready for Chapter 2 (Design and Implementation) next, same surgical format, whenever you want to continue.
