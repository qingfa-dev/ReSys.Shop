==== Product Management

// Diagram placeholder: Product Management use case diagram

==== UC-ADM-PROD-01 — Create Product

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PROD-01],
    [*Use Case Name*], [Create Product],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Add a new product with a master variant and taxon assignments.],
    [*Trigger*], [Administrator initiates product creation.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
    ],
    [*Postconditions*], [
      - Product created in Draft status with a master variant.
    ],
    [*Main Success Scenario*], [
      1. Selects the option to create a new product.
      2. System presents the product creation form.
      3. Enters product name, description, slug, and fashion metadata (style code, season, material, department, gender target).
      4. Defines at least one variant with SKU and price as the master variant.
      5. Assigns the product to relevant taxons.
      6. Submits the product.
      7. System validates all fields and slug uniqueness.
      8. System creates the product record with Draft status and the master variant.
      9. System confirms successful creation.
    ],
    [*Alternative Flows*], [
      A1. Slug not unique: system rejects and prompts for a different slug.
      A2. No master variant: system rejects and instructs to designate one.
      A3. Partial variant info: system saves product but marks variant as incomplete.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports and retains form data for retry.
    ],
    [*Related Requirements*], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-11, CAT-FR-13],
  ),
  caption: [UC-ADM-PROD-01 -- Create Product.],
)

==== UC-ADM-PROD-02 — Update Product

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PROD-02],
    [*Use Case Name*], [Update Product],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Modify an existing product's catalog information and status.],
    [*Trigger*], [Administrator selects a product and opens the edit form.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Product exists.
    ],
    [*Postconditions*], [
      - Product updated. Status transitions logged.
    ],
    [*Main Success Scenario*], [
      1. Selects a product from the catalog listing.
      2. System displays the edit form with current values.
      3. Modifies fields: name, description, slug, fashion metadata, SEO attributes, or status.
      4. Submits the changes.
      5. System validates all fields and slug uniqueness if slug was changed.
      6. System persists the updated product.
      7. System confirms successful update.
    ],
    [*Alternative Flows*], [
      A1. Slug already in use: system rejects and prompts for a unique slug.
      A2. Active to Archived transition: system verifies no active orders reference this product before allowing.
      A3. Required fields cleared: system highlights missing fields and prevents submission.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports and retains form data for retry.
    ],
    [*Related Requirements*], [CAT-FR-01, CAT-FR-02, CAT-FR-11, CAT-FR-12],
  ),
  caption: [UC-ADM-PROD-02 -- Update Product.],
)

==== UC-ADM-PROD-03 — Archive Product

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PROD-03],
    [*Use Case Name*], [Archive Product],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Remove a product from the active catalog, retaining data for history.],
    [*Trigger*], [Administrator selects the archive action on a product.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Product exists.
    ],
    [*Postconditions*], [
      - Product archived and removed from storefront views.
    ],
    [*Main Success Scenario*], [
      1. Selects a product from the catalog listing.
      2. Initiates the archive action.
      3. System displays a confirmation prompt with product details.
      4. Confirms the archive operation.
      5. System transitions the product to Archived.
      6. System removes the product from storefront views and search.
      7. System confirms successful archival.
    ],
    [*Alternative Flows*], [
      A1. Product referenced by active orders: system warns and allows archival or cancellation.
      A2. Cancels confirmation: system aborts and returns to product detail.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system detects change, refreshes data, asks for retry.
    ],
    [*Related Requirements*], [CAT-FR-12],
  ),
  caption: [UC-ADM-PROD-03 -- Archive Product.],
)

==== Variant and Pricing

// Diagram placeholder: Variant and Pricing use case diagram

==== UC-ADM-VAR-01 — Add Product Variant

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-VAR-01],
    [*Use Case Name*], [Add Product Variant],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Add a variant with SKU, attributes, and option values to a product.],
    [*Trigger*], [Administrator opens the variant creation form from the product detail page.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Parent product exists.
    ],
    [*Postconditions*], [
      - Variant created and available for inventory and pricing.
    ],
    [*Main Success Scenario*], [
      1. Navigates to product detail and selects add variant.
      2. System presents the variant creation form.
      3. Enters variant details: SKU, barcode, dimensions, weight, and position.
      4. Assigns option values (e.g. Size M, Colour Red) from available option types.
      5. Submits the variant.
      6. System validates SKU uniqueness.
      7. System validates option combination does not conflict with existing variants.
      8. System creates the variant and associates it with the product.
      9. System confirms successful creation.
    ],
    [*Alternative Flows*], [
      A1. SKU not unique: system rejects and prompts for a different SKU.
      A2. Duplicate option combination: system rejects and highlights the conflict.
      A3. No option values: system accepts but reminds to configure options.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports and retains form data for retry.
    ],
    [*Related Requirements*], [CAT-FR-03, CAT-FR-21],
  ),
  caption: [UC-ADM-VAR-01 -- Add Product Variant.],
)

==== UC-ADM-VAR-02 — Configure Variant Options

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-VAR-02],
    [*Use Case Name*], [Configure Variant Options],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Assign option values such as size and colour to a variant.],
    [*Trigger*], [Administrator opens the option configuration panel for a variant.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Variant exists.
      - Relevant option types and values exist.
    ],
    [*Postconditions*], [
      - Variant options configured for storefront display.
    ],
    [*Main Success Scenario*], [
      1. Selects a variant from the product detail page.
      2. System displays variant detail with current option assignments.
      3. Selects an option type (e.g. Size) and chooses a value from the list.
      4. Repeats for additional option types (e.g. Colour, Material).
      5. Saves the option configuration.
      6. System validates the combination against existing variants.
      7. System persists the updated option assignments.
      8. System confirms the change.
    ],
    [*Alternative Flows*], [
      A1. Combination already used: system rejects and highlights conflicting variant.
      A2. Removes assigned option value: system accepts after confirmation.
      A3. Required option has no value: system warns variant may not appear in filtering.
    ],
    [*Exception Flows*], [
      E1. Option type deleted concurrently: system refreshes and notifies to reselect.
    ],
    [*Related Requirements*], [CAT-FR-10, CAT-FR-21],
  ),
  caption: [UC-ADM-VAR-02 -- Configure Variant Options.],
)

==== UC-ADM-VAR-03 — Configure Variant Pricing

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-VAR-03],
    [*Use Case Name*], [Configure Variant Pricing],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Set or update prices for variants with currency and optional validity periods.],
    [*Trigger*], [Administrator opens the pricing management interface.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Variants exist.
    ],
    [*Postconditions*], [
      - Pricing updated and used by catalog and checkout.
    ],
    [*Main Success Scenario*], [
      1. Navigates to pricing management for a product.
      2. System displays current prices for all variants, grouped by currency.
      3. Selects variants and specifies new price with currency and optional validity dates.
      4. Submits the pricing changes.
      5. System validates non-negative prices and valid currency.
      6. System persists the updated prices.
      7. System confirms the pricing update.
    ],
    [*Alternative Flows*], [
      A1. Zero price: system accepts but warns variant appears as free.
      A2. Overlapping date ranges for same variant and currency: system rejects.
      A3. Bulk percentage adjustment: system previews calculated prices before applying.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports and retains input for retry.
    ],
    [*Related Requirements*], [CAT-FR-22],
  ),
  caption: [UC-ADM-VAR-03 -- Configure Variant Pricing.],
)

==== Image and Embedding Management

// Diagram placeholder: Image and Embedding Management use case diagram

==== UC-ADM-IMG-01 — Upload Variant Images

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-IMG-01],
    [*Use Case Name*], [Upload Variant Images],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Associate images with a variant and initiate image processing.],
    [*Trigger*], [Administrator selects a variant and initiates image upload.],
    [*Preconditions*], [
      - Authenticated with image management permissions.
      - Variant exists.
    ],
    [*Postconditions*], [
      - Image stored and associated with variant. Processing scheduled.
    ],
    [*Main Success Scenario*], [
      1. Selects a product variant.
      2. Selects an image file from the local file system.
      3. System validates format (JPEG, PNG, WebP) and size (max 10 MB).
      4. Provides alt text and display order position.
      5. System validates the metadata.
      6. Confirms and submits the image.
      7. System stores the image and creates the image record.
      8. System generates thumbnails for catalog listing and preview.
      9. System schedules embedding generation via the ML service.
      10. System reports successful upload.
    ],
    [*Alternative Flows*], [
      A1. Unsupported format: system rejects and lists accepted formats.
      A2. Exceeds max size: system rejects and displays size constraint.
      A3. Multiple images: system processes sequentially and reports individually.
    ],
    [*Exception Flows*], [
      E1. Processing cannot be scheduled: system stores image and notifies that search will exclude it until processing succeeds.
      E2. Storage unreachable: system reports failure and suggests retry.
    ],
    [*Related Requirements*], [CAT-FR-04, CAT-FR-05, CAT-FR-14],
  ),
  caption: [UC-ADM-IMG-01 -- Upload Variant Images.],
)

==== UC-ADM-IMG-02 — Regenerate Image Embeddings

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-IMG-02],
    [*Use Case Name*], [Regenerate Image Embeddings],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Regenerate embeddings for selected product images.],
    [*Trigger*], [Administrator initiates embedding regeneration.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Images exist.
      - ML service is operational.
    ],
    [*Postconditions*], [
      - Embeddings updated and available for visual search.
    ],
    [*Main Success Scenario*], [
      1. Navigates to image management and selects images.
      2. Initiates the regenerate embeddings action.
      3. System displays confirmation showing affected image count.
      4. Confirms the operation.
      5. System sends each image to the ML service for embedding generation.
      6. System stores embeddings with model metadata.
      7. System reports completion with success count.
    ],
    [*Alternative Flows*], [
      A1. All images of a product: system batches processing and reports incrementally.
      A2. No images selected: system disables the action and prompts to select.
    ],
    [*Exception Flows*], [
      E1. ML service unavailable: system reports failure and suggests retry when operational.
      E2. Missing or corrupted file: system skips and continues; reports summary with failures.
    ],
    [*Related Requirements*], [CAT-FR-05, CAT-FR-15],
  ),
  caption: [UC-ADM-IMG-02 -- Regenerate Image Embeddings.],
)

==== Taxonomy and Classification

// Diagram placeholder: Taxonomy and Classification use case diagram

==== UC-ADM-TAX-01 — Manage Taxonomies

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-TAX-01],
    [*Use Case Name*], [Manage Taxonomies],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, reorder, or remove taxonomies and their hierarchical taxons.],
    [*Trigger*], [Administrator navigates to taxonomy management.],
    [*Preconditions*], [
      - Authenticated with taxonomy management permissions.
    ],
    [*Postconditions*], [
      - Taxonomy structure updated.
    ],
    [*Main Success Scenario*], [
      1. Navigates to taxonomy management.
      2. System displays the taxonomy tree with existing taxons.
      3. Creates a new taxonomy root or selects an existing taxonomy.
      4. Adds, edits, reorders, or removes taxon nodes.
      5. Optionally defines business rules attached to taxon nodes.
      6. Saves the changes.
      7. System persists the updated taxonomy.
      8. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Delete taxon with children: system prompts to cascade-delete or reassign to sibling.
      A2. Delete taxon with products: system warns products lose classification.
      A3. Reorder taxon: system accepts and adjusts sibling ordering.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes tree and asks to retry.
    ],
    [*Related Requirements*], [CAT-FR-09, CAT-FR-18],
  ),
  caption: [UC-ADM-TAX-01 -- Manage Taxonomies.],
)

==== UC-ADM-TAX-02 — Classify Products

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-TAX-02],
    [*Use Case Name*], [Classify Products],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Assign or remove product associations with taxons.],
    [*Trigger*], [Administrator opens the classification panel on a product detail page or batch view.],
    [*Preconditions*], [
      - Authenticated with catalog permissions.
      - Products and taxons exist.
    ],
    [*Postconditions*], [
      - Classifications updated.
    ],
    [*Main Success Scenario*], [
      1. Selects products from the catalog listing.
      2. Opens the classification panel.
      3. System displays taxonomy tree with current classifications.
      4. Selects taxons to assign or deselects to remove.
      5. Saves the changes.
      6. System validates each taxon path.
      7. System persists the updated associations.
      8. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Parent taxon without children: system accepts; product appears in parent browsing.
      A2. All taxons removed: system warns product won't appear in category browsing.
      A3. Auto-classification: system evaluates rules and assigns matching taxons.
    ],
    [*Exception Flows*], [
      E1. Referenced taxon deleted concurrently: system refreshes and notifies.
    ],
    [*Related Requirements*], [CAT-FR-09, CAT-FR-19],
  ),
  caption: [UC-ADM-TAX-02 -- Classify Products.],
)

==== Option Type Configuration

// Diagram placeholder: Option Type Configuration use case diagram

==== UC-ADM-OPT-01 — Manage Option Types

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-OPT-01],
    [*Use Case Name*], [Manage Option Types],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Manage option types and their predefined values.],
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
      6. Saves the changes.
      7. System validates name uniqueness and non-empty values.
      8. System persists the configuration.
      9. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Delete type in use by products: system warns and asks for confirmation.
      A2. Remove value in use by variants: system warns and asks for confirmation.
      A3. Reorder values: system persists new order; existing configurations unaffected.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes data and asks to retry.
    ],
    [*Related Requirements*], [CAT-FR-10, CAT-FR-20],
  ),
  caption: [UC-ADM-OPT-01 -- Manage Option Types.],
)
