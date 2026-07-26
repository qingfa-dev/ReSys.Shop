== Context and Motivation

Global fashion e-commerce revenue exceeded 770 billion USD in 2024, with projections surpassing one trillion by 2030 @statista2024fashion. Yet its dominant interface, keyword search, fails where the domain succeeds: fashion products are defined by silhouette, drape, print density, and colour relationships -- attributes that resist textual description. This *semantic gap*, the discrepancy between a garment's visual richness and a user's ability to express it in words, is well documented. A customer can recognise a desired aesthetic instantly from a photograph yet cannot produce query terms that retrieve it. When catalogue indexing uses inconsistent terminology (one vendor tags a pattern as "floral," another as "botanical," a third as "flower print"), relevant products are systematically excluded. Industry estimates place the session abandonment rate after an unsuccessful search at approximately 30 percent @pinterest2023visual.

Content-Based Image Retrieval (CBIR) addresses this gap by replacing textual intermediaries with direct visual comparison. Products are indexed not by human-authored labels but by dense vector embeddings computed from images, with similarity measured through mathematical distance functions. A query image of a dress with a particular neckline and print pattern retrieves visually similar products without any keyword translation step. Pre-trained convolutional neural networks @he2016deep @tan2019efficientnet, vision transformers @radford2021learning, and fashion-specific models @chia2022fashionclip have substantially advanced this capability.

The contribution of this work is architectural rather than algorithmic. It investigates how to embed existing CBIR capabilities into a practical e-commerce system built with conventional web technologies, and provides empirical data on which embedding models deliver the optimal balance of accuracy, latency, and resource efficiency. The work bridges two distinct software ecosystems, the Python machine learning stack and the .NET enterprise web stack, under real-time latency constraints appropriate for interactive search.

== Problem Statement

Keyword-reliant fashion search suffers from four compounding inefficiencies.

Catalogue descriptors vary across vendors, such that a single visual pattern appears under multiple labels and fragments result sets. Users must reformulate queries iteratively as the vocabulary mismatch between catalogue indexing and customer search silently excludes relevant products.

Visual attributes -- fabric drape, texture gradient, silhouette proportion, and pattern rhythm -- cannot be captured reliably through text queries. A customer identifies a desired aesthetic instantly in a photograph yet cannot produce keywords that retrieve it. The search engine cannot match what the user cannot name.

Recommendation models based on collaborative filtering depend on historical user-item interactions. Newly listed products lack this data at their point of highest commercial value: initial release. Visual feature extraction bypasses this limitation, as product images are available from catalogue ingestion and embeddings enable similarity-based discovery without interaction history.

Finally, integrating pre-trained vision models into a .NET transactional backend introduces a recurring engineering challenge in applied machine learning. The Python deep learning ecosystem (PyTorch, HuggingFace) does not natively interoperate with the .NET enterprise stack. Achieving sub-second response latency across this boundary requires architectural design that isolates the ML workload from the main application while bridging incompatible package managers, runtime environments, and deployment conventions.

== Objectives

This project builds a functional fashion e-commerce platform with integrated image-based search and evaluates the effectiveness of pre-trained deep learning models within that system. The contribution is not a novel AI architecture but the engineering demonstration of embedding existing models into a conventional web application stack.

=== Technical Objectives

#list(
  [Integrate pre-trained vision models into a PostgreSQL and .NET e-commerce stack, establishing a reference pattern for teams with existing web infrastructure.],
  [Architect a polyglot system in which a dedicated Python sidecar handles AI inference while the .NET backend manages transactional logic, business rules, and API routing.],
  [Validate pgvector (an open-source PostgreSQL extension) as the sole vector storage and retrieval layer, evaluating whether it meets real-time search latency requirements at catalogue scales representative of small-to-medium fashion retailers.],
  [Benchmark multiple embedding models spanning convolutional and transformer architectures on shared hardware, producing empirical guidance for model selection in resource-constrained deployments.],
)

=== Research Questions

Three questions guide the investigation and are answered empirically in Chapter 3.

*RQ1* addresses model comparison: how do fashion-specific embedding models compare with general-purpose CNN and ViT architectures on fashion product retrieval? This question tests whether domain-specific training on fashion data yields measurable improvements over models trained on generic image corpora.

*RQ2* addresses the accuracy-speed trade-off: what trade-offs exist between retrieval accuracy and inference latency across pre-trained embedding models, and which model offers the best balance for real-time search? The most accurate model is rarely the fastest; deployment decisions require weighing both dimensions.

*RQ3* addresses architecture viability: can a service-oriented architecture with a dedicated AI sidecar separate image inference from the main application while maintaining interactive response times? This question evaluates whether the chosen polyglot pattern (Python ML service alongside a .NET application) is practical for production use.

=== Tasks Completed

#list(
  [*Build an AI service.* A Python FastAPI service that loads multiple pre-trained embedding models and generates feature vectors from product images within interactive latency bounds.],
  [*Set up vector search.* PostgreSQL configured with pgvector to store and index high-dimensional embeddings, with similarity queries validated for correctness and performance on a catalogue of representative size.],
  [*Connect the services.* A .NET backend layer that orchestrates image upload, embedding generation, vector database query, and result assembly into a single end-to-end search flow.],
  [*Create the user interface.* A Vue.js storefront with drag-and-drop image upload and a results grid displaying visually similar products with similarity scores.],
  [*Evaluate the results.* A systematic benchmark measuring retrieval accuracy via standard information retrieval metrics, comparing inference speed across models, and analyzing operational trade-offs that inform production model selection.],
)

== Scope and Limitations

The thesis encompasses the design, implementation, and empirical evaluation of a fashion e-commerce platform with integrated visual search. Five areas define the included scope:

#list(
  [Visual search from the storefront, with image upload via drag-and-drop or file selection.],
  [Product recommendations derived from embedding similarity scores.],
  [Core e-commerce functionality: product catalogue browsing, shopping cart, and simulated checkout.],
  [Multi-model comparison spanning several CNN and transformer architectures.],
  [End-to-end latency, throughput, and storage footprint measurement.],
)

The following areas are explicitly excluded:

#list(
  [Real payment processing (transactions are simulated for demonstration purposes).],
  [Shipping, logistics, and warehouse management workflows.],
  [User authentication via social login providers.],
  [Mobile application development (the system targets desktop and tablet web browsers).],
  [Custom model training or fine-tuning (all models are used as published).],
)

=== Known Limitations

Four limitations define the boundaries of this work and are revisited in the concluding chapter.

#list(
  [*Dataset size.* Evaluation uses 5,000 fashion product images from the Fashion Product Images dataset @kaggle-fashion-dataset. Controlled comparative benchmarking is feasible at this scale, but results may not extrapolate to production catalogues containing millions of items.],
  [*Hardware.* Experiments ran on consumer-grade hardware (Intel i7-1165G7, 16 GB RAM) with all inference executed on CPU. Reported latency and throughput figures are relative to this CPU-only profile. A dedicated inference server with GPU acceleration would likely improve both metrics.],
  [*Evaluation method.* Evaluation is exclusively quantitative: retrieval accuracy, inference latency, and throughput. Search output was reviewed qualitatively through visual inspection, but no formal user experience study was conducted. The relationship between measured accuracy metrics and subjective user satisfaction remains an open question.],
  [*Model training.* All embedding models were used as published by their original authors, without fine-tuning on the evaluation dataset. Domain-specific fine-tuning, particularly for models pre-trained on generic image corpora rather than fashion data, might improve retrieval quality but was beyond scope.],
)

== Research Methodology

This thesis follows a Design Science Research (DSR) methodology @hevner2004design @peffers2008design, a problem-solving paradigm that produces and evaluates an IT artifact (here, the e-commerce platform with integrated visual search) to address a defined problem domain.

The project progressed through four phases:

#list(
  [*Research and planning.* Literature survey on visual search in fashion and e-commerce architectures; exploration of available pre-trained embedding models; evaluation of vector database options; technology stack selection.],
  [*Design.* Formalization of a modular .NET monolith architecture with a Python AI sidecar communicating via HTTP; database schema design supporting both relational and vector data within a single PostgreSQL instance.],
  [*Implementation.* Construction of three principal components: the .NET backend following vertical slice architecture, the Python embedding service with lazy model loading, and the Vue.js storefront.],
  [*Test and evaluation.* Systematic benchmark measuring retrieval accuracy through standard information retrieval metrics; latency and throughput measurement on consumer-grade hardware; qualitative review of search output.],
)

This methodology suits the project's engineering focus: the primary output is not a theoretical contribution but a working system accompanied by empirical data on performance trade-offs practitioners encounter when integrating academic models into production web stacks.

== Thesis Outline

The thesis is organized into five chapters across three parts.

*Part I: Introduction.* Chapter 1 establishes research context, defines the problem, states objectives and research questions, delineates scope and limitations, describes the methodology, and provides the present outline.

*Part II: Thesis Content* contains three chapters:

#list(
  [*Chapter 1: Background and Related Work.* Surveys vector embeddings, convolutional and transformer-based neural architectures, vector database technologies including pgvector, prior work in fashion image retrieval, and the technology stack selected for this project.],
  [*Chapter 2: Design and Implementation.* Translates the problem into functional and non-functional requirements (Section 2.1), presents the system architecture including domain-driven design, C4 diagrams, database design, API design, and security model (Section 2.2), and describes the concrete implementation of the .NET backend, Python ML sidecar, and Vue.js storefront (Section 2.3).],
  [*Chapter 3: Testing and Evaluation.* Reports a systematic benchmark comparing retrieval accuracy and inference efficiency across multiple embedding models using a cross-validation protocol on 5,000 fashion product images, and analyzes the accuracy-speed trade-offs that inform model selection.],
)

*Part III: Conclusion.* Chapter 4 synthesizes findings against each research objective, evaluates contributions and limitations, and proposes directions for future work.
