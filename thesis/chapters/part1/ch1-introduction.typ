= Introduction

== Context and Motivation

The discovery and purchase of clothing have shifted fundamentally toward digital platforms, yet the methods for locating specific products remain largely tethered to text-based retrieval. Global fashion e-commerce revenue exceeded 770 billion U.S. dollars in 2024 and is projected to surpass one trillion by 2030 @statista2024fashion, positioning the sector among the fastest-growing segments of online retail. Despite this expansion, the search bar — which serves as the primary interface between users and product catalogs — often struggles to interpret the visual nuances that define fashion. This limitation is central to the motivation of this project.

A persistent challenge in this domain is the *semantic gap*: the discrepancy between the visual complexity of a product and the linguistic capability of a user to describe it. A customer may easily recognize a specific pattern, silhouette, or texture but struggle to articulate it using standardized metadata terms like "bohemian asymmetric maxi dress with botanical motifs." If the product catalog's indexing does not perfectly align with the user's vocabulary, the search fails despite the product's existence. Studies on e-commerce search behavior indicate that up to 30 percent of shoppers abandon a site after an unsuccessful search @pinterest2023visual, reflecting the commercial cost of this gap. The problem is particularly acute in fashion, where visual attributes — drape, proportion, colour gradients, and texture — resist reduction to keywords, and where the same garment is described differently by different sellers, different brands, and different customers.

Modern visual search systems, powered by deep learning, offer a path beyond keyword limitation. Instead of relying on human-assigned labels, these systems encode the visual content of images into dense vector representations — embeddings — that can be compared mathematically for similarity. A query image of a dress with a particular neckline and print pattern can retrieve visually similar products without any textual intermediary. This capability, known as Content-Based Image Retrieval (CBIR), has been accelerated by advances in pre-trained convolutional neural networks @he2016deep @tan2019efficientnet and vision transformers @radford2021learning, including fashion-specific models trained on domain-relevant data @chia2022fashionclip.

The central question this thesis investigates is not whether visual search works — that has been demonstrated in both academic literature and industrial deployments — but rather *how* to integrate these capabilities into a practical e-commerce system built with conventional web technologies, and *which* embedding models deliver the best balance of accuracy, speed, and resource efficiency in that context. This is fundamentally an engineering problem: the spanning of two distinct software ecosystems — the Python machine learning stack and the .NET enterprise web stack — while serving real-time user requests within latency budgets acceptable for interactive search.

== Problem Statement

The core problem addressed by this project is the inherent inefficiency of keyword-reliant search for fashion discovery. This broad challenge is decomposed into four concrete technical and functional issues:

*Linguistic inconsistency.* Traditional search systems depend on precise, consistent product labels. Large-scale catalogs, however, frequently suffer from descriptor variance: one listing uses "floral," another "botanical," a third "flower print," all referring to the same visual property. This inconsistency fragments search results, surfaces incomplete matches, and forces users to iterate through multiple query formulations to find what they seek.

*Visual inexpressibility.* Many defining fashion attributes — draping, texture, colour gradients, silhouette shape, and pattern density — are intuitive to the human eye but difficult to translate into text queries. A customer can identify a desired aesthetic in a photograph but cannot produce the keywords that would retrieve items matching that aesthetic. This mismatch leads to high bounce rates and lost conversions.

*Cold start data scarcity.* Recommendation systems frequently rely on collaborative filtering, which aggregates historical user-item interactions to surface relevant products. New items, by definition, lack this interaction data. Visual feature extraction offers an alternative path: by encoding product images into embeddings, similarity-based recommendations become possible for newly listed inventory based solely on appearance, without any interaction history.

*Polyglot integration complexity.* The deep learning ecosystem — frameworks such as PyTorch, the HuggingFace model hub, and the broader Python scientific computing stack — does not natively interoperate with the .NET ecosystem used for transactional e-commerce backends. Bridging these two environments in a production-capable system requires careful architectural design to ensure low latency, fault isolation, and maintainable inter-service communication. This thesis addresses this challenge through a modular monolith architecture augmented with a dedicated Python sidecar service, avoiding the operational overhead of a full microservices deployment while preserving the benefits of technology-appropriate service boundaries.

== Objectives

The primary goal of this project is to build a functional fashion e-commerce platform that enables image-based product search, and to evaluate how effectively pre-trained deep learning models can be integrated into such a system. Rather than developing novel AI architectures, this work concentrates on the engineering challenge of embedding existing models into a practical web application stack.

=== Technical Objectives

This project addresses specific engineering challenges through the following objectives:

- *Demonstrating the integration* of pre-trained deep learning models into a conventional e-commerce stack built on PostgreSQL and .NET, establishing a reference pattern for teams with existing web infrastructure who wish to add visual search capabilities.
- *Architecting a polyglot system* that efficiently bridges .NET transactional logic with Python-based AI inference, using a sidecar service pattern that isolates the machine learning workload while maintaining low-latency communication with the main application.
- *Validating the feasibility* of open-source vector database tools — specifically the pgvector extension for PostgreSQL — for real-time similarity search at a scale representative of small-to-medium fashion catalogs.
- *Benchmarking the performance* of multiple embedding models spanning convolutional neural network and vision transformer architectures within a constrained hardware environment, providing empirical guidance for practitioners selecting models for similar deployments.

=== Research Questions

The project aims to answer the following questions:

+ *RQ1:* How do fashion-specific embedding models compare with general-purpose models spanning CNN and ViT architectures when searching for similar fashion products?
+ *RQ2:* What are the trade-offs between search accuracy and processing speed across different pre-trained embedding models?
+ *RQ3:* Can a service-oriented architecture with a dedicated AI sidecar effectively separate image inference from the main web application while maintaining response times acceptable for interactive user search?

=== Specific Tasks

To answer these questions, the following tasks were completed:

+ *Build an AI service.* Develop a Python service using FastAPI that loads and executes multiple pre-trained image embedding models. The service accepts product images, generates feature vectors, and returns results within a target latency budget appropriate for interactive use.

+ *Set up vector search.* Configure PostgreSQL with the pgvector extension to store and index high-dimensional image embeddings. Validate that similarity search queries execute correctly and efficiently on a catalog scale representative of a small-to-medium fashion retailer.

+ *Connect the services.* Implement a .NET backend layer that communicates with the Python AI service via HTTP, orchestrating the end-to-end search flow — image upload, feature extraction, vector database query, and result return — within the latency envelope expected by end users.

+ *Create the user interface.* Build a Vue.js storefront application where users can upload query images via drag-and-drop or file selection and browse visually similar products returned by the search pipeline.

+ *Evaluate the results.* Conduct a systematic benchmark measuring retrieval accuracy through standard information retrieval metrics, comparing processing speed across models, and analyzing the operational trade-offs that inform model selection for production deployment.

== Scope and Limitations

*Included Scope:*
- Image upload and visual search functionality accessible from the storefront user interface
- Product recommendations derived from visual similarity
- Core e-commerce features, including product catalog browsing and shopping cart management
- Systematic comparison of multiple embedding models spanning both CNN-based and transformer-based architectures
- End-to-end performance measurement covering inference latency, query throughput, and storage footprint

*Excluded Scope:*
- Real payment processing — transactions are simulated for demonstration purposes
- Complex shipping, logistics, and inventory management workflows
- User authentication via social login providers
- Mobile application development — the system targets desktop and tablet web browsers
- Custom model training or fine-tuning — this work evaluates pre-trained models exclusively

=== Known Limitations

This project has several limitations that should be acknowledged:

+ *Dataset size.* The evaluation uses 5,000 fashion product images from the Fashion Product Images dataset @kaggle-fashion-dataset. While sufficient for controlled comparative benchmarking, this is smaller than production catalogs that may contain millions of items. Results may not extrapolate directly to web-scale deployments.

+ *Hardware constraints.* All experiments were conducted on consumer-grade hardware with a mid-range GPU. A production deployment would likely benefit from dedicated inference servers with larger GPU memory and higher compute throughput. Reported latency and throughput figures should be interpreted relative to the experimental hardware profile.

+ *No user testing.* Due to scope constraints, the evaluation focuses exclusively on quantitative technical metrics — retrieval accuracy, inference latency, and throughput. Results were qualitatively reviewed by visual inspection of search output, but no formal user experience study was conducted. The relationship between measured accuracy metrics and subjective user satisfaction remains an open question for future investigation.

+ *Pre-trained models only.* All embedding models are used as published by their original authors, without fine-tuning on the evaluation dataset. Domain-specific fine-tuning might improve retrieval quality, particularly for models pre-trained on generic image corpora rather than fashion-specific data, but this was beyond the scope of the present work.

These limitations define starting points for future improvement and are revisited in the concluding chapter.

== Research Methodology

This thesis adopts a Design Science Research (DSR) methodology @hevner2004design @peffers2008design. DSR is a problem-solving paradigm that produces and evaluates an IT artifact — in this case, a fashion e-commerce platform with integrated visual search — to address a defined problem domain. The methodology is structured around the iterative construction and assessment of a working system, with knowledge contributions derived from both the artifact itself and the empirical measurements obtained during its evaluation.

The project progressed through four phases. The *research and planning* phase involved surveying literature on visual search in fashion, exploring available pre-trained embedding models, evaluating database options for vector storage, and selecting the technology stack. The *design* phase formalized the system architecture — a modular .NET monolith with a Python AI sidecar communicating via HTTP — and structured the database schema to support both relational and vector data. The *implementation* phase built the three principal components: the .NET backend following vertical slice architecture, the Python embedding service, and the Vue.js storefront. The *test and evaluation* phase measured retrieval accuracy using standard information retrieval metrics, benchmarked inference latency and throughput, and conducted qualitative review of search output.

This methodology is appropriate for the project's goals because the primary contribution is not a theoretical advance in machine learning but an engineering demonstration: a working system that integrates academic model research into a conventional web stack, accompanied by empirical data on the performance trade-offs that practitioners face when making model and architecture decisions.

== Thesis Outline

This thesis is organized into seven chapters across three parts.

*Part I: Introduction.*

*Chapter 1 — Introduction* establishes the research context and motivation, defines the problem, states the objectives and research questions, delineates the scope and limitations, describes the methodology, and provides the present outline.

*Part II: Thesis Content.*

*Chapter 2 — Background and Related Work* provides the technical foundation necessary to understand the system. It covers vector embeddings and their mathematical role in similarity search, surveys convolutional and transformer-based neural network architectures for image feature extraction, examines vector database technologies with emphasis on pgvector, reviews prior academic and commercial work in fashion image retrieval, and concludes with a summary of the technology stack selected for this project.

*Chapter 3 — Requirements Analysis* translates the problem statement into concrete requirements for the system. It identifies functional and non-functional requirements organized by business domain, describes the user roles and their interactions with the platform, and presents use cases for the visual search workflow.

*Chapter 4 — System Architecture and Design* presents the architectural design of the platform. It describes the modular monolith structure and the bounded contexts of each business module, explains the sidecar pattern used to integrate the Python AI service with the .NET backend, details the database schema including the pgvector extension configuration, and documents the API design for the visual search pipeline.

*Chapter 5 — Implementation* describes the realization of the design in code. It covers the .NET backend implementation using vertical slice architecture, the Python embedding service built with FastAPI and PyTorch, and the Vue.js storefront application, including key patterns and conventions applied across the codebase.

*Chapter 6 — Testing and Evaluation* presents the empirical evaluation of the system. It reports the results of a systematic benchmark comparing retrieval accuracy across four embedding models using a three-fold cross-validation protocol on 5,000 fashion product images, measures inference latency and throughput on consumer-grade hardware, and analyzes the accuracy-speed trade-offs that inform model selection decisions.

*Part III: Conclusion.*

*Chapter 7 — Conclusion and Future Work* synthesizes the findings, evaluates the achievement of each research objective against the empirical results, discusses the significance and limitations of the work, and proposes directions for future research and system improvement.
