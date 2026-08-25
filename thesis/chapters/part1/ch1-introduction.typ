== Context and Motivation

Global fashion e-commerce revenue exceeded *770 billion USD* in 2024, with projections surpassing *one trillion by 2030* @statista2024fashion. However, keyword search does not work well for fashion products. Fashion items are defined by silhouette, drape, print density, and colour. These attributes are hard to describe in text. Shoppers who cannot find what they want often leave the session instead of rewriting their query @pinterest2023visual.

*Content-Based Image Retrieval (CBIR)* solves this problem by comparing images directly instead of using text labels. Products are indexed using *dense vector embeddings* computed from images, and similarity is measured through mathematical distance functions, instead of using labels written by humans. For example, if a user uploads an image of a dress with a specific neckline and print pattern, the system can find visually similar products without needing any text search. Pre-trained convolutional neural networks @he2016deep @tan2019efficientnet, vision transformers @radford2021learning, and fashion-specific models @chia2022fashionclip have greatly improved this capability in recent years.

This thesis focuses on system architecture, not on creating new algorithms. It studies how to add existing CBIR methods into a practical e-commerce system built with conventional web technologies, and provides experimental data showing which embedding models give the best balance between accuracy, speed, and resource use. The work connects two different technology stacks: the *Python machine learning stack* and the *.NET web stack*. This must be done while keeping real-time speed for interactive search.

== Problem Statement

*Fashion search that relies only on keywords has four main problems.*

*Catalogue vocabulary mismatch.* Different vendors use different words to describe the same product. This can cause relevant products to be excluded from search results.

*Visual attributes are hard to describe in text.* Attributes such as fabric drape, texture, silhouette shape, and pattern style are difficult to search for using text.

*Cold-start invisibility.* New products do not yet have interaction history. Visual feature extraction allows these products to be found as soon as they are added to the catalogue.

*Polyglot integration cost.* The Python deep learning ecosystem does not work directly with .NET. To keep search speed under one second, the machine learning part of the system must be kept separate from the main application.

== Objectives

This project builds a working fashion e-commerce platform with image-based search, and it benchmarks pre-trained deep learning models within this system. The main contribution is an *engineering demonstration* of adding existing models into a conventional web application stack.

=== Technical Objectives

#list(
  [*Model integration.* Integrate pre-trained vision models into a PostgreSQL and .NET e-commerce stack. This can serve as an example for teams that already use similar web technology.],
  [*Polyglot architecture.* Architect a polyglot system in which a dedicated Python sidecar handles AI inference while the .NET backend manages transactional logic, business rules, and API routing.],
  [*Vector storage validation.* Validate *pgvector* (an open-source PostgreSQL extension) as the sole vector storage and retrieval layer, assessing whether it meets real-time search latency requirements at catalogue scales representative of small-to-medium fashion retailers.],
  [*Empirical benchmarking.* Benchmark multiple embedding models spanning convolutional and transformer architectures on shared hardware, giving practical guidance for choosing a model when resources are limited.],
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
  [*Assess results.* Systematic benchmark measuring retrieval accuracy, inference speed, and operational trade-offs across models.],
)

== Scope and Limitations

In scope: visual search via image upload, embedding-based product recommendation delivered through the *Similar Products* feature (a visual-similarity mechanism, Section 2.3), core e-commerce (catalogue, cart, checkout), and multi-model comparison across CNN and transformer architectures. Out of scope: real payment processing, shipping and logistics, social login, mobile applications, and custom model training.

=== Known Limitations

Four limitations define the boundaries of this work.

#list(
  [*Dataset.* 5,000 fashion product images @kaggle-fashion-dataset. Controlled benchmarking is feasible at this scale but results may not apply directly to production catalogues containing millions of items.],
  [*Hardware.* Consumer-grade (Intel i7-1165G7, 16 GB RAM), all inference on CPU. Latency and throughput figures are relative to this profile; GPU acceleration would improve both metrics.],
  [*Assessment.* Exclusively quantitative: accuracy, latency, throughput. No formal user study, so the relationship between these metrics and actual user satisfaction is still an open question.],
  [*Model training.* All models used as published. Domain-specific fine-tuning, particularly for models pre-trained on generic corpora, might improve quality but was beyond scope.],
)

== Research Methodology

This section describes the methodology and tools used to implement and assess the system.

=== Development Methodology

The project follows *Design Science Research* (DSR) @hevner2004design @peffers2008design across four phases: Research and Planning (literature review, model and tool selection), Design (technology stack, system architecture, schema design), Implementation (.NET backend with VSA, Python FastAPI sidecar, Vue 3 storefront), and Testing and Assessment (mAP accuracy with cross-validation, inference latency, throughput across six models).

=== Technologies Used

The system is built using a modular stack designed for performance and scalability:
- *Backend:* .NET 10 with Carter, MediatR, FluentValidation.
- *AI Service:* Python 3.12 with FastAPI, PyTorch, Hugging Face Transformers.
- *Frontend:* Vue 3 with TypeScript, Vite, Pinia.
- *Database:* PostgreSQL with pgvector for relational and vector data in a single ACID database.

The system is assessed using quantitative metrics: Mean Average Precision (mAP) with 3-fold cross-validation for retrieval accuracy, per-image inference latency and throughput (images/second) for efficiency, across six models spanning CNN, vision-transformer, and CLIP architectures and the Fashion Product Images Dataset @kaggle-fashion-dataset (5,000 images). Detailed results appear in Chapter 3.

== Thesis Outline

This thesis is organized into three parts.

*Part 1: Introduction* (this part) establishes the research context, problem statement, objectives, research questions, scope, methodology, and this outline.

*Part 2: Thesis Content* contains three chapters:
#list(
  [*Chapter 1: Background and Related Work.* Surveys vector embeddings, neural architectures, vector databases, prior work in fashion image retrieval, and the technology stack.],
  [*Chapter 2: Design and Implementation.* Functional and non-functional requirements, system architecture (DDD, C4, database, API, security), and concrete implementation (.NET backend, Python ML sidecar, Vue storefront).],
  [*Chapter 3: Testing and Assessment.* Systematic benchmark comparing retrieval accuracy and inference efficiency across embedding models using cross-validation on 5,000 fashion images.],
)

*Part 3: Conclusion and Future Work* synthesizes findings, assesses contributions and limitations, and proposes future work.
