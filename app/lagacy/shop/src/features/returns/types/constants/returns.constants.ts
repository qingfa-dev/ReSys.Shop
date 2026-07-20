export const RETURNS_ENDPOINTS = {
  RETURNS: '/returns',
  RETURN: (id: string) => `/returns/${id}`,
  CREATE: '/returns',
  CANCEL: (id: string) => `/returns/${id}/cancel`,
  TRACK: (id: string) => `/returns/${id}/track`,
} as const