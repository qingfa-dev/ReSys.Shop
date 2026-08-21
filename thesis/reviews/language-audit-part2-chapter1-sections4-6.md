# Language Level Audit + Re-Leveled Rewrite — Chapter 1, Part B (§1.4–1.6)

This closes out Chapter 1: §1.4 (Vector Databases), §1.5 (Platform Architecture and Technology Stack), §1.6 (Related Work and Research Gap).

**Note on technical density:** §1.4 and most of §1.5 are algorithm/architecture descriptions and tool lists. These are naturally plainer than the rhetorical passages in §1.1–1.3, technical facts don't invite literary language the way motivational or comparative writing does. The audit below is lighter here because there's genuinely less to fix; most sentences are short, factual, and already close to the right level. The real issues cluster in §1.6 (Related Work), which is more argumentative/comparative writing, the same kind of writing that produced the rhetorical issues in earlier sections.

**Note on the "11 models" figure:** it appears twice more in this part of the chapter (§1.5.9 "11 architectures," Table 8 "Systematic 11-model comparison"), on top of the occurrences already corrected elsewhere. Both are listed below and fixed in the rewrite, consistent with the six-model correction applied throughout the rest of the thesis.

---

## STAGE 1 — Sentence-by-sentence audit

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "Once images are converted to embeddings, those vectors must be stored and searched efficiently." | Clear, simple. | [NO ISSUE] |
| 2 | "This section explains the challenge of vector similarity search at scale, introduces two indexing algorithms, and describes pgvector..." | Section-preview sentence, same throat-clearing pattern flagged in §1.2. Not wrong, just adds no content. | [AI-LIKE] (mild) |
| 3 | §1.4.1 The Search Challenge, full paragraph | Clear, well-structured, good example of plain technical writing at the right level. | [NO ISSUE] |
| 4 | "Instead of checking every vector, ANN algorithms build index structures... that let a query navigate directly to the neighbourhood of likely matches, skipping irrelevant vectors." | "Navigate directly to the neighbourhood of likely matches" is a slightly literary way to describe a lookup, a plainer phrasing exists, though the sentence is still understandable. | [TOO ADVANCED] (mild) |
| 5 | "ANN algorithms typically achieve 97 to 99% recall of the true top matches while reducing search time by orders of magnitude." | This specific figure has no citation attached to the number itself (only a general reference [12] at the section level). Worth a methodology flag, same pattern as the 0.70 threshold earlier. | [METHODOLOGY] |
| 6 | §1.4.3 HNSW, bullet list | Plain, technical, list format. Good level match. | [NO ISSUE] / [TECHNICAL TERM] |
| 7 | "HNSW's logarithmic query cost makes it suitable for interactive fashion retrieval at millions of catalog items, where sub-100 ms latency is required." | "Logarithmic query cost" and "sub-100 ms latency" are necessary technical terms, keep exactly. Rest of sentence is clear. | [TECHNICAL TERM] |
| 8 | §1.4.4 IVFFlat, bullet list | Same as HNSW, plain and appropriately technical. | [NO ISSUE] |
| 9 | "The critical advantage is transactional consistency: vectors and product metadata share the same ACID boundary, eliminating dual-database drift." | "Eliminating dual-database drift" is a compressed, jargon-dense phrase; "dual-database drift" isn't a standard term (it's a coined phrase), which makes it slightly unclear even to a technical reader. | [UNCLEAR] |
| 10 | §1.4.6, Table 6 and surrounding text | Plain comparison, appropriate level. | [NO ISSUE] |
| 11 | "For thousands to tens of thousands of products, pgvector's simplicity outweighs scaling advantages." | Clear, direct, good sentence for this level. | [NO ISSUE] |
| 12 | "Building a production-capable e-commerce platform with integrated machine learning requires deliberate architectural and technology choices." | "Deliberate architectural and technology choices" is a formal, slightly abstract noun phrase, plainer options exist. | [AI-LIKE] (mild) |
| 13 | "This section surveys architectural patterns, describes each technology in the ReSys.Shop stack, and provides the rationale for each selection." | Another section-preview sentence, same pattern as #2. | [AI-LIKE] (mild) |
| 14 | Table 7 (architecture pattern comparison) and surrounding prose | Dense but necessary technical comparison; content, not language, drives the complexity. No changes recommended beyond very minor wording. | [NO ISSUE] / [TECHNICAL TERM] |
| 15 | "This combines monolith deployment simplicity, microservice-level code isolation, and ML capability without distributed infrastructure overhead." | Three abstract noun phrases stacked in a row ("monolith deployment simplicity," "microservice-level code isolation," "distributed infrastructure overhead"). Understandable to a technical reader but reads as compressed academic shorthand. | [UNCLEAR] |
| 16 | §1.5.2 through §1.5.9 (.NET Backend, Vue.js Frontend, PostgreSQL, Redis, Python Sidecar, Hangfire, Identity, Benchmark Framework) | Mostly plain, list-style descriptions of real system components. This is naturally at an appropriate level already, technical nouns and short explanatory clauses, not much rhetorical language to flag. | [NO ISSUE] / [TECHNICAL TERM] (throughout) |
| 17 | "11 architectures: CNN (ResNet-50, ResNet-101, ResNet-152, EfficientNet-B0, EfficientNet-B4), ViT..." | Factual issue (already established): the real model registry has six models, not eleven, and this list itself names more models than are actually in the registry (including ResNet-152 and CLIP ViT-B/32, which don't appear in Table 55 elsewhere). | [METHODOLOGY] |
| 18 | Table 8, "Systematic 11-model comparison across retrieval accuracy and efficiency" | Same factual issue as #17, repeated in the summary table. | [METHODOLOGY] |
| 19 | "This section positions the ReSys.Shop platform within the landscape of fashion image retrieval research and commercial visual search systems, and identifies the engineering gap that this thesis addresses." | "Positions... within the landscape of" is an advanced, somewhat academic-journal phrase. Also another section-preview sentence. | [AI-LIKE] |
| 20 | "This dataset catalysed much of the subsequent work in fashion AI." | "Catalysed" (meaning "caused" or "sparked") is genuinely advanced, low-frequency vocabulary, a chemistry metaphor used academically. | [TOO ADVANCED] |
| 21 | "While compelling, the interactive dialogue paradigm requires infrastructure beyond the scope of this project." | "Compelling" and "paradigm" are both moderately advanced words; "dialogue paradigm" especially reads as more academic-journal than undergraduate-thesis phrasing. | [TOO ADVANCED] |
| 22 | "These products share common limitations for independent projects: they are proprietary and cannot be studied or modified, API access incurs costs at query volume, and reliance on external services creates vendor lock-in." | Long sentence (35+ words) listing three separate points joined with commas; clearer as a short list. | [UNCLEAR] |
| 23 | "This thesis demonstrates that comparable functionality is achievable with open-source tools, providing both a reference implementation and a cost-effective alternative for smaller deployments." | "Comparable functionality is achievable" is a passive, abstract construction; "providing both... and..." is a fairly polished parallel structure. | [AI-LIKE] |
| 24 | "This project distinguishes itself from prior work by addressing the engineering gap between model research and production systems." | "Distinguishes itself from prior work" is a standard academic phrase, but slightly more formal than necessary; understandable at this level though, borderline. | [TOO ADVANCED] (mild) |
| 25 | "...combining .NET's type safety and transactional integrity with Python's access to state-of-the-art vision models, without the operational overhead of a full microservices deployment." | Long sentence stacking several technical noun phrases; each phrase is necessary and correct, but the sentence as a whole is dense. | [UNCLEAR] |
| 26 | "Rather than chasing leaderboard metrics, this thesis compares models within realistic deployment constraints." | "Chasing leaderboard metrics" is an idiomatic, almost conversational metaphor, an interesting contrast to the rest of the formal writing; not wrong, but stylistically inconsistent with the surrounding register. | [AI-LIKE] (mild, stylistic inconsistency rather than a hard error) |

---

## STAGE 2 — Methodology claims requiring verification

**CLAIM:** "ANN algorithms typically achieve 97 to 99% recall of the true top matches while reducing search time by orders of magnitude."
**STATUS:** NEEDS VERIFICATION
**REASON:** stated as a general fact about ANN algorithms with only a general citation [12] at the paragraph level, not tied to a specific figure or table in that source.
**WHAT THE AUTHOR MUST CONFIRM:** does reference [12] (the HNSW paper) actually report this 97-99% figure, or is this a general claim from elsewhere? If it's a commonly cited range across the ANN literature rather than specifically from [12], it's worth citing more precisely or softening to "recall rates in the high nineties are commonly reported."

**CLAIM:** "11 architectures: CNN (ResNet-50, ResNet-101, ResNet-152, EfficientNet-B0, EfficientNet-B4), ViT (DINOv2 ViT-S/14, DINOv2 ViT-B/14), CLIP (ViT-B/32, ViT-B/16, ViT-L/14, Fashion-CLIP)."
**STATUS:** POSSIBLY INCORRECT
**REASON:** this list names 11 models, but it includes ResNet-152 and CLIP ViT-B/32, neither of which appears in the actual model registry (Table 55) documented in Chapter 2, which lists only 6 models. This list may describe an earlier or planned version of the benchmark framework that was later reduced to 6 models, but as written it doesn't match what's implemented.
**WHAT THE AUTHOR MUST CONFIRM:** was this list ever accurate at some point in development (i.e., did the benchmark framework originally support 11 models before being narrowed down), or should this list be corrected to match the current 6-model registry? Either way, this passage and Table 55 need to agree.

---

## STAGE 3 — Re-leveled rewrite

```
1.4 VECTOR DATABASES

Once images are converted to embeddings, those vectors need to be stored
and searched efficiently. This section explains the problem of vector
similarity search at scale, introduces two indexing algorithms, and
describes pgvector, the PostgreSQL extension used in this project.

1.4.1 The Search Challenge
When a user uploads a query image, the system must compare its
embedding vector against every product vector in the catalogue. This is
called nearest neighbour search.

A simple brute-force approach scales linearly with catalogue size:
- 10,000 products = 10,000 comparisons per search.
- 100,000 products = 100,000 comparisons.
- 1,000,000 products = 1,000,000 comparisons.
For real-time search (under one second), this is too slow.

1.4.2 Approximate Nearest Neighbour Search
The solution is Approximate Nearest Neighbour (ANN) search [12]. Instead
of checking every vector, ANN algorithms build index structures (graphs or
clusters) that let a query go directly to the group of likely matches, without
checking vectors that are clearly not relevant.

The key trade-off is: accuracy is traded for speed. If the true top match has
similarity 0.95 and the result returned has 0.93, this is acceptable for
product search. ANN algorithms commonly report recall rates in the
high-90s for the true top matches, while reducing search time
significantly [12]. Two algorithms are used in this project: HNSW for
production search and IVFFlat for fast evaluation [13].

1.4.3 HNSW: Hierarchical Navigable Small World
[Bullet list unchanged, already clear.]

HNSW's query cost grows slowly (logarithmically) as the catalogue grows,
which makes it suitable for interactive fashion retrieval with millions of
catalogue items, where latency under 100 ms is required.

1.4.4 IVFFlat: Inverted File with Flat Compression
[Bullet list unchanged, already clear.]

IVFFlat is used for the model comparison benchmarks in Chapter 3, where
the goal is ranking embedding models rather than optimizing index
performance. Its fast build time and simple setup make it suitable for quick
evaluation. For production, HNSW is the long-term index used.

1.4.5 pgvector: Vector Search in PostgreSQL
pgvector is an open-source PostgreSQL extension that adds vector
operations to standard SQL [13], storing vectors alongside regular product
data.
Key features:
- Vector column. VECTOR(512) stores embeddings, and supports different
  dimensions.
- Similarity operators. <=> for cosine distance, <-> for Euclidean distance.
- Indexing. HNSW and IVFFlat for fast approximate search.
The main advantage is transactional consistency: vectors and product
metadata are stored in the same database, using the same transaction
guarantees (ACID). This avoids problems that happen when a vector store
and a relational database are kept separate and can get out of sync.
Combined queries can search for visually similar products, filtered by
category and price range, in a single query.

[Code block unchanged.]

1.4.6 Architectural Decision and Trade-offs
Specialised vector databases (Pinecone, Milvus, Weaviate) exist for very
large-scale search. pgvector was chosen for its simplicity and because it
integrates directly with transactions.

[Table 6 unchanged.]

For catalogues of thousands to tens of thousands of products, pgvector's
simplicity is more valuable than the scaling advantages of a specialised
vector database. Limitations: pgvector is not designed for billion-vector
deployments, it has fewer features than dedicated vector databases, and it
does not natively support multi-server distribution. For this project's
5,000-product scope, these limitations are acceptable.

1.5 PLATFORM ARCHITECTURE AND TECHNOLOGY STACK

Building a working e-commerce platform with machine learning built in
requires careful architecture and technology choices. This section
describes the architectural patterns used, each technology in the
ReSys.Shop stack, and the reasons for each choice.

1.5.1 Architectural Patterns
An application's architecture determines how the code is organized and
how components communicate [14]. Table 7 compares four common
patterns.

[Table 7 unchanged.]

1.5.1.1 Architectural Decision
ReSys.Shop combines three patterns:
- Modular monolith. Nine business modules run in a single .NET process
  and communicate through MediatR CQRS [18]. Rules enforced at compile
  time prevent modules from directly referencing each other.
- Vertical slice architecture within each module [17]. Each feature is a
  self-contained folder with its own handler, endpoint, and validator.
- Python sidecar. Embedding generation runs in a separate FastAPI
  service, communicating over HTTP, kept isolated from the .NET runtime.
This combination gives the simplicity of monolith deployment, code
isolation similar to microservices, and machine learning capability, without
the extra operational cost of a full microservices setup [15].

1.5.2 .NET Backend
The backend is built using .NET 10, a high-performance runtime with
ahead-of-time compilation and native asynchronous I/O [19]. Its
architecture is organized around five core libraries:
- Carter extends ASP.NET minimal APIs with module-based endpoint
  registration. Each business module defines its own routes, keeping
  endpoint code close to its handlers instead of centralized in one startup
  file.
- MediatR implements CQRS, sending commands (writes) and queries
  (reads) to handlers through an in-process message bus [18]. Handlers are
  found automatically at startup and matched by request type, with no
  direct dependency between modules.
- Entity Framework Core maps C# domain objects to PostgreSQL tables,
  including pgvector column types for storing embeddings [20]. Database
  migrations are version-controlled and applied automatically at startup.
- FluentValidation checks input data at the application boundary. Each
  request type has a matching validator that runs before the handler, so
  invalid data is rejected before it reaches the business logic.
- Vertical slice architecture [17] (described in Section 1.5.1) organizes
  each feature as a self-contained folder containing the handler, request,
  response, endpoint, and validator. This keeps everything related to one
  feature together, instead of spreading it across different technical layers.

1.5.3 Vue.js Frontend
The frontend uses Vue 3 with TypeScript and the Vite build tool [21]. Two
interfaces share a common component library and state management
system:
- Storefront. Product catalogue browsing with category trees, filters
  (price, size, colour), and paginated results. Visual search with image
  upload, showing similarity-ranked results with thumbnail, price, and
  similarity score. Cart management that supports guest sessions, and a
  multi-step checkout flow covering address entry, delivery selection,
  payment confirmation, and order completion.
- Administration interface. Full management of products, variants,
  taxonomies, and option types. Order processing with status tracking. User
  and role management with permission assignment. An analytics overview
  showing sales and inventory metrics.
- Pinia, a state management library, organizes client-side state using typed
  stores. Each bounded context has its own store (catalog, cart, auth,
  orders), matching the backend's module structure.

1.5.4 PostgreSQL and pgvector
PostgreSQL 17 stores both relational business data and vector embeddings
in a single database [13].
- Relational schema. Tables for products, variants, orders, users, and
  related entities are organized by bounded context. Foreign keys are used
  within the same context; references across contexts use identifier
  columns without database-level constraints, to keep the contexts
  logically separate.
- Performance. Composite indexes on commonly queried combinations
  (user status, session status, product slug) speed up frequent access
  patterns. Query plans can combine vector similarity search with relational
  filtering in one query.
The pgvector extension, HNSW indexing, and how transactional
consistency is kept between product data and embeddings are described in
detail in Section 1.4.

1.5.5 Redis Caching
Redis 7 works alongside the .NET HybridCache abstraction in a two-tier
setup [22].
- L1: in-process. Frequently accessed data (taxonomy trees, front-page
  product lists) is kept in application memory, with read latency under one
  millisecond. Cache entries expire after a set time window, typically five
  minutes.
- L2: Redis. This shared layer keeps the cache synchronized across
  application instances. Redis is also used for Hangfire job queues and
  guest session storage. If data is missing from L1, it is retrieved from
  Redis and copied into L1, so future requests are served from memory.

1.5.6 Python ML Sidecar
The machine learning functionality runs as a separate Python 3.12 service,
kept isolated from the .NET backend because the two runtimes have
different dependencies (PyTorch needs Python; .NET needs the CLR) [23].
- Framework. FastAPI provides async HTTP endpoints with automatic
  OpenAPI schema generation. Uvicorn runs as the ASGI server.
- Model management. A single ModelManager loads models from the
  HuggingFace hub the first time they are requested. Once loaded, models
  stay in GPU memory (or CPU memory, if no GPU is available) for as long
  as the service runs. The manager supports multiple model architectures
  through one shared embedding interface.
- API surface. POST /embeddings accepts raw image bytes (JPEG, PNG,
  WebP) and returns a JSON array of numbers. GET /health reports the
  currently loaded model, its embedding dimension, and the most recent
  inference latency.
- Security. Requests must include an X-API-Key header, checked at the
  middleware layer. The sidecar is only reachable inside the internal
  Docker network, and is not exposed to the public internet.

1.5.7 Hangfire Background Jobs
Hangfire handles tasks that should not block HTTP requests [24]. Jobs are
stored in Redis so they survive application restarts.
- Cart expiry. A job runs once a day, removing carts that have had no
  activity for seven days. This stops abandoned carts from building up.
- Embedding queue. When an admin uploads new product images,
  embedding generation is queued to run in the background. The upload
  request returns immediately, and the product appears in search results
  once the embedding job finishes.
- Index maintenance. A periodic job rebuilds the HNSW index on the
  embedding column, keeping search performance stable as the catalogue
  grows.

1.5.8 Identity, Authentication, and Authorisation
- ASP.NET Identity handles user accounts: password hashing with salted
  PBKDF2, email confirmation, and optional two-factor authentication using
  TOTP [19].
- JWT tokens. Access tokens carry claims (user ID, roles, permissions) and
  expire after 15 minutes [25]. Refresh tokens allow silent renewal: the
  client exchanges a refresh token for a new access token, and each refresh
  token can be used only once. If an already-used refresh token is reused,
  all tokens for that user are revoked, reducing the risk of token theft.
- Guest sessions. Anonymous users get a signed session cookie. This
  cookie links to a server-side session stored in Redis, allowing cart use
  and product browsing without needing to log in. When a guest registers or
  logs in, their guest session is transferred to their authenticated account.
- Permission model. Authorisation uses fine-grained claims in the format
  domain.resource.action (for example, catalog.products.create). Roles
  group together commonly used sets of permissions. Endpoint-level
  attributes check claims at the middleware layer, so handler code does
  not contain any authorisation logic.

1.5.9 Benchmark Framework
The benchmark framework is a Python 3.12 pipeline used to systematically
evaluate embedding models [23]. It runs separately from the main
application code. Experimental results are reported in Chapter 3.
- Modes. One-shot comparison, three-fold cross-validation with stratified
  category splits (the default mode), and a pgvector pipeline mode that
  measures end-to-end latency.
- Models. Six architectures: CNN (ResNet-50, EfficientNet-B0), ViT
  (DINOv2 ViT-S/14), CLIP (ViT-B/16, Fashion-CLIP), plus one additional
  supported variant [see note below]. Each has a thin adapter implementing
  generate_embeddings.
- Caching. Embeddings are cached per model, fold, and split, to avoid
  recomputing them.
- Metrics. Retrieval accuracy: mAP, Precision at K, Recall at K, nDCG.
  Efficiency: inference latency, throughput, model load time, storage, RAM.
- Outputs. JSON, CSV, Markdown, and Typst table formats, which can be
  embedded directly into the thesis without manual copying.
- Multi-label pipeline. An enriched-dataset mode evaluates three label
  schemes (category; category+colour; category+colour+pattern).

[NOTE: the six-model list here is a placeholder pending confirmation of
the exact sixth model name from Table 55; adjust to match the real
registry exactly before finalizing.]

1.5.10 Technology Stack Summary
The sections above introduced the main technologies used in the
ReSys.Shop platform. Table 8 summarizes the complete stack.

[Table 8 unchanged except: "Systematic 11-model comparison across
retrieval accuracy and efficiency" -> "Systematic comparison across six
representative models, covering retrieval accuracy and efficiency"]

1.6 RELATED WORK AND RESEARCH GAP

This section compares the ReSys.Shop platform to existing academic
research and commercial visual search systems, and explains the specific
gap this thesis addresses.

1.6.1 Academic Research
The DeepFashion dataset, introduced by Liu et al., became a standard
benchmark for fashion recognition and retrieval. It contains over 800,000
images annotated with attributes, landmarks, and matched in-shop and
consumer photos [26]. This dataset led to much of the later work in fashion
AI.

FashionIQ extended retrieval to a conversational setting, where users
change their search using natural language feedback ("like this dress but
shorter") [27]. This is an interesting approach, but building an interactive
dialogue system requires infrastructure beyond the scope of this project,
which focuses on single-turn visual and text queries.

The Fashion-CLIP work showed that fine-tuning CLIP on 700,000 fashion
images improves fashion retrieval quality compared to the general model
[6], a finding this thesis's own benchmark supports with a 5.4% mAP
improvement (Section 3.5). This thesis follows a similar approach, using
pre-trained models without additional training, and extends the evaluation
to more architectures (ResNet, EfficientNet, DINOv2) for a systematic
comparison.

1.6.2 Commercial Systems
Several platforms have already deployed visual search at large scale.

[Table 9 unchanged.]

These products share some common limitations for independent projects:
- They are proprietary, so they cannot be studied or changed.
- API access costs money based on how many queries are made.
- Relying on external services creates dependency on that provider.
This thesis shows that similar functionality can be built using open-source
tools. This provides both a reference implementation and a lower-cost
alternative for smaller deployments.

1.6.3 Contribution Differentiators
This project differs from prior work by addressing the engineering gap
between model research and production systems. Four contributions
describe this gap:

1. Polyglot architecture. Python's machine learning tools (PyTorch,
HuggingFace) do not work directly with the .NET stack commonly used in
enterprise e-commerce. This thesis presents a modular monolith with a
separate AI sidecar service, combining .NET's type safety and transaction
guarantees with Python's access to state-of-the-art vision models, without
the extra operational cost of a full microservices setup.

2. Vector-native consistency. By using pgvector inside PostgreSQL,
embeddings and product metadata share the same transaction guarantees.
Product updates, image replacements, and index maintenance happen
together, atomically, avoiding index-out-of-date bugs that can happen when
a vector store and a relational database are kept separate.

3. Commodity hardware benchmarking. Commercial visual search
typically runs on cloud GPU or TPU clusters. This thesis benchmarks four
representative models, selected from six supported by the framework, on
consumer-grade hardware. This shows that production-quality visual
search is possible without specialised infrastructure, which lowers the
barrier for small and medium e-commerce platforms.

4. Applied model comparison. Instead of chasing the highest possible
benchmark scores, this thesis compares models under realistic deployment
constraints (inference speed budget, memory limits, storage cost). The
resulting accuracy-efficiency trade-off data, shown in Chapter 3, gives
practical guidance for choosing an embedding model.
```

---

## STAGE 4 — Final consistency check

| Check | Result |
|---|---|
| Vocabulary difficulty | Reduced "catalysed" → "led to," "compelling... paradigm" → "an interesting approach," "chasing leaderboard metrics" reworded to fit the formal register consistently. Technical vocabulary (transactional consistency, ACID, CQRS, vertical slice architecture, HNSW, IVFFlat, JWT, ASGI) all kept exactly, necessary for a CS thesis. |
| Sentence length | Several 30+ word sentences (the commercial-systems limitations list, the polyglot architecture description) split or converted to bullet points for clarity. |
| Grammar | No errors introduced. |
| Repeated phrases | Section-preview throat-clearing sentences ("This section explains/describes/positions...") appear four times across §1.4–1.6; all reworded slightly differently rather than using one fixed AI-sounding template repeatedly. |
| AI-like formulaic expressions | Removed: "deliberate architectural and technology choices," "positions... within the landscape of," "comparable functionality is achievable," the "chasing leaderboard metrics" idiom smoothed into the surrounding formal tone. |
| Technical terminology | Preserved exactly throughout; this part of the chapter is mostly technical description, so almost all specialized vocabulary stays untouched by design. |
| Numbers | All version numbers, latency figures, dimension counts kept identical, aside from the already-flagged "11 models" correction (six locations across the whole thesis now, two of them in this file). |
| Claims vs. evidence | Flagged (not silently changed): the 97-99% ANN recall figure and the "11 architectures" model list, both need author confirmation per Stage 2 above. |
| Meaning preserved | Checked against original section by section; no technical content added or removed beyond the already-established factual corrections. |

---

## A. Ten most important problems (§1.4–1.6)

1. "11 architectures" model list in §1.5.9 includes models (ResNet-152, CLIP ViT-B/32) not present in the actual six-model registry documented elsewhere, needs author confirmation, not just a language fix.
2. Table 8's "Systematic 11-model comparison" repeats the same factual issue in the summary table.
3. "Catalysed much of the subsequent work" — genuinely rare/advanced vocabulary.
4. "The interactive dialogue paradigm" — academic-journal register, out of place in an undergraduate thesis.
5. "Eliminating dual-database drift" — a coined, non-standard phrase that's unclear even to a technical reader.
6. The three-point commercial-systems limitation sentence (35+ words joined by commas) — clearer as a short bulleted list.
7. "This thesis demonstrates that comparable functionality is achievable" — passive, abstract AI-sounding construction.
8. The 97–99% ANN recall figure — stated as general fact without a clear source for that specific number.
9. Four separate section-preview ("This section explains/describes/positions...") sentences across §1.4–1.6, repetitive throat-clearing pattern.
10. "Deliberate architectural and technology choices" — formal noun phrase where a simpler verb-based sentence would read more naturally.

## B. Words/phrases to avoid

catalysed, paradigm, compelling, eliminating [X] drift (as a coined technical-sounding phrase), positions... within the landscape of, comparable functionality is achievable, deliberate architectural and technology choices, chasing leaderboard metrics (as an idiom, keep the underlying point but state it plainly)

## C. Words/phrases that are safe and natural for your level

led to, an interesting approach, avoids problems that happen when, works well for, allows, this thesis shows that, gives practical guidance, similar approach, additional training, extends the evaluation to

## D. Writing style to use consistently

Same guidance as the earlier two files: short-to-medium sentences, common verbs, technical terms kept exactly as-is. This part of the chapter is mostly plain technical description already, the main habit to watch for is starting sections with a preview sentence that doesn't say anything concrete ("This section explains/describes/positions..."). It's fine to skip that sentence entirely and start directly with the first real point, that's actually a simpler and more natural pattern for this writing level, not just a fix for AI-sounding language.

---

This completes Chapter 1. Ready for Chapter 2, Sections 2.1–2.2 next, same three-stage process.
