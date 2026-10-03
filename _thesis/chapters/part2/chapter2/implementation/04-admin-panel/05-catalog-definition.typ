===== 3. Catalog Definition (Catalog Context)
The *Product Definition Wizard* manages *Complex Object Construction*. Since Products in the system are complex Aggregates (comprising the Root entity, Variants, Attributes, and Assets), the UI implements the *Builder Pattern* to ensure these graphs are constructed atomically.

- *The Interface:* A multi-step stepper (Info $\to$ Variants $\to$ Media $\to$ SEO) prevents the creation of invalid or incomplete product states.
- *Behind the Scenes:*
  - *Vectorization Hook:* When images are uploaded in the "Media" step, the UI immediately polls the *ML Service*. The final "SAVE" button remains disabled until the ML Service confirms embedding generation, ensuring no product exists without vector search capabilities.

/*
  Product Management Logic (UC-0009)
*/
*Product CRUD Operations:*
- *UI Interaction:* The "Save Product" action acts as a strict *Transactional Boundary*. The UI tracks "Dirty State" on a per-field basis, enabling the Save button only when the entire Aggregate (Root + Variants) satisfies client-side validation rules.
- *Sequence Flow:* As shown in @fig:sq-0009-manage, the `CreateProductCommand` payload is structurally identical to the Aggregate Root. The backend handler ensures atomicity: either the Product and all its Variants are persisted, or the entire transaction rolls back.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0009-manage-products.png", width: 65%),
  caption: [Product Management Sequence: Lifecycle management for creating and updating product entities (UC-0009).],
) <fig:sq-0009-manage>

// #figure(
//   figure-placeholder("UI Screenshot: Image Upload Widget (UC-0009)"),
//   caption: [Asset Manager UI: Drag-and-drop zone for product images with progress tracking.],
// )

/*
  Image Upload Logic (UC-0010)
*/
*Asset Pipeline Integration:*
- *UI Pattern (Parallel Uploads):* The "Media" tab implements a Multi-File Drag-and-Drop zone. Each file triggers an independent `axios` POST request, utilizing the `onUploadProgress` hook to render a real-time progress bar for each asset.
- *Sequence Flow:* Reference @fig:sq-0010-upload. The backend processes these uploads concurrently using `Promise.all` (or `.NET Task.WhenAll`). Crucially, the completion of the upload to Blob Storage emits a `ProductImageCreated` event, which asynchronously wakes up the ML Service for vectorization.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0010-upload-images.png", width: 85%),
  caption: [Product Image Upload Sequence: Parallel upload to blob storage and asynchronous vector generation (UC-0010).],
) <fig:sq-0010-upload>

/*
  Taxonomy Logic (UC-0011)
*/
*Hierarchical Organization:*
- *UI Structure (Tree View):* A *Recursive Tree* component renders the nested category structure. The interface supports Drag-and-Drop reordering, listening for specific `@drop` events to calculate the new parent node and index position.
- *Sequence Flow:* @fig:sq-0011-taxonomy details the Graph Mutation logic. A single drag event translates into a `ReparentCategory` command. The backend manages the complexity of the *Materialized Path* update, ensuring that moving a "Parent" category automatically updates the paths of all its "Children" (Cascade Update).

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0011-taxonomy.png", width: 65%),
  caption: [Taxonomy Management: Manipulating the hierarchical category tree (UC-0011).],
) <fig:sq-0011-taxonomy>

#figure(
  placement: none,
  image("../../../../../images/ui/admin/ui-admin-catalog-taxonomy-dashboard.png", width: 100%),
  caption: [Catalog Operations: Interface for managing hierarchical Taxonomies and Categories (UC-0011).],
)

#figure(
  placement: none,
  grid(
    columns: (1fr, 1fr),
    gutter: 1em,
    image("../../../../../images/ui/admin/ui-admin-catalog-taxonomy-list.png", width: 100%),
    image("../../../../../images/ui/admin/ui-admin-catalog-propertytype-create.png", width: 100%),
  ),
  caption: [Catalog Definition UI: Taxonomy Tree (Left) and Dynamic Property Builder (Right) for extending product schemas.],
)
