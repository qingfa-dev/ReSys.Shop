=== Machine Learning Service



The *Machine Learning (ML) Service* is a specialized AI microservice responsible for visual intelligence tasks. It handles the computationally intensive tasks of image embedding generation and recommendation logic, exposing endpoints consumed by the .NET Backend via HTTP.

Before diving into the implementation details, it is essential to understand the data flow within the service. The following diagram illustrates the complete *Inference Pipeline*, tracking a request from the HTTP interface through the validation layer, into the lazy-loading manager, and finally to the GPU for execution.

#figure(
  placement: none,
  image("/images/diagrams/01-ml-models/ml-05-inference-pipeline.png", width: 45%),
  caption: [ML Inference Implementation: End-to-end data flow tracking the request through internal service components.],
)

- *Runtime:* *Python 3.12*.
- *Framework:* *FastAPI* (Async web framework) run with *Uvicorn*.
- *Key Models:*
  - *CLIP (ViT-B/32):* For generating shared latent space embeddings for images and text, powering the visual search.
  - *EfficientNet-B0:* Utilized for feature extraction benchmarks.
- *Libraries:*
  - *PyTorch (Torch):* Core deep learning framework.
  - *TIMM (PyTorch Image Models):* Access to state-of-the-art vision backbones.
  - *Pillow:* Image data preprocessing.
- *Integration:* Deployed as a Docker container orchestrator by *.NET Aspire*.

Decoupling this service from the primary .NET backend allows for independent scaling of GPU-intensive workloads and simplifies dependency management for Python-native AI libraries.

#figure(
  placement: none,
  align(center)[
    #box(stroke: 1pt + gray, inset: 15pt, radius: 5pt)[
      #stack(
        dir: ttb,
        spacing: 15pt,
        // API Layer
        box(width: 200pt, height: 40pt, stroke: 1pt, radius: 5pt, fill: rgb("#e6f7ff"))[
          #align(center + horizon)[*FastAPI Interface*\ (Routes: /embed, /health)]
        ],
        // Arrow
        align(center)[$arrow.b$ Call Manager],
        // Logic Layer
        box(width: 200pt, height: 40pt, stroke: 1pt, radius: 5pt, fill: rgb("#fff7e6"))[
          #align(center + horizon)[*ModelManager (Singleton)*\ (Cache & Lazy Loading)]
        ],
        // Arrow
        align(center)[$arrow.b$ Dispatch to Device],
        // Hardware Layer
        box(width: 200pt, height: 60pt, stroke: 1pt, radius: 5pt, fill: rgb("#f6ffed"))[
          #align(center + horizon)[
            *PyTorch Runtime* \
            (CUDA / CPU)
          ]
        ],
      )
    ]
  ],
  caption: [ML Service Internal Architecture: Layered design separating API handling from Model Lifecycle management.],
)

#include "01-ml-service/01-architecture.typ"
#include "01-ml-service/02-lazy-loading.typ"
#include "01-ml-service/03-functional-interface.typ"
#include "01-ml-service/04-health-monitoring.typ"
#include "01-ml-service/05-model-zoo.typ"
