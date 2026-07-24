import type { FeatureLocales } from '@/shared/locales/locale.types';

export const optionValueLocales: FeatureLocales = {
  titles: {
    list: 'Option Values',
    create: 'Create Option Value',
    edit: 'Edit Option Value',
  },
  descriptions: {
    list: 'Manage specific values for all option types in one place.',
  },
  labels: {
    name: 'Value Name',
    presentation: 'Display Label',
    position: 'Position',
    option_type: 'Option Type',
  },
  placeholders: {
    name: 'e.g. small',
    presentation: 'e.g. Small',
    option_type: 'Select Option Type',
    search: 'Search values...',
  },
  table: {
    name: 'Name',
    presentation: 'Display Name',
    position: 'Pos',
    actions: 'Actions',
    clear_filter: 'Clear Filters',
  },
  messages: {
    create_success: 'Option value created successfully.',
    update_success: 'Option value updated successfully.',
    delete_success: 'Option value deleted successfully.',
    empty_list: 'No option values found.',
    loading: 'Loading values...',
  },
  actions: {
    delete: 'Delete',
    cancel: 'Cancel',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  }
};
