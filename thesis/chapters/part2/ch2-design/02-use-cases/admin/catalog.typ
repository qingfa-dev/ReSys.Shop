==== Product Management

// Diagram placeholder: Product Management use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-PROD-01], [Create product], [Admin], [Enter product information, configure a master variant, and assign to taxons.], [Admin is authenticated with catalog management permissions.], [New product created in Draft status and available for further catalog configuration.],
  [UC-ADM-PROD-02], [Update product], [Admin], [Modify catalog information, metadata, SEO attributes, or status of an existing product.], [Admin is authenticated. The product exists.], [Product updated with new information and consistent with catalog constraints.],
  [UC-ADM-PROD-03], [Archive product], [Admin], [Remove a product from the active catalog while retaining data for history.], [Admin is authenticated. The product exists.], [Product removed from storefront browsing and search. Associated data retained for order history and reporting.],
)

==== Variant and Pricing

// Diagram placeholder: Variant and Pricing use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-VAR-01], [Add product variant], [Admin], [Define variant identification, physical attributes, and assign option values to a product.], [Admin is authenticated. The parent product exists.], [New variant associated with the product and available for inventory, pricing, and catalog operations.],
  [UC-ADM-VAR-02], [Configure variant options], [Admin], [Assign a valid combination of option values such as size and colour to a variant.], [Admin is authenticated. The variant and option types exist.], [Variant has a defined option configuration presentable as product choices in the storefront.],
  [UC-ADM-VAR-03], [Configure variant pricing], [Admin], [Set or update applicable prices for one or more product variants.], [Admin is authenticated. The variants exist.], [Selected variants have updated pricing configurations used by catalog display and checkout operations.],
)

==== Image and Embedding Management

// Diagram placeholder: Image and Embedding Management use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-IMG-01], [Upload variant images], [Admin], [Associate product images with a selected variant and initiate image processing.], [Admin is authenticated with permission to manage variant images. The selected variant exists.], [Image stored and associated with variant. Image processing scheduled.],
  [UC-ADM-IMG-02], [Regenerate image embeddings], [Admin], [Regenerate embeddings for selected product images using the configured model.], [Admin is authenticated. The selected images exist.], [Selected images have current embeddings available for visual search and recommendation.],
)

=== UC-ADM-IMG-01 — Upload Variant Images

#table(
  columns: (auto, 1fr),
  stroke: 0.5pt,
  [*Field*], [*Description*],
  [Use Case ID], [UC-ADM-IMG-01],
  [Use Case Name], [Upload Variant Images],
  [Primary Actor], [Admin],
  [Goal], [Associate product images with a selected variant and initiate image processing.],
  [Trigger], [The administrator selects a variant and initiates image upload.],
  [Preconditions], [
    - The administrator is authenticated with permission to manage variant images.
    - The selected variant exists.
  ],
  [Postconditions], [
    - The image is stored and associated with the selected variant.
    - Image processing is scheduled.
  ],
  [Related FR], [CAT-FR-04, CAT-FR-05, CAT-FR-15],
)

*Main Success Scenario*

#table(
  columns: (auto, 1fr, 2fr),
  stroke: 0.5pt,
  [*Step*], [*Actor*], [*System Response*],
  [1], [Admin], [Selects a product variant in the administration interface.],
  [2], [Admin], [Selects an image file from the local file system.],
  [3], [System], [Validates the image format (JPEG, PNG, WebP) and size (max 10 MB).],
  [4], [Admin], [Provides display metadata: alt text and display order position.],
  [5], [System], [Validates the provided metadata.],
  [6], [Admin], [Confirms and submits the image.],
  [7], [System], [Stores the image file and creates the image record associated with the variant.],
  [8], [System], [Schedules embedding generation for the new image.],
  [9], [System], [Reports successful upload with a confirmation message.],
)

*Alternative and Exception Flows*

#table(
  columns: (auto, 1fr, 2fr),
  stroke: 0.5pt,
  [*ID*], [*Condition*], [*System Response*],
  [A1], [Unsupported image format], [Rejects the upload and displays a message listing the accepted formats.],
  [A2], [Image exceeds the maximum file size], [Rejects the upload and displays the applicable size constraint.],
  [E1], [Image processing cannot be scheduled], [Stores the image and records the processing failure for later handling; notifies the administrator that the image is uploaded but search results will not include it until processing succeeds.],
)

==== Taxonomy and Classification

// Diagram placeholder: Taxonomy and Classification use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-TAX-01], [Manage taxonomies], [Admin], [Create, update, reorder, or remove taxonomies and their hierarchical taxons.], [Admin is authenticated with taxonomy management permissions.], [Taxonomy structure reflects changes and remains available for product classification.],
  [UC-ADM-TAX-02], [Classify products], [Admin], [Assign or remove product associations with applicable taxons.], [Admin is authenticated. The products and taxons exist.], [Products have updated classifications reflected in catalog browsing and filtering.],
)

==== Option Type Configuration

// Diagram placeholder: Option Type Configuration use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-OPT-01], [Manage option types], [Admin], [Create, update, reorder, or remove product option types and their predefined option values.], [Admin is authenticated with option type management permissions.], [Configured option types and values available for product and variant configuration.],
)
