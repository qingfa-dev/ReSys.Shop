export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface Promotion {
  id: string
  code: string
  type: 'percentage' | 'fixed' | 'bogo' | 'shipping'
  value: number
  minOrderAmount?: number
  maxUses?: number
  usedCount: number
  startsAt: string
  expiresAt: string
  isActive: boolean
}

export interface Coupon {
  code: string
  description: string
  discount: string
  expiresAt?: string
}
