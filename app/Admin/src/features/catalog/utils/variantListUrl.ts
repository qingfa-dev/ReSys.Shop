export function variantsListUrl(productId: string | null | undefined): string {
  return productId
    ? `api/admin/catalog/variants?productId=${productId}`
    : 'api/admin/catalog/variants'
}
