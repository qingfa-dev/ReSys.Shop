import type { ProductSummary, ProductDetail } from '../types/product.response'
import { ProductStatusMap } from '@/shared/utils/enums'

export interface ProductSummaryModel extends ProductSummary {
  statusLabel: string
}

export interface ProductDetailModel extends ProductDetail {
  statusLabel: string
}

export function toProductSummaryModel(dto: ProductSummary): ProductSummaryModel {
  return { ...dto, statusLabel: ProductStatusMap[dto.status] ?? 'Unknown' }
}

export function toProductDetailModel(dto: ProductDetail): ProductDetailModel {
  return { ...dto, statusLabel: ProductStatusMap[dto.status] ?? 'Unknown' }
}
