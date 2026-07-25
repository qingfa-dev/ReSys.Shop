=== ML Embedding Pipeline

The Python machine learning sidecar is the core research contribution of this thesis. It is a specialised AI service responsible for generating high-dimensional vector embeddings from product images, exposing endpoints consumed by the .NET backend through HTTP. This section describes the internal architecture, the model loading strategy, the embedding generation flow, and the health monitoring mechanism.

==== Service Architecture

The ML sidecar is built on Python 3.12 with the FastAPI framework and runs under the Uvicorn ASGI server. Its internal architecture follows a three-layer design:

- The *FastAPI interface layer* handles HTTP concerns: routing requests to the correct endpoint, validating the API key presented in the `X-API-Key` header, parsing multipart form data containing image bytes, and serialising embedding results to JSON. This layer contains no machine learning logic.

- The *Model Manager layer* is a singleton service responsible for the lifecycle of deep learning models. It maintains a registry of loaded models, loads new models on demand, caches them in GPU or CPU memory, and dispatches inference requests to the correct model instance. The singleton pattern ensures that a single copy of each model, typically consuming several gigabytes of VRAM, serves all inbound requests.

- The *PyTorch Runtime layer* is the hardware-facing component that executes forward passes through the neural network. It handles device placement, selecting CUDA for NVIDIA GPUs, MPS for Apple Silicon, or CPU as a fallback, and manages the tensor operations that convert raw image data into embedding vectors.

The service exposes two endpoints: `POST /embeddings`, which accepts image bytes and returns a 512-dimensional float vector, and `GET /health`, which reports the operational status of the underlying hardware and loaded models. The service is containerised as a Docker image and orchestrated by .NET Aspire, which manages service discovery, the backend addresses the ML sidecar as `http://ml-service` on the internal Docker network, and health-check restarts.

==== Model Loading Strategy

The ML sidecar employs a *lazy loading* strategy to optimise resource utilisation. Deep learning models are not loaded at service startup, when they would consume gigabytes of GPU memory before the first request arrives. Instead, models are loaded upon their first invocation, when the .NET backend sends an image to be embedded for the first time, the Model Manager instantiates the configured model, loads its weights into memory, and caches the instance for all subsequent requests.

The Model Manager implements the *Strategy Pattern*: each model conforms to a common interface with a standard `generate_embedding(image_bytes)` method, and the manager selects the active model based on the `EMBEDDING_MODEL` environment variable. Changing models requires only a configuration change and a service restart, no code changes. The Model Manager maintains an internal dictionary-style cache keyed by model name, so a request for a previously loaded model returns the cached instance immediately.

The supported models span four architectural families. *Convolutional neural networks* are represented by ResNet-50 (2048-dimensional output) and EfficientNet-B0 (1280-dimensional), both pre-trained on ImageNet and serving as lightweight feature extractors. *Vision transformers* include DINOv2 ViT-S/14 (384-dimensional) and DINOv2 ViT-B/14 (768-dimensional), leveraging self-supervised pre-training that requires no labelled data. *CLIP-based models*, CLIP ViT-B/32, CLIP ViT-B/16, and CLIP ViT-L/14, produce 512-dimensional embeddings in a shared text-image latent space, enabling multimodal search where a text query can find visually similar products. *Fashion-specific models* are represented by Fashion-CLIP, a CLIP variant fine-tuned on over 700 000 fashion images with domain-specific vocabulary, also producing 512-dimensional vectors.

==== Embedding Generation Flow

The embedding generation process follows a precise pipeline from image reception to vector output. Figure @fig-ml-pipeline illustrates this flow.

#figure(
  image("../../../../images/diagrams/11-ml-pipeline.png", width: 100%),
  caption: [ML embedding pipeline: the step-by-step flow from image bytes received by the FastAPI interface to a 512-dimensional float vector returned as JSON to the .NET backend.],
) <fig-ml-pipeline>

The pipeline comprises seven sequential steps. The .NET backend initiates the process by sending raw image bytes to the `POST /embeddings` endpoint. The FastAPI interface validates the `X-API-Key` header against the expected key and rejects unauthorised requests with a 401 response. The Model Manager retrieves the currently configured model from its cache, loading it from disk on the first request, and dispatches the image payload to the model's preprocessing pipeline.

The preprocessing stage normalises the input image to the format expected by the selected model. The image is resized to the model's expected input dimensions, 224 by 224 pixels for most architectures, and converted from interleaved colour channels to separated channel tensors. ImageNet-normalisation statistics are applied: each colour channel is centred to the ImageNet mean and scaled by the standard deviation, matching the distribution on which the model was originally trained.

The preprocessed tensor is passed through the model's forward pass. For convolutional models, this involves a cascade of convolution, activation, and pooling operations that extract hierarchical features. For transformer-based models, the image is divided into patches, each patch is projected into a token embedding, and self-attention layers compute contextual relationships between patches across the entire image. The output of the forward pass is pooled, typically via global average pooling, to produce a single fixed-length vector regardless of the original image size.

The resulting vector is a 512-dimensional array of single-precision floating-point numbers (for CLIP-family models; other dimensionalities for other architectures). This vector encodes the visual essence of the image, colour palette, texture patterns, silhouette shape, and semantic category, in a compressed numerical form suitable for similarity computation. The vector is serialised to a JSON array and returned to the .NET backend as the response body, together with metadata fields identifying the model name and generation time in milliseconds.

==== Health Monitoring

The `/health` endpoint provides a comprehensive operational status for the Aspire orchestration layer. It verifies three conditions: that the GPU environment is active and accessible, confirming CUDA or MPS availability; that the configured model is successfully loaded into memory and ready for inference; and that a baseline inference pass completes without errors, validating the entire pipeline from preprocessing to output generation. The health response includes diagnostic data, model load status, last inference time, GPU memory utilisation, enabling the orchestrator to detect degraded states and trigger Docker restart policies automatically without human intervention.
