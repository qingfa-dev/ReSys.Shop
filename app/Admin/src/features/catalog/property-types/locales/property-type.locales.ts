import type { FeatureLocales } from '@/shared/locales/locale.types';

export const propertyTypeLocales: FeatureLocales = {
  titles: {
    list: 'Property Types',
    create: 'Create Property Type',
    edit: 'Edit Property Type',
    basic_info: 'Basic Information',
  },
  descriptions: {
    list: 'Manage product properties like Material, Brand, etc.',
    create: 'Define a new property type for your catalog.',
  },
  labels: {
    name: 'Internal Name',
    presentation: 'Display Name',
    kind: 'Data Type',
    position: 'Position',
    filterable: 'Filterable',
  },
  placeholders: {
    name: 'e.g. material',
    presentation: 'e.g. Material',
  },
  table: {
    name: 'Name',
    presentation: 'Display Name',
    kind: 'Type',
    position: 'Pos',
    filterable: 'Filterable',
    actions: 'Actions',
  },
  actions: {
    create: 'New Property Type',
    edit: 'Edit',
    delete: 'Delete',
    cancel: 'Cancel',
    save_create: 'Create Property Type',
    save_edit: 'Update Property Type',
  },
  messages: {
    create_success: 'Property Type created successfully.',
    update_success: 'Property Type updated successfully.',
    delete_success: 'Property Type deleted successfully.',
    empty_list: 'No property types found.',
    loading: 'Loading property types...',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  }
};
