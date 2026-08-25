=== Python ML Sidecar

The machine learning capability runs as a dedicated Python 3.12 service, isolated from the .NET backend due to incompatible runtime dependencies (PyTorch requires Python, .NET requires the CLR) @paszke2019pytorch.

- *Framework.* FastAPI provides async HTTP endpoints with automatic OpenAPI schema generation. Uvicorn is the ASGI runtime.

- *Model management.* A singleton *ModelManager* lazy-loads models from the HuggingFace hub on first request. Once loaded, models persist in GPU memory (or CPU, if no GPU is available) for the lifetime of the service. The manager supports multiple architectures through a common embedding interface.

- *API surface.* #emph[POST /embeddings] accepts raw image bytes (JPEG, PNG, WebP) and returns a JSON array of floating-point values. #emph[GET /health] reports the currently loaded model, its embedding dimension, and the most recent inference latency.

- *Security.* Requests require an #emph[X-API-Key] header validated at the middleware layer. The sidecar listens only on the internal Docker network, inaccessible from the public internet.
