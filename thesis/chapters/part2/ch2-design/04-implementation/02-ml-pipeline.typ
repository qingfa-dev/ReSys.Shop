=== ML Embedding Pipeline

The Python ML sidecar generates high-dimensional vector embeddings from product images, serving the .NET backend via HTTP endpoints. Built on Python 3.12, FastAPI, and PyTorch, it manages deep learning model lifecycles, hardware acceleration, and embedding inference pipelines.

==== Service Architecture

The sidecar uses a three-layer architecture running under the Uvicorn ASGI server:

- *FastAPI Interface Layer:* Handles HTTP routing, #emph[X-API-Key] header authentication, multipart payload parsing, and JSON response serialization.
- *Model Manager Layer:* A singleton registry that lazily instantiates models, maintains GPU/CPU memory caches, and routes inference calls using the Strategy Pattern.
- *PyTorch Runtime Layer:* Executes forward passes across hardware targets (CUDA for NVIDIA GPUs, MPS for Apple Silicon, or CPU fallback).

The service exposes #emph[POST /embeddings] (returns a float vector) and #emph[GET /health]. Containerized via Docker and orchestrated by .NET Aspire, it is reachable at #emph("http://ml-service") over the internal network.

==== Model Loading Strategy

Models load *lazily* on first request rather than at service startup to prevent unnecessary GPU memory consumption. The Model Manager enforces a standardized #emph("generate_embedding(image_bytes)") interface, selecting the active model via the #emph("EMBEDDING_MODEL") environment variable.

The sidecar supports four architectural model families:

- *Convolutional Networks:* ResNet-50 (2048-dimensional) and EfficientNet-B0 (1280-dimensional) for lightweight feature extraction.
- *Vision Transformers:* DINOv2 ViT-S/14 (384-dimensional) and ViT-B/14 (768-dimensional) for self-supervised representation learning.
- *CLIP Multimodal Models:* CLIP ViT-B/32, ViT-B/16, and ViT-L/14 (512-dimensional) for joint text-image latent space retrieval.
- *Domain-Specific Models:* Fashion-CLIP (512-dimensional), fine-tuned on 700,000+ fashion images for high-accuracy product categorization.

==== Embedding Generation Flow

The vector generation process moves from raw image ingestion to JSON serialization across seven structured stages (@fig-ml-pipeline).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/04-implementation/diagrams/P2S2.2.4_ml-pipeline.png", width: 100%),
  caption: [ML embedding pipeline: step-by-step flow from image upload to vector response.],
) <fig-ml-pipeline>

1. *Authentication:* Validates the #emph[X-API-Key] header, rejecting unauthorized calls with a #emph[401 Unauthorized] status.
2. *Model Selection:* Retrieves the active model from the Singleton cache, initializing it from disk if unallocated.
3. *Preprocessing:* Resizes input images to $224 times 224$ pixels, converts interleaved channels to tensor arrays, and applies ImageNet normalization.
4. *Forward Pass:* Propagates tensors through convolution or self-attention layers to extract visual features.
5. *Pooling:* Applies global average pooling to output a fixed-length float vector regardless of initial image dimensions.
6. *Serialization:* Packs the float vector (e.g., 512 dimensions for CLIP variants) alongside execution latency metadata into a JSON payload.

==== Health Monitoring

The #emph[GET /health] endpoint provides diagnostic health checks for the .NET Aspire orchestrator by validating three conditions:

1. *Hardware Runtime:* Confirms CUDA or MPS acceleration availability.
2. *Model Availability:* Ensures the active model is instantiated in memory.
3. *Inference Pass:* Runs a baseline synthetic pass through preprocessing and forward execution.

The endpoint returns GPU memory utilization, model load state, and latency metrics, enabling automated orchestrator restarts if performance degrades.