import { post, get } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export interface OptionTypeAssignment {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

interface OptionTypeSyncItem {
  optionTypeId: string
  position: number
}

export class ProductOptionTypeApi {
  private static getBase(productId: string): string {
    return `${CATALOG}/products/${productId}/option-types`
  }

  static getOptionTypes(productId: string): Promise<Result<{ items: OptionTypeAssignment[] }>> {
    return get<Result<{ items: OptionTypeAssignment[] }>>(ProductOptionTypeApi.getBase(productId))
  }

  static syncOptionTypes(productId: string, items: OptionTypeSyncItem[]): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.getBase(productId)}/sync`, { items })
  }
}
