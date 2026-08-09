import { get, post, put, del } from '@/shared/api/client'
import { CartResponseSchema } from '../validations/cart'
import type { Result } from '@/shared/types'
import type { CartResponse, AddCartItemRequest, UpdateCartItemRequest } from '../types'

// Service: Cart API client with runtime response validation via Zod schemas.
export class CartApi {
  private static readonly BASE = '/api/storefront/ordering/cart'

  static async getCart(): Promise<Result<CartResponse>> {
    const result = await get<Result<CartResponse>>(this.BASE)
    if (!result.isSuccess) return result
    // Validate: Parse API response against CartResponseSchema for type safety.
    result.value = CartResponseSchema.parse(result.value)
    return result
  }

  static async addItem(req: AddCartItemRequest): Promise<Result<CartResponse>> {
    const result = await post<Result<CartResponse>>(`${this.BASE}/items`, req)
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value)
    return result
  }

  static async updateItem(lineItemId: string, req: UpdateCartItemRequest): Promise<Result<CartResponse>> {
    const result = await put<Result<CartResponse>>(`${this.BASE}/items/${lineItemId}`, req)
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value)
    return result
  }

  static async removeItem(lineItemId: string): Promise<Result<CartResponse>> {
    const result = await del<Result<CartResponse>>(`${this.BASE}/items/${lineItemId}`)
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value)
    return result
  }

  static async emptyCart(): Promise<Result<null>> {
    return await post<Result<null>>(`${this.BASE}/empty`)
  }

  // Call: Link guest cart to authenticated user — triggers server-side merge.
  static async associateCart(guestOrderId: string): Promise<Result<CartResponse>> {
    const result = await post<Result<CartResponse>>(`${this.BASE}/associate`, { guestOrderId })
    if (!result.isSuccess) return result
    result.value = CartResponseSchema.parse(result.value)
    return result
  }
}
