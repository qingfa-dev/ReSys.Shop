import { get, post, patch, del } from '@/shared/api/client'
import { CartResponseSchema } from '../validations/cart'
import type { Result } from '@/shared/types'
import type { CartResponse, AddCartItemRequest, UpdateCartItemRequest } from '../types'

// Service: Cart API client with runtime response validation via Zod schemas.
export class CartApi {
  static async getCart(): Promise<Result<CartResponse>> {
    const result = await get<Result<CartResponse>>('/api/storefront/cart')
    if (!result.isSuccess) return result
    // Validate: Parse API response against CartResponseSchema for type safety.
    result.value = CartResponseSchema.parse(result.value) as CartResponse
    return result
  }

  static async addItem(req: AddCartItemRequest): Promise<Result<CartResponse>> {
    const result = await post<Result<CartResponse>>('/api/storefront/cart/items', req)
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value) as CartResponse
    return result
  }

  static async updateItem(lineItemId: string, req: UpdateCartItemRequest): Promise<Result<CartResponse>> {
    const result = await patch<Result<CartResponse>>(`/api/storefront/cart/items/${lineItemId}`, req)
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value) as CartResponse
    return result
  }

  static async removeItem(lineItemId: string): Promise<Result<CartResponse>> {
    const result = await del<Result<CartResponse>>(`/api/storefront/cart/items/${lineItemId}`)
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value) as CartResponse
    return result
  }

  // Call: Remove all items from the cart — backend maps to DELETE api/storefront/cart/items.
  static async emptyCart(): Promise<Result<null>> {
    return await del<Result<null>>('/api/storefront/cart/items')
  }

  // Call: Link guest cart to authenticated user — triggers server-side merge.
  static async associateCart(guestOrderId: string): Promise<Result<CartResponse>> {
    const result = await post<Result<CartResponse>>('/api/storefront/cart/associate', { guestOrderId })
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value) as CartResponse
    return result
  }
}
