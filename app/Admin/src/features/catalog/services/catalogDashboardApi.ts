import { get } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { CatalogDashboard } from '../types/catalogDashboard'

export class CatalogDashboardApi {
  private static readonly BASE = `${CATALOG}/dashboard`

  static getCatalogDashboard(): Promise<Result<CatalogDashboard>> {
    return get<Result<CatalogDashboard>>(CatalogDashboardApi.BASE)
  }
}
