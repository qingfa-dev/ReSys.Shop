== Implementation

This section describes the concrete realization of the ReSys.Shop system, showing how the architecture and design decisions from Sections 2.2 and 2.3 were translated into working software. The presentation follows the system's actual structure: the technology stack that underpins development, the vertical slice pattern that organizes the .NET codebase, the persistence layer that stores relational and vector data in a single database, the machine learning sidecar that constitutes the core research contribution, and the frontend applications that deliver the user-facing experience.

- *Technology Stack.* Framework versions and containerization strategy.
- *Vertical Slice Core.* Feature co-location, the Carter--MediatR request pipeline, and the functional Result pattern.
- *Data Persistence.* Multi-schema EF Core, pgvector integration with HNSW indexing, and concurrency control.
- *ML Sidecar.* Model management, the embedding generation pipeline, and the end-to-end CBIR search flow.
- *Frontend Applications.* Dual-SPA architecture, visual search interface, and key administration workflows.

#include "01-technology-stack/technology-stack.typ"
#include "02-vsa-core/vsa-core.typ"
#include "03-data-persistence/data-persistence.typ"
#include "04-ml-sidecar/ml-sidecar.typ"
#include "05-frontend-ux/frontend-ux.typ"
