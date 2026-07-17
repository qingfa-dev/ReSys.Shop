export interface CreateVariantRequest {
  productId?: string;
  sku: string;
  barcode?: string;
  price: number;
  compareAtPrice?: number | null;
  costPrice?: number | null;
  position?: number;
  trackInventory?: boolean;
  weight?: number | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  optionValueIds?: string[];
}

export type UpdateVariantRequest = Partial<CreateVariantRequest>;
