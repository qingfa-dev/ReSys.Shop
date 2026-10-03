import type { FeatureLocales } from '@/shared/locales/locale.types';

export interface ProductLocales extends FeatureLocales {
  tabs: {
    general: string;
    variants: string;
    images: string;
    categories: string;
    inventory: string;
    seo: string;
    metadata: string;
  };
  images: {
    assets_title: string;
    add_new: string;
    select_prompt: string;
    upload_now: string;
    edit_title: string;
    alt_text: string;
    alt_placeholder: string;
    role_label: string;
    conflict_header: string;
    conflict_msg: string;
    roles: {
      primary: string;
      thumbnail: string;
      square: string;
      gallery: string;
      search: string;
      desc_primary: string;
      desc_thumbnail: string;
      desc_square: string;
      desc_gallery: string;
      desc_search: string;
    };
  };
  variants: {
    sku_variants: string;
    sku_desc: string;
    generate: string;
    empty: string;
    wizard: {
      step1: string;
      step2: string;
      select_prompt: string;
      no_options: string;
      no_options_desc: string;
      preview: string;
      combinations: string;
      confirm_btn: string;
    };
    form: {
      new_variant: string;
      edit_variant: string;
      attributes: string;
      track_inventory: string;
      track_inventory_desc: string;
      barcode: string;
    };
  };
  inventory_table: {
    title: string;
    location: string;
    on_hand: string;
    reserved: string;
    available: string;
    backorder: string;
    no_records: string;
    manage_stock: string;
  };
  confirm: {
    delete_header: string;
    delete_message: string;
    accept_label: string;
    reject_label: string;
  };
}

export const productLocales: ProductLocales = {
  titles: {
    list: 'Products',
    create: 'Create Product',
    edit: 'Edit Product',
    basic_info: 'Basic Information',
    classifications: 'Product Classifications',
    manage: 'Manage Categories',
    inventory: 'Inventory & Dimensions',
    metadata: 'Metadata',
    variants: 'Variants',
    images: 'Images',
  },
  tabs: {
    general: 'General',
    variants: 'Variants',
    images: 'Images',
    categories: 'Categories',
    inventory: 'Inventory',
    seo: 'SEO',
    metadata: 'Metadata',
  },
  descriptions: {
    list: 'Manage your product catalog, pricing and stock.',
    classifications: 'Assign this product to one or more categories across different taxonomies.',
    general: 'Basic identification and pricing details.',
    inventory: 'Physical dimensions and shipping weights.',
    variants: 'Manage SKU variations like sizes or colors.',
    images: 'Visual assets for the storefront.',
    inventory_management: 'Track stock levels per variant and location.',
  },
  images: {
    assets_title: 'Visual Assets',
    add_new: 'Add New Image',
    select_prompt: 'Select Image to Upload',
    upload_now: 'Upload Now',
    edit_title: 'Edit Image Details',
    alt_text: 'Alt Text (SEO)',
    alt_placeholder: 'Describe the image...',
    role_label: 'Image Role',
    conflict_header: 'Confirm Replacement',
    conflict_msg: 'A {role} image already exists. Uploading this will replace the existing one.',
    roles: {
      primary: 'Primary',
      thumbnail: 'Thumbnail',
      square: 'Square',
      gallery: 'Gallery',
      search: 'Search',
      desc_primary: 'Main cover image for listings. Replaces existing primary.',
      desc_thumbnail: 'Small preview for carts and lists.',
      desc_square: '1:1 ratio optimized for feeds.',
      desc_gallery: 'Standard image shown in the product gallery carousel.',
      desc_search: 'Optimized for semantic search results. Replaces existing search.',
    }
  },
  variants: {
    sku_variants: 'SKU Variants',
    sku_desc: 'Manage sizes, colors, and other variations of this product.',
    generate: 'Generate Variants',
    empty: 'No variations defined yet. Only the Master SKU exists.',
    wizard: {
      step1: '1. Select Options',
      step2: '2. Preview & Generate',
      select_prompt: 'Select the option values you want to combine. A variant will be created for every possible combination.',
      no_options: 'No Option Types Assigned',
      no_options_desc: 'Please go to the "Options" tab and assign types (e.g. Size, Color) first.',
      preview: 'Preview',
      combinations: '{count} Variants',
      confirm_btn: 'Generate Variants',
    },
    form: {
      new_variant: 'New Variant',
      edit_variant: 'Edit Variant',
      attributes: 'Attributes',
      track_inventory: 'Track Inventory',
      track_inventory_desc: 'Enable stock tracking for this variant',
      barcode: 'Barcode / GTIN',
    }
  },
  inventory_table: {
    title: 'Inventory Management',
    location: 'Location',
    on_hand: 'On Hand',
    reserved: 'Reserved',
    available: 'Available',
    backorder: 'Backorder',
    no_records: 'No stock records initialized.',
    manage_stock: 'Manage Stock',
  },
  table: {
    preview: 'Preview',
    name: 'Name',
    sku: 'SKU',
    price: 'Price',
    status: 'Status',
    actions: 'Actions',
    clear_filter: 'Clear Filters'
  },
  labels: {
    name: 'Product Name',
    slug: 'Slug (URL)',
    sku: 'SKU',
    price: 'Base Price',
    description: 'Description',
    status: 'Active Status',
    taxonomy: 'Taxonomy',
    category: 'Category',
    is_main: 'Main Category',
    weight: 'Weight (kg)',
    height: 'Height (cm)',
    width: 'Width (cm)',
    depth: 'Depth (cm)',
    brand: 'Brand',
    meta_title: 'Meta Title',
    meta_description: 'Meta Description',
    meta_keywords: 'Meta Keywords',
    public_metadata: 'Public Metadata',
    private_metadata: 'Private Metadata',
    actions: 'Actions',
  },
  actions: {
    new: 'New Product',
    save: 'Save Product',
    cancel: 'Cancel',
    delete: 'Delete',
    edit: 'Edit',
    add: 'Assign Category',
  },
  placeholders: {
    search: 'Search by name or SKU...',
    name: 'e.g. Vintage Denim Jacket',
    slug: 'e.g. vintage-denim-jacket',
  },
  messages: {
    create_success: 'Product created successfully.',
    update_success: 'Product updated successfully.',
    delete_success: 'Product removed successfully.',
    empty_list: 'No products found matching your criteria.',
    loading: 'Loading products...',
  },
  confirm: {
    delete_header: 'Confirm Delete',
    delete_message: 'Are you sure you want to delete "{name}"? This action cannot be undone.',
    accept_label: 'Delete',
    reject_label: 'Cancel'
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  }
};