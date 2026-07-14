# inference

A high-performance Python microservice dedicated to **Image Embedding Inference**, fully integrated with the .NET Aspire ecosystem.

---

## 🔐 API Key & Authentication Architecture

### Overview

The inference service uses a **sidecar pattern** with centralized API key management:

```
┌─────────────────────────────────────────────────────────────────┐
│                         EXTERNAL WORLD                          │
└─────────────────────────────────────────────────────────────────┘
                                │
           ┌────────────────────┼────────────────────┐
           │                    │                    │
           ▼                    ▼                    ▼
    ┌────────────┐       ┌────────────┐       ┌────────────┐
    │   Mobile   │       │    Web     │       │   3rd Party│
    │    App     │       │  Frontend  │       │   Service  │
    └─────┬──────┘       └─────┬──────┘       └─────┬──────┘
          │  🔑 X-API-Key      │  🔑 X-API-Key       │  🔑 X-API-Key
          └────────────────────┼────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      INTERNAL NETWORK                           │
│                     (Aspire Service Mesh)                       │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │  Inference Sidecar    │
                    │  🔑 Validates Key     │
                    │  (localhost:8000)     │
                    └───────────┬──────────┘
                                │ Internal (no key)
                                ▼
                    ┌──────────────────────┐
                    │      API Service     │
                    │  🔑 Has Key (future)  │
                    │  For embedding gen   │
                    └──────────────────────┘
```

### API Key Distribution

| Service | Receives Key | Purpose |
|---------|-------------|---------|
| **Inference Sidecar** | ✅ `API_KEY` | Validates external client requests |
| **API Service** | ✅ `INFERENCE_API_KEY` | Future use for calling inference (pgvector embeddings) |

### Why Both Services?

1. **Inference Sidecar**: 
   - Handles direct external API calls
   - Validates `X-API-Key` header from clients

2. **API Service (Future)**:
   - Will call inference to generate embeddings
   - Embeddings stored in pgvector for semantic search
   - Provides flexibility for external inference deployment

### Technical Notes

- **OWASP Compliant**: Key only shared with services that need it
- **Zero Trust**: External = verify; Internal = trust local network
- **Defense in Depth**: Key available in both services for architectural flexibility

### Configuration

The API key is managed centrally in Aspire:

```csharp
// platform/aspire/src/ReSys.AppHost/AppHost.cs
var inferenceApiKey = builder.AddParameter("inference-api-key", secret: true);

// Passed to Inference Sidecar
inference.WithEnvironment("API_KEY", inferenceApiKey);

// Passed to API Service (future use)
api.WithEnvironment("INFERENCE_API_KEY", inferenceApiKey);
```

### Environment Variables

| Variable | Service | Description |
|----------|---------|-------------|
| `API_KEY` | Inference Sidecar | Validates external requests |
| `INFERENCE_API_KEY` | API Service | Future embedding generation |

---

## 🚀 Overview

inference provides a standardized API for generating visual embeddings from images. By transforming images into high-dimensional vectors, it enables semantic search and visual similarity features across the ReSys ecosystem.

### Key Features
- **Stateless & Scalable**: Designed for high-throughput sidecar deployment.
- **Centralized Model Registry**: Models are stored in a versioned, root-level directory for consistency across services.
- **Standardized Result Pattern**: Uniform error handling that mirrors the .NET `Result<T>` implementation.
- **Observability**: Native OpenTelemetry support for the Aspire Dashboard (Traces, Metrics, Logs).
- **Hybrid Engine**: Supports both **PyTorch** (for research/flexibility) and **ONNX** (for production speed).

---

## 🏗️ Architecture & Data Flow

### High-Level Components
```mermaid
graph TD
    Client[.NET API / Gateway] -- "POST /inference/embeddings (JSON)" --> API[FastAPI Layer]
    API -- "Inference Request" --> Engine[Inference Engine Factory]
    Engine -- "Lazy Load" --> Registry[Root Model Registry /models/production]
    Registry -- "PyTorch / ONNX" --> Processor[Image Preprocessor]
    Processor -- "Tensor" --> Runner[Inference Runner]
    Runner -- "Raw Output" --> Norm[L2 Normalizer]
    Norm -- "ValueResult[Vector]" --> API
    API -- "ValueResult[JSON]" --> Client
    API -- "ValueResult[JSON]" --> Client
    ```

    ### File Structure (Source)
    The service follows a flat, modern Python structure:
    - `src/api/`: FastAPI route handlers and dependency injection.
    - `src/core/`: Configuration, constants, and telemetry setup.
    - `src/models/`: Domain-organized model "skills" and dynamic registry.
    - `src/schemas/`: Consolidated Pydantic models for requests, responses, and result envelopes.
    - `src/services/`: Core business logic and the Inference Engine singleton.

---

## 🧠 Supported Models & References

The service supports several state-of-the-art visual embedding models. Note that ONNX versions are preferred for production.

| Model ID | Architecture | Dimensions | Best For | Reference |
| :--- | :--- | :--- | :--- | :--- |
| **efficientnet_b0** | EfficientNet-B0 | 1280 | General visual features | [Paper](https://arxiv.org/abs/1905.11946) |
| **clip_vit_b16** | CLIP ViT-B/16 | 512 | Semantic search | [Site](https://openai.com/research/clip) |
| **fashion_clip** | Fashion-CLIP | 512 | Fashion visual search | [Repo](https://github.com/vub-soft-be/fashion-clip) |
| **dinov2_vits14** | DINOv2 ViT-S/14 | 384 | Structural similarity | [Site](https://dinov2.metademolab.com/) |
| **onnx/clip_vit_b16** | CLIP ViT-B/16 (ONNX) | 768 | High-speed semantic search | [Registry](../../models/production) |
| **onnx/fashion_clip** | Fashion-CLIP (ONNX) | 768 | High-speed fashion search | [Registry](../../models/production) |

---

## 🛠️ Tech Stack

- **Framework**: FastAPI (Asynchronous, Type-safe)
- **Runtime**: Python 3.11+ / uv
- **AI Libraries**: PyTorch, Torchvision, HuggingFace Transformers, ONNX Runtime
- **Networking**: HTTPX (Asynchronous client)
- **Observability**: OpenTelemetry SDK
- **Security**: SlowAPI (Rate Limiting), API Key (Sidecar Auth)

---

## 🚦 Getting Started

### Prerequisites
- [uv](https://github.com/astral-sh/uv) installed.
- Microsoft Visual C++ Redistributable (for Windows).

### Quick Start (One-Command Setup)
Run the automated setup script to configure environment files, sync dependencies, and optionally export models to ONNX for production speed:

```bash
python scripts/setup.py
```

### Running the Service
To start the FastAPI server in development mode (with hot-reload):
```bash
uv run python run.py
```

### Testing Inference
Once the server is running, you can verify it using the provided test tool:
```bash
# Test with default image and model
uv run python scripts/test_inference.py

# Test a specific model with verbose output
uv run python scripts/test_inference.py --model fashion_clip --verbose
```

### Manual Setup
1. Copy `.env.template` to `.env` and `.env.dev`.
2. Configure your `API_KEY` and `HUGGING_FACE_TOKEN`.
3. Run `uv sync` to install dependencies.
4. (Optional) Run `uv run python scripts/export_onnx.py` to populate the root model registry.

---

## 🧪 Testing

The service implements a comprehensive test suite with 85+ tests:

| Test Type | Target | Command |
| :--- | :--- | :--- |
| **Unit** | Schemas & Core Logic | `uv run pytest tests/unit` |
| **Integration** | API & Model Weights | `uv run pytest tests/integration` |
| **All** | Full Stack | `uv run pytest` |

---

## 📡 API Reference

### Generate Embedding
`POST /inference/embeddings`

**Headers:**
- `X-API-Key`: `inference-sidecar-key` (default)

**Body:**
```json
{
  "image_url": "https://example.com/image.jpg",
  "model": "efficientnet_b0"
}
```

**Supported Models:**
- `efficientnet_b0`: Fast, 1280 dimensions.
- `fashion_clip`: Fashion-specific, 512 dimensions.
- `clip_vit_b16`: Semantic, 512 dimensions.
- `dinov2_vits14`: Structural, 384 dimensions.
- `onnx/<name>`: Optimized versions from the registry (e.g., `onnx/fashion_clip`).
