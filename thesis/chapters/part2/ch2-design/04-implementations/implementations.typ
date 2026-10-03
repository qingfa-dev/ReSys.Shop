== Implementation

This section shows how the design from Sections 2.2 and 2.3 was built into working software. It follows the actual structure of the system: the technology stack used for development, the vertical slice pattern that organizes the .NET codebase, the layer that stores relational and vector data in a single database, the machine learning sidecar (the main research contribution of this project), and the frontend applications that users interact with.

- *Technology Stack.* Framework versions and how the system is containerized.
- *Vertical Slice Core.* Feature co-location, the Carter--MediatR request pipeline, and the functional Result pattern.
- *Data Persistence.* Multi-schema EF Core, pgvector integration with HNSW indexing, and concurrency control.
- *ML Sidecar.* Model management, the embedding generation pipeline, and the full CBIR search flow.
- *Frontend Applications.* Dual-SPA architecture, the visual search interface, and key administration workflows.

#include "01-technology-stack/technology-stack.typ"
#include "02-vsa-core/vsa-core.typ"
#include "03-data-persistence/data-persistence.typ"
#include "04-ml-sidecar/ml-sidecar.typ"
#include "05-frontend-ux/frontend-ux.typ"
