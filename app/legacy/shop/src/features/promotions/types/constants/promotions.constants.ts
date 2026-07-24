export const PROMOTIONS_ENDPOINTS = {
  PROMOTIONS: '/promotions',
  PROMOTION: (id: string) => `/promotions/${id}`,
  ACTIVE: '/promotions/active',
  VALIDATE: '/promotions/validate',
} as const

export const CAMPAIGN_ENDPOINTS = {
  CAMPAIGNS: '/promotions/campaigns',
  CAMPAIGN: (id: string) => `/promotions/campaigns/${id}`,
  ACTIVE: '/promotions/campaigns/active',
} as const