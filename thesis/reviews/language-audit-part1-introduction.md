# Language Level Audit + Re-Leveled Rewrite — Part 1: Introduction

**Base text used:** the original thesis (with the three factual corrections already established: the abandonment stat, the model count, and the Thesis Outline structure. These factual fixes are kept; only the *language level* changes in this pass.)
**Target level:** pre-B2/B1-B2, Vietnamese undergraduate writer. Goal: clear, correct, natural, not native-sounding.

---

## STAGE 1 — Sentence-by-sentence audit

Table format for efficiency across ~40 sentences. Full explanation given wherever an issue is flagged; `[NO ISSUE]` rows are kept brief.

| # | Original sentence / phrase | Issue | Class |
|---|---|---|---|
| 1 | "Global fashion e-commerce revenue exceeded 770 billion USD in 2024, with projections surpassing one trillion by 2030." | Clean, factual, simple structure. Numbers and citation untouched. | [NO ISSUE] |
| 2 | "Yet keyword search fails where the domain succeeds" | This is a clever, compressed contrast ("fails where X succeeds") that reads as a deliberately literary rhetorical flourish. Native-writer move, not how a B1-B2 writer naturally phrases a contrast. | [AI-LIKE] |
| 3 | "attributes that resist textual description" | "Resist" used metaphorically (an attribute "resisting" description) is an advanced, literary usage. A B1-B2 writer would more naturally say "are hard to describe in words." | [TOO ADVANCED] |
| 4 | "Industry estimates place session abandonment after unsuccessful search at approximately 30 percent" | Dense noun-heavy construction ("session abandonment... at approximately 30 percent") stacks three abstract nouns before the number. Also this specific stat's citation was already flagged as unsupported in the earlier factual review, so this sentence should be removed/softened per that finding, independent of language level. | [UNCLEAR] + [METHODOLOGY, see note below] |
| 5 | "CBIR addresses this gap by replacing textual intermediaries with direct visual comparison." | "Addresses this gap by replacing textual intermediaries" is a textbook AI-generated-sounding construction, abstract nouns doing the work instead of a concrete verb. | [AI-LIKE] |
| 6 | "Products are indexed not by human-authored labels but by dense vector embeddings computed from images" | "Not by X but by Y" parallel structure is a rhetorical device more common in polished academic or journalistic writing than undergraduate prose. Content is fine, structure is advanced. | [AI-LIKE] |
| 7 | "with similarity measured through mathematical distance functions" | Acceptable, "mathematical distance functions" is a real technical term (needed), rest is plain. | [TECHNICAL TERM] |
| 8 | "A query image of a dress with a particular neckline and print pattern retrieves visually similar products without any keyword translation step." | Long sentence (30+ words) with a complex subject ("A query image of a dress with a particular neckline and print pattern") before the verb. Grammatically correct but harder to follow than necessary. | [UNCLEAR] |
| 9 | "Pre-trained convolutional neural networks, vision transformers, and fashion-specific models have substantially advanced this capability." | "Substantially advanced this capability" is vague academic filler, doesn't say what changed. Also "substantially" is a slightly advanced adverb where "greatly" or "a lot" would be more natural at this level. | [AI-LIKE] + [TOO ADVANCED] |
| 10 | "The contribution of this work is architectural, not algorithmic." | This one-line abstract contrast ("architectural, not algorithmic") is a very compressed, sophisticated rhetorical pattern. Reads as a native academic writer's stylistic signature, not a B1-B2 sentence. | [AI-LIKE] |
| 11 | "It investigates how to embed existing CBIR capabilities into a practical e-commerce system built with conventional web technologies" | "Embed... capabilities into" is a fine technical usage but the sentence overall is long and stacks several noun phrases ("practical e-commerce system," "conventional web technologies"). | [UNCLEAR] |
| 12 | "provides empirical data on which embedding models deliver the optimal balance of accuracy, latency, and resource efficiency" | "Deliver the optimal balance of X, Y, and Z" is a very polished, almost marketing-like phrase. "Optimal" especially reads as an AI/formal-writing tic here. | [AI-LIKE] + [TOO ADVANCED] |
| 13 | "The work bridges two distinct software ecosystems, the Python machine learning stack and the .NET enterprise web stack, under real-time latency constraints appropriate for interactive search." | "Bridges two distinct software ecosystems" is a metaphor ("bridges") stacked onto technical vocabulary, a combination that signals advanced/native fluency rather than a learner writing carefully. The sentence is also very long (35+ words). | [AI-LIKE] |
| 14 | "Keyword-reliant fashion search suffers from four compounding inefficiencies." | "Compounding inefficiencies" is genuinely advanced vocabulary (an economics/business term for problems that make each other worse). Correct usage, but above this level. | [TOO ADVANCED] |
| 15 | "Catalogue vocabulary mismatch. Varying vendor descriptors fragment result sets, silently excluding relevant products." | "Fragment result sets" and "silently excluding" are both compressed, abstract, almost literary phrasings ("silently" personifies the system). Advanced. | [TOO ADVANCED] |
| 16 | "Visual inexpressibility. Attributes such as fabric drape, texture, silhouette proportion, and pattern rhythm elude text queries." | "Inexpressibility" is not a common word even in advanced academic English, it's close to invented/rare usage. "Elude" is also advanced ("escape from," used metaphorically). "Pattern rhythm" is a poetic phrase, not a standard technical term. | [TOO ADVANCED] |
| 17 | "Cold-start invisibility. New products lack interaction history. Visual feature extraction enables discovery immediately from catalogue ingestion." | "Cold-start" is a real, necessary technical term (keep). "Invisibility," "enables discovery," and "catalogue ingestion" stacked together read as more polished than necessary, though each word alone is fine. | [TECHNICAL TERM] (cold-start) + [TOO ADVANCED] (rest) |
| 18 | "Polyglot integration cost. The Python deep learning ecosystem does not natively interoperate with .NET." | "Polyglot" (meaning "multiple programming languages") is a real, standard term in software engineering, keep it. "Natively interoperate" is a correct but fairly formal pairing, a simpler phrasing exists. | [TECHNICAL TERM] (polyglot) + [TOO ADVANCED] (interoperate natively) |
| 19 | "Sub-second latency requires architectural isolation of the ML workload." | "Architectural isolation of the ML workload" is dense, three abstract nouns in a row. Grammatically fine, but reads as compressed academic shorthand rather than a learner's natural phrasing. | [UNCLEAR] |
| 20 | "This project builds a functional fashion e-commerce platform with integrated image-based search and evaluates pre-trained deep learning models within that system." | Long (30+ words), two separate ideas (building the platform, evaluating the models) joined with "and" into one sentence. Clearer as two sentences. | [UNCLEAR] |
| 21 | "The contribution is the engineering demonstration of embedding existing models into a conventional web application stack." | "The engineering demonstration of embedding X into Y" is a very compressed noun-phrase-heavy sentence, three nested abstract nouns before you reach the actual point. | [UNCLEAR] |
| 22 | Technical Objectives bullets (Model integration, Polyglot architecture, Vector storage validation, Empirical benchmarking) | These are mostly fine and appropriately technical (they're meant to name concrete engineering tasks). "Establishing a reference pattern for teams with existing web infrastructure" is the one phrase that reads as more polished/business-report style than undergraduate thesis style. | [AI-LIKE] (one phrase only) |
| 23 | "producing empirical guidance for model selection in resource-constrained deployments" | "Empirical guidance," "resource-constrained deployments" — two advanced, report-style noun phrases stacked together. | [TOO ADVANCED] |
| 24 | Research Questions (RQ1–RQ3) | Clear, direct, appropriately simple already. Good models for the writing level throughout the rest of the thesis. | [NO ISSUE] |
| 25 | Tasks Completed bullets | Plain, list-style, technical terms are necessary (FastAPI, pgvector, etc.). No issues. | [NO ISSUE] / [TECHNICAL TERM] |
| 26 | "Controlled benchmarking is feasible at this scale but results may not extrapolate to production catalogues containing millions of items." | "Extrapolate" is a real, useful academic word, but it's a genuinely advanced vocabulary item (Latin-root, low-frequency). A simpler equivalent exists. | [TOO ADVANCED] |
| 27 | "relationship between measured metrics and user satisfaction remains open" | "Remains open" (meaning "is still an unanswered question") is idiomatic academic English, correct but a slightly advanced usage of "open." | [TOO ADVANCED] |
| 28 | Known Limitations, remaining bullets | Mostly plain and appropriately simple already. | [NO ISSUE] |
| 29 | "The project follows Design Science Research (DSR) across four phases" | Fine, DSR is a necessary named methodology (keep as technical term). | [TECHNICAL TERM] |
| 30 | Technologies Used bullets | Plain list of tools and versions, no language issues. | [NO ISSUE] |
| 31 | "The system is evaluated using quantitative metrics" | Fine, "quantitative metrics" is a standard, necessary methodological term. | [TECHNICAL TERM] |

**Note on item 4 (30% abandonment stat):** this is a methodology/evidence concern, not just a language one, it was already flagged in the earlier factual review as citing a source that doesn't support the number. Recommend keeping the earlier fix (drop the specific figure) rather than just simplifying its wording.

---

## STAGE 2 — Methodology claims requiring verification (Part 1 specific)

Part 1 is mostly introduction and doesn't carry heavy methodological claims itself (those live in Chapter 3), but two items are worth flagging here since they appear in this part of the text:

**CLAIM:** "Industry estimates place session abandonment after unsuccessful search at approximately 30 percent."
**STATUS:** POSSIBLY INCORRECT (already identified in the earlier factual audit)
**REASON:** the cited source ([2], Pinterest's press release about search volume) doesn't contain an abandonment-rate statistic.
**WHAT THE AUTHOR MUST CONFIRM:** do you have an actual source for this 30% figure? If not, the number should be removed, not just reworded.

**CLAIM:** "3-fold cross-validation for retrieval accuracy... across four representative models and the Fashion Product Images Dataset (5,000 images)."
**STATUS:** SUPPORTED BY TEXT (consistent with Chapter 3's own methodology description)
**REASON:** this matches what Chapter 3 describes in detail; no contradiction found in Part 1 itself.
**WHAT THE AUTHOR MUST CONFIRM:** nothing further needed here, this is just a forward-reference to Chapter 3's methodology, which is audited separately.

---

## STAGE 3 — Re-leveled rewrite

```
PART 1: INTRODUCTION

I. CONTEXT AND MOTIVATION

Global fashion e-commerce revenue exceeded 770 billion USD in 2024, and it
is expected to pass one trillion USD by 2030 [1]. However, keyword search
does not work well for fashion products. Fashion items are defined by things
like silhouette, drape, print density, and colour. These attributes are hard to
describe using text.

Content-Based Image Retrieval (CBIR) solves this problem by comparing
images directly instead of using text labels. Products are indexed using vector
embeddings computed from images, and similarity is measured using
mathematical distance functions, instead of using labels written by humans.
For example, if a user uploads an image of a dress with a specific neckline
and print pattern, the system can find visually similar products without
needing any text search. Pre-trained convolutional neural networks [3], [4],
vision transformers [5], and fashion-specific models [6] have greatly improved
this capability in recent years.

This thesis focuses on system architecture, not on creating new algorithms.
It studies how to add existing CBIR methods into a real e-commerce system
built using common web technologies. It also provides experimental data
showing which embedding models give the best balance between accuracy,
speed, and resource use. The work connects two different technology stacks:
the Python machine learning stack and the .NET web stack. This must be
done while keeping real-time speed for interactive search.

II. PROBLEM STATEMENT

Fashion search that relies only on keywords has four main problems.

Catalogue vocabulary mismatch. Different vendors use different words to
describe the same product. This can cause relevant products to be excluded
from search results.

Visual attributes are hard to describe in text. Attributes such as fabric
drape, texture, silhouette shape, and pattern style are difficult to search for
using text.

New products are invisible at first. New products do not yet have
interaction history (such as clicks or purchases). Visual feature extraction
allows these products to be found as soon as they are added to the
catalogue.

High cost of connecting different technologies. The Python machine
learning ecosystem does not work directly with .NET. To keep search speed
under one second, the machine learning part of the system must be kept
separate from the main application.

III. OBJECTIVES

This project builds a working fashion e-commerce platform with image-based
search, and it evaluates pre-trained deep learning models within this system.
The main contribution is an engineering demonstration: showing how existing
models can be added into a normal web application.

Technical Objectives
- Model integration. Add pre-trained vision models into a PostgreSQL and
  .NET e-commerce system. This can serve as an example for teams that
  already use similar web technology.
- Polyglot architecture. Design a system where a separate Python service
  handles AI inference, while the .NET backend handles business logic and
  API routing.
- Vector storage validation. Test whether pgvector (an open-source
  PostgreSQL extension) can be used as the only vector storage and search
  system, and check whether it meets real-time search speed requirements at
  a catalogue size similar to small or medium fashion retailers.
- Empirical benchmarking. Compare several embedding models, including
  CNN-based and transformer-based models, using the same hardware. This
  gives practical guidance for choosing a model when resources are limited.

Research Questions
Three research questions guide this project. They are answered using
experimental results in Chapter 3.

RQ1: Model comparison. How do fashion-specific embedding models
compare to general-purpose CNN and ViT models for fashion product
retrieval?

RQ2: Accuracy-speed trade-off. What trade-offs exist between retrieval
accuracy and inference speed? Which model gives the best balance for
real-time search?

RQ3: Architecture viability. Can a service-based architecture, with a
separate AI service, keep image inference separate from the main
application while still responding quickly?

Tasks Completed
- Build AI service. A Python FastAPI service that loads pre-trained
  embedding models to generate vectors quickly enough for interactive use.
- Set up vector search. PostgreSQL with pgvector, used to store
  high-dimensional embeddings and run similarity search.
- Connect services. A .NET backend that manages image upload, embedding
  generation, vector database queries, and building the final result.
- Create user interface. A Vue.js storefront with drag-and-drop image
  upload and a results grid showing similarity scores.
- Evaluate results. A systematic benchmark that measures retrieval
  accuracy, inference speed, and trade-offs between different models.

IV. SCOPE AND LIMITATIONS

In scope: visual search using image upload, embedding-based
recommendations, core e-commerce features (catalogue, cart, checkout), and
comparison of multiple models across CNN and transformer architectures.
Out of scope: real payment processing, shipping and logistics, social login,
mobile applications, and training custom models.

Known Limitations
This work has four main limitations.
- Dataset. 5,000 fashion product images were used [7]. This is enough for
  controlled benchmarking, but the results may not apply directly to
  production catalogues with millions of items.
- Hardware. Consumer-grade hardware was used (Intel i7-1165G7, 16 GB
  RAM), and all inference ran on CPU. The latency and throughput results
  are specific to this hardware. Using a GPU would likely improve both.
- Evaluation. Only quantitative metrics were used: accuracy, latency, and
  throughput. There was no user study, so the relationship between these
  metrics and actual user satisfaction is still an open question.
- Model training. All models were used as published, without extra
  training. Fine-tuning the models on fashion-specific data, especially for
  models originally trained on general data, might improve results, but this
  was outside the scope of this project.

V. RESEARCH METHODOLOGY

This section describes the methodology and tools used to build and evaluate
the system.

Development Methodology
This project follows Design Science Research (DSR) [8], [9], with four
phases: Research and Planning (literature review, choosing models and
tools), Design (technology stack, system architecture, database schema),
Implementation (.NET backend using VSA, Python FastAPI service, Vue 3
storefront), and Testing and Evaluation (mAP accuracy using cross-validation,
inference latency, and throughput for four representative models, chosen
from six supported by the framework).

Technologies Used
The system uses a modular technology stack chosen for performance and
scalability:
- Backend: .NET 10 with Carter, MediatR, FluentValidation.
- AI Service: Python 3.12 with FastAPI, PyTorch, Hugging Face Transformers.
- Frontend: Vue 3 with TypeScript, Vite, Pinia.
- Database: PostgreSQL with pgvector, storing both relational and vector
  data in one ACID-compliant database.
The system is evaluated using quantitative metrics: Mean Average Precision
(mAP) with 3-fold cross-validation for accuracy, and per-image inference
latency and throughput (images per second) for efficiency. These metrics are
measured for four representative models on the Fashion Product Images
Dataset [7] (5,000 images). Full results are shown in Chapter 3.

VI. THESIS OUTLINE

This thesis has three parts.

Part 1: Introduction (this part) presents the research context, problem
statement, objectives, research questions, scope, methodology, and this
outline.

Part 2: Thesis Content has three chapters:
- Chapter 1: Background. Covers vector embeddings, neural network
  architectures, vector databases, related work in fashion image retrieval,
  and the technology stack used.
- Chapter 2: Design and Implementation. Covers functional and
  non-functional requirements, system architecture (DDD, C4, database,
  API, security), and the actual implementation (.NET backend, Python
  ML service, Vue storefront).
- Chapter 3: Testing and Evaluation. Presents a systematic benchmark
  comparing retrieval accuracy and inference efficiency across embedding
  models, using cross-validation on 5,000 fashion images.

Part 3: Conclusion and Future Work summarizes the findings, discusses
the contributions and limitations of this thesis, and suggests directions for
future work.
```

---

## STAGE 4 — Final consistency check (new rewrite vs. writer profile)

| Check | Result |
|---|---|
| Vocabulary difficulty | Reduced from advanced/native-level (e.g. "resist textual description," "bridges two distinct software ecosystems") to B1-B2 range throughout. Technical terms (CBIR, pgvector, DSR, VSA, mAP, cold-start) kept as-is, they're necessary and expected in a CS thesis regardless of English level. |
| Sentence length | Original had several 30-40 word sentences with layered clauses. Rewrite mostly uses 12-22 word sentences, with longer ones split into two. |
| Grammar | No errors introduced; standard subject-verb-object structures used throughout. |
| Academic tone | Still formal (no contractions, no casual phrasing), but plain, not literary or metaphorical. |
| Repeated phrases | "This gives," "This can," "This section" used a few times, that's natural and acceptable at this level, not flagged as a problem. |
| AI-like formulaic expressions | Removed: "addresses this gap by," "bridges two distinct... ecosystems," "delivers the optimal balance," "substantially advanced this capability." None reintroduced. |
| Technical terminology | Preserved exactly: CBIR, pgvector, DSR, VSA, mAP, FastAPI, Carter, MediatR, cold-start, polyglot. |
| Tense consistency | Present tense used consistently for describing the system and its capabilities; past tense used consistently for what was done/measured. Matches the original's tense pattern. |
| British vs. American English | Original mixes "catalogue" (British) with a few American-leaning constructions. Rewrite keeps "catalogue" throughout (matching Vietnamese universities' common British-English convention) and doesn't introduce American spellings like "catalog." Worth confirming this matches your university's house style. |
| Numbers and units | All numbers (770 billion, one trillion, 5,000 images, 16 GB RAM, 3-fold, four models, six supported) kept identical to the source. |
| Claims vs. evidence | No claims strengthened or weakened; the 30% abandonment figure is flagged for removal per the earlier factual audit, consistent with that finding, not changed here to a softer wording that would just hide the sourcing problem. |
| Meaning preserved | Checked paragraph by paragraph against the original; no content added, removed (aside from the already-flagged unsupported stat), or reordered. |

---

## A. Ten most important problems (original text)

1. "Yet keyword search fails where the domain succeeds" — literary contrast structure, not natural for this level.
2. "CBIR addresses this gap by replacing textual intermediaries with direct visual comparison" — classic AI-sounding abstraction.
3. "The contribution of this work is architectural, not algorithmic" — compressed, native-level rhetorical pattern.
4. "The work bridges two distinct software ecosystems... under real-time latency constraints appropriate for interactive search" — one 35-word sentence stacking metaphor + technical vocabulary.
5. "provides empirical data on which embedding models deliver the optimal balance of accuracy, latency, and resource efficiency" — "optimal balance" is a polished/marketing-style phrase.
6. "Keyword-reliant fashion search suffers from four compounding inefficiencies" — "compounding inefficiencies" is genuinely advanced vocabulary.
7. "Visual inexpressibility... Attributes... elude text queries" — "inexpressibility" and "elude" are both rare/advanced words.
8. "Catalogue vocabulary mismatch. Varying vendor descriptors fragment result sets, silently excluding relevant products" — dense, personified ("silently"), abstract.
9. "results may not extrapolate to production catalogues" — "extrapolate" is advanced vocabulary with a simpler common equivalent.
10. The 30% abandonment statistic — not a language issue, a sourcing issue (already flagged), but still worth listing since it's the most important non-language fix in this part.

## B. Words/phrases to avoid

resist (metaphorical use), addresses this gap, bridges, distinct ecosystems, optimal, deliver the optimal balance, compounding inefficiencies, inexpressibility, elude, silently excluding, fragment result sets, extrapolate, remains open (as idiom), architectural, not algorithmic (as a rhetorical contrast pattern), substantially (prefer "greatly" or "a lot")

## C. Words/phrases that are safe and natural for your level

use, build, connect, greatly improve, hard to describe, find, compare, test, check, works well / does not work well, gives (guidance/results), separate, main problem, main contribution, still an open question, applies to / does not apply to, is expected to

## D. Writing style to use consistently

Write in plain, formal academic English with short-to-medium sentences (roughly 12-22 words). One main idea per sentence. Use common verbs (use, build, test, connect, show, find) instead of abstract or metaphorical ones (bridge, address, deliver, elude). Keep necessary technical terms exactly as they are (CBIR, pgvector, DSR, VSA, mAP), since these are expected vocabulary in a CS thesis regardless of general English level, but don't add extra formal vocabulary around them. When you need to contrast two things, use simple connectors ("but," "however," "instead of") rather than compressed literary patterns ("X, not Y" or "fails where Y succeeds"). This should read as careful, correct, and formal, but not as native or highly polished.

---

Ready for Chapter 1 (Background and Related Work) next, same three-stage process.
