export function variantsListUrl(productId: string | null | undefined): string {
  return productId
    ? `api/catalog/variants?productId=${productId}`
    : 'api/catalog/variants'
}
