import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { CatalogDashboard } from '../types/catalogDashboard'

export class CatalogDashboardApi {
  private static readonly BASE = 'api/admin/catalog/dashboard'

  static getCatalogDashboard(): Promise<Result<CatalogDashboard>> {
    return get<Result<CatalogDashboard>>(CatalogDashboardApi.BASE)
  }
}
