== Context and Problem Statement

This thesis is submitted in partial fulfillment of the requirements for the degree of [*Bachelor of Engineering in Information Technology*]. It presents the complete analysis, design, implementation, and evaluation of ReSys.Shop — a fashion e-commerce platform with Content-Based Image Retrieval (CBIR) capabilities, featuring a comparative evaluation of multiple pretrained visual feature extraction models.

Fashion e-commerce represents one of the most competitive and technically demanding domains in online retail. Consumers expect rich visual experiences, personalized recommendations, and seamless checkout flows across multiple devices. Traditional text-based search often fails in fashion because shoppers struggle to articulate visual preferences (e.g., "a dress like this but in blue").

ReSys.Shop addresses three distinct problems simultaneously:

#enum(numbering: "1.")[
  [*The user-facing problem*]: How can a fashion e-commerce platform provide intuitive visual search using modern machine learning techniques?
][
  [*The engineering problem*]: How can a complex e-commerce system be architected to maintain modularity, testability, and operational clarity as it scales across 8 business domains?
][
  [*The ML evaluation problem*]: Which pretrained visual feature extraction model (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) offers the optimal balance of retrieval effectiveness and operational performance for fashion CBIR?
]

== Problem Statement

Existing fashion e-commerce platforms typically fall into one of two categories:

#enum(numbering: "1.")[
  [*Monolithic platforms*] (e.g., early Shopify, Magento) that become unmaintainable as business logic interleaves across features
][
  [*Microservice platforms*] that introduce excessive operational overhead for small-to-medium teams, with distributed-transaction complexity for e-commerce workflows
]

Neither approach optimally serves a research context where rapid iteration on ML-powered features must coexist with stable transactional domains, the system must be demonstrable as a single deployable unit, and code quality must be examinable and justifiable.

#figure(
  caption: [Specific technical gaps identified],
  table(
    columns: 3,
    align: (left, left, left),
    table.header(
      [*Gap*], [*Evidence from prior art*], [*Consequence*],
    ),
    [Exception-driven error handling], [Typical ASP.NET controllers throw exceptions for validation failures], [Unpredictable control flow],
    [Anemic domain models], [EF entities are data bags with no behavior], [Business rules scattered across services],
    [Horizontal layering], [Controllers → Services → Repositories → Entities], [Changes touch 4+ files],
    [Tight module coupling], [Services directly reference other modules' repositories], [Cannot test modules in isolation],
    [Missing vector search], [Standard SQL databases cannot perform similarity search], [Fashion image search requires separate infrastructure],
    [No model comparison for CBIR], [Prior art selects embedding models arbitrarily], [Suboptimal model may be deployed],
  )
) <tab:gaps>
