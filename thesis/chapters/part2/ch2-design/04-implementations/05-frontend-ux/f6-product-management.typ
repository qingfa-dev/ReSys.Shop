===== Product Management: UC-ADM-PROD, UC-ADM-VAR, UC-ADM-IMG, UC-ADM-TAX, UC-ADM-OPT

The product management module is the most data-intensive admin area, organized into five interlinked management surfaces.

*Product list (UC-ADM-PROD).* Paginated data table with thumbnail, name, SKU, category, status badge (Draft/Active/Archived), and price. Toolbar: keyword search, category/status filters, New Product button. Bulk actions support status and category changes (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-list.png", width: 100%),
//   caption: [Admin product listing: paginated table (25 rows) with thumbnail, Product Name (linked), SKU, Category breadcrumb, Status badge (coloured pill), Price (VND), Last Modified. Toolbar: search, category dropdown, status filter, "New Product" button.],
// ) <fig-admin-product-list>

*Product form (UC-ADM-PROD).* Multi-section page: Basic Info (name, auto-generated slug, description, status), Fashion Metadata (style code, season, material, department, gender), SEO (meta title, description). Inline validation below each field (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-create.png", width: 100%),
//   caption: [Product creation form: Basic Info (name "Floral Summer Midi Dress", auto-generated slug, description textarea, status "Draft"), Fashion Metadata (style code, season "Spring/Summer", material "100% Cotton", department "Women", gender "Female"), SEO section. "Save" and "Save and Publish" buttons.],
// ) <fig-admin-product-create>

*Variant management (UC-ADM-VAR).* Embedded table: SKU, barcode, master-variant radio, option combination, inventory toggle, price, dimensions. Add Variant modal with option-value selectors, SKU auto-generation, and time-bound pricing (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-variants.png", width: 100%),
//   caption: [Variants tab: table (5 rows) with Master radio, SKU, Barcode, Options ("S / Black", "M / Navy"), Inventory toggle, Price (750,000 VND), Dimensions, Edit/Delete actions. "Add Variant" button.],
// ) <fig-admin-variants>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-variant-pricing.png", width: 100%),
//   caption: [Variant pricing dialog: price history with 3 entries (Current: 750,000 VND, Sale: 600,000 VND Dec 20-31, Scheduled: 700,000 VND Jan 15). "Add Price" inline form with amount, currency, date range.],
// ) <fig-admin-variant-pricing>

*Image and embedding management (UC-ADM-IMG).* Sortable image grid with drag handles, thumbnails, alt-text input, and delete action. Each image shows embedding status: green checkmark with model name for processed, yellow spinner for pending, red exclamation with Regenerate button for failed. "Regenerate All Embeddings" triggers re-embedding with active model (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-product-images.png", width: 100%),
//   caption: [Images tab: sortable grid of 6 image cards (200x200px), each with drag handle, alt text ("Front view: Navy variant"), embedding status badge (green checkmark + "Fashion-CLIP"), delete icon. Upload zone with drag-drop. "Regenerate All Embeddings" button.],
// ) <fig-admin-images>

*Taxonomy and option types (UC-ADM-TAX, UC-ADM-OPT).* Tree editor with left panel showing expandable taxonomy, drag-and-drop reordering, context-menu actions. Right panel shows selected taxon details. Option types table lists Size/Colour/Material with ordered values and drag handles (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-taxonomy-tree.png", width: 100%),
//   caption: [Taxonomy: left tree (Clothing > Dresses > Evening Dresses, Casual Dresses; Clothing > Tops > T-Shirts, Blouses). Right panel: "Evening Dresses" detail with name, slug, parent, product count (45). Add/Edit buttons.],
// ) <fig-admin-taxonomy>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-option-types.png", width: 100%),
//   caption: [Option types: table (Size: S/M/L/XL, Colour: Black/White/Navy/Burgundy/Olive, Material: Cotton/Silk/Linen/Polyester). Each row shows name, value count, actions. Click expands to show ordered values with drag handles.],
// ) <fig-admin-option-types>
