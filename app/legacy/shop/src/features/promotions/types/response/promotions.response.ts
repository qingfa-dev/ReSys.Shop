import type { Result } from '@/core/models/result'
import type { PromotionSchemaType, CouponSchemaType } from '../schemas'

export interface PromotionResponse extends PromotionSchemaType {}
export interface CouponResponse extends CouponSchemaType {}

export interface ValidateCouponResponse {
  valid: boolean
  coupon?: CouponResponse
  message?: string
}

export interface ApplyPromotionResponse {
  success: boolean
  promotion: PromotionResponse
  appliedAt: string
}

export interface GetActivePromotionsResponse {
  promotions: PromotionResponse[]
  totalCount: number
}

export interface GetPromotionByCodeResponse {
  promotion: PromotionResponse
}

export interface GetPromotionsHistoryResponse {
  promotions: PromotionResponse[]
  totalCount: number
}

export type PromotionListResponse = Result<PromotionResponse[]>
export type PromotionSingleResponse = Result<PromotionResponse>
export type CouponSingleResponse = Result<CouponResponse>