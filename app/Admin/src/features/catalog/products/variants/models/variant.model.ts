import type { VariantSummary, VariantDetail } from '../types/variant.response'
import { decimalToDisplay } from '@/shared/utils/currency'

export interface VariantSummaryModel extends VariantSummary {
  priceDisplay: string
}

export interface VariantDetailModel extends VariantDetail {
  priceDisplay: string
}

export function toVariantSummaryModel(dto: VariantSummary): VariantSummaryModel {
  return { ...dto, priceDisplay: decimalToDisplay(dto.price) }
}

export function toVariantDetailModel(dto: VariantDetail): VariantDetailModel {
  return { ...dto, priceDisplay: decimalToDisplay(dto.price) }
}
