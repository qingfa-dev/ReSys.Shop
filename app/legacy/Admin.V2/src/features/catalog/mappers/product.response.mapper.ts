import type { ProductResponse } from '../types'
import type { CurrentUser } from '@/shared/types/user'

export class ProductResponseMapper {
  static fromApi(product: ProductResponse) {
    return product
  }

  static toCurrentUser(product: ProductResponse): Partial<CurrentUser> {
    return {
      id: product.id,
      name: product.name,
    }
  }
}
