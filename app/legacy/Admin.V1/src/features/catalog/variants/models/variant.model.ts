import type { VariantSummary, VariantDetail } from './variant.response'

export interface VariantSummaryModel extends VariantSummary {
  priceDisplay: string
}

export interface VariantDetailModel extends VariantDetail {
  priceDisplay: string
}
