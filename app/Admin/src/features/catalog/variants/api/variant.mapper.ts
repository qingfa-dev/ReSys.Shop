import type { VariantSummary, VariantDetail } from '../models/variant.response'
import { decimalToDisplay } from '@/shared/utils/currency'

export function mapToSummary(dto: Record<string, unknown>): VariantSummary {
  return {
    id: String(dto.id ?? ''),
    productId: String(dto.productId ?? ''),
    sku: dto.sku as string | null ?? null,
    price: Number(dto.price ?? 0),
    costPrice: dto.costPrice as number | null ?? null,
    costCurrency: String(dto.costCurrency ?? ''),
    isMaster: Boolean(dto.isMaster),
    position: Number(dto.position ?? 0),
    trackInventory: Boolean(dto.trackInventory),
    weightUnit: String(dto.weightUnit ?? ''),
    dimensionsUnit: String(dto.dimensionsUnit ?? ''),
  }
}

export function mapToDetail(dto: Record<string, unknown>): VariantDetail {
  return {
    ...mapToSummary(dto),
    weight: dto.weight as number | null ?? null,
    height: dto.height as number | null ?? null,
    width: dto.width as number | null ?? null,
    depth: dto.depth as number | null ?? null,
    pricesCount: Number(dto.pricesCount ?? 0),
    discontinuedOn: dto.discontinuedOn as string | null ?? null,
  }
}

export function mapToSummaryModel(dto: Record<string, unknown>) {
  const summary = mapToSummary(dto)
  return { ...summary, priceDisplay: decimalToDisplay(summary.price) }
}

export function mapToDetailModel(dto: Record<string, unknown>) {
  const detail = mapToDetail(dto)
  return { ...detail, priceDisplay: decimalToDisplay(detail.price) }
}
