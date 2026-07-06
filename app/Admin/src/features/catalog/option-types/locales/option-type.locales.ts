import type { FeatureLocales } from '@/shared/locales/locale.types'

export const optionTypeLocales: FeatureLocales = {
  titles: {
    list: 'Option Types',
    create: 'Create Option Type',
    edit: 'Edit Option Type',
    basic_info: 'Basic Information',
    values: 'Option Values',
  },
  descriptions: {
    list: 'Manage product options like Color, Size, etc.',
    create: 'Define a new option type for your catalog.',
    values: 'Add and manage specific values for this option type.',
  },
  labels: {
    name: 'Internal Name',
    presentation: 'Display Name',
    description: 'Description',
    position: 'Position',
    filterable: 'Filterable',
    value_name: 'Value Name',
    value_presentation: 'Display Label',
  },
  placeholders: {
    name: 'e.g. tshirt-size',
    presentation: 'e.g. Size',
    description: 'Internal notes...',
    value_name: 'e.g. small',
    value_presentation: 'e.g. Small',
  },
  table: {
    name: 'Name',
    presentation: 'Display Name',
    position: 'Pos',
    filterable: 'Filterable',
    actions: 'Actions',
  },
  actions: {
    create: 'New Option Type',
    edit: 'Edit',
    delete: 'Delete',
    cancel: 'Cancel',
    save_create: 'Create Option Type',
    save_edit: 'Update Option Type',
    add_value: 'Add Value',
  },
  messages: {
    create_success: 'Option Type created successfully.',
    update_success: 'Option Type updated successfully.',
    delete_success: 'Option Type deleted successfully.',
    value_create_success: 'Option value added.',
    value_update_success: 'Option value updated.',
    value_delete_success: 'Option value removed.',
    empty_list: 'No option types found.',
    loading: 'Loading option types...',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  },
}
