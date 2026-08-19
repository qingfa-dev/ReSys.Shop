# Thesis Rewrite — Part 1: Introduction

This is a ready-to-paste replacement for Part 1, plus a change log underneath explaining exactly what moved and why. Every change here maps to a specific finding in `thesis-review-MASTER-FIXLIST.md`, so you can trace it back to the evidence if you want to double-check before accepting it.

No content was invented. Where a number needed correcting (the model count, the outline structure), I used the value your own thesis establishes elsewhere (Table 55's model registry, the actual Table of Contents) rather than picking a new one.

---

## REWRITTEN TEXT

```
PART 1: INTRODUCTION

I. CONTEXT AND MOTIVATION

Global fashion e-commerce revenue exceeded 770 billion USD in 2024, with
projections surpassing one trillion by 2030 [1]. Yet keyword search fails where
the domain succeeds: fashion products are defined by silhouette, drape, print
density, and colour, attributes that resist textual description. Text-based
search compounds the problem: shoppers who fail to find what they are
looking for frequently abandon the session rather than reformulate the query.

Content-Based Image Retrieval (CBIR) addresses this gap by replacing
textual intermediaries with direct visual comparison. Products are indexed not
by human-authored labels but by dense vector embeddings computed from
images, with similarity measured through mathematical distance functions. A
query image of a dress with a particular neckline and print pattern retrieves
visually similar products without any keyword translation step. Pre-trained con-
volutional neural networks [3], [4], vision transformers [5], and fashion-specific
models [6] have substantially advanced this capability.

The contribution of this work is architectural, not algorithmic. It inves-
tigates how to embed existing CBIR capabilities into a practical e-commerce
system built with conventional web technologies, and provides empirical data
on which embedding models deliver the optimal balance of accuracy, latency,
and resource efficiency. The work bridges two distinct software ecosystems, the
Python machine learning stack and the .NET enterprise web stack, under
real-time latency constraints appropriate for interactive search.

II. PROBLEM STATEMENT

Keyword-reliant fashion search suffers from four compounding inefficien-
cies.

Catalogue vocabulary mismatch. Varying vendor descriptors fragment
result sets, silently excluding relevant products.

Visual inexpressibility. Attributes such as fabric drape, texture, silhouette
proportion, and pattern rhythm elude text queries.

Cold-start invisibility. New products lack interaction history. Visual fea-
ture extraction enables discovery immediately from catalogue ingestion.

Polyglot integration cost. The Python deep learning ecosystem does
not natively interoperate with .NET. Sub-second latency requires architectural
isolation of the ML workload.

III. OBJECTIVES

This project builds a functional fashion e-commerce platform with integrated
image-based search and evaluates pre-trained deep learning models within that
system. The contribution is the engineering demonstration of embedding
existing models into a conventional web application stack.

Technical Objectives
- Model integration. Integrate pre-trained vision models into a PostgreSQL
  and .NET e-commerce stack, establishing a reference pattern for teams with
  existing web infrastructure.
- Polyglot architecture. Architect a polyglot system in which a dedicated
  Python sidecar handles AI inference while the .NET backend manages trans-
  actional logic, business rules, and API routing.
- Vector storage validation. Validate pgvector (an open-source PostgreSQL
  extension) as the sole vector storage and retrieval layer, evaluating whether it
  meets real-time search latency requirements at catalogue scales representative
  of small-to-medium fashion retailers.
- Empirical benchmarking. Benchmark multiple embedding models spanning
  convolutional and transformer architectures on shared hardware, producing
  empirical guidance for model selection in resource-constrained deployments.

Research Questions
Three questions guide the investigation and are answered empirically in Chapter
3.

RQ1: Model comparison. How do fashion-specific embedding models
compare with general-purpose CNN and ViT architectures on fashion product
retrieval?

RQ2: Accuracy-speed trade-off. What trade-offs exist between retrieval
accuracy and inference latency, and which model offers the best balance for real-
time search?

RQ3: Architecture viability. Can a service-oriented architecture with a
dedicated AI sidecar separate image inference from the main application while
maintaining interactive response times?

Tasks Completed
- Build AI service. Python FastAPI service loading pre-trained embedding
  models for vector generation within interactive latency bounds.
- Set up vector search. PostgreSQL with pgvector for high-dimensional em-
  bedding storage and similarity queries.
- Connect services. .NET backend orchestrating image upload, embedding
  generation, vector database query, and result assembly.
- Create user interface. Vue.js storefront with drag-and-drop image upload and
  similarity-scored results grid.
- Evaluate results. Systematic benchmark measuring retrieval accuracy, infer-
  ence speed, and operational trade-offs across models.

IV. SCOPE AND LIMITATIONS

In scope: visual search via image upload, embedding-based recommendations,
core e-commerce (catalogue, cart, checkout), and multi-model comparison
across CNN and transformer architectures. Out of scope: real payment process-
ing, shipping and logistics, social login, mobile applications, and custom model
training.

Known Limitations
Four limitations define the boundaries of this work.
- Dataset. 5,000 fashion product images [7]. Controlled benchmarking is fea-
  sible at this scale but results may not extrapolate to production catalogues
  containing millions of items.
- Hardware. Consumer-grade (Intel i7-1165G7, 16 GB RAM), all inference
  on CPU. Latency and throughput figures are relative to this profile; GPU
  acceleration would improve both metrics.
- Evaluation. Exclusively quantitative: accuracy, latency, throughput. No for-
  mal user study; relationship between measured metrics and user satisfaction
  remains open.
- Model training. All models used as published. Domain-specific fine-tuning,
  particularly for models pre-trained on generic corpora, might improve quality
  but was beyond scope.

V. RESEARCH METHODOLOGY

This section describes the methodology and tools used to implement and evalu-
ate the system.

Development Methodology
The project follows Design Science Research (DSR) [8], [9] across four phases:
Research and Planning (literature review, model and tool selection), Design
(technology stack, system architecture, schema design), Implementation (.NET
backend with VSA, Python FastAPI sidecar, Vue 3 storefront), and Testing and
Evaluation (mAP accuracy with cross-validation, inference latency, and through-
put for four representative models, selected from six supported by the
embedding framework).

Technologies Used
The system is built using a modular stack designed for performance and scala-
bility:
- Backend: .NET 10 with Carter, MediatR, FluentValidation.
- AI Service: Python 3.12 with FastAPI, PyTorch, Hugging Face Transformers.
- Frontend: Vue 3 with TypeScript, Vite, Pinia.
- Database: PostgreSQL with pgvector for relational and vector data in a single
  ACID database.
The system is evaluated using quantitative metrics: Mean Average Precision
(mAP) with 3-fold cross-validation for retrieval accuracy, per-image inference
latency and throughput (images/second) for efficiency, across four representa-
tive models and the Fashion Product Images Dataset [7] (5,000 images). Detailed
results appear in Chapter 3.

VI. THESIS OUTLINE

This thesis is organized into three parts.

Part 1: Introduction (this part) establishes research context, the problem
statement, objectives, research questions, scope, methodology, and this outline.

Part 2: Thesis Content contains three chapters:
- Chapter 1: Background. Surveys vector embeddings, neural architectures,
  vector databases, prior work in fashion image retrieval, and the technology
  stack.
- Chapter 2: Design and Implementation. Functional and non-functional
  requirements, system architecture (DDD, C4, database, API, security), and
  concrete implementation (.NET backend, Python ML sidecar, Vue storefront).
- Chapter 3: Testing and Evaluation. Systematic benchmark comparing
  retrieval accuracy and inference efficiency across embedding models using
  cross-validation on 5,000 fashion images.

Part 3: Conclusion and Future Work synthesizes findings, evaluates
contributions and limitations, and proposes future work.
```

---

## CHANGE LOG — what moved and why

### 1. Removed the mismatched citation [2] and reworded the abandonment claim
**Before:** *"Industry estimates place session abandonment after unsuccessful search at approximately 30 percent [2]."*
**After:** *"Text-based search compounds the problem: shoppers who fail to find what they are looking for frequently abandon the session rather than reformulate the query."*

Reference [2] in your bibliography is Pinterest's press release about search volume (600M+ monthly searches), not a source for a 30% abandonment statistic. Rather than leave a specific number attached to a citation that doesn't support it, I dropped the precise figure and kept the qualitative point, which your source list can support. If you have (or can find) a real source for the 30% number, specifically, you can put the number back and cite that source instead. I'd suggest something like a Baymard Institute e-commerce search UX study, that's the kind of source that typically publishes abandonment-rate figures like this.
*(Master Fix List, Tier 2, item 18)*

### 2. Fixed the model count in the Development Methodology bullet
**Before:** *"...inference latency, throughput across 11 models."*
**After:** *"...inference latency, and throughput for four representative models, selected from six supported by the embedding framework."*

"Eleven" doesn't match anything in the actual thesis, Table 55 in Chapter 2 (§2.4.4.1) lists the real model registry, and it has six entries. This also fixes a smaller internal inconsistency that existed even within the original Part 1: this bullet said "11 models" while the very next paragraph ("Technologies Used") correctly said "four representative models." Now both say the same thing, and the six-model figure gives the "four out of how many" framing something real to point to.
*(Master Fix List, Tier 1, item 2)*

### 3. Rewrote the Thesis Outline to match your real Table of Contents
**Before:** used "Chapter 1" for both Part I's introduction and the first chapter of Part II, and called the conclusion "Chapter 4," which doesn't exist anywhere in your actual document (Part 3 uses Roman numerals, not chapter numbers).
**After:** describes exactly what your real Table of Contents shows: three parts, with Part 2 numbered Chapter 1 through 3, and Part 3 unnumbered.

This was the clearest, most easily-caught error in Part 1, a reader only has to flip to the Table of Contents to see it doesn't match. The rewritten version is shorter and just describes the structure as it actually is.
*(Master Fix List, Tier 1, item 12)*

### 4. Left everything else untouched
The $770 billion market figure, the problem statement, the objectives, the research questions, the scope and limitations, and the technologies list all checked out during review and needed no changes. I didn't touch the wording anywhere those held up, so the diff you'd see against your current draft is limited to the three items above.

---

## What to do with this

1. Paste the rewritten text back into your actual `.typ` source (this file is plain text, not Typst syntax, since I only had the compiled PDF to work from, not your source files).
2. Double-check citation [1] (Statista) and [7] (Kaggle dataset) still point to valid entries in your reference list, those weren't touched.
3. If you find a real source for the 30% abandonment figure, restore the number with the correct citation.

Ready to do Chapter 1 (Background and Related Work) next whenever you want, same format: rewritten text plus a change log tied to the fix list.
