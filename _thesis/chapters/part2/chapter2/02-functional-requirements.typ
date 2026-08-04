== FUNCTIONAL REQUIREMENTS

This section specifies the functional requirements for ReSys.Shop, detailing the capabilities implemented to support the research objectives. The system is designed to provide a realistic e-commerce environment for evaluating AI-driven visual search and recommendation features.

=== Role-Based Functional Requirements

The system supports three distinct actors, each with specific capabilities designed to validate the research hypotheses.

==== Customer (Storefront)
The primary actor for evaluating the visual search experience.

- *Visual Search:* Upload reference images (JPEG/PNG/WebP, max 10MB) to find visually similar products using DINOv2 and Fashion-CLIP models.
- *Style Recommendations:* Receive "You May Also Like" suggestions based on visual embedding similarity (cosine distance).
- *Catalog Discovery:* Browse hierarchical category trees (Taxonomy) and filter products by attributes (Price, Size, Color).
- *Shopping Workflow:*  Full transaction lifecycle including Cart management, Address selection, and secure Checkout.
- *Account Management:*  manage profile information, view order history, and track status.

==== Administrator (Back Office)
Responsible for data management and system monitoring.

- *Product Management:* CRUD operations for products, variants, and dynamic properties.
- *Image Management:* Upload product images with automatic preprocessing (resizing, thumbnail generation, vectorization trigger).
- *Inventory Logistics:* Monitor real-time physical stock levels (`OnHand`) and logical reservations (`Reserved`).
- *Order Fulfillment:* Review transactions, capture payments, and manage shipment tracking.
- *System Monitoring:* View background job status, including embedding generation success rates.

==== System (Background Services)
Automated processes ensuring data consistency and performance.

- *Vector Generation:* Asynchronous job that computes AI embeddings for new images using the Python ML service.
- *Index Maintenance:* Automated updates to the HNSW (Hierarchical Navigable Small World) index to ensure \<50ms search latency.
- *Stock Reservation:* Management of temporary inventory holds during checkout to prevent overselling.

=== Research Contribution and Feature Scope

To clarify the scope of the thesis, features are classified into *Core Research* (novel contributions) and *Supporting* (necessary infrastructure).

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, center, left),
    [*Feature Area*], [*Classification*], [*Rationale*],
    [Visual Search],
    [Core Research],
    [Primary contribution: Demonstrates hybrid AI model integration (Fashion-CLIP/DINOv2) for accurate retrieval.],

    [Style Recommendations],
    [Core Research],
    [Secondary contribution: Validates embedding utility for passive discovery.],

    [Vector Pipeline],
    [Core Research],
    [Critical infrastructure: Automated ingestion, normalization, and indexing of visual data.],

    [Product Catalog], [Supporting], [Required context: Provides the dataset for search and recommendation algorithms.],
    [Order System],
    [Supporting],
    [Metric validation: Provides "Add to Cart" and "Purchase" events to measure search success.],

    [Inventory], [Supporting], [Realism constraint: Ensures search results reflect actual product availability.],
  ),
  caption: [Feature Classification and Research Relevance],
)


=== Data Integrity & Validation Constraints

Input validation is enforced at the Application layer (FluentValidation) to ensure data integrity before processing.

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    [*Entity / Action*], [*Validation Rules*],
    [Image Upload], [- Max size: 10 MB \ - Formats: JPEG, PNG, WebP, GIF \ - Min dimension: 100px],
    [Visual Search], [- TopK: 1-100 results \ - MinSimilarity: 0.0-1.0 (default 0.7)],
    [Product], [- Name: Max 100 chars, unique per slug \ - Price: Must be non-negative \ - SKU: Unique system-wide],
    [Order], [- Cart: Must contain >= 1 item \ - Shipping: Valid address required \ - Payment: Amount must match total],
  ),
  caption: [Key data validation rules enforced by the system],
)

