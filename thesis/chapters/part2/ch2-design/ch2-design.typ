= Design and Implementation

This chapter translates the problem statement and research questions into a concrete system design. It defines functional and non-functional requirements, presents use cases spanning the core research capability and primary e-commerce workflows, details the system architecture and database schema, and describes the implementation of the three principal components.

The chapter is organized into the following key sections:

- *Requirements Analysis.* Defines system actors, 88 functional requirements across nine business modules, non-functional requirements, and feature classification.
- *Functional Decomposition and Use Cases.* Specifies 26 capability-grouped use cases covering administration, storefront operations, and system-level services.
- *System Architecture and Design.* Presents the system overview, domain-driven design, C4 architecture diagrams, database schema, API design, and security model.
- *Implementation.* Describes the vertical slice organisation, machine learning pipeline, CBIR search flow, model configuration, and core e-commerce features across the .NET backend, Python sidecar, and Vue.js storefront.

#include "01-requirements/requirements.typ"
#include "02-use-cases/use-cases.typ"
#include "03-architecture/architecture.typ"
#include "04-implementation/implementation.typ"
