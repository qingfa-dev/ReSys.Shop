import type { VariantImage } from '../models/image.response'

export const ImageMapper = {
  toImage(dto: Record<string, unknown>): VariantImage {
    return {
      id: String(dto.id ?? ''),
      variantId: String(dto.variantId ?? ''),
      url: String(dto.url ?? ''),
      alt: dto.alt as string | null,
      position: Number(dto.position ?? 0),
      role: Number(dto.role ?? 0),
      fileSize: dto.fileSize as number | null,
      isDefault: Boolean(dto.isDefault),
    }
  },
}
