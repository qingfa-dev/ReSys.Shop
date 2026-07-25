== Context and Motivation

The global fashion e-commerce market exceeded 770 billion U.S. dollars in revenue during 2024, with projections surpassing one trillion by 2030 @statista2024fashion. Despite this scale, the dominant interface, the text search bar, remains poorly suited to the domain. Fashion products are defined by attributes that resist keyword description: silhouette shape, fabric drape, print density, and colour relationships.

A well-documented barrier is the *semantic gap*: the discrepancy between the visual richness of a garment and a user's ability to express that richness in words. A customer may identify a desired aesthetic instantly in a photograph yet fail to produce query terms that retrieve matching items. When catalogue indexing uses inconsistent terminology (one vendor tags a pattern as "floral," another as "botanical," a third as "flower print"), relevant products are systematically excluded from search results. Industry data suggests that approximately 30 percent of online shoppers abandon a session after an unsuccessful search @pinterest2023visual, a loss rate with direct commercial consequence.

#quote(block: true, attribution: [The semantic gap in fashion search])[
  A customer may easily recognize a specific pattern, silhouette, or texture but struggle to articulate it using standardized metadata terms like "bohemian asymmetric maxi dress with botanical motifs."
]

Content-Based Image Retrieval (CBIR) addresses this gap by replacing textual intermediaries with direct visual comparison. Rather than indexing products by human-authored labels, CBIR systems encode images into dense vector representations (embeddings) and measure similarity through mathematical distance functions. A query image of a dress with a particular neckline and print pattern retrieves visually similar products without any keyword translation step. This capability has been advanced substantially by pre-trained convolutional neural networks @he2016deep @tan2019efficientnet, vision transformers @radford2021learning, and fashion-specific models trained on domain-relevant corpora @chia2022fashionclip.

*The contribution of this thesis is therefore not algorithmic but architectural.* It investigates how to embed existing CBIR capabilities into a practical e-commerce system built with conventional web technologies, and provides empirical data on which embedding models deliver the optimal balance of accuracy, latency, and resource efficiency within that setting. The work bridges two distinct software ecosystems: the Python machine learning stack and the .NET enterprise web stack, under real-time latency constraints appropriate for interactive search.

== Problem Statement

Keyword-reliant search for fashion discovery suffers from inefficiencies that compound across four dimensions.

#figure(
  table(
    columns: (auto, 1fr),
    align: (start, start),
    [*Constraint*], [*Impact on search*],
    [Linguistic inconsistency],
    [Catalogue descriptors vary across vendors and brands. A single visual pattern appears under multiple labels ("floral," "botanical," "flower print"), fragmenting result sets and forcing users to reformulate queries iteratively.],
    [Visual inexpressibility],
    [Attributes such as drape, texture gradient, silhouette proportion, and pattern rhythm resist textual articulation. A customer can identify a desired aesthetic visually but cannot produce keywords that retrieve matching products, leading to search abandonment.],
    [Cold start data scarcity],
    [Collaborative filtering based on interaction history cannot serve newly listed items. Visual feature extraction bypasses this limitation: embeddings enable similarity-based discovery immediately upon catalogue ingestion, without behavioural data.],
    [Polyglot integration complexity],
    [The Python deep learning ecosystem (PyTorch, HuggingFace, and the scientific Python stack) does not natively interoperate with the .NET transactional backend common in enterprise e-commerce. Bridging these environments requires architectural design that preserves low latency while isolating the machine learning workload from the main application.],
  ),
  caption: [Four dimensions of the keyword search problem in fashion e-commerce],
)

== Objectives

The project builds a functional fashion e-commerce platform with integrated image-based search and evaluates the effectiveness of pre-trained deep learning models within this system. The contribution is not a novel AI architecture but the engineering demonstration of embedding existing models into a conventional web application stack.

=== Technical Objectives

#list(
  [Integrate pre-trained vision models into a PostgreSQL and .NET e-commerce stack, establishing a reference pattern for teams with existing web infrastructure.],
  [Architect a polyglot system in which a dedicated Python sidecar handles AI inference while the .NET backend manages transactional logic, business rules, and API routing.],
  [Validate pgvector (an open-source PostgreSQL extension) as the sole vector storage and retrieval layer, evaluating whether it meets real-time search latency requirements at catalogue scales representative of small-to-medium fashion retailers.],
  [Benchmark multiple embedding models spanning convolutional and transformer architectures on shared hardware, producing empirical guidance for model selection in resource-constrained deployments.],
)

=== Research Questions

#list(
  [*RQ1:* How do fashion-specific embedding models compare with general-purpose models spanning CNN and ViT architectures when retrieving similar fashion products?],
  [*RQ2:* What are the trade-offs between retrieval accuracy and inference latency across different pre-trained embedding models?],
  [*RQ3:* Can a service-oriented architecture with a dedicated AI sidecar effectively separate image inference from the main web application while maintaining response times acceptable for interactive search?],
)

=== Tasks Completed

#list(
  [*Build an AI service.* A Python FastAPI service that loads multiple pre-trained embedding models, generates feature vectors from product images, and returns results within interactive latency bounds.],
  [*Set up vector search.* PostgreSQL configured with pgvector to store and index high-dimensional embeddings, with similarity queries validated for correctness and performance on a catalogue of representative size.],
  [*Connect the services.* A .NET backend layer that orchestrates image upload, embedding generation, vector database query, and result assembly into a single end-to-end search flow.],
  [*Create the user interface.* A Vue.js storefront with drag-and-drop image upload and a results grid displaying visually similar products with similarity scores.],
  [*Evaluate the results.* A systematic benchmark measuring retrieval accuracy via standard information retrieval metrics, comparing inference speed across models, and analyzing the operational trade-offs that inform production model selection.],
)

== Scope and Limitations

*Included:*

#list(
  [Visual search from the storefront, with image upload via drag-and-drop or file selection.],
  [Product recommendations derived from embedding similarity scores.],
  [Core e-commerce: product catalogue browsing, shopping cart, and simulated checkout.],
  [Multi-model comparison spanning several CNN and transformer architectures.],
  [End-to-end latency, throughput, and storage footprint measurement.],
)

*Excluded:*

#list(
  [Real payment processing (transactions are simulated).],
  [Shipping, logistics, and warehouse management workflows.],
  [Social login authentication.],
  [Mobile application development.],
  [Custom model training or fine-tuning (pre-trained models only).],
)

=== Known Limitations

#list(
  [*Dataset.* Evaluation uses 5,000 fashion product images from the Fashion Product Images dataset @kaggle-fashion-dataset. This is smaller than production catalogues containing millions of items; results may not extrapolate to web-scale deployments.],
  [*Hardware.* Experiments ran on consumer-grade hardware with a mid-range GPU. A dedicated inference server with higher GPU memory and compute throughput would likely improve latency and throughput figures. Reported numbers are relative to the experimental hardware profile.],
  [*No user testing.* Evaluation is exclusively quantitative: retrieval accuracy, inference latency, and throughput. Search output was reviewed qualitatively through visual inspection, but no formal user experience study was conducted. The relationship between measured metrics and subjective satisfaction remains an open question.],
  [*Pre-trained models only.* All embedding models were used as published by their original authors. Domain-specific fine-tuning (particularly for models pre-trained on generic image corpora) might improve retrieval quality but was beyond scope.],
)

These limitations define the starting points for future research discussed in Chapter 7.

== Research Methodology

This thesis follows a Design Science Research (DSR) methodology @hevner2004design @peffers2008design, a problem-solving paradigm that produces and evaluates an IT artifact (here, a fashion e-commerce platform with integrated visual search) to address a defined problem domain.

The project progressed through four phases:

#list(
  [*Research and planning.* Literature survey on visual search in fashion and e-commerce architectures; exploration of available pre-trained embedding models; evaluation of vector database options; technology stack selection.],
  [*Design.* Formalization of a modular .NET monolith architecture with a Python AI sidecar communicating via HTTP; database schema design supporting both relational and vector data within a single PostgreSQL instance.],
  [*Implementation.* Construction of three principal components: the .NET backend following vertical slice architecture, the Python embedding service with lazy model loading, and the Vue.js storefront.],
  [*Test and evaluation.* Systematic benchmark measuring retrieval accuracy through standard information retrieval metrics; latency and throughput measurement on consumer-grade hardware; qualitative review of search output.],
)

This methodology suits the project's engineering focus: the primary output is not a theoretical contribution to machine learning research but a working system accompanied by empirical data on the performance trade-offs practitioners encounter when integrating academic models into production web stacks.

== Thesis Outline

The thesis is organized into seven chapters across three parts.

*Part I: Introduction.* Chapter 1 establishes research context, defines the problem, states objectives and research questions, delineates scope and limitations, describes the methodology, and provides the present outline.

*Part II: Thesis Content.* Chapter 2 introduces the technical foundations: vector embeddings, CNN and transformer architectures, vector database technologies, and related work in fashion image retrieval. Chapter 3 translates the problem statement into functional and non-functional requirements organized by business domain. Chapter 4 presents the modular monolith architecture, bounded contexts, database schema, API design, and security model. Chapter 5 describes the implementation: the .NET backend, Python AI service, and Vue.js storefront. Chapter 6 reports the empirical evaluation: a systematic benchmark comparing retrieval accuracy and efficiency across multiple embedding models, with analysis of accuracy-speed trade-offs.

*Part III: Conclusion.* Chapter 7 synthesizes findings against each research objective, evaluates contributions and limitations, and proposes directions for future work.
