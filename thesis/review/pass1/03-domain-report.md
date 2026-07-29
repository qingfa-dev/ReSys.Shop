# Domain Review Report — Pass 1 (Introduction + Background)

**Reviewer:** Dr. Shreya Kapoor, Research Scientist, Amazon Visual Search (Bangalore)
**Role:** Peer Reviewer 2 — Domain
**Review Date:** 30 July 2026

---

## Literature Coverage Assessment

The literature survey covers the expected canon: He et al. (2016) for ResNet, Tan & Le (2019) for EfficientNet, Dosovitskiy et al. (2020) for ViT, Radford et al. (2021) for CLIP, Oquab et al. (2023) for DINOv2, Chia et al. (2022) for Fashion-CLIP, Malkov & Yashunin (2018) for HNSW, and pgvector (2023) for the vector database extension. Liu et al. (2016) DeepFashion is correctly cited as the foundational fashion recognition benchmark, and Wu et al. (2019) FashionIQ is acknowledged as conversational retrieval.

**However, only three papers appear in the dedicated Related Work section (Section 1.6).** For a thesis nominally positioned at the intersection of fashion AI and software engineering, this is insufficient. **Critical omission:** DeepFashion2 (Ge et al., 2019, CVPR) — the standard benchmark sequel that added dense landmark annotations and in-shop clothes retrieval to the DeepFashion family. A thesis evaluating fashion retrieval without citing DeepFashion2 looks uninformed to domain specialists.

The commercial-systems comparison (Google Lens, Pinterest Lens, ASOS, ViSenze) is a good addition, but the "black box, cannot be studied" argument is undercut by published engineering papers from Pinterest (Visual Discovery at Pinterest, WWW 2015), Google (Large-Scale Visual Search, CVPR workshops), and Alibaba (Fashion Retrieval at Scale, KDD). Including these would demonstrate deeper engagement with the field beyond consumer-facing product descriptions.

## Research Gap Evaluation

The thesis identifies the "engineering gap" — that model research and production systems are disconnected, and that building a practical e-commerce CBIR system requires crossing the Python/.NET boundary. This gap exists, but it is **overstated.** The Python ML sidecar communicating over HTTP is standard practice in any enterprise with a non-Python backend. The contribution is better described as "a practical integration demonstration with systematic benchmarking" rather than as filling a gap in the literature.

The student does not strawman prior work, which is commendable. The problem statement honestly acknowledges that commercial systems and academic benchmarks exist; the thesis merely claims that no accessible, open-source integration bridges the specific technology stack chosen.

## Domain Argumentation Quality

**What's accurate:**
- CNN hierarchical feature extraction (early/middle/late layer descriptions) is pedagogically sound.
- ViT patch embedding and self-attention mechanism is correctly explained.
- CLIP dual-tower architecture is correctly described.
- Fashion-CLIP's fine-tuning on 700K images is factually correct.
- HNSW logarithmic search time is accurate.

**What's imprecise:**
- **DINOv2 "ignoring colour"** — DINOv2 does not ignore colour; its self-supervised objective deprioritizes low-level features but colour remains present in the embedding. Rephrase to "deprioritizes colour in favour of structural features."
- **CNN "processes patches"** — CNNs apply convolutional kernels, not patch tokenization. "Patches" is ViT terminology. The sentence comparing CNN and ViT incorrectly describes CNNs as processing "patches in order."
- **Circular narration around Fashion-CLIP:** Section 1.3.4 explains CLIP → Fashion-CLIP as a natural progression, then Section 1.3.6 selects Fashion-CLIP. The Background chapter is structured to make Fashion-CLIP appear inevitable, which undermines the empirical objectivity of the evaluation.

## Model Selection Framework Critique

The four criteria (retrieval quality, latency, multimodality, hardware) form a reasonable trade-off space. However, the **multimodal capability criterion** is given equal weight, which structurally advantages CLIP-based models: CNNs and ViTs are vision-only by design and cannot compete on this axis. This is not incorrect — the system needs text-to-image search — but the thesis should acknowledge that the weighting effectively makes the comparison a "CLIP variant ranking" with vision-only models as baselines.

**Model count inconsistency:** Section 1.3.6 states "Eleven pre-trained models" but the candidate table (Table <tbl-candidate-models>) lists only 10. ResNet-152 appears in the benchmark framework description (Section 1.5.10) but is absent from the selection table. This must be reconciled.

**Missing CLIP-RN50 baseline:** All CLIP variants use ViT encoders. Including CLIP-RN50 (ResNet-50 vision encoder) would isolate whether Fashion-CLIP's improvement comes from domain fine-tuning or from the ViT architecture. Without this, the architecture effect is confounded with the domain-specialization effect.

## Strengths

1. **Model comparison depth is appropriate for bachelor's level.** Explaining CNN, ViT, CLIP, and Fashion-CLIP at sufficient detail for an informed reader shows engagement with the material.

2. **Alternative deployment scenarios (Section 1.3.7).** Providing deployment-specific guidance (EfficientNet-B0 for ultra-low latency, DINOv2 for structural fidelity, CLIP ViT-L/14 for high-resource environments) shows practitioner awareness beyond academic comparison.

3. **Honest positioning.** The thesis repeatedly and explicitly states the contribution is "architectural, not algorithmic." This precision is commendable.

4. **Technology stack depth.** The thesis demonstrates understanding of not just models but the entire serving pipeline.

## Weaknesses / Issues

1. **CRITICAL — DeepFashion2 (2019) missing from literature review.** The standard benchmark sequel to DeepFashion with dense landmarks and in-shop retrieval is a conspicuous gap. Add citation and discussion.

2. **MAJOR — Model count inconsistency: 11 claimed, 10 shown.** ResNet-152 appears in Section 1.5.10 but not in the candidate model table (Section 1.3.6). Reconcile the count.

3. **MAJOR — Missing CLIP-RN50 baseline to isolate architecture vs. domain effects.** Without a ResNet-based CLIP variant, the comparison cannot distinguish "ViT is better than CNN for retrieval" from "Fashion-CLIP training data is richer."

4. **MAJOR — Engineering gap overstated.** The sidecar pattern is standard practice, not a novel architectural contribution. Tone down the novelty language.

5. **MINOR — Imprecise language:** DINOv2 "ignoring colour" is an overstatement. CNN "processes patches" should be "applies convolutional kernels."

6. **MINOR — Circular narration:** The Background chapter builds a narrative arc toward Fashion-CLIP as the inevitable choice when that decision properly belongs in Chapter 3.

## Dimension Scores (domain-relevant)

- **Literature Integration: 3/5.** The canon is covered, but a major benchmark (DeepFashion2) is missing, and only 3 papers appear in the dedicated Related Work section. The commercial-systems comparison lacks scholarly depth.

- **Originality (within bachelor's scope): 3/5.** The thesis correctly identifies its contribution as architectural integration. For a bachelor's thesis, demonstrating competence in bridging ML and web engineering is sufficient. For a publication, the novelty bar would be higher.

- **Writing Quality: 4/5.** Technical descriptions are largely accurate and accessible. The pedagogical flow from CNN → ViT → CLIP → Fashion-CLIP is well-structured. Pulled down by imprecise terminology and circular narration.

## Confidence Score: **4/5**

I have published extensively on fashion image retrieval and multimodal embeddings. The model architectures and benchmarks discussed are directly within my expertise. The areas outside my core expertise are the .NET software architecture patterns and DSR methodology — I defer to R1 and R3 on those aspects.
