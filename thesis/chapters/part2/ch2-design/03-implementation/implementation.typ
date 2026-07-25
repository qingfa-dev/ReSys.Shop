== Implementation

This section describes the concrete realisation of the ReSys.Shop system architecture presented in Section 2.2. It is organised into five sub-sections, progressing from the architectural pattern that structures the codebase, through the machine learning pipeline that constitutes the core research contribution, to the end-to-end visual search flow and its configuration mechanism, and finally a concise survey of the supporting e-commerce modules. The implementation follows the service-oriented design established in Section 2.2: a Vue 3 frontend, a .NET 10 modular monolith backend, and a Python machine learning sidecar, each implemented in the technology most appropriate for its domain.

#include "01-vertical-slice.typ"
#include "02-ml-pipeline.typ"
#include "03-cbir-search.typ"
#include "04-model-config.typ"
#include "05-ecommerce-core.typ"
