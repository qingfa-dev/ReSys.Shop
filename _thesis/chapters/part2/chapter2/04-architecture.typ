== SYSTEM ARCHITECTURE <chap2_arch>

#import "../../../template/ctu-styles.typ": figure-placeholder

This section details the architectural design of the system, employing a hybrid approach that combines *Vertical Slice Architecture (VSA)* for the core backend logic with a *Distributed Service* model for specialized AI integration.

The architecture was chosen to balance *modularity*, *maintainability*, and *scalability*, adhering to modern software engineering principles while avoiding the complexity of a full microservices mesh for the core business domain.

#figure(
  image("../../../images/diagrams/02-system-architecture/sys-01-overview.png", width: 90%),
  caption: [High-Level System Architecture],
)

#include "architecture/01-style-and-rationale.typ"
#include "architecture/02-core-components.typ"
#include "architecture/03-backend-patterns.typ"
#include "architecture/04-vertical-slice.typ"
#include "architecture/05-unified-vertical.typ"
#include "architecture/06-cqrs.typ"
#include "architecture/07-ddd.typ"
#include "architecture/08-cross-context-patterns.typ"
#include "architecture/09-domain-events.typ"
#include "architecture/10-resilience-observability.typ"
