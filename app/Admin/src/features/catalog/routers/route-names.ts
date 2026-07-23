export const ROUTE_CATALOG = {
  DASHBOARD: 'catalog.dashboard',
  PRODUCTS: {
    LIST: 'catalog.products.list',
    CREATE: 'catalog.products.create',
    VIEW: 'catalog.products.view',
    EDIT: 'catalog.products.edit',
  },
  TAXONOMIES: {
    LIST: 'catalog.taxonomies.list',
    CREATE: 'catalog.taxonomies.create',
    VIEW: 'catalog.taxonomies.view',
    EDIT: 'catalog.taxonomies.edit',
  },
  OPTION_TYPES: {
    LIST: 'catalog.option-types.list',
    CREATE: 'catalog.option-types.create',
    VIEW: 'catalog.option-types.view',
    EDIT: 'catalog.option-types.edit',
  },
} as const
