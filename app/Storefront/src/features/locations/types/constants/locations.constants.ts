export const LOCATIONS_ENDPOINTS = {
  LOCATIONS: '/api/store/profiles/addresses',
  LOCATION: (id: string) => `/api/store/profiles/addresses/${id}`,
  COUNTRIES: '/api/store/profiles/addresses/countries',
  REGIONS: '/api/store/profiles/addresses/regions',
  CITIES: '/api/store/profiles/addresses/cities',
} as const

export const ADDRESS_ENDPOINTS = {
  ADDRESSES: '/api/store/profiles/addresses',
  ADDRESS: (id: string) => `/api/store/profiles/addresses/${id}`,
  DEFAULT: '/api/store/profiles/addresses/default',
  SET_DEFAULT: (id: string) => `/api/store/profiles/addresses/${id}/default`,
} as const
