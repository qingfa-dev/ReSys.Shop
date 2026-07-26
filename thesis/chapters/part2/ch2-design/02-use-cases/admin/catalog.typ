==== Product Management

// Diagram placeholder: Product Management use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-CAT-01], [Create product], [Administrator],
    [Enter product information, configure a master variant, and upload product images. Assign the product to relevant taxons.],
    [A new product is created in Draft status and available for further catalog configuration.],
    [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-13],
    [UC-ADM-CAT-02], [Update product], [Administrator],
    [Select an existing product and modify its catalog information, metadata, SEO attributes, or status.],
    [The product is updated with the new information and remains consistent with catalog constraints.],
    [CAT-FR-01, CAT-FR-12],
    [UC-ADM-CAT-03], [Archive product], [Administrator],
    [Select an existing product and archive it from the active catalog.],
    [The product is removed from storefront browsing and search. Associated data is retained for order history and reporting.],
    [CAT-FR-01, CAT-FR-12],
  ),
  caption: [Administrator use cases — Product Management.],
)

==== Variant and Pricing

// Diagram placeholder: Variant and Pricing use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-CAT-04], [Add product variant], [Administrator],
    [Select a product, define variant identification and physical attributes, and assign the appropriate option values.],
    [A new variant is associated with the product and available for inventory, pricing, and catalog operations.],
    [CAT-FR-03, CAT-FR-21],
    [UC-ADM-CAT-05], [Configure variant options], [Administrator],
    [Select a product variant and assign a valid combination of option values such as size and colour.],
    [The variant has a defined option configuration that can be presented as a product choice in the storefront.],
    [CAT-FR-10, CAT-FR-20, CAT-FR-21],
    [UC-ADM-CAT-06], [Configure variant pricing], [Administrator],
    [Set or update the applicable prices for one or more product variants.],
    [The selected variants have updated pricing configurations used by catalog display and checkout operations.],
    [CAT-FR-21, CAT-FR-22],
  ),
  caption: [Administrator use cases — Variant and Pricing.],
)

==== Image and Embedding Management

// Diagram placeholder: Image and Embedding Management use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-CAT-07], [Upload variant image], [Administrator],
    [Select a product variant, upload an image, and provide its display metadata.],
    [The image is associated with the variant and available for catalog display and visual search processing.],
    [CAT-FR-04, CAT-FR-05, CAT-FR-15],
    [UC-ADM-CAT-08], [Regenerate image embeddings], [Administrator],
    [Select one or more product images and request embedding regeneration using the configured model.],
    [The selected images have current embeddings available for visual search and recommendation.],
    [CAT-FR-05, CAT-FR-08, CAT-FR-15],
  ),
  caption: [Administrator use cases — Image and Embedding Management.],
)

==== Taxonomy and Classification

// Diagram placeholder: Taxonomy and Classification use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-CAT-09], [Manage taxonomies], [Administrator],
    [Create, update, reorder, or remove taxonomies and their hierarchical taxons.],
    [The taxonomy structure reflects the changes and remains available for product classification.],
    [CAT-FR-09],
    [UC-ADM-CAT-10], [Classify products], [Administrator],
    [Select products and assign or remove their associations with applicable taxons.],
    [The products have updated classifications reflected in catalog browsing and filtering.],
    [CAT-FR-09, CAT-FR-19],
  ),
  caption: [Administrator use cases — Taxonomy and Classification.],
)

==== Option Type Configuration

// Diagram placeholder: Option Type Configuration use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-CAT-11], [Manage option types], [Administrator],
    [Create, update, reorder, or remove product option types and their predefined option values.],
    [The configured option types and values are available for product and variant configuration.],
    [CAT-FR-10, CAT-FR-20],
  ),
  caption: [Administrator use cases — Option Type Configuration.],
)
