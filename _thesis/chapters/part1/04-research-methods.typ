== Research Methods

This section describes the methodology and tools used to implement and evaluate the system.

=== Development Methodology

The project follows an iterative development process comprising four main phases:

==== Phase 1: Research and Planning
The initial phase focused on understanding the problem domain and selecting appropriate tools:
- Reviewed literature on visual search and fashion AI.
- Explored pre-trained models (EfficientNet, DINOv2, Fashion-CLIP).
- Evaluated database options for vector storage.
- Planned the microservices architecture.

==== Phase 2: Design
Based on the research, the system design was formalized:
- Defined the technology stack (.NET, Python, Vue.js, PostgreSQL).
- Designed the communication protocols between services.
- Structured the database schema to support hybrid data (relational + vector).

==== Phase 3: Implementation
The core development involved building the distinct components:
- *Backend:* Implemented the .NET API with Vertical Slice Architecture.
- *ML Service:* Developed the Python service for image inference.
- *Frontend:* Built the Vue.js storefront with reactive search features.
- *Infrastructure:* Configured PostgreSQL with the `pgvector` extension.

==== Phase 4: Testing and Evaluation
The final phase verified the system's performance:
- Conducted accuracy tests using the mAP\@10 metric.
- Measured latency and throughput.
- Performed qualitative review of search results.

=== Technologies Used

The system is built using a modern, distributed stack designed for performance and scalability:

- *Backend:* .NET 10 with ASP.NET Core Minimal APIs for high-performance handling of business logic and requests.
- *AI Service:* Python 3.12 with PyTorch, utilizing libraries like `transformers` and `torchvision` for running deep learning models.
- *Frontend:* Vue 3 with TypeScript and Vite, providing a responsive and type-safe user interface.
- *Database:* PostgreSQL 16 equipped with the `pgvector` extension to enable high-speed vector similarity searches alongside standard relational data.

=== Testing Approach

The system is evaluated using a combination of quantitative and qualitative metrics:

- *Accuracy Testing:* Measuring Mean Average Precision at 10 (mAP\@10) to quantify how relevant the search results are.
- *Performance Testing:* Benchmarking inference time (per image) and total end-to-end search latency to ensure the system is responsive.
- *Dataset:* The evaluation uses a controlled subset of the *Fashion Product Images Dataset* @kaggle-fashion-dataset (5,000 items) to ensure consistent and reproducible results.

More details on the experimental setup and results are provided in Chapter 3.

