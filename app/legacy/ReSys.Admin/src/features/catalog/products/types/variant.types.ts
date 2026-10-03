export interface VariantOption {
  name: string;
  value: string;
}

export interface VariantSummary {
  id: string;
  product_id: string;
  sku: string | null;
  barcode: string | null;
  price: number;
  compare_at_price: number | null;
  cost_price: number | null;
  is_master: boolean;
  position: number;
  track_inventory: boolean;
  options: VariantOption[];
}

export interface VariantDetail extends VariantSummary {
  weight: number | null;
  height: number | null;
  width: number | null;
  depth: number | null;
  public_metadata: Record<string, any>;
  private_metadata: Record<string, any>;
  option_value_ids: string[];
}

export interface CreateVariantRequest {
  product_id?: string;
  sku: string;
  barcode?: string;
  price: number;
  compare_at_price?: number | null;
  cost_price?: number | null;
  position?: number;
  track_inventory?: boolean;
  weight?: number | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  option_value_ids?: string[];
}

export type UpdateVariantRequest = Partial<CreateVariantRequest>;
