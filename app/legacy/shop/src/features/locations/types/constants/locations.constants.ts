export const LOCATIONS_ENDPOINTS = {
  LOCATIONS: '/locations',
  LOCATION: (id: string) => `/locations/${id}`,
  COUNTRIES: '/locations/countries',
  REGIONS: '/locations/regions',
  CITIES: '/locations/cities',
} as const

export const ADDRESS_ENDPOINTS = {
  ADDRESSES: '/locations/addresses',
  ADDRESS: (id: string) => `/locations/addresses/${id}`,
  DEFAULT: '/locations/addresses/default',
  SET_DEFAULT: (id: string) => `/locations/addresses/${id}/default`,
} as const