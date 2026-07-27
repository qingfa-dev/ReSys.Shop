export const ROUTES = {
  LOGIN: '/login',
  DASHBOARD: '/',
  CATALOG: {
    DASHBOARD: '/catalog',
    PRODUCTS: '/catalog/products',
    PRODUCT_CREATE: '/catalog/products/create',
    PRODUCT_DETAIL: '/catalog/products/:id',
    TAXA: '/catalog/taxa',
    OPTION_TYPES: '/catalog/option-types',
    OPTION_VALUES: '/catalog/option-values',
  },
  INVENTORY: {
    DASHBOARD: '/inventory',
    STOCK: '/inventory/stock',
    LOCATIONS: '/inventory/locations',
    MOVEMENTS: '/inventory/movements',
    TRANSFERS: '/inventory/transfers',
    UNITS: '/inventory/units',
  },
  LOCATION: {
    COUNTRIES: '/location/countries',
    STATES: '/location/states',
  },
  ORDERING: {
    DASHBOARD: '/ordering',
    ORDERS: '/ordering/orders',
    ORDER_CREATE: '/ordering/orders/create',
    ORDER_DETAIL: '/ordering/orders/:id',
    FULFILLMENT: '/ordering/fulfillment',
  },
  PAYMENT: {
    PAYMENTS: '/payment/payments',
    METHODS: '/payment/methods',
  },
  SHIPPING: {
    METHODS: '/shipping/methods',
    RATES: '/shipping/rates',
  },
  PROFILE: {
    PROFILE: '/profile',
    ADDRESSES: '/profile/addresses',
  },
  USERS: {
    STAFF: '/users/staff',
    STAFF_CREATE: '/users/staff/create',
    CUSTOMERS: '/users/customers',
    ROLES: '/users/roles',
    PERMISSIONS: '/users/permissions',
  },
  REPORTS: {
    DASHBOARD: '/reports',
  },
} as const
