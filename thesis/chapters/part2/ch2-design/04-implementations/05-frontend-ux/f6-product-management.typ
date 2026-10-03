===== Product Management: UC-ADM-PROD, UC-ADM-VAR, UC-ADM-IMG, UC-ADM-TAX, UC-ADM-OPT

The product management module is the most data-intensive admin area, organized into five interlinked management surfaces.

*Product list (UC-ADM-PROD).* Paginated data table with thumbnail, name, SKU, category, status badge (Draft/Active/Archived), and price. Toolbar: keyword search, category/status filters, New Product button. Bulk actions support status and category changes (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-list.png", width: 100%),
  caption: [Admin product listing: paginated table with SKU, status, VND price, toolbar.],
) <fig-admin-product-list>

*Product form (UC-ADM-PROD).* Multi-section page: Basic Info (name, auto-generated slug, description, status), Fashion Metadata (style code, season, material, department, gender), SEO (meta title, description). Option Type assignment uses a source/target PickList dual-transfer control (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-create.png", width: 100%),
  caption: [Product creation form: basic info, fashion metadata, SEO, option assignment.],
) <fig-admin-product-create>

*Variant management (UC-ADM-VAR).* Embedded table: SKU, barcode, master-variant radio, option combination, inventory toggle, price, dimensions. Add Variant modal with option-value selectors, SKU auto-generation, and time-bound pricing (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-variants.png", width: 100%),
  caption: [Variant management: Master radio, SKU, options, inventory toggle, pricing.],
) <fig-admin-variants>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-variant-pricing.png", width: 100%),
  caption: [Variant pricing tab: Current/Sale/Scheduled history with inline add form.],
) <fig-admin-variant-pricing>

*Image and embedding management (UC-ADM-IMG).* Sortable image grid with drag handles, thumbnails, alt-text input, and delete action. Each image shows embedding status: green checkmark with model name for processed, yellow spinner for pending, red exclamation with Regenerate button for failed. "Regenerate All Embeddings" triggers re-embedding with active model (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-images.png", width: 100%),
  caption: [Images tab: sortable grid with Fashion-CLIP embedding status and regeneration.],
) <fig-admin-images>

*Taxonomy and option types (UC-ADM-TAX, UC-ADM-OPT).* Tree editor with left panel showing expandable taxonomy, drag-and-drop reordering, context-menu actions. Right panel shows selected taxon details. Option types table lists Size/Colour/Material with ordered values and drag handles.
