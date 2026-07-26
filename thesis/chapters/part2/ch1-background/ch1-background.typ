= Background and Related Work

This chapter establishes the theoretical foundation and technical prerequisites for the project. It surveys the problem domain, the core search paradigm, the machine learning models and vector database technologies that enable visual search, the software architecture and platform stack, and the academic and commercial landscape that positions this work.

The chapter is organized into the following key sections:

- *Fashion E-commerce.* Establishes the market context, semantic gap, and business case for visual search in fashion.
- *Content-Based Image Retrieval.* Introduces vector embeddings, cosine similarity, and the end-to-end CBIR pipeline.
- *Machine Learning Models.* Compares CNN, ViT, and CLIP-based architectures including Fashion-CLIP for visual feature extraction.
- *Vector Databases.* Examines approximate nearest neighbour search, HNSW and IVFFlat indexing algorithms, and the pgvector PostgreSQL extension.
- *Platform Architecture and Technology Stack.* Details the modular monolith pattern with vertical slice architecture, the .NET backend, Vue.js frontend, PostgreSQL database, Redis caching, Python ML sidecar, orchestration, background jobs, authentication, and benchmarking framework.
- *Related Work and Research Gap.* Contextualises the project within academic research and commercial visual search systems, identifying the engineering gap it addresses.

#include "f1-fashion-ecommerce.typ"
#include "f2-cbir.typ"
#include "f4-ml-models.typ"
#include "f5-vector-databases.typ"
#include "f3-software-tech.typ"
#include "f6-related-work.typ"
