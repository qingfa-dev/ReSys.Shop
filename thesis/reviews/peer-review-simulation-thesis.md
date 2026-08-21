# Peer Review Simulation — ReSys.Shop Thesis

**Manuscript:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Venue type:** Undergraduate thesis, IT/Software Engineering, Can Tho University
**Review type:** Full multi-perspective review (five independent angles, then editorial synthesis)

**A note on how this was produced:** I read the thesis directly and evaluated it from five distinct angles below, methodology, domain/literature, cross-disciplinary/practical impact, a deliberately adversarial "devil's advocate" pass, and an editor's overall judgment. These aren't five separate model instances (I can't actually run that), they're five separate passes I made through the material with a different lens each time, the way a single careful reader would if asked to wear different hats. Where a finding overlaps with the earlier factual/hallucination audit, I say so explicitly rather than re-presenting it as new; the value-add here is evaluating the thesis as a piece of research and argumentation, not just fact-checking it.

---

## REVIEWER 1 — Methodology

**Focus: research design, benchmark validity, statistical soundness, reproducibility**

### Strengths
- The evaluation design itself is sound in structure: fixed dataset, 3-fold cross-validation, consistent hardware, multiple accuracy and efficiency metrics measured together rather than accuracy alone. That's a genuinely good instinct for an applied systems thesis, most undergraduate CBIR projects report a single accuracy number and stop.
- The RAM-measurement failure is disclosed honestly (psutil producing negative/zero values on the Linux host) rather than hidden or faked. This is a real strength; acknowledging an instrument failure is exactly what a careful researcher does, and it's rare to see in undergraduate work.
- The category-only vs. enriched-label (category+colour, category+colour+pattern) evaluation design is a thoughtful way to probe how sensitive the results are to the relevance definition, this is a more sophisticated experimental design than the thesis's own methodology section gives itself credit for.

### Major concerns
1. **The central results table doesn't reproduce against its own appendix.** Table 67 (the numbers driving the abstract, both RQ1/RQ2 answers, and the deployment recommendation) reports Fashion-CLIP mAP = 0.8788 under a category-only ground truth. Appendix A.1, describing the identical category-only methodology, reports mAP = 0.9309 for the same model. This is not a rounding difference, it's a 5-point gap on the primary reported metric. I checked every model and every column; none of Table 67's numbers reconcile with any of the three appendix ground-truth schemes. Appendix C states both were collected on the same workstation, which rules out "different hardware" as an explanation. **This is the single most serious issue in the manuscript.** A methodology reviewer at a real venue would not pass this to review without resolution; a benchmark result that can't be reproduced from the artifacts documenting it undermines every downstream claim, including the abstract's headline number.
2. **The 5,000-image sample's representativeness is asserted, not demonstrated.** Appendix B.1 states images were "chosen sequentially... to preserve the natural category distribution." Sequential selection from a raw file listing only preserves distribution if the source is already randomized, and the thesis doesn't establish that it is. This is a fixable methodological gap, but as written, the sampling procedure doesn't logically support the conclusion drawn from it.
3. **Small effective sample size for the statistical claims made.** Four models, three folds, is 12 data points per metric. The confidence-interval language in §3.5 ("Fashion-CLIP's mAP lower bound exceeds the upper bound of every other model") is doing more statistical work than a 3-fold design can really support, this is closer to "consistently ahead across folds" than a claim that would survive a formal significance test with an appropriately conservative correction for multiple comparisons. The thesis's own Limitations section partially acknowledges this ("may miss smaller differences"), which is honest, but the strength of language used in the main results section (§3.5, §3.7.3) doesn't fully match that caveat.
4. **The 0.70 cosine-similarity threshold is presented as a general finding rather than a project-specific configuration choice**, at least in its first appearance (§1.2.3.2). It's better contextualized later in §2.3.4 as a configurable system parameter, but the two passages read as if they're making different kinds of claims about the same number.

### Minor concerns
- Hardware is a single consumer laptop; this is disclosed as a limitation, appropriately, but it's worth being explicit in the abstract (not just the limitations section) that "fast" and "efficient" claims are relative to CPU-only inference, since a reader skimming the abstract could reasonably assume GPU benchmarks.
- The PostgreSQL version (16 vs. 17) and pgvector version (0.3.2 vs. 0.7.0) discrepancies between the pinned-versions table and the actual test environment are small individually, but collectively they suggest the "reproducibility" claim implicit in a pinned-version table wasn't fully verified before submission.

### Recommendation on this dimension
Major revision. The Table 67/Appendix A discrepancy alone would justify this; everything else is secondary once that's resolved.

---

## REVIEWER 2 — Domain / Literature

**Focus: literature coverage, correctness of citations, positioning relative to prior work**

### Strengths
- The related-work section correctly identifies the right reference points for this space, DeepFashion, Fashion IQ, Fashion-CLIP, and commercial systems (Pinterest Lens, Google Lens-style visual search). For an undergraduate thesis, that's an appropriately scoped and relevant literature base, not padded with irrelevant citations, and not missing the obvious ones.
- The "engineering gap" framing (§1.6.3), positioning the contribution as bridging model research and production deployment rather than claiming new modeling technique, is honest and appropriate for the actual contribution being made. This thesis correctly does NOT claim algorithmic novelty, and says so directly, that's good scientific hygiene many undergraduate theses lack.

### Major concerns
1. **Two of the reference entries checked don't match the real published work.** Reference [6] (cited three times, load-bearing for the model-selection justification in §1.3.4) attributes the wrong title and venue to the Fashion-CLIP paper, and includes a co-author who doesn't exist on the real paper. Reference [27] similarly misattributes an author name and venue for the Fashion IQ paper. For a literature reviewer, this is a serious concern independent of the underlying facts being roughly right, if the manuscript is citing a paper it hasn't actually verified against the original, that's a citation-integrity problem regardless of intent, and it's the kind of thing a diligent committee member checks by clicking through.
2. **The claimed magnitude of Fashion-CLIP's improvement over CLIP is inconsistent with the source material.** §1.3.3.5, §1.3.4.4, and §1.6.1 all state the original Fashion-CLIP paper reports "15 to 20% improvement," and twice claim this is "confirmed in Chapter 3." I could not locate a 15-20% figure in the actual paper (their own reported gap, on a different metric, HITS@5, is much larger), and Chapter 3's own measured figure is 5.4%, not 15-20%. A domain reviewer would flag this as either a misreading of the source paper or an inflated claim used to justify the model-selection decision before the actual benchmark data existed to support it.
3. **No discussion of why fashion-specific benchmarks like DeepFashion or Fashion IQ weren't used for evaluation instead of a generic Kaggle product-image dataset.** The thesis cites these datasets as related work but doesn't explain the choice to build a custom 5,000-image evaluation set rather than using an established benchmark, which would have given the results more external comparability. This isn't necessarily wrong (a custom set tied to the actual deployment scenario is defensible), but the decision isn't argued for, it's just what was done.

### Minor concerns
- The related-work section is comparative in only one direction (this system vs. commercial products); it doesn't discuss any other academic e-commerce/CBIR system papers (there is a reasonably large literature on multimodal fashion retrieval beyond just Fashion-CLIP and Fashion IQ) that could have situated the architectural contribution more precisely.

### Recommendation on this dimension
Minor-to-major revision. The citation accuracy issues are fixable but must be fixed; the inflated improvement figure is the more substantive concern since it currently misrepresents both the cited source and the thesis's own results.

---

## REVIEWER 3 — Cross-Disciplinary / Practical Impact

**Focus: real-world applicability, generalizability, business/practitioner relevance**

### Strengths
- This is, honestly, the strongest dimension of the thesis. The framing throughout, "can this be built by a small team without specialized ML infrastructure", is a genuinely useful practical question, and the thesis answers it with real, working evidence: sub-second latency on a consumer laptop, an open-source-only stack, and a documented cost/accuracy trade-off between models. A practitioner reading this thesis walks away with something actionable: "if you need speed, use EfficientNet-B0; if you need accuracy, use Fashion-CLIP; here's the actual gap."
- The pluggable model architecture (switch embedding models via one environment variable) is a nice piece of practical engineering that directly serves the thesis's stated goal of giving deployment guidance, not just benchmark numbers. This is the kind of detail that suggests the author was actually thinking about who would use this system, not just optimizing for a number.

### Major concerns
1. **The business case in the introduction rests on a statistic that doesn't check out.** The "30% search abandonment" figure, used to motivate the entire project in the opening paragraphs, is attributed to a citation (Pinterest's press release about search volume) that doesn't contain that statistic. This matters more from a practical-impact angle than it might first appear: the whole argument for why this system is worth building leans on quantifying the cost of the problem it solves, and that quantification isn't currently supported.
2. **Generalizability beyond the specific 5,000-image, single-dataset, single-hardware-profile setup is asserted more strongly in places (the introduction, the contributions section) than the limitations section itself would support.** A practitioner in a different vertical (say, furniture or electronics e-commerce rather than fashion) reading the contributions section might reasonably expect the polyglot-sidecar pattern's viability claims to transfer more directly than the thesis's own limitations section suggests they should.
3. **No cost analysis.** The thesis argues open-source tools are a viable, lower-cost alternative to commercial visual search APIs, which is a real and useful claim, but there's no actual cost comparison (infrastructure cost, engineering time, API pricing for a comparable commercial service) to substantiate "lower-cost." This is a natural, low-effort addition that would strengthen exactly the dimension this thesis is strongest on.

### Minor concerns
- The mobile/on-device future-work direction (quantized EfficientNet-B0) is a good idea but stated without any preliminary feasibility discussion (model size after quantization, expected mobile inference latency), it reads as a placeholder future-work bullet rather than a considered direction.

### Recommendation on this dimension
Minor revision. This is the thesis's best dimension; the fixes needed here are additive (a cost comparison, a more honest scoping of the generalizability claims) rather than corrective.

---

## REVIEWER 4 — Devil's Advocate

**Focus: the strongest case against this thesis's central claims, and the arguments it doesn't engage with**

### Strongest counter-argument

The thesis's central empirical claim, "Fashion-CLIP is meaningfully better than general CLIP for fashion retrieval, and this benchmark demonstrates it", currently rests on a table (Table 67) whose provenance cannot be verified against the thesis's own supporting appendix. Set aside every other issue in this review: if a skeptical reader's very first move is to check whether the headline number in the abstract (0.8788 mAP) is reproducible from the detailed appendix that's supposed to document exactly that number, and it isn't, the reader has no principled reason to trust any other number in the document, including the ones that are probably correct. This is not a minor presentation issue. It is the load-bearing empirical claim of the entire thesis, and it currently fails the most basic test a benchmark result can be put to: does the summary table match the detailed data behind it? Everything else in this thesis, the architecture, the engineering, the writing, could be excellent, and it still wouldn't matter if the one number everyone will actually check doesn't hold up. Until this is resolved, a rigorous reader is justified in treating every quantitative claim in Chapters 3 and the conclusion as unverified, not merely "probably fine with minor caveats."

### Issue list

**CRITICAL**
- Table 67 vs. Appendix A non-reproducibility (dimension: methodological rigor; location: §3.5, Appendix A). Already detailed above; repeated here because a Devil's Advocate pass exists specifically to make sure this kind of issue doesn't get buried under a list of smaller ones.
- The thesis explicitly claims (twice) that Chapter 3 "confirms" a 15-20% improvement figure that Chapter 3 does not contain (dimension: argument coherence; location: §1.3.3.5, §1.3.4.4). This is a citation of the thesis's own future content that turns out to be false when you actually read that content. That's a stronger problem than a normal citation error, because it's self-referential and checkable within the same document.

**MAJOR**
- The core contribution claim ("this thesis demonstrates that production-quality visual search is achievable without specialized infrastructure") is evaluated only on a 5,000-image catalogue. E-commerce visual search's actual hard problem, at scale, is index performance and recall degradation as the catalogue grows into the hundreds of thousands or millions of items, which is explicitly out of scope here and pushed to future work. A skeptic could reasonably argue the thesis demonstrates viability at a scale where the interesting engineering problem hasn't actually appeared yet.
- The "eleven models supported" claim, repeated across roughly six locations, doesn't match the actual six-model registry documented in the implementation chapter. A skeptical reader would ask: was this number ever true, or was it aspirational language that never got corrected as the project's actual scope narrowed during development? Either explanation is plausible, but the thesis doesn't say which, and that ambiguity itself is a minor credibility cost.

**MINOR**
- The "confirming X" sentence construction is used to close out results paragraphs six times across Chapters 3 and the conclusion. This is a rhetorical, not factual, issue, but a skeptical reader notices when a document repeatedly asserts that something has been "confirmed" using the same wrapper phrase; it starts to read as a rhetorical tic asserting certainty rather than demonstrating it each time.

### Ignored alternative explanations
- The thesis attributes Fashion-CLIP's advantage entirely to domain-specific fine-tuning. An alternative explanation not discussed: Fashion-CLIP may simply have a different (later, larger, or differently-curated) training corpus than the specific "general CLIP" checkpoint used for comparison, in which case the improvement may be partly attributable to training-data differences unrelated to fashion-domain specialization specifically. The thesis doesn't rule this out.
- The near-uniform inference-time gap between the two CLIP models and the two CNN models could partly reflect implementation-level factors (batching, warm-up, framework overhead) rather than purely architectural ones. This isn't addressed.

### Missing stakeholder perspectives
- **The end customer's actual search experience** is discussed only through proxy accuracy metrics (mAP, P@K), never through any qualitative signal about whether visually "similar" results are ones a real shopper would find useful. The thesis is explicit about this being a limitation (no user study), which is honest, but it means the entire practical-value argument rests on an assumption (higher mAP = better shopping experience) that is never actually tested.
- **A small business operator's actual cost/effort to adopt this**, not just infrastructure cost (partially covered in Reviewer 3's comments above) but the engineering effort to maintain a two-language, two-runtime system versus a single-stack alternative, isn't discussed. The thesis argues polyglot architecture is "viable," but viable-with-a-dedicated-engineer and viable-for-a-two-person-startup are very different claims, and the thesis doesn't distinguish between them.

### Observations (non-defects)
- The willingness to report a negative/uncertain result honestly (the RAM measurement failure) is a genuinely good sign about the author's research integrity, worth noting explicitly since a Devil's Advocate pass can otherwise read as uniformly negative. This is exactly the kind of thing that should make a reviewer more inclined to believe the corrected version of the benchmark, once the Table 67 issue is resolved, is trustworthy.

---

## REVIEWER 5 (EDITOR-IN-CHIEF) — Overall Assessment

**Focus: fit for purpose, overall quality, publication/pass-worthiness**

Evaluating this as an undergraduate thesis in software engineering / applied ML, not as a research-venue submission, since that's the actual context.

**Scope and ambition:** appropriate, arguably slightly ambitious in a good way. Building a full e-commerce platform (nine modules, ~87 requirements, 262 endpoints) *and* running a rigorous multi-model benchmark *and* writing it all up is more than most undergraduate theses attempt. The engineering work, judged on its own (architecture, DDD structure, requirements traceability, use case documentation), is genuinely thorough and well-organized. Several reviewers above independently praised different parts of the engineering rigor; that consistency across independent readings is a real signal of quality, not a coincidence.

**The central problem:** the thesis's empirical chapter, the part that turns "we built a system" into "we learned something generalizable from building it", currently doesn't hold together internally. This is fixable, and importantly, it's fixable without redoing the engineering work: it requires re-running or reconciling one benchmark, correcting a handful of citations, and toning down a few overstated figures. But it cannot be waved off as a formatting issue. A thesis committee that catches the Table 67/Appendix A mismatch (and it is exactly the kind of thing a committee cross-checks) will reasonably ask which number is real, and "I'm not sure" is not an acceptable answer at a defense.

**Writing quality:** solid engineering documentation throughout, appropriately plain in the requirements/architecture sections, with a tendency toward more polished, almost native-academic-journal phrasing in the summary and conclusion sections specifically. This is a minor concern compared to the empirical issue, but worth fixing for internal consistency of voice.

### Editorial Decision: **MAJOR REVISION**

Not reject, the underlying engineering contribution and most of the writing are strong, and every identified issue has a clear, achievable fix. Not minor revision, because the central quantitative claim of the thesis is currently unverifiable against its own supporting data, and that has to be resolved, not just polished, before the thesis can be defended with full confidence.

---

## CONSOLIDATED REVISION ROADMAP

Ranked by what blocks the defense vs. what strengthens it.

**Must resolve before defense (blocking):**
1. Reconcile Table 67/68 against Appendix A. Re-run the benchmark or determine which existing numbers are authoritative; update every downstream number (abstract, RQ1-3 answers, Figures 42-45, deployment recommendation) to match. *(All five reviewers flagged this independently; it is the single highest-priority item.)*
2. Correct references [6] and [27] to match the real published papers.
3. Fix the "15 to 20%" claim in three locations to match the thesis's own 5.4% figure, and remove the false "confirmed in Chapter 3" framing.
4. Either source or remove the "30% search abandonment" statistic in the introduction.

**Should resolve before defense (strengthens the argument materially):**
5. Justify the sampling methodology (why sequential selection preserves distribution, or switch to a method that clearly does).
6. Correct the "eleven models" figure to match the actual six-model registry, in all locations.
7. Add a brief cost comparison (open-source stack vs. a commercial visual-search API) to substantiate the "lower-cost alternative" claim.
8. Soften the statistical-certainty language in §3.5/§3.7.3 to match what a 4-model, 3-fold design can actually support.

**Optional, improves polish:**
9. Vary the "confirming X" sentence pattern used repeatedly in Chapters 3 and the conclusion.
10. Add one paragraph acknowledging that the 5,000-image evaluation doesn't test the scaling behavior that's actually the hard problem in production visual search, and explicitly frame that as future work rather than leaving it implicit.
11. Distinguish "viable for a team with a dedicated engineer" from "viable for a small two-person startup" when discussing the polyglot architecture's practical adoptability.

---

## Note on overlap with the earlier factual audit

Items 1-6 above overlap with findings already documented in detail in the earlier fact-checking pass (`thesis-review-MASTER-FIXLIST.md` and the corresponding rewrite files). That's expected and, honestly, a good sign, when two independent evaluation approaches (a line-by-line fact/hallucination audit, and a role-based peer-review simulation) converge on the same handful of issues as the most serious ones, that's stronger evidence those are the real priorities than either method alone would give you. The genuinely new material in this review is the argumentative framing (why the Table 67 issue is existentially threatening to the thesis's core claim, not just factually wrong), the practical/business-case gaps (no cost comparison, unexamined scaling claim), and the alternative-explanation and stakeholder-blind-spot analysis from the Devil's Advocate pass, none of which came up in the fact-checking-oriented review.
