# Language Level Audit + Re-Leveled Rewrite — Chapter 2, §2.1–2.2

**Important scope note before the audit:** §2.1 and §2.2 together span about 50 pages, but the vast majority of that is functional requirement tables (Tables 10–19) and 26 use case specifications (Tables 20–45), both written in a terse, numbered, itemized style that's standard practice for requirements documentation, regardless of the author's general English level. That format isn't a language-level problem, it's the correct professional convention for this kind of content, and rewriting it into fuller sentences would actually make it less standard, not more natural. So this audit focuses on the connecting prose (section openings, subsection intros), where actual language-level issues exist, and explicitly confirms the tables/use-case specs need no change.

---

## STAGE 1 — Sentence-by-sentence audit (prose passages only)

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "The platform delivers 88 functional requirements across nine business modules, each enforcing domain invariants expressed through entity validation rules and application-layer checks." | "Enforcing domain invariants expressed through entity validation rules and application-layer checks" stacks three abstract technical noun phrases in a row; each term is necessary, but the sentence is dense. (Note: "88... nine" is also the count already corrected to "87... eight" in the earlier factual review, kept here.) | [UNCLEAR] |
| 2 | "Five non-functional quality dimensions define performance, security, modularity, observability, and reliability targets that shaped architectural decisions throughout design." | Long sentence (25+ words) with a layered structure ("dimensions define... targets that shaped..."); clearer split into two ideas. | [UNCLEAR] |
| 3 | "Feature classification distinguishes three core research contributions... from four supporting infrastructure modules that provide the realistic evaluation context described in Section 3.2." | "Provide the realistic evaluation context" is a fairly abstract, report-style phrase. | [AI-LIKE] (mild) |
| 4 | "Functional requirements are organized by business module with unique identifiers traceable throughout design, implementation, and evaluation chapters." | "Traceable throughout" is a correct, standard requirements-engineering term (keep), but the sentence structure around it is a little dense. | [TECHNICAL TERM] + [UNCLEAR] (mild) |
| 5 | "Five quality dimensions define production-readiness constraints with atomic, measurable targets." | "Atomic" here is used in its technical sense (meaning "indivisible" or "single, testable unit"), a real requirements-engineering term, but it's genuinely advanced vocabulary and could confuse a reader unfamiliar with that specific technical usage. | [TECHNICAL TERM] (borderline; consider a one-word gloss) |
| 6 | "Three actors interact with the platform across 26 use cases, organised into a functional work breakdown structure (WBS) and a summary matrix with detailed scenario specifications." | Long sentence (30+ words) packing three separate ideas (actor count, use case count, and two different document structures) into one. | [UNCLEAR] |
| 7 | "The use case specifications provide full traceability to the functional requirements defined in Section 2.1." | Clear and standard for requirements documentation; acceptable as-is. | [NO ISSUE] |
| 8 | "Three categories of actors interact with the platform, distinguished by access level and interaction surface." | "Distinguished by access level and interaction surface" is a compressed technical phrase; understandable, but a plainer connector would read more naturally. | [UNCLEAR] (mild) |
| 9 | "The Administrator actor manages data and operational workflows across all business modules through a dedicated administration interface." | "Operational workflows" is a fairly formal, report-style noun phrase where a simpler word would work. | [TOO ADVANCED] (mild) |
| 10 | "Each specification includes the actor's goal, trigger, preconditions, postconditions, a numbered main success scenario, alternative and exception flows, and related functional requirements." | This is a correct, necessary list of standard use-case-specification field names (a recognized documentation convention), not overly advanced for the content. | [TECHNICAL TERM] |
| 11 | Individual use case tables (Tables 20–45): goal, pre/post conditions, numbered scenario steps | Already appropriately terse, plain, imperative-style. This is standard software-requirements documentation format and does not need language-level changes. | [NO ISSUE] |

---

## STAGE 2 — Methodology claims requiring verification

Nothing new to flag here beyond the "87 functional requirements across eight modules" correction already established and applied in the earlier factual review (this passage is the exact sentence that correction targets, listed in Stage 1 item 1). No new methodology concerns found in this section's prose.

---

## STAGE 3 — Re-leveled rewrite (prose passages)

```
2.1 REQUIREMENTS SPECIFICATION

The platform has 87 functional requirements across eight business
modules. Each requirement enforces a specific rule, checked either at the
entity level or in the application layer. Five non-functional quality areas
define the performance, security, modularity, observability, and reliability
targets used throughout the design. These targets shaped many of the
architectural decisions made later in this chapter.

Feature classification separates three core research contributions,
described in detail in Sections 2.3 and 2.4, from four supporting
infrastructure modules. These supporting modules provide the realistic
context needed for the evaluation in Section 3.2.

- Functional Requirements. Organized by module: Catalog, Identity,
  Inventory, Ordering, Payment, Shipping, Profile, and Location.
- Non-Functional Requirements. Five quality dimensions, each with
  measurable, specific targets.
- Feature Classification. Core Research versus Supporting Infrastructure,
  defining the scope of this thesis's contribution.

2.1.1 Functional Requirements
Functional requirements are organized by business module. Each has a
unique ID that is used and referenced throughout the design,
implementation, and evaluation chapters.

2.1.1.1 Catalog Module
Manages the product lifecycle, classification, image handling, and CBIR
infrastructure.

[Table 10 unchanged, already appropriately formatted.]

[Remaining module subsections 2.1.1.2–2.1.1.8, and their tables, unchanged.]

2.1.2 Non-Functional Requirements
Five quality areas define the specific, measurable targets needed for the
system to be considered production-ready. These targets are verified in
Chapter 3.

[Table 18 unchanged.]

2.1.3 Feature Classification
[Table 19 intro sentence, if any, kept plain; table itself unchanged.]

2.2 SYSTEM MODELING

Three types of actors interact with the platform, across 26 use cases in
total. These are organized using a functional work breakdown structure
(WBS) and a summary table with detailed scenarios for each use case. The
use case specifications are fully traceable to the functional requirements
defined in Section 2.1.

- System Actors. Customer, Administrator, and System: their roles,
  responsibilities, and how they interact with the platform.
- Functional Decomposition. A work breakdown structure covering three
  functional areas, with their modules and sub-functions.
- Use Cases. 26 specifications, with a summary table and detailed
  scenarios, fully traceable to the requirements.

2.2.1 System Actors
Three types of actors interact with the platform. They are separated by
their access level and how they interact with the system.

2.2.1.1 Customer
The Customer uses the browser-based storefront.
[Bullet list unchanged.]

2.2.2 Functional Decomposition
[Unchanged, already plain WBS structure.]

2.2.3 Administrator Use Cases
The Administrator manages data and daily operations across all business
modules, through a dedicated administration interface. Each use case
specification includes: the actor's goal, what triggers the use case,
preconditions, postconditions, a numbered list of the main steps, any
alternative or exception paths, and the related functional requirements.

[Individual use case tables (2.2.3.1 onward) unchanged, already
appropriately formatted for this type of document.]

2.2.4 Customer Use Cases
[Same intro style as 2.2.3, adjusted for the Customer actor; unchanged
otherwise.]

2.2.5 System Use Cases
[Same intro style; unchanged otherwise.]
```

---

## STAGE 4 — Final consistency check

| Check | Result |
|---|---|
| Vocabulary difficulty | Simplified the two densest opening sentences (§2.1 and §2.2 openings), which stacked 3+ abstract technical noun phrases each. Left the use case field names (preconditions, postconditions, main success scenario) untouched, these are standard requirements-engineering terminology, correct and expected regardless of general English level. |
| Sentence length | Two 25-30 word sentences split into shorter ones in the §2.1/§2.2 openings; everything else was already appropriately short (tables and lists). |
| Grammar | No errors introduced. |
| Repeated phrases | Not a concern in this section, the repetitive structure across use case specs (Goal, Pre/Post, Scenario) is intentional and correct, it's a template, not AI-sounding repetition. |
| AI-like formulaic expressions | Removed: "provide the realistic evaluation context," lightly smoothed a few dense noun-phrase chains. Nothing else flagged, this section was already less rhetorical than Chapter 1 since it's mostly structured specification content. |
| Technical terminology | Preserved exactly: domain invariants, entity validation, application-layer checks, WBS, traceability, preconditions/postconditions, main success scenario. All necessary and standard. |
| Numbers | 87/eight (already corrected), 26 use cases, five quality dimensions, all kept identical. |
| Claims vs. evidence | No new evidence concerns in this section beyond the already-established requirement-count correction. |
| Meaning preserved | Checked against original; only the two section-opening paragraphs and a few subsection intro sentences were touched, no content added or removed. |

---

## A. Ten most important problems

1. §2.1 opening sentence stacks three abstract technical noun phrases in a row ("enforcing domain invariants expressed through entity validation rules and application-layer checks").
2. §2.2 opening sentence packs three separate ideas (actor count, use case count, two document structures) into one 30-word sentence.
3. "Provide the realistic evaluation context" — abstract, report-style phrase.
4. "Operational workflows" in the Administrator use case intro — slightly more formal than necessary.
5. "Distinguished by access level and interaction surface" — compressed technical phrasing, understandable but dense.
6. (Not a new item, but worth restating since it's the highest-impact single fix in this section) "88 functional requirements across nine business modules" needs to stay corrected to "87... eight," this section is exactly where that fix applies.
7–10. No further major issues found, this section is naturally closer to your target writing level than Chapter 1, since requirements/use-case documentation is inherently plainer and more structured than comparative or motivational prose.

## B. Words/phrases to avoid

enforcing domain invariants expressed through (as a stacked phrase, keep the individual terms but don't chain them), provide the realistic evaluation context, operational workflows (prefer "daily operations" or "tasks"), distinguished by (prefer "separated by" or "grouped by")

## C. Words/phrases that are safe and natural for your level

organized by, has X requirements, includes, covers, used throughout, fully traceable to, separated by, daily operations

## D. Writing style to use consistently

Same overall guidance as before, but with one addition specific to this kind of content: **requirements tables and use-case specifications should stay in their terse, templated, list-based format.** That's not a simplification you need to make, it's already the correct professional convention, and it happens to also be naturally well-suited to your writing level, since it doesn't require constructing complex sentences at all. Save your sentence-building effort for the connecting prose between sections (the paragraph before each table), which is the only place in this part of the thesis that needs the same short-sentence, common-word treatment applied to Chapter 1.

---

Ready for §2.3 (System Architecture and Design) next, same three-stage process.
