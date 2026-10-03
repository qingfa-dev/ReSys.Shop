==== Architecture

The service operates as a stateless HTTP API. It consumes image URLs and returns high-dimensional vector embeddings.

- *Framework:* FastAPI (Asynchronous, High-Performance).
- *Runtime:* PyTorch with CUDA support (for NVIDIA GPUs) or MPS (for Apple Silicon).
- *Orchestration:* Managed via .NET Aspire service discovery (`http://ml-service`).
- *Security:* Protected by `X-API-Key` authentication.
- *Containerization:* Deployed as a Docker container with pre-installed CUDA drivers to ensure consistent runtime environments across development and production.
