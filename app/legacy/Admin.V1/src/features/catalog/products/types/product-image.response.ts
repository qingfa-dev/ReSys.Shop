export interface ProductImage {
  id: string; productId: string; variantId: string | null; url: string
  alt: string | null; position: number; role: number; fileSize: number | null
  width: number | null; height: number | null; isDefault: boolean
}
