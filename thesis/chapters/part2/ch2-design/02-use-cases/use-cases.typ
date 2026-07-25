== Functional Decomposition and Use Cases

This section presents three representative use cases that illustrate how the system's functional requirements translate into concrete interaction flows. The use cases span the core research capability (visual search), the primary e-commerce workflow (checkout), and the evaluation infrastructure (model benchmarking).

#include "01-visual-search.typ"
#include "02-checkout.typ"
#include "03-benchmark.typ"

Figure @fig-use-case-diagram positions these three use cases alongside the broader system functionality within a single visual summary.

#figure(
  image("../../../../images/diagrams/02-use-case.png", width: 85%),
  caption: [
    System use case diagram showing the three actors, Customer, Administrator, and System background services, and their primary interactions with the ReSys.Shop platform.
  ],
) <fig-use-case-diagram>

The three use cases serve distinct purposes within the thesis. The visual search use case defines the functional behaviour of the system's primary research capability; the checkout use case establishes the realistic e-commerce context in which search success can be measured through downstream conversion events; and the benchmark use case defines the systematic methodology used in Chapter 3 to evaluate and compare embedding models. The breadth of the system, nine background actors and use cases in the diagram, encompassing catalog browsing, account management, product administration, and order processing, reflects the full operational scope of the platform, while the three detailed use cases focus on the scenarios most relevant to the research questions.
