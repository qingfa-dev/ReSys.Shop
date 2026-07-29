# EIC Review Report — Pass 1 (Introduction + Background)

**Reviewer:** Dr. Elena Vasquez, Senior Associate Editor at *SoftwareX*, University of Amsterdam
**Role:** Editor-in-Chief
**Review Date:** 30 July 2026
**Review Scope:** Introduction (Chapter 0) + Background and Related Work (Chapter 1)

---

## Overall Assessment

This is a well-structured, competently written bachelor's thesis that knows exactly what it is and makes no pretense of being otherwise. The introduction is exceptionally clear about the nature of the contribution: "architectural, not algorithmic." This intellectual honesty is rare at the bachelor's level and serves the work well. The argument flows logically from problem motivation through research questions to a methodological framing that suits the engineering focus. A defense committee reading these chapters would immediately grasp the project's scope, its boundaries, and the practical value proposition. The student has made an explicit, defensible case for why building an engineering artifact with empirical benchmarking constitutes valid scholarship.

The Background chapter demonstrates facility with the technical material. The pedagogical sequencing — moving from foundational CBIR concepts through model architectures (CNN → ViT → CLIP → Fashion-CLIP) and then into infrastructure choices — is well judged. Each section builds on the last, and the concluding Model Selection and Justification section ties the survey back to the project's specific needs. The prose is generally clear and the technical depth is appropriate for the target audience. There are, however, several polish issues that prevent this from being fully defense-ready.

## Recommendation

- [x] **Minor Revision**

These chapters are close to defense-ready. The structural skeleton is sound and the argument is coherent. Revision effort should focus on eliminating redundancy, resolving diagram placeholders, and smoothing a few rough transitions. This is not a rewrite — it is a polish pass.

## Writing Quality Assessment

**Academic style**: The register is consistently formal and appropriate. The student avoids colloquialisms and maintains a measured, evidence-oriented tone throughout. Technical terms are introduced before deployment. One stylistic quirk: the thesis oscillates between third-person "this project" / "this thesis" and impersonal passive voice, occasionally making some sentences feel detached.

**Clarity**: Generally good. The problem statement's four inefficiencies (Section 1.2) are the strongest passage — each is named, defined, and linked to a concrete failure mode. The Background chapter's explanation of cosine similarity, the latent space, and the CBIR pipeline is accessible without being reductive.

**Terminology consistency**: Strong. "Polyglot architecture," "semantic gap," "modular monolith," and "vertical slice architecture" are used consistently with their first-use definitions. The student does not introduce competing terms for the same concept.

**Grammar and flow**: Minor issues. Some sentences are unnecessarily long (the "Polyglot integration cost" paragraph in Section 1.2 is a single sentence of 68 words that could be split for readability). Transitional phrases between subsections are formulaic at times — the pattern "The next section introduces..." or "The preceding sections..." appears six times in the Background chapter and becomes noticeable.

**Redundancy**: The most significant writing issue. The semantic gap is defined in Section 1.1 (Introduction) and redefined almost verbatim in Section 2.2.3 (Background). Similarly, the 770B/1T fashion e-commerce statistics appear in both Section 1.1 and Section 2.1. The technology stack is surveyed at moderate length in Section 2.5 and then summarized in a table — this is defensible, but the prose leading up to the table could be tightened since the table itself is comprehensive. Estimated dead text from redundancy: approximately 300-400 words.

**Diagram placeholders**: Seven commented-out `// Diagram placeholder` or `// #figure(...)` markers remain. These are presumably awaiting final image insertion. A defense committee would flag these as incomplete. At minimum, each placeholder should be replaced with a brief textual description of what the figure would convey, or the figures should be inserted.

### Well-written passages

- **Section 1.2 Problem Statement** (lines 13-21): The four-point structure with named inefficiencies is tight, specific, and convincing.
- **Section 1.3 Scope and Limitations** (lines 56-87): The known limitations are candid and professionally stated, preempting obvious committee questions.
- **Section 2.5.5 Architectural Decision** (lines 678-687): The three-pattern combination (modular monolith + vertical slice + sidecar) is explained in one crisp paragraph with precise rationale.
- **Section 2.6 Related Work and Contribution Differentiators** (lines 815-858): The four differentiators are concrete, comparable, and honest about what is *not* novel.

## Strengths

1. **Honest scoping.** The thesis explicitly states it is not about novel algorithms but about engineering integration and empirical comparison. This directly manages reader expectations and prevents the committee from evaluating the work on the wrong criteria.

2. **Weighted model selection criteria (Section 2.3.5).** Rather than defaulting to the most accurate model, the student defines four selection criteria (retrieval quality, latency, multimodality, hardware constraints) and provides alternative deployment scenarios. This shows engineering judgment beyond rote benchmarking.

3. **pgvector rationale (Section 2.4.4).** The transactional consistency argument — embeddings and product metadata sharing the same ACID boundary — is a genuinely sharp architectural insight that many industry engineers overlook.

4. **Limitations as a forward-looking scaffold.** The known limitations in Section 1.3 are explicitly keyed to the concluding chapter, creating a structural promise the thesis can fulfill in Chapter 4.

5. **Toolchain completeness.** The technology stack table (Section 2.5.9) is comprehensive, covering frontend through orchestration through benchmarking. The student clearly built the full system they describe.

## Weaknesses / Areas for Improvement

1. **Section redundancy between Introduction and Background.** — Section 1.1 and Section 2.1 both repeat the 770B/1T statistics. Section 1.1 and Section 2.2.3 both define the semantic gap. — **Severity: MAJOR** — Merge the Background treatment of topics already addressed in the Introduction, or in the Introduction reference forward to the Background's deeper treatment rather than duplicating content.

2. **Seven unresolved diagram placeholders remain in the Background chapter.** — Lines 186-187, 198-199, 233, 243, 307, 378, 418, 689. — **Severity: CRITICAL** — These communicate incompleteness to any reader. Insert figures or replace each placeholder with a numbered figure caption and a brief textual bridge sentence.

3. **Formulaic section transitions.** — The phrase variants "The next section introduces..." and "The preceding sections..." appear at lines 265-267, 355, 439, 724, 792-794, and 817. — **Severity: MINOR** — Vary transition phrasing. Some can be removed entirely since the heading hierarchy already signals the relationship.

4. **Run-on sentences in problem statement reduce impact.** — The "Polyglot integration cost" paragraph (Section 1.2, lines 21-22) is a 68-word single sentence. — **Severity: MINOR** — Split into two or three sentences to improve readability.

5. **CBIR concept is defined twice at the same depth.** — Section 2.2.1 and Section 2.2 both open with nearly identical definitions of CBIR. — **Severity: MINOR** — Remove the first instance or merge into a single introductory paragraph.

6. **Training data inconsistencies in model tables.** — The Evaluated CNN Variants table shows "ImageNet (1.2M images)" for ResNet variants, but the standard ImageNet-1K training set is 1.28M images, not 1.2M. — **Severity: MINOR** — Verify and correct dataset size figures or cite the specific ImageNet subset used.

7. **"Research and planning" methodology phase is thin.** — Section 1.4's methodology section allocates one bullet to the literature survey and planning phase. — **Severity: MINOR** — Expand by 2-3 sentences describing what the survey covered and how it informed subsequent design decisions.

8. **Thesis Outline numbering inconsistency.** — Section 1.5 numbers "Chapter 1" as both the current Introduction chapter and as "Background and Related Work." This is likely a Typst rendering artifact of the chapter counter being incremented between Part I and Part II, but the prose should clarify the chapter numbering scheme explicitly. — **Severity: MAJOR** — Revise the outline to use unambiguous chapter numbers or add a clarifying sentence.

## Confidence Score: **4/5**

The writing is consistent and competent; I am confident in the structural assessment. The score is 4 rather than 5 because the Background chapter contains substantial technical domain knowledge (model architectures, vector search algorithms) whose factual accuracy I have not independently verified — that is Reviewer 2's responsibility. I am also evaluating without access to the figures that the placeholders reference, which form part of the intended reading experience.
