export const API_STOREFRONT = 'api/storefront'
export const API_STORE = 'api/store'

export const ENDPOINTS = {
  // Catalog
  products: `${API_STOREFRONT}/products`,
  productBySlug: (slug: string) => `${API_STOREFRONT}/products/${slug}`,
  productAvailability: `${API_STOREFRONT}/products/availability`,
  productRelated: `${API_STOREFRONT}/products/related`,
  productSimilar: `${API_STOREFRONT}/products/similar`,
  searchByImage: `${API_STOREFRONT}/search-by-image`,
  taxonomies: `${API_STOREFRONT}/taxonomies`,
  taxonomyById: (id: string) => `${API_STOREFRONT}/taxonomies/${id}`,
  taxons: `${API_STOREFRONT}/taxons`,
  taxonProducts: `${API_STOREFRONT}/taxons/products`,
  optionTypes: `${API_STOREFRONT}/option-types`,
  images: `${API_STOREFRONT}/images`,

  // Ordering
  cart: `${API_STOREFRONT}/cart`,
  cartItems: `${API_STOREFRONT}/cart/items`,
  cartItem: (id: string) => `${API_STOREFRONT}/cart/items/${id}`,
  cartEmpty: `${API_STOREFRONT}/cart/empty`,
  cartAssociate: `${API_STOREFRONT}/cart/associate`,
  cartShippingRate: `${API_STOREFRONT}/cart/shipping-rate`,
  cartValidate: `${API_STOREFRONT}/cart/validate`,
  cartCheckout: `${API_STOREFRONT}/cart/checkout`,
  orders: `${API_STOREFRONT}/orders`,
  orderById: (id: string) => `${API_STOREFRONT}/orders/${id}`,
  orderCancel: (id: string) => `${API_STOREFRONT}/orders/${id}/cancel`,

  // Identity
  authLoginPassword: `${API_STORE}/identity/auth/login/password`,
  authLoginExternal: `${API_STORE}/identity/auth/login/external`,
  authLoginProviders: `${API_STORE}/identity/auth/login/providers`,
  authRegister: `${API_STORE}/identity/auth/register`,
  authLogout: `${API_STORE}/identity/auth/logout`,
  sessions: `${API_STORE}/identity/auth/sessions`,
  sessionsRefresh: `${API_STORE}/identity/auth/sessions/refresh`,
  sessionById: (id: string) => `${API_STORE}/identity/auth/sessions/${id}`,
  passwordsForgot: `${API_STORE}/identity/passwords/forgot`,
  passwordsReset: `${API_STORE}/identity/passwords/reset`,
  passwordsChange: `${API_STORE}/identity/passwords/change`,
  emailsChange: `${API_STORE}/identity/emails/change`,
  emailsConfirm: `${API_STORE}/identity/emails/confirm`,
  emailsResend: `${API_STORE}/identity/emails/resend`,

  // Payment
  paymentMethods: `${API_STOREFRONT}/payment/methods`,
  paymentCreateIntent: `${API_STOREFRONT}/payment/create-intent`,
  paymentConfirm: (id: string) => `${API_STOREFRONT}/payment/confirm/${id}`,
  paymentSetupIntent: `${API_STOREFRONT}/payment/setup-intent`,

  // Shipping
  shippingMethods: `${API_STOREFRONT}/shipping/methods`,
  shippingCalculate: `${API_STOREFRONT}/shipping/calculate`,
  shippingRates: `${API_STOREFRONT}/shipping/rates`,

  // Inventory
  availability: (variantId: string) => `${API_STOREFRONT}/availability/${variantId}`,
  cartReserve: `${API_STOREFRONT}/cart/reserve`,
  cartReserveById: (id: string) => `${API_STOREFRONT}/cart/reserve/${id}`,
  // GET (status) and POST (reserve) share the same route on the backend.
  cartReserveStatus: `${API_STOREFRONT}/cart/reserve`,

  // Profile
  // NOTE: the backend store profile routes live under api/store/profiles/profiles
  // (the plan's bare api/store/profiles would 404). Set-default uses PUT address.
  profiles: `${API_STORE}/profiles/profiles`,
  addresses: `${API_STORE}/profiles/addresses`,
  addressById: (id: string) => `${API_STORE}/profiles/addresses/${id}`,
  wishlists: `${API_STORE}/profiles/wishlists`,
  wishlistById: (id: string) => `${API_STORE}/profiles/wishlists/${id}`,
  wishlistItems: (id: string) => `${API_STORE}/profiles/wishlists/${id}/items`,
  wishlistItem: (listId: string, itemId: string) => `${API_STORE}/profiles/wishlists/${listId}/items/${itemId}`,
  notificationPreferences: `${API_STORE}/profiles/notification-preferences`,

  // Location
  countries: `${API_STORE}/locations/countries`,
  countryByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/countries/${idOrIso}`,
  states: `${API_STORE}/locations/states`,
  stateByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/states/${idOrIso}`,
} as const
