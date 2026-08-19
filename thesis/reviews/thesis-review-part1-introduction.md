# Thesis Review — Part 1: Introduction

**Document:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Scope of this file:** Part 1 only (printed pages 2–5: Context and Motivation, Problem Statement, Objectives, Scope and Limitations, Research Methodology, Thesis Outline)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. Thesis Outline describes a chapter structure that doesn't exist

**Location:** Section VI, "Thesis Outline" (p.5)

> "The thesis is organized into five chapters across three parts.
> Part I: Introduction. **Chapter 1** establishes research context...
> Part II: Thesis Content contains three chapters: **Chapter 1**: Background... Chapter 2: Design and Implementation... Chapter 3: Evaluation...
> Part III: Conclusion. **Chapter 4** synthesizes findings..."

**Problem:** This paragraph is internally contradictory and doesn't match your own Table of Contents.
- Part I is called "Chapter 1," and Part II *also* starts with "Chapter 1" — the same number used for two different chapters.
- Part III is called "Chapter 4," but if Part I's introduction counts as a chapter, Part II's three chapters would be 2, 3, 4 — making Part III's conclusion "Chapter 5," not "Chapter 4."
- Your actual Table of Contents doesn't use "Chapter" numbering for Part 1 or Part 3 at all — it uses Roman numerals (I–VI for Part 1, I–V for Part 3) and only numbers chapters inside Part 2 (Chapter 1 Background, Chapter 2 Design and Implementation, Chapter 3 Testing and Evaluation). There is no "Chapter 4" anywhere in the real document.

**Why this matters:** This reads like a leftover paragraph from an earlier drafting pass (or an AI-generated outline stub) that was never reconciled with the final structure. It's the first thing after your objectives and research questions, so it's a bad first impression and an easy, guaranteed catch for any reader who flips to the Table of Contents.

**Fix:** Rewrite to match your real structure, e.g.:
> "This thesis is organized into three parts. Part 1 (this part) introduces the research context, problem statement, objectives, scope, and methodology. Part 2 contains three chapters: Chapter 1 (Background and Related Work), Chapter 2 (Design and Implementation), and Chapter 3 (Testing and Evaluation). Part 3 presents the conclusion, contributions, limitations, future work, and requirements traceability."

---

## 🟠 REWRITE — 2. "11 models" overstates what was actually tested

**Location:** Section V, "Development Methodology" (p.4–5)

> "Testing and Evaluation (mAP accuracy with cross-validation, inference latency, throughput **across 11 models**)."

**Problem:** Everywhere else in the thesis (Chapter 3, Appendix A/B) you're careful and explicit that only **four** representative models were formally benchmarked with full accuracy/efficiency tables, and that these four were "selected from the eleven supported by the benchmark framework." This sentence in Part 1 drops that distinction and implies all 11 were tested end-to-end, which isn't what Chapter 3 shows.

**Fix:** Match the phrasing you already use correctly elsewhere:
> "Testing and Evaluation (mAP accuracy with cross-validation and inference latency/throughput for four representative models, selected from eleven supported by the benchmark framework)."

---

## 🟠 REWRITE — 3. Citation [2] doesn't obviously support the stat it's attached to

**Location:** Section I, "Context and Motivation" (p.2), also repeated in Chapter 1.1

> "Industry estimates place session abandonment after unsuccessful search at approximately 30 percent [2]."

**Problem:** Reference [2] in your bibliography is "Pinterest Engineering, 'Pinterest Visual Search: 600M+ Monthly Searches.'" That's Pinterest's own PR/newsroom post about search *volume*, not an industry-wide abandonment-rate study. A 30% search-abandonment figure needs its own source (there are UX/retail research reports that report numbers in this range — e.g. Baymard Institute–style search-abandonment studies) rather than being attached to a citation about a different statistic. As written, this looks like a plausible-sounding number attached to whichever reference was nearby, which is exactly the pattern a hallucination/citation check flags.

**Fix:** Either find the actual source for the 30% figure and cite that separately, or soften the claim to something you can support with [2] alone (e.g., cut the specific number and just cite Pinterest's search-volume growth as evidence that customers are shifting toward visual search).

---

## 🟢 KEEP — 4. "$770 billion in 2024" market-size figure

**Location:** Section I, "Context and Motivation" (p.2) and Chapter 1.1

> "Global fashion e-commerce revenue exceeded 770 billion USD in 2024, with projections surpassing one trillion by 2030 [1]."

I checked this against Statista's own published figure for fashion e-commerce (Statista's "Fashion eCommerce: market data & analysis" page states global revenues of US$770.9 billion in 2024), and it matches closely. The trillion-by-2030 trajectory is also broadly consistent with other market forecasts. **No change needed**, this one is solid.

---

## 🟢 KEEP — 5. Overall prose quality (AI-writing check)

I scanned Part 1 specifically for common LLM tells (leverage, delve, seamless, robust, testament, cutting-edge, "in today's landscape," triadic "not only X but also Y," stray em dashes in prose, etc.). It's clean. The one em dash present ("attributes that resist textual description" — no wait, checked: the em dash at "print density, and colour – attributes that resist..." uses an en dash, consistent with your Typst formatting elsewhere, not a stray AI em dash) is stylistic, not a red flag. The writing here reads as genuinely authored: specific, technical, no filler. **No action needed.**

---

## 🟡 CUT / TIGHTEN — 6. Minor redundancy between Objectives and Thesis Outline

**Location:** Section III "Objectives" (p.3) vs Section VI "Thesis Outline" (p.5)

The "Technical Objectives" bullets (model integration, polyglot architecture, vector storage validation, empirical benchmarking) and the Chapter 3 outline description say almost the same thing twice, once as forward-looking objectives and once as a chapter summary. Not wrong, just a little repetitive across two pages. Optional trim if you're tightening for length; not a correctness issue.

---

## Not checked in this pass

Plagiarism: I don't have an institutional plagiarism-detection tool (Turnitin/DoIT), so I can't give Part 1 a clean bill on text-matching against other theses or paywalled sources. Nothing in Part 1 read as copy-pasted prose during my read-through (the phrasing is specific to your project's architecture and numbers), but a formal check before submission is still worth doing.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | Thesis Outline chapter numbering contradicts the real TOC | High | Correct |
| 2 | "11 models" overstates benchmark scope vs. Chapter 3 | Medium | Rewrite |
| 3 | Citation [2] mismatched to the 30% abandonment stat | Medium | Rewrite / re-source |
| 4 | $770B market figure | — | Keep, verified |
| 5 | Prose / AI-writing check | — | Keep, clean |
| 6 | Objectives vs. Outline redundancy | Low | Optional trim |

---

*Next: send "Chapter 1: Background and Related Work" (Part 2) when you're ready, and I'll do the same pass on it.*
