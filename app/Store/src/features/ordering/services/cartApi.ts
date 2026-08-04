import { get, post, put, del } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { CartResponse, AddCartItemRequest, UpdateCartItemRequest } from '../types/cart'

export function getCart(): Promise<Result<CartResponse>> {
  return get<Result<CartResponse>>(ENDPOINTS.cart)
}

export function addItem(req: AddCartItemRequest): Promise<Result<CartResponse>> {
  return post<Result<CartResponse>>(ENDPOINTS.cartItems, req)
}

export function updateItem(lineItemId: string, req: UpdateCartItemRequest): Promise<Result<CartResponse>> {
  return put<Result<CartResponse>>(ENDPOINTS.cartItem(lineItemId), req)
}

export function removeItem(lineItemId: string): Promise<Result<CartResponse>> {
  return del<Result<CartResponse>>(ENDPOINTS.cartItem(lineItemId))
}

export function emptyCart(): Promise<Result<null>> {
  return post<Result<null>>(ENDPOINTS.cartEmpty)
}

export function associateCart(): Promise<Result<CartResponse>> {
  return post<Result<CartResponse>>(ENDPOINTS.cartAssociate)
}
