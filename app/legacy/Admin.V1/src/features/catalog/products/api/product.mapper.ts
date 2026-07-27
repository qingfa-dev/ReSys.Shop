import type { ProductSummary, ProductDetail } from '../models/product.response'
import { ProductStatusMap } from '@/shared/utils/enums'

export const ProductMapper = {
  toSummary(dto: Record<string, unknown>): ProductSummary {
    return {
      id: String(dto.id ?? ''),
      name: String(dto.name ?? ''),
      slug: String(dto.slug ?? ''),
      description: dto.description as string | null ?? null,
      masterVariantId: String(dto.masterVariantId ?? ''),
      status: Number(dto.status ?? 0),
      availableOn: dto.availableOn as string | null ?? null,
      discontinueOn: dto.discontinueOn as string | null ?? null,
      trackInventory: Boolean(dto.trackInventory),
      variantsCount: Number(dto.variantsCount ?? 0),
      createdAtUtc: String(dto.createdAtUtc ?? ''),
      modifiedAtUtc: dto.modifiedAtUtc as string | null ?? null,
    }
  },

  toDetail(dto: Record<string, unknown>): ProductDetail {
    return {
      ...this.toSummary(dto),
      metaTitle: dto.metaTitle as string | null ?? null,
      metaDescription: dto.metaDescription as string | null ?? null,
      metaKeywords: dto.metaKeywords as string | null ?? null,
    }
  },

  toSummaryModel(dto: Record<string, unknown>) {
    const summary = this.toSummary(dto)
    return { ...summary, statusLabel: ProductStatusMap[summary.status] ?? 'Unknown' }
  },

  toDetailModel(dto: Record<string, unknown>) {
    const detail = this.toDetail(dto)
    return { ...detail, statusLabel: ProductStatusMap[detail.status] ?? 'Unknown' }
  },
}
