import { post, get } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export interface ClassificationAssignment {
  taxonId: string
  name: string
  prettyName: string | null
  position: number
  isAssigned: boolean
}

interface ClassificationSyncItem {
  taxonId: string
  position: number
}

export class ProductClassificationApi {
  private static getBase(productId: string): string {
    return `${CATALOG}/products/${productId}/classifications`
  }

  static getClassifications(productId: string): Promise<Result<{ items: ClassificationAssignment[] }>> {
    return get<Result<{ items: ClassificationAssignment[] }>>(ProductClassificationApi.getBase(productId))
  }

  static syncClassifications(productId: string, items: ClassificationSyncItem[]): Promise<Result<void>> {
    return post<Result<void>>(`${ProductClassificationApi.getBase(productId)}/sync`, { items })
  }
}
