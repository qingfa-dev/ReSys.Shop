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

*Linguistic inconsistency.* Catalogue descriptors vary across vendors and brands. A single visual pattern appears under multiple labels ("floral," "botanical," "flower print"), fragmenting result sets across queries and forcing users to reformulate searches iteratively. This mismatch between a catalogue's indexing vocabulary and a customer's search vocabulary silently excludes relevant products from results.

*Visual inexpressibility.* Attributes such as fabric drape, texture gradient, silhouette proportion, and pattern rhythm cannot be reliably captured through text queries. A customer can identify a desired aesthetic instantly in a photograph yet cannot produce keywords that retrieve visually matching products. The user recognises what they want; the search engine does not.

*Cold start data scarcity.* Recommendation models based on collaborative filtering require historical user-item interaction data. Newly listed products, by definition, lack this data and are invisible to interaction-based recommenders at the point of highest commercial value: their initial release window. Visual feature extraction bypasses this limitation: product images are available from catalogue ingestion, and embeddings computed from those images enable similarity-based discovery without any interaction history.

*Polyglot integration complexity.* The Python deep learning ecosystem (PyTorch, HuggingFace, and the broader scientific Python stack) does not natively interoperate with the .NET transactional backend common in enterprise e-commerce. Integrating pre-trained vision models into an existing .NET application stack requires architectural design that isolates the machine learning workload from the main application while preserving sub-second response latency. This integration problem, bridging two software ecosystems with incompatible package managers, runtime environments, and deployment conventions, is a recurring engineering challenge in applied machine learning that the system architecture must directly address.

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

Three questions guide the investigation and are answered empirically in Chapter 6.

*RQ1* addresses model comparison: how do fashion-specific embedding models compare with general-purpose CNN and ViT architectures on fashion product retrieval? This question tests whether domain-specific training on fashion data yields measurable improvements over models trained on generic image corpora.

*RQ2* addresses the accuracy-speed trade-off: what trade-offs exist between retrieval accuracy and inference latency across pre-trained embedding models, and which model offers the best balance for real-time search? This question acknowledges that the most accurate model is rarely the fastest, and that deployment decisions require weighing both dimensions.

*RQ3* addresses architecture viability: can a service-oriented architecture with a dedicated AI sidecar separate image inference from the main application while maintaining interactive response times? This question evaluates whether the chosen polyglot pattern (Python ML service alongside a .NET application) is practical for production use, not merely a laboratory demonstration.

=== Tasks Completed

#list(
  [*Build an AI service.* A Python FastAPI service that loads multiple pre-trained embedding models, generates feature vectors from product images, and returns results within interactive latency bounds.],
  [*Set up vector search.* PostgreSQL configured with pgvector to store and index high-dimensional embeddings, with similarity queries validated for correctness and performance on a catalogue of representative size.],
  [*Connect the services.* A .NET backend layer that orchestrates image upload, embedding generation, vector database query, and result assembly into a single end-to-end search flow.],
  [*Create the user interface.* A Vue.js storefront with drag-and-drop image upload and a results grid displaying visually similar products with similarity scores.],
  [*Evaluate the results.* A systematic benchmark measuring retrieval accuracy via standard information retrieval metrics, comparing inference speed across models, and analyzing the operational trade-offs that inform production model selection.],
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
  [*Hardware.* Experiments ran on consumer-grade hardware with a mid-range GPU. Reported latency and throughput figures are relative to this experimental hardware profile. A dedicated inference server with higher GPU memory and compute throughput would likely improve both metrics.],
  [*Evaluation method.* Evaluation is exclusively quantitative: retrieval accuracy, inference latency, and throughput. Search output was reviewed qualitatively through visual inspection, but no formal user experience study was conducted. The relationship between measured accuracy metrics and subjective user satisfaction remains an open question.],
  [*Model training.* All embedding models were used as published by their original authors, without fine-tuning on the evaluation dataset. Domain-specific fine-tuning, particularly for models pre-trained on generic image corpora rather than fashion data, might improve retrieval quality but was beyond scope.],
)

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

*Part II: Thesis Content* contains five chapters:

#list(
  [*Chapter 2: Background and Related Work.* Surveys vector embeddings, convolutional and transformer-based neural network architectures, vector database technologies including pgvector, prior academic and commercial work in fashion image retrieval, and the technology stack selected for this project.],
  [*Chapter 3: Requirements Analysis.* Translates the problem statement into functional requirements organized by business domain, specifies non-functional requirements, and presents use cases for the visual search workflow.],
  [*Chapter 4: System Architecture and Design.* Details the modular monolith structure with bounded contexts, the Python sidecar integration pattern, the database schema including pgvector configuration, the REST API design, and the security model.],
  [*Chapter 5: Implementation.* Describes the .NET backend built with vertical slice architecture, the Python embedding service using FastAPI and PyTorch, and the Vue.js storefront, including key patterns applied across the system.],
  [*Chapter 6: Testing and Evaluation.* Reports a systematic benchmark comparing retrieval accuracy and inference efficiency across multiple embedding models using a cross-validation protocol on 5,000 fashion product images, and analyzes the accuracy-speed trade-offs that inform model selection.],
)

*Part III: Conclusion.* Chapter 7 synthesizes findings against each research objective, evaluates contributions and limitations, and proposes directions for future work.
