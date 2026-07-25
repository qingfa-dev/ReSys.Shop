== System Architecture & Design

This chapter presents the architectural design of ReSys.Shop, progressing from a high-level system overview through domain modelling, to the structural, data, API, and security layers. The design follows a service-oriented approach with three independently deployable services, a Vue 3 frontend, a .NET 10 modular monolith backend, and a Python machine learning sidecar, each responsible for a distinct technological concern. The chapter is organised into six sections, each accompanied by architectural diagrams that provide visual representations of the system's structure, behaviour, and deployment topology.

#include "01-system-overview.typ"
#include "02-domain-driven-design.typ"
#include "03-c4-architecture.typ"
#include "04-database-design.typ"
#include "05-api-design.typ"
#include "06-security-design.typ"
