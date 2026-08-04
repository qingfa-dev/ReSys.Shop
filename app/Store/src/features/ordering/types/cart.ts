/**
 * Cart line item as returned by the storefront cart endpoints.
 * Mirrors the backend `CartItem` DTO
 * (service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.cs).
 */
export interface CartLineItem {
  /** Line item id (used for update-quantity / remove). */
  id: string
  /** The product variant added to the cart. */
  variantId: string
  /** Display name of the variant (backend maps it to the variant SKU). */
  variantName: string
  /** Stock-keeping unit for the variant. */
  sku: string
  /** Display name of the parent product. */
  productName: string | null
  /** Primary image URL of the product (may be null). */
  productImageUrl: string | null
  /** Number of units in this line item. */
  quantity: number
  /** Unit price at the time of addition. */
  price: number
  /** Computed total for this line item (price × quantity). */
  total: number
}

export interface CartResponse {
  id: string
  /** Sum of all line item totals before adjustments. */
  itemTotal: number
  /** Grand total after adjustments and shipping. */
  total: number
  /** ISO 4217 currency code for all monetary values. */
  currency: string
  /** Total number of line items in the cart. */
  itemCount: number
  /** Current checkout step (e.g. address, delivery, payment, confirm, complete). */
  checkoutState: string
  items: CartLineItem[]
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
