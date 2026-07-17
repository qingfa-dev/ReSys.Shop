import type { FeatureLocales } from '@/shared/locales/locale.types';

export const taxonomyLocales: FeatureLocales = {
  titles: {
    list: 'Taxonomies',
    create: 'Create Taxonomy',
    edit: 'Edit Taxonomy',
    basic_info: 'Basic Information',
    meta: 'Metadata',
  },
  descriptions: {
    list: 'Organize your products into root hierarchies (e.g. Categories, Brands).',
    create: 'Define a new taxonomy container for hierarchical classification.',
    edit: 'Update the taxonomy details and metadata.',
  },
  tabs: {
    general: 'General',
    metadata: 'Metadata',
  },
  labels: {
    name: 'Internal Name',
    presentation: 'Display Name',
    position: 'Sort Position',
    taxon_count: 'Category Count',
    public_metadata: 'Public Metadata',
    private_metadata: 'Private Metadata',
  },
  placeholders: {
    name: 'e.g. categories',
    presentation: 'e.g. Categories',
  },
  table: {
    name: 'Name',
    presentation: 'Display Name',
    position: 'Pos',
    taxons: 'Categories',
    actions: 'Actions',
  },
  actions: {
    create: 'New Taxonomy',
    edit: 'Edit',
    delete: 'Delete',
    cancel: 'Cancel',
    save_create: 'Create Taxonomy',
    save_edit: 'Update Taxonomy',
    manage_tree: 'Manage Tree',
  },
  messages: {
    create_success: 'Taxonomy created successfully.',
    update_success: 'Taxonomy updated successfully.',
    delete_success: 'Taxonomy deleted successfully.',
    empty_list: 'No taxonomies found.',
    loading: 'Loading taxonomies...',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  },
  confirm: {
    delete_header: 'Confirm Deletion',
    delete_message: 'Are you sure you want to delete the taxonomy "{name}"? All categories under it will also be deleted.',
  },
  actions_extra: {
    sign_in: 'Sign In'
  }
};
