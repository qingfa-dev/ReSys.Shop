import type { ProductClassification } from "../models/classification.response";

export const ClassificationMapper = {
  toClassification(dto: Record<string, unknown>): ProductClassification {
    return {
      id: String(dto.id ?? ""),
      productId: String(dto.productId ?? ""),
      taxonId: String(dto.taxonId ?? ""),
      position: Number(dto.position ?? 0),
      isAutomatic: Boolean(dto.isAutomatic),
      isMain: Boolean(dto.isMain),
      taxonName: dto.taxonName as string | undefined,
      taxonomyName: dto.taxonomyName as string | undefined,
    };
  },
};
