=== Catalog Context

The *Catalog Context* is the core of the e-commerce storefront. It handles how products are defined, categorized, and presented to customers. The data model is designed for high flexibility and read-performance, supporting complex product hierarchies and AI-driven discovery.

==== Product Definition
The catalog uses a *Product-Variant* parent-child relationship to handle SKU variations.

- *Product:* Represents the abstract "concept" of an item (e.g., "Cotton T-Shirt"). It holds shared metadata like the description, slug, and general status.
- *Variant:* Represents the actual sellable physical unit (e.g., "Cotton T-Shirt - Red - Large"). It holds the specific SKU, price, inventory tracking flags, and physical dimensions.

This separation allows the storefront to group related items (Color/Size permutations) under a single page while tracking inventory for each specific combination strictly.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [Name], [VARCHAR(100)], [The display name of the product family.],
    [3], [Slug], [VARCHAR(255)], [Unique URL fragment (e.g. 'cotton-tshirt'). Indexed for strict SEO lookup.],
    [4], [Description], [TEXT], [HTML or Markdown description of the product.],
    [5], [Status], [VARCHAR(20)], [Lifecycle state: 'Draft' (Hidden), 'Active' (Visible), 'Archived' (Hidden).],
    [6], [AvailableOn], [TIMESTAMP], [Scheduled publishing date. Products are hidden until this time passes.],
    [7], [MetaTitle], [VARCHAR(255)], [SEO: Overrides the <title> tag for search engines.],
    [8], [MetaDescription], [VARCHAR(500)], [SEO: Meta description for search results snippets.],
    [9], [MetaKeywords], [VARCHAR(255)], [SEO: Comma-separated keywords.],
    [10], [IsDeleted], [BOOL], [Soft-delete flag to preserve historical data.],
  ),
  caption: [Products table],
)

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [ProductId], [UUID], [Foreign Key linking to the parent Product.],
    [3], [Sku], [VARCHAR(100)], [Stock Keeping Unit. Must be globally unique for warehouse tracking.],
    [4], [Price], [DECIMAL(18,2)], [The base selling price. Uses DECIMAL to prevent floating point errors.],
    [5], [CostPrice], [DECIMAL(18,2)], [Internal cost for margin calculation.],
    [6], [CompareAtPrice], [DECIMAL(18,2)], [Original price for discount display (strikethrough).],
    [7], [IsMaster], [BOOL], [Flag indicating if this is the 'default' variant shown on listing pages.],
    [8], [TrackInventory], [BOOL], [If true, prevents sales when stock is 0. If false, allows infinite sales.],
    [9], [Weight], [DECIMAL(10,2)], [Physical weight for shipping calculation.],
    [10], [Height], [DECIMAL(10,2)], [Physical height dimension.],
    [11], [Width], [DECIMAL(10,2)], [Physical width dimension.],
    [12], [Depth], [DECIMAL(10,2)], [Physical depth dimension.],
  ),
  caption: [Variants table],
)

==== Hierarchical Categorization (Taxonomies)
Categories are implemented using a *Taxonomy* model backed by a *Nested Set* data structure instead of a simple adjacency list.

- *Rationale for Nested Set Strategy:* A classic parent-child recursion requires $N$ database queries to traverse a tree of depth $N$. The Nested Set model (using `Lft` and `Rgt` bounds) allows selecting an entire subtree (e.g., "All Men's Clothing") in a *single* SQL query, drastically improving read performance for mega-menus and breadcrumbs.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [TaxonomyId], [UUID], [Links the category to a specific tree (e.g. 'Brand', 'Department').],
    [3], [ParentId], [UUID], [Self-reference for easy editing (Adjacency List view).],
    [4], [Name], [VARCHAR(100)], [Category name (e.g. 'Sneakers').],
    [5], [Slug], [VARCHAR(255)], [URL fragment for the category page.],
    [6], [Lft], [INT], [Nested Set Left Bound. Used for subtree inclusion checks.],
    [7], [Rgt], [INT], [Nested Set Right Bound.],
    [8], [Depth], [INT], [Cached tree depth for indentation in UI.],
    [9], [MetaTitle], [VARCHAR(255)], [SEO: Title override.],
    [10], [MetaDescription], [VARCHAR(500)], [SEO: Description snippet.],
  ),
  caption: [Taxons table],
)

==== Dynamic Attributes (Options)
To support diverse product types without hardcoding columns (e.g., "Screen Size" vs "Fabric Type"), the system uses an *EAV-lite* (Entity-Attribute-Value) pattern called *Options*.

- *OptionType:* Defines the attribute class (e.g., "Color").
- *OptionValue:* Defines the specific choices (e.g., "Red", "Blue").
- *ProductOption:* Links these choices to variants, generating the permutations.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [Name], [VARCHAR(100)], [Internal code (e.g. 'tshirt-size').],
    [3], [Presentation], [VARCHAR(100)], [Customer-facing label (e.g. 'Size').],
    [4], [Key], [VARCHAR(50)], [System identifier for filtering logic.],
  ),
  caption: [OptionTypes table],
)

The flexibility of this EAV-lite model is managed through a dedicated interface that allows administrators to define new attribute classes without database schema migrations.

#figure(
  image("../../../../images/ui/admin/ui-admin-catalog-optiontype-list.png", width: 100%),
  caption: [Attribute Management: Interface for defining reusable Option Types (e.g., Colors, Sizes) across the catalog.],
)

==== AI Visual Intelligence
A key differentiator of ReSys.Shop is its vector-native architecture. Instead of treating images as simple URLs, they are analyzed and embedded into high-dimensional space.

- *Separation of Concerns:* `ImageEmbeddings` are decoupled from `ProductImages`. This allows multiple AI models (e.g., *Fashion-CLIP* for style similarity, *ResNet* for color matching, *OCR* for text) to attach their own vectors to the same image without polluting the main table.
- *pgvector:* Vectors are stored using the `vector(512)` type, enabling nearest-neighbor search directly in the database.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [ProductId], [UUID], [The product this image belongs to.],
    [3], [Url], [TEXT], [Direct link to the image blob storage.],
    [4], [Status], [SMALLINT], [Processing State: 0=Pending, 1=Processed, 2=Failed. Drives the background ML queue.],
    [5], [AltText], [VARCHAR(255)], [Accessibility text (often AI-generated).],
  ),
  caption: [ProductImages table],
)

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [ProductImageId], [UUID], [Foreign Key to the source image.],
    [3], [ModelName], [VARCHAR(50)], [Discriminator for the AI model used (e.g., 'fashion_clip').],
    [4], [Vector], [VECTOR(512)], [The 512-dimension float32 embedding vector.],
  ),
  caption: [ImageEmbeddings table (pgvector)],
)

