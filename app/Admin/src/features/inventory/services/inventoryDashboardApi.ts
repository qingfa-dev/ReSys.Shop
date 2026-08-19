import { get } from '@/shared/api/client'

import type { Result } from '@/shared/types'
import type { InventoryDashboard } from '../types/inventoryDashboard'

export class InventoryDashboardApi {
  static getInventoryDashboard(): Promise<Result<InventoryDashboard>> {
    return get<Result<InventoryDashboard>>('/api/admin/inventory/dashboard')
  }
}
