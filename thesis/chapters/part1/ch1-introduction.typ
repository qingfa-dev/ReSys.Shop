== Context and Motivation

Global fashion e-commerce revenue exceeded *770 billion USD* in 2024, with projections surpassing *one trillion by 2030* @statista2024fashion. Yet keyword search fails where the domain succeeds: fashion products are defined by silhouette, drape, print density, and colour -- attributes that resist textual description. Shoppers who fail to find what they are looking for frequently abandon the session rather than reformulate the query @pinterest2023visual.

*Content-Based Image Retrieval (CBIR)* addresses this gap by replacing textual intermediaries with direct visual comparison. Products are indexed not by human-authored labels but by *dense vector embeddings* computed from images, with similarity measured through mathematical distance functions. A query image of a dress with a particular neckline and print pattern retrieves visually similar products without any keyword translation step. Pre-trained convolutional neural networks @he2016deep @tan2019efficientnet, vision transformers @radford2021learning, and fashion-specific models @chia2022fashionclip have substantially advanced this capability.

The contribution of this work is *architectural*, not algorithmic. It investigates how to embed existing CBIR capabilities into a practical e-commerce system built with conventional web technologies, and provides empirical data on which embedding models deliver the optimal balance of accuracy, latency, and resource efficiency. The work bridges two distinct software ecosystems, the *Python machine learning stack* and the *.NET enterprise web stack*, under real-time latency constraints appropriate for interactive search.

== Problem Statement

*Keyword-reliant fashion search suffers from four compounding inefficiencies.*

*Catalogue vocabulary mismatch.* Varying vendor descriptors fragment result sets, silently excluding relevant products.

*Visual inexpressibility.* Attributes such as fabric drape, texture, silhouette proportion, and pattern rhythm elude text queries.

*Cold-start invisibility.* New products lack interaction history. Visual feature extraction enables discovery immediately from catalogue ingestion.

*Polyglot integration cost.* The Python deep learning ecosystem does not natively interoperate with .NET. Sub-second latency requires architectural isolation of the ML workload.

== Objectives

This project builds a functional fashion e-commerce platform with integrated image-based search and evaluates pre-trained deep learning models within that system. The contribution is the *engineering demonstration* of embedding existing models into a conventional web application stack.

=== Technical Objectives

#list(
  [*Model integration.* Integrate pre-trained vision models into a PostgreSQL and .NET e-commerce stack, establishing a reference pattern for teams with existing web infrastructure.],
  [*Polyglot architecture.* Architect a polyglot system in which a dedicated Python sidecar handles AI inference while the .NET backend manages transactional logic, business rules, and API routing.],
  [*Vector storage validation.* Validate *pgvector* (an open-source PostgreSQL extension) as the sole vector storage and retrieval layer, evaluating whether it meets real-time search latency requirements at catalogue scales representative of small-to-medium fashion retailers.],
  [*Empirical benchmarking.* Benchmark multiple embedding models spanning convolutional and transformer architectures on shared hardware, producing empirical guidance for model selection in resource-constrained deployments.],
)

=== Research Questions

Three questions guide the investigation and are answered empirically in Chapter 3.

*RQ1: Model comparison.* How do fashion-specific embedding models compare with general-purpose CNN and ViT architectures on fashion product retrieval?

*RQ2: Accuracy-speed trade-off.* What trade-offs exist between retrieval accuracy and inference latency, and which model offers the best balance for real-time search?

*RQ3: Architecture viability.* Can a service-oriented architecture with a dedicated AI sidecar separate image inference from the main application while maintaining interactive response times?

=== Tasks Completed

#list(
  [*Build AI service.* Python FastAPI service loading pre-trained embedding models for vector generation within interactive latency bounds.],
  [*Set up vector search.* PostgreSQL with pgvector for high-dimensional embedding storage and similarity queries.],
  [*Connect services.* .NET backend orchestrating image upload, embedding generation, vector database query, and result assembly.],
  [*Create user interface.* Vue.js storefront with drag-and-drop image upload and similarity-scored results grid.],
  [*Evaluate results.* Systematic benchmark measuring retrieval accuracy, inference speed, and operational trade-offs across models.],
)

== Scope and Limitations

In scope: visual search via image upload, embedding-based recommendations, core e-commerce (catalogue, cart, checkout), and multi-model comparison across CNN and transformer architectures. Out of scope: real payment processing, shipping and logistics, social login, mobile applications, and custom model training.

=== Known Limitations

Four limitations define the boundaries of this work.

#list(
  [*Dataset.* 5,000 fashion product images @kaggle-fashion-dataset. Controlled benchmarking is feasible at this scale but results may not extrapolate to production catalogues containing millions of items.],
  [*Hardware.* Consumer-grade (Intel i7-1165G7, 16 GB RAM), all inference on CPU. Latency and throughput figures are relative to this profile; GPU acceleration would improve both metrics.],
  [*Evaluation.* Exclusively quantitative: accuracy, latency, throughput. No formal user study; relationship between measured metrics and user satisfaction remains open.],
  [*Model training.* All models used as published. Domain-specific fine-tuning, particularly for models pre-trained on generic corpora, might improve quality but was beyond scope.],
)

== Research Methodology

This section describes the methodology and tools used to implement and evaluate the system.

=== Development Methodology

The project follows *Design Science Research* (DSR) @hevner2004design @peffers2008design across four phases: Research and Planning (literature review, model and tool selection), Design (technology stack, system architecture, schema design), Implementation (.NET backend with VSA, Python FastAPI sidecar, Vue 3 storefront), and Testing and Evaluation (mAP accuracy with cross-validation, inference latency, throughput across six models).

=== Technologies Used

The system is built using a modular stack designed for performance and scalability:
- *Backend:* .NET 10 with Carter, MediatR, FluentValidation.
- *AI Service:* Python 3.12 with FastAPI, PyTorch, Hugging Face Transformers.
- *Frontend:* Vue 3 with TypeScript, Vite, Pinia.
- *Database:* PostgreSQL with pgvector for relational and vector data in a single ACID database.

The system is evaluated using quantitative metrics: Mean Average Precision (mAP) with 3-fold cross-validation for retrieval accuracy, per-image inference latency and throughput (images/second) for efficiency, across four representative models and the Fashion Product Images Dataset @kaggle-fashion-dataset (5,000 images). Detailed results appear in Chapter 3.

== Thesis Outline

This thesis is organized into three parts.

*Part 1: Introduction* (this part) establishes the research context, problem statement, objectives, research questions, scope, methodology, and this outline.

*Part 2: Thesis Content* contains three chapters:
#list(
  [*Chapter 1: Background and Related Work.* Surveys vector embeddings, neural architectures, vector databases, prior work in fashion image retrieval, and the technology stack.],
  [*Chapter 2: Design and Implementation.* Functional and non-functional requirements, system architecture (DDD, C4, database, API, security), and concrete implementation (.NET backend, Python ML sidecar, Vue storefront).],
  [*Chapter 3: Testing and Evaluation.* Systematic benchmark comparing retrieval accuracy and inference efficiency across embedding models using cross-validation on 5,000 fashion images.],
)

*Part 3: Conclusion and Future Work* synthesizes findings, evaluates contributions and limitations, and proposes future work.
