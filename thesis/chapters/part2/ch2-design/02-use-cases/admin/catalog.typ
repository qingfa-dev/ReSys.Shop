==== Product Management

// Diagram placeholder: Product Management use case diagram

*UC-ADM-CAT-01 — Create product.*
*Primary Actor:* Administrator. \
*Main Flow:* Enter product information, configure a master variant, and upload product images. Assign the product to relevant taxons. \
*Postcondition:* A new product is created in Draft status and available for further catalog configuration. \
*Related FR:* CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-13.

#v(0.5cm)
*UC-ADM-CAT-02 — Update product.*
*Primary Actor:* Administrator. \
*Main Flow:* Select an existing product and modify its catalog information, metadata, SEO attributes, or status. \
*Postcondition:* The product is updated with the new information and remains consistent with catalog constraints. \
*Related FR:* CAT-FR-01, CAT-FR-12.

#v(0.5cm)
*UC-ADM-CAT-03 — Archive product.*
*Primary Actor:* Administrator. \
*Main Flow:* Select an existing product and archive it from the active catalog. \
*Postcondition:* The product is removed from storefront browsing and search. Associated data is retained for order history and reporting. \
*Related FR:* CAT-FR-01, CAT-FR-12.

==== Variant and Pricing

// Diagram placeholder: Variant and Pricing use case diagram

*UC-ADM-CAT-04 — Add product variant.*
*Primary Actor:* Administrator. \
*Main Flow:* Select a product, define variant identification and physical attributes, and assign the appropriate option values. \
*Postcondition:* A new variant is associated with the product and available for inventory, pricing, and catalog operations. \
*Related FR:* CAT-FR-03, CAT-FR-21.

#v(0.5cm)
*UC-ADM-CAT-05 — Configure variant options.*
*Primary Actor:* Administrator. \
*Main Flow:* Select a product variant and assign a valid combination of option values such as size and colour. \
*Postcondition:* The variant has a defined option configuration that can be presented as a product choice in the storefront. \
*Related FR:* CAT-FR-10, CAT-FR-20, CAT-FR-21.

#v(0.5cm)
*UC-ADM-CAT-06 — Configure variant pricing.*
*Primary Actor:* Administrator. \
*Main Flow:* Set or update the applicable prices for one or more product variants. \
*Postcondition:* The selected variants have updated pricing configurations used by catalog display and checkout operations. \
*Related FR:* CAT-FR-21, CAT-FR-22.

==== Image and Embedding Management

// Diagram placeholder: Image and Embedding Management use case diagram

*UC-ADM-CAT-07 — Upload variant image.*
*Primary Actor:* Administrator. \
*Main Flow:* Select a product variant, upload an image, and provide its display metadata. \
*Postcondition:* The image is associated with the variant and available for catalog display and visual search processing. \
*Related FR:* CAT-FR-04, CAT-FR-05, CAT-FR-15.

#v(0.5cm)
*UC-ADM-CAT-08 — Regenerate image embeddings.*
*Primary Actor:* Administrator. \
*Main Flow:* Select one or more product images and request embedding regeneration using the configured model. \
*Postcondition:* The selected images have current embeddings available for visual search and recommendation. \
*Related FR:* CAT-FR-05, CAT-FR-08, CAT-FR-15.

==== Taxonomy and Classification

// Diagram placeholder: Taxonomy and Classification use case diagram

*UC-ADM-CAT-09 — Manage taxonomies.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, reorder, or remove taxonomies and their hierarchical taxons. \
*Postcondition:* The taxonomy structure reflects the changes and remains available for product classification. \
*Related FR:* CAT-FR-09.

#v(0.5cm)
*UC-ADM-CAT-10 — Classify products.*
*Primary Actor:* Administrator. \
*Main Flow:* Select products and assign or remove their associations with applicable taxons. \
*Postcondition:* The products have updated classifications reflected in catalog browsing and filtering. \
*Related FR:* CAT-FR-09, CAT-FR-19.

==== Option Type Configuration

// Diagram placeholder: Option Type Configuration use case diagram

*UC-ADM-CAT-11 — Manage option types.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, reorder, or remove product option types and their predefined option values. \
*Postcondition:* The configured option types and values are available for product and variant configuration. \
*Related FR:* CAT-FR-10, CAT-FR-20.
