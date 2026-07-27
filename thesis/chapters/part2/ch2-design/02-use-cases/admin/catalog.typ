==== Product Management

// Diagram placeholder: Product Management

==== UC-ADM-PROD — Manage Products

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PROD],
    [*Use Case Name*], [Manage Products],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, and archive products in the catalog.],
    [*Trigger*], [Administrator accesses the product management section of the administration interface.],
    [*Preconditions*], [
      - Authenticated with catalog management permissions.
    ],
    [*Postconditions*], [
      - Product catalog reflects the performed operation.
      - Status transitions logged for audit.
    ],
    [*Main Success Scenario*], [
      *Create Product*
      1. Selects the create product option.
      2. System presents the creation form.
      3. Enters product name, description, slug, and fashion metadata (style code, season, material, department, gender target).
      4. Defines at least one variant with SKU and price as the master variant.
      5. Assigns the product to relevant taxons.
      6. Submits. System validates slug uniqueness and persists. Confirms creation with Draft status.
      ,
      *Update Product*
      1. Selects a product from the catalog listing.
      2. System displays the edit form with current values.
      3. Modifies fields: name, description, slug, fashion metadata, SEO attributes, or status.
      4. Submits. System validates, persists, and confirms the update.
      ,
      *Archive Product*
      1. Selects a product and chooses the archive option.
      2. System requests confirmation.
      3. Confirms. Product status changes to Archived and is hidden from the storefront.
    ],
    [*Alternative Flows*], [
      A1. Slug not unique (Create/Update): system rejects and prompts for a different slug.
      A2. No master variant (Create): system rejects and instructs to designate one.
      A3. Product has active orders (Archive): system warns and requests explicit confirmation.
    ],
    [*Exception Flows*], [
      E1. System fails to persist: reports failure and retains form data for retry.
    ],
    [*Related Requirements*], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-11, CAT-FR-12, CAT-FR-13],
  ),
    kind: table,
  caption: [Manage Products.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-product-management.png",
    width: 100%
  ),
  caption: [Use case diagram for Product Management (UC-ADM-PROD).],
) <fig-uc-adm-prod-d>

==== Variant and Pricing

// Diagram placeholder: Variant and Pricing use case diagram

==== UC-ADM-VAR — Manage Variants

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-VAR],
    [*Use Case Name*], [Manage Variants],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Add and manage product variants including option-value configuration and pricing.],
    [*Trigger*], [Administrator opens the variant management section from the product detail page.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Parent product exists.
    ],
    [*Postconditions*], [
      - Variant created or updated with option assignments and pricing.
    ],
    [*Main Success Scenario*], [
      *Add Variant*
      1. Navigates to product detail and selects add variant.
      2. System presents the variant creation form.
      3. Enters variant details: SKU, barcode, dimensions, weight, and position.
      4. Assigns option values (e.g. Size M, Colour Red) from available option types.
      5. Submits. System validates SKU uniqueness and option combination. Creates the variant and confirms.
      ,
      *Configure Options*
      1. Selects a variant from the product detail page.
      2. System displays variant detail with current option assignments.
      3. Selects an option type and chooses a value; repeats for additional types.
      4. Saves. System validates the combination and persists. Confirms the change.
      ,
      *Configure Pricing*
      1. Navigates to pricing management for a product.
      2. System displays current prices for all variants grouped by currency.
      3. Selects variants and specifies new price with currency and optional validity dates.
      4. Submits. System validates non-negative prices and valid currency. Persists and confirms.
    ],
    [*Alternative Flows*], [
      A1. SKU not unique: system rejects and prompts for a different SKU.
      A2. Duplicate option combination: system rejects and highlights the conflict.
      A3. Zero price: system accepts but warns variant appears as free.
      A4. Overlapping date ranges for same variant and currency: system rejects.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports and retains form data for retry.
    ],
    [*Related Requirements*], [CAT-FR-03, CAT-FR-10, CAT-FR-21, CAT-FR-22],
  ),
    kind: table,
  caption: [Manage Variants.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-variant-pricing.png",
    width: 100%
  ),
  caption: [Use case diagram for Variant and Pricing (UC-ADM-VAR).],
) <fig-uc-adm-var-d>

==== Image and Embedding Management

// Diagram placeholder: Image and Embedding Management use case diagram

==== UC-ADM-IMG — Manage Images and Embeddings

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-IMG],
    [*Use Case Name*], [Manage Images and Embeddings],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Upload variant images and manage embedding generation.],
    [*Trigger*], [Administrator navigates to image management for a product variant.],
    [*Preconditions*], [
      - Authenticated with image management permissions.
      - Variant exists.
    ],
    [*Postconditions*], [
      - Image stored and associated with variant. Embeddings available for visual search.
    ],
    [*Main Success Scenario*], [
      *Upload Images*
      1. Selects a product variant and initiates image upload.
      2. Selects an image file from the local file system.
      3. System validates format (JPEG, PNG, WebP) and size (max 10 MB).
      4. Provides alt text and display order position.
      5. Confirms and submits. System stores the image, generates thumbnails, and schedules embedding generation. Confirms upload.
      ,
      *Regenerate Embeddings*
      1. Navigates to image management and selects images.
      2. Initiates the regenerate embeddings action.
      3. System displays confirmation with affected image count.
      4. Confirms. System sends each image to the ML service for embedding generation. Stores embeddings with model metadata. Reports completion with success count.
    ],
    [*Alternative Flows*], [
      A1. Unsupported format: system rejects and lists accepted formats.
      A2. Exceeds max size: system rejects and displays size constraint.
      A3. No images selected for regeneration: system disables the action and prompts to select.
    ],
    [*Exception Flows*], [
      E1. ML service unavailable: system reports failure and suggests retry when operational.
      E2. Processing cannot be scheduled: system stores image and notifies search will exclude it until processing succeeds.
    ],
    [*Related Requirements*], [CAT-FR-04, CAT-FR-05, CAT-FR-14, CAT-FR-15],
  ),
    kind: table,
  caption: [Manage Images and Embeddings.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-image-embedding.png",
    width: 100%
  ),
  caption: [Use case diagram for Image and Embedding Management (UC-ADM-IMG).],
) <fig-uc-adm-img-d>

==== Taxonomy and Classification

// Diagram placeholder: Taxonomy and Classification use case diagram

==== UC-ADM-TAX — Manage Taxonomies and Classification

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-TAX],
    [*Use Case Name*], [Manage Taxonomies and Classification],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create and modify taxonomy structures and assign product classifications.],
    [*Trigger*], [Administrator navigates to taxonomy management or product classification panel.],
    [*Preconditions*], [
      - Authenticated with taxonomy management permissions.
    ],
    [*Postconditions*], [
      - Taxonomy structure and product classifications updated.
    ],
    [*Main Success Scenario*], [
      *Manage Taxonomies*
      1. Navigates to taxonomy management.
      2. System displays the taxonomy tree with existing taxons.
      3. Creates a new taxonomy root or selects an existing taxonomy.
      4. Adds, edits, reorders, or removes taxon nodes; optionally defines business rules.
      5. Saves. System persists the updated taxonomy and confirms.
      ,
      *Classify Products*
      1. Selects products from the catalog listing.
      2. Opens the classification panel.
      3. System displays taxonomy tree with current classifications.
      4. Selects taxons to assign or deselects to remove.
      5. Saves. System validates each taxon path, persists associations, and confirms.
    ],
    [*Alternative Flows*], [
      A1. Delete taxon with children: system prompts to cascade-delete or reassign.
      A2. Delete taxon with products: system warns products lose classification.
      A3. All taxons removed from product: system warns product will not appear in category browsing.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes data and asks to retry.
    ],
    [*Related Requirements*], [CAT-FR-09, CAT-FR-18, CAT-FR-19],
  ),
    kind: table,
  caption: [Manage Taxonomies and Classification.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-taxonomy-classification.png",
    width: 100%
  ),
  caption: [Use case diagram for Taxonomy and Classification (UC-ADM-TAX).],
) <fig-uc-adm-tax-d>

==== Option Type Configuration

// Diagram placeholder: Option Type Configuration use case diagram

==== UC-ADM-OPT — Manage Option Types

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-OPT],
    [*Use Case Name*], [Manage Option Types],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, modify, and remove option types and their associated values.],
    [*Trigger*], [Administrator navigates to option type management.],
    [*Preconditions*], [
      - Authenticated with option type management permissions.
    ],
    [*Postconditions*], [
      - Option types updated and available for product configuration.
    ],
    [*Main Success Scenario*], [
      1. Navigates to option type management.
      2. System displays all option types with their values.
      3. Creates a new option type with name and presentation style.
      4. Adds ordered option values (e.g. S, M, L, XL for Size).
      5. Optionally edits, reorders, or removes existing types and values.
      6. Saves. System validates name uniqueness and non-empty values. Persists and confirms.
    ],
    [*Alternative Flows*], [
      A1. Delete type in use by products: system warns and asks for confirmation.
      A2. Remove value in use by variants: system warns and asks for confirmation.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes data and asks to retry.
    ],
    [*Related Requirements*], [CAT-FR-10, CAT-FR-20],
  ),
    kind: table,
  caption: [Manage Option Types.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-option-type-config.png",
    width: 100%
  ),
  caption: [Use case diagram for Option Type Configuration (UC-ADM-OPT).],
) <fig-uc-adm-opt-d>
