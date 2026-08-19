import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { CatalogDashboard } from '../types/catalogDashboard'

export class CatalogDashboardApi {
  static getCatalogDashboard(): Promise<Result<CatalogDashboard>> {
    return get<Result<CatalogDashboard>>('/api/admin/catalog/dashboard')
  }
}
