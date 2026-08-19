==== Product Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-product-management.png",
    width: 50%
  ),
  caption: [Use case diagram for Product Management (UC-ADM-PROD).],
) <fig-uc-adm-prod-d>

==== UC-ADM-PROD: Manage Products

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-PROD: Manage Products],
    [*Actor*], [Administrator],
    [*Goal*], [Create, update, and archive products in the catalog.],
    [*Pre/Post*], [
      Pre: authenticated with catalog management permissions.
      Post: product catalog reflects the performed operation; status transitions logged for audit.
    ],
    [*Scenario*], [
      *Create Product*
      + Selects create option.
      + System presents form.
      + Enters product name, description, slug, and fashion metadata (style code, season, material, department, gender target).
      + Defines at least one variant with SKU and price as master variant.
      + Assigns product to relevant taxons.
      + Submits; system validates slug uniqueness, persists, confirms creation with Draft status.
      ,
      *Update Product*
      + Selects product from catalog listing.
      + System displays edit form with current values.
      + Modifies fields (name, description, slug, fashion metadata, SEO attributes, or status).
      + Submits; system validates, persists, confirms update.
      ,
      *Archive Product*
      + Selects product, chooses archive option.
      + System requests confirmation.
      + Confirms; product status changes to Archived, hidden from storefront.
      ,
    ],
    [*Alternatives*], [
      + A1. Slug not unique (Create/Update) → system rejects, prompts for different slug.
      + A2. No master variant (Create) → system rejects, instructs to designate one.
      + A3. Product has active orders (Archive) → system warns, requests explicit confirmation.
    ],
    [*Exceptions*], [
      + E1. System fails to persist → reports failure, retains form data for retry.
    ],
    [*Requirements*], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-11, CAT-FR-12, CAT-FR-13],
  ),
    kind: table,
  caption: [Manage Products.],
)

==== Variant and Pricing

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-variant-pricing.png",
    width: 70%
  ),
  caption: [Use case diagram for Variant and Pricing (UC-ADM-VAR).],
) <fig-uc-adm-var-d>

==== UC-ADM-VAR: Manage Variants

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-VAR: Manage Variants],
    [*Actor*], [Administrator],
    [*Goal*], [Add and manage product variants including option-value configuration and pricing.],
    [*Pre/Post*], [
      Pre: authenticated with catalog permissions; parent product exists.
      Post: variant created or updated with option assignments and pricing.
    ],
    [*Scenario*], [
      *Add Variant*
      + Navigates to product detail, selects add variant.
      + System presents variant creation form.
      + Enters variant details (SKU, barcode, dimensions, weight, position).
      + Assigns option values (e.g. Size M, Colour Red) from available option types.
      + Submits; system validates SKU uniqueness and option combination, creates variant, confirms.
      ,
      *Configure Options*
      + Selects variant from product detail page.
      + System displays variant detail with current option assignments.
      + Selects option type and chooses value, repeats for additional types.
      + Saves; system validates combination, persists, confirms change.
      ,
      *Configure Pricing*
      + Navigates to pricing management for product.
      + System displays current prices for all variants grouped by currency.
      + Selects variants and specifies new price with currency and optional validity dates.
      + Submits; system validates non-negative prices and valid currency, persists, confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. SKU not unique → system rejects, prompts for different SKU.
      + A2. Duplicate option combination → system rejects, highlights conflict.
      + A3. Zero price → system accepts but warns variant appears as free.
      + A4. Overlapping date ranges for same variant and currency → system rejects.
    ],
    [*Exceptions*], [
      + E1. Persistence failure → system reports, retains form data for retry.
    ],
    [*Requirements*], [CAT-FR-03, CAT-FR-10, CAT-FR-21, CAT-FR-22],
  ),
    kind: table,
  caption: [Manage Variants.],
)

==== Image and Embedding Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-image-embedding.png",
    width: 70%
  ),
  caption: [Use case diagram for Image and Embedding Management (UC-ADM-IMG).],
) <fig-uc-adm-img-d>

==== UC-ADM-IMG: Manage Images and Embeddings

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-IMG: Manage Images and Embeddings],
    [*Actor*], [Administrator],
    [*Support*], [ML Service],
    [*Goal*], [Upload variant images and manage embedding generation.],
    [*Pre/Post*], [
      Pre: authenticated with image management permissions; variant exists.
      Post: image stored and associated with variant; embeddings available for visual search.
    ],
    [*Scenario*], [
      *Upload Images*
      + Selects product variant, initiates image upload.
      + Selects image file from local file system.
      + System validates format (JPEG, PNG, WebP) and size (max 10 MB).
      + Provides alt text and display order position.
      + Confirms, submits; system stores image, generates thumbnails, schedules embedding generation, confirms upload.
      ,
      *Regenerate Embeddings*
      + Navigates to image management, selects images.
      + Initiates regenerate embeddings action.
      + System displays confirmation with affected image count.
      + Confirms; system sends each image to ML service for embedding generation, stores embeddings with model metadata, reports completion with success count.
      ,
    ],
    [*Alternatives*], [
      + A1. Unsupported format → system rejects, lists accepted formats.
      + A2. Exceeds max size → system rejects, displays size constraint.
      + A3. No images selected for regeneration → system disables action, prompts to select.
    ],
    [*Exceptions*], [
      + E1. ML service unavailable → system reports failure, suggests retry when operational.
      + E2. Processing cannot be scheduled → system stores image, notifies search will exclude it until processing succeeds.
    ],
    [*Requirements*], [CAT-FR-04, CAT-FR-05, CAT-FR-14, CAT-FR-15],
  ),
    kind: table,
  caption: [Manage Images and Embeddings.],
)

==== Taxonomy and Classification

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-taxonomy-classification.png",
    width: 70%
  ),
  caption: [Use case diagram for Taxonomy and Classification (UC-ADM-TAX).],
) <fig-uc-adm-tax-d>

==== UC-ADM-TAX: Manage Taxonomies and Classification

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-TAX: Manage Taxonomies and Classification],
    [*Actor*], [Administrator],
    [*Goal*], [Create and modify taxonomy structures and assign product classifications.],
    [*Pre/Post*], [
      Pre: authenticated with taxonomy management permissions.
      Post: taxonomy structure and product classifications updated.
    ],
    [*Scenario*], [
      *Manage Taxonomies*
      + Navigates to taxonomy management.
      + System displays taxonomy tree with existing taxons.
      + Creates new taxonomy root or selects existing taxonomy.
      + Adds, edits, reorders, or removes taxon nodes, optionally defines business rules.
      + Saves; system persists updated taxonomy, confirms.
      ,
      *Classify Products*
      + Selects products from catalog listing.
      + Opens classification panel.
      + System displays taxonomy tree with current classifications.
      + Selects taxons to assign or deselects to remove.
      + Saves; system validates each taxon path, persists associations, confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. Delete taxon with children → system prompts to cascade-delete or reassign.
      + A2. Delete taxon with products → system warns products lose classification.
      + A3. All taxons removed from product → system warns product will not appear in category browsing.
    ],
    [*Exceptions*], [
      + E1. Concurrent modification → system refreshes data, asks to retry.
    ],
    [*Requirements*], [CAT-FR-09, CAT-FR-18, CAT-FR-19],
  ),
    kind: table,
  caption: [Manage Taxonomies and Classification.],
)

==== Option Type Configuration

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-option-type-config.png",
    width: 60%
  ),
  caption: [Use case diagram for Option Type Configuration (UC-ADM-OPT).],
) <fig-uc-adm-opt-d>

==== UC-ADM-OPT: Manage Option Types

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-OPT: Manage Option Types],
    [*Actor*], [Administrator],
    [*Goal*], [Create, modify, and remove option types and their associated values.],
    [*Pre/Post*], [
      Pre: authenticated with option type management permissions.
      Post: option types updated and available for product configuration.
    ],
    [*Scenario*], [
      + Navigates to option type management.
      + System displays all option types with their values.
      + Creates new option type with name and presentation style.
      + Adds ordered option values (e.g. S, M, L, XL for Size).
      + Optionally edits, reorders, or removes existing types and values.
      + Saves; system validates name uniqueness and non-empty values, persists, confirms.
    ],
    [*Alternatives*], [
      + A1. Delete type in use by products → system warns, asks for confirmation.
      + A2. Remove value in use by variants → system warns, asks for confirmation.
    ],
    [*Exceptions*], [
      + E1. Concurrent modification → system refreshes data, asks to retry.
    ],
    [*Requirements*], [CAT-FR-10, CAT-FR-20],
  ),
    kind: table,
  caption: [Manage Option Types.],
)
