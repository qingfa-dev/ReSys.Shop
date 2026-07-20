export interface ValidateCouponRequest {
  code: string
  orderTotal?: number
}

export interface ApplyPromotionRequest {
  code: string
  orderId: string
}

export interface CreatePromotionRequest {
  code: string
  type: 'percentage' | 'fixed' | 'bogo' | 'shipping'
  value: number
  minOrderAmount?: number
  maxUses?: number
  startsAt: string
  expiresAt: string
  isActive?: boolean
}

export interface UpdatePromotionRequest {
  code?: string
  type?: 'percentage' | 'fixed' | 'bogo' | 'shipping'
  value?: number
  minOrderAmount?: number
  maxUses?: number
  startsAt?: string
  expiresAt?: string
  isActive?: boolean
}