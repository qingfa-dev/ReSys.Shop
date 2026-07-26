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
    [*Goal*], [Enter product information, configure a master variant, and assign the product to taxons.],
    [*Trigger*], [Administrator initiates product creation in the administration interface.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
    ],
    [*Postconditions*], [
      - New product created in Draft status.
      - At least one master variant associated with the product.
      - Product available for further catalog configuration.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects the option to create a new product.
      2. System -- Presents the product creation form.
      3. Administrator -- Enters product details: name, description, slug, and fashion-specific metadata (style code, season, material, department, gender target).
      4. Administrator -- Defines at least one variant with SKU and price as the master variant.
      5. Administrator -- Assigns the product to relevant taxons.
      6. Administrator -- Submits the product.
      7. System -- Validates all fields and the slug for uniqueness.
      8. System -- Creates the product record with Draft status and the master variant.
      9. System -- Confirms successful creation and displays the new product.
    ],
    [*Alternative Flows*], [
      A1. Slug is not unique -- System rejects the submission and prompts administrator to enter a different slug.
      A2. Administrator submits without a master variant -- System rejects and instructs administrator to designate a master variant.
      A3. Administrator provides only partial variant information -- System saves the product but marks the variant as incomplete.
    ],
    [*Exception Flows*], [
      E1. System fails to persist the product -- System reports the failure and retains the form data for retry.
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
    [*Goal*], [Modify catalog information, metadata, SEO attributes, or status of an existing product.],
    [*Trigger*], [Administrator selects a product from the catalog listing and opens the edit form.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The product exists.
    ],
    [*Postconditions*], [
      - Product updated with new information.
      - Catalog constraints remain consistent.
      - Status transitions logged for audit.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects a product from the catalog listing.
      2. System -- Displays the product edit form with current values.
      3. Administrator -- Modifies one or more fields: name, description, slug, fashion metadata, SEO attributes, or status.
      4. Administrator -- Submits the changes.
      5. System -- Validates all fields and checks slug uniqueness if the slug was changed.
      6. System -- Persists the updated product record.
      7. System -- Confirms successful update.
    ],
    [*Alternative Flows*], [
      A1. Administrator changes the slug to one already in use -- System rejects and prompts for a unique slug.
      A2. Administrator transitions the product from Active to Archived -- System verifies no active orders reference this product before allowing the transition.
      A3. Administrator clears required fields -- System highlights the missing fields and prevents submission.
    ],
    [*Exception Flows*], [
      E1. System fails to persist the update -- System reports the failure and retains the form data for retry.
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
    [*Goal*], [Remove a product from the active catalog while retaining data for order history and reporting.],
    [*Trigger*], [Administrator selects the archive action on a product.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The product exists.
    ],
    [*Postconditions*], [
      - Product removed from storefront browsing and search.
      - Associated data retained for order history and reporting.
      - Product status set to Archived.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects a product from the catalog listing.
      2. Administrator -- Initiates the archive action.
      3. System -- Displays a confirmation prompt with details about the product and its active variants.
      4. Administrator -- Confirms the archive operation.
      5. System -- Transitions the product status to Archived.
      6. System -- Removes the product from all storefront-facing views and search results.
      7. System -- Confirms successful archival.
    ],
    [*Alternative Flows*], [
      A1. Product is referenced by currently active orders -- System warns the administrator and allows archival (active orders remain unaffected) or cancellation of the operation.
      A2. Administrator cancels the confirmation -- System aborts the archive operation and returns to the product detail view.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the product was modified by another session, refreshes the data, and asks the administrator to retry.
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
    [*Goal*], [Define a variant with SKU, barcode, physical attributes, and optional option values for a product.],
    [*Trigger*], [Administrator opens the variant creation form from the product detail page.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The parent product exists.
    ],
    [*Postconditions*], [
      - New variant associated with the product.
      - Variant available for inventory, pricing, and catalog operations.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the product detail page and selects the option to add a variant.
      2. System -- Presents the variant creation form.
      3. Administrator -- Enters variant details: SKU, barcode, dimensions, weight, and position.
      4. Administrator -- Assigns option values (e.g. Size M, Colour Red) from available option types.
      5. Administrator -- Submits the variant.
      6. System -- Validates that the SKU is unique across the catalog.
      7. System -- Validates that the option value combination does not conflict with an existing variant for this product.
      8. System -- Creates the variant and associates it with the product.
      9. System -- Confirms successful creation.
    ],
    [*Alternative Flows*], [
      A1. SKU is not unique -- System rejects and prompts administrator to enter a different SKU.
      A2. Duplicate option value combination -- System rejects and warns that the same combination already exists for another variant of this product.
      A3. Administrator omits all option values -- System accepts the variant but displays a reminder that configuring options improves the storefront experience.
    ],
    [*Exception Flows*], [
      E1. System fails to persist the variant -- System reports the failure and retains the form data for retry.
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
    [*Goal*], [Assign a valid combination of option values such as size and colour to a variant.],
    [*Trigger*], [Administrator opens the option configuration panel for a variant.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The variant exists.
      - The relevant option types and values exist.
    ],
    [*Postconditions*], [
      - Variant has a defined option configuration.
      - Option values are presentable as product choices in the storefront.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects a variant from the product detail page.
      2. System -- Displays the variant detail including current option assignments.
      3. Administrator -- Selects an option type (e.g. Size) and chooses a value from the predefined list.
      4. Administrator -- Repeats for additional option types (e.g. Colour, Material).
      5. Administrator -- Saves the option configuration.
      6. System -- Validates the option value combination against existing variants of the same product.
      7. System -- Persists the updated option assignments.
      8. System -- Confirms the change.
    ],
    [*Alternative Flows*], [
      A1. Selected option value combination already used by another variant -- System rejects and highlights the conflicting variant.
      A2. Administrator removes a previously assigned option value -- System accepts the change after confirmation.
      A3. Required option type has no value selected -- System warns the administrator that the variant may not appear in option-based filtering.
    ],
    [*Exception Flows*], [
      E1. Option type or value was deleted by a concurrent session -- System refreshes the available options and notifies the administrator to reselect.
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
    [*Goal*], [Set or update applicable prices for one or more product variants with currency specification and optional validity periods.],
    [*Trigger*], [Administrator opens the pricing management interface for a product or variant.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The variants exist.
    ],
    [*Postconditions*], [
      - Selected variants have updated pricing configurations.
      - Prices are used by catalog display and checkout operations.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the pricing management section for a product.
      2. System -- Displays current prices for all variants, grouped by currency.
      3. Administrator -- Selects one or more variants and specifies a new price with currency and optional validity dates.
      4. Administrator -- Submits the pricing changes.
      5. System -- Validates the price values (non-negative, valid currency).
      6. System -- Persists the updated prices.
      7. System -- Confirms the pricing update.
    ],
    [*Alternative Flows*], [
      A1. Administrator sets a zero price -- System accepts but displays a warning that the variant will appear as free.
      A2. Administrator specifies overlapping date ranges for the same variant and currency -- System rejects and asks administrator to adjust the date ranges.
      A3. Administrator bulk-updates all variants with a percentage adjustment -- System calculates new prices and presents a preview before applying.
    ],
    [*Exception Flows*], [
      E1. System fails to persist pricing changes -- System reports the failure and retains the input data for retry.
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
    [*Goal*], [Associate product images with a selected variant and initiate image processing.],
    [*Trigger*], [Administrator selects a variant and initiates image upload.],
    [*Preconditions*], [
      - Administrator is authenticated with permission to manage variant images.
      - The selected variant exists.
    ],
    [*Postconditions*], [
      - Image is stored and associated with the selected variant.
      - Image processing and embedding generation are scheduled.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects a product variant in the administration interface.
      2. Administrator -- Selects an image file from the local file system.
      3. System -- Validates the image format (JPEG, PNG, WebP) and size (max 10 MB).
      4. Administrator -- Provides display metadata: alt text and display order position.
      5. System -- Validates the provided metadata.
      6. Administrator -- Confirms and submits the image.
      7. System -- Stores the image file and creates the image record associated with the variant.
      8. System -- Generates thumbnails for catalog listing and preview displays.
      9. System -- Schedules embedding generation for the new image via the ML service.
      10. System -- Reports successful upload with a confirmation message.
    ],
    [*Alternative Flows*], [
      A1. Unsupported image format -- System rejects the upload and displays a message listing the accepted formats.
      A2. Image exceeds the maximum file size -- System rejects the upload and displays the applicable size constraint.
      A3. Administrator uploads multiple images simultaneously -- System processes each image in sequence and reports results individually.
    ],
    [*Exception Flows*], [
      E1. Image processing cannot be scheduled -- System stores the image and records the processing failure for later handling; notifies the administrator that the image is uploaded but search results will not include it until processing succeeds.
      E2. Storage service is unreachable -- System reports the failure and suggests the administrator retry the upload.
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
    [*Goal*], [Regenerate embeddings for selected product images using the configured model.],
    [*Trigger*], [Administrator initiates embedding regeneration for one or more images.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The selected images exist.
      - The ML service is operational.
    ],
    [*Postconditions*], [
      - Selected images have current embeddings.
      - Embeddings are available for visual search and recommendation.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the image management section and selects one or more product images.
      2. Administrator -- Initiates the regenerate embeddings action.
      3. System -- Displays a confirmation prompt showing the number of affected images.
      4. Administrator -- Confirms the operation.
      5. System -- Sends each image to the ML service for embedding generation.
      6. System -- Stores the generated embeddings with model metadata (model name, version, dimension).
      7. System -- Reports completion with a count of successfully regenerated embeddings.
    ],
    [*Alternative Flows*], [
      A1. Administrator regenerates embeddings for all images of a product -- System batches the processing and reports progress incrementally.
      A2. No images are selected -- System disables the regenerate action and prompts the administrator to select at least one image.
    ],
    [*Exception Flows*], [
      E1. ML service is unavailable -- System reports the failure and suggests the administrator retry when the service is operational.
      E2. Image file is missing or corrupted -- System skips the image, records the failure, and continues processing the remaining images; reports a summary including failures.
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
    [*Trigger*], [Administrator navigates to the taxonomy management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with taxonomy management permissions.
    ],
    [*Postconditions*], [
      - Taxonomy structure reflects changes.
      - Taxons remain available for product classification.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the taxonomy management interface.
      2. System -- Displays the taxonomy tree with existing taxons in hierarchical order.
      3. Administrator -- Creates a new taxonomy root or selects an existing taxonomy.
      4. Administrator -- Adds, edits, reorders, or removes taxon nodes within the hierarchy.
      5. Administrator -- Optionally defines business rules attached to taxon nodes (attribute constraints, automatic assignments).
      6. Administrator -- Saves the taxonomy changes.
      7. System -- Persists the updated taxonomy structure.
      8. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator attempts to delete a taxon with child nodes -- System prompts to either cascade-delete all descendants or reassign them to a sibling taxon.
      A2. Administrator attempts to delete a taxon with associated products -- System warns that products will lose their classification and asks for confirmation.
      A3. Administrator reorders a taxon -- System accepts the new position and adjusts sibling ordering automatically.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the taxonomy was modified by another session, refreshes the tree, and asks the administrator to retry.
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
    [*Goal*], [Assign or remove product associations with applicable taxons.],
    [*Trigger*], [Administrator opens the classification panel on a product detail page or from a batch classification view.],
    [*Preconditions*], [
      - Administrator is authenticated with catalog management permissions.
      - The products and taxons exist.
    ],
    [*Postconditions*], [
      - Products have updated classifications.
      - Changes reflected in catalog browsing and filtering.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects one or more products from the catalog listing.
      2. Administrator -- Opens the classification panel.
      3. System -- Displays the taxonomy tree next to the current product classifications.
      4. Administrator -- Selects taxons to assign or deselects taxons to remove.
      5. Administrator -- Saves the classification changes.
      6. System -- Validates that each assigned taxon path is valid.
      7. System -- Persists the updated product-taxon associations.
      8. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator assigns a parent taxon without its children -- System accepts the assignment; the product appears when browsing the parent category.
      A2. Administrator removes all taxons from a product -- System warns that the product will not appear in any category-based browsing path.
      A3. Administrator triggers auto-classification for a product -- System evaluates applicable taxon rules and assigns matching taxons automatically.
    ],
    [*Exception Flows*], [
      E1. Referenced taxon was deleted by a concurrent session -- System refreshes the taxonomy tree and notifies the administrator to re-select.
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
    [*Goal*], [Create, update, reorder, or remove product option types and their predefined option values.],
    [*Trigger*], [Administrator navigates to the option type management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with option type management permissions.
    ],
    [*Postconditions*], [
      - Configured option types and values available for product and variant configuration.
      - Changes propagated to product option type associations.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the option type management interface.
      2. System -- Displays all existing option types with their values.
      3. Administrator -- Creates a new option type with a name and presentation style (e.g. dropdown, colour swatch).
      4. Administrator -- Adds ordered option values to the type (e.g. S, M, L, XL for Size).
      5. Administrator -- Optionally edits, reorders, or removes existing option types and values.
      6. Administrator -- Saves the changes.
      7. System -- Validates that option type names are unique and values are not empty.
      8. System -- Persists the updated option type configuration.
      9. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator attempts to delete an option type in use by products -- System warns that affected products will lose that variant dimension and asks for confirmation.
      A2. Administrator removes an option value in use by variants -- System warns that affected variants will have incomplete option configurations and asks for confirmation.
      A3. Administrator reorders option values -- System persists the new order; existing variant configurations are unaffected.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the option type was modified by another session, refreshes the data, and asks the administrator to retry.
    ],
    [*Related Requirements*], [CAT-FR-10, CAT-FR-20],
  ),
  caption: [UC-ADM-OPT-01 -- Manage Option Types.],
)
