export const STOREFRONT = 'api/storefront'
export const STORE = 'api/store'
export const CATALOG = `${STOREFRONT}`
export const IDENTITY = `${STORE}/identity`
export const PROFILES = `${STORE}/profiles`
export const LOCATIONS = `${STORE}/locations`
export const ORDERS = `${STOREFRONT}/orders`
export const CART = `${STOREFRONT}/cart`
export const PAYMENT = `${STOREFRONT}/payment`
export const SHIPPING = `${STOREFRONT}/shipping`
export const AVAILABILITY = `${STOREFRONT}/availability`
// Context: Backend — GET /api/storefront/taxonomies returns paged taxonomy list
export const TAXONOMIES = `${STOREFRONT}/taxonomies`
// Context: Backend — GET /api/storefront/taxons/all returns flat paged taxon list
export const TAXONS_ALL = `${STOREFRONT}/taxons/all`

export const ENDPOINTS = {
  availability: (variantId: string) => `${AVAILABILITY}/${variantId}`,
  cartReserve: `${CART}/reserve`,
  cartReserveById: (id: string) => `${CART}/reserve/${id}`,
  cartReserveStatus: `${CART}/reserve`,
  paymentMethods: `${PAYMENT}/methods`,
  paymentConfirm: (paymentId: string) => `${PAYMENT}/confirm/${paymentId}`,
  paymentSetupIntent: `${PAYMENT}/setup-intent`,
  paymentCreateIntent: `${PAYMENT}/create-intent`,
  shippingMethods: `${SHIPPING}/methods`,
  shippingRates: `${SHIPPING}/rates`,
  shippingCalculate: `${SHIPPING}/calculate`,
  countries: `${LOCATIONS}/countries`,
  states: `${LOCATIONS}/states`,
  sessionsRefresh: `${IDENTITY}/auth/sessions/refresh`,
  optionValues: `${CATALOG}/option-values`,
} as const
