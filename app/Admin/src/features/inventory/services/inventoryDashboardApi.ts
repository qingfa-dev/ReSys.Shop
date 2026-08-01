import { get } from '@/shared/api/client'
import { INVENTORY } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { InventoryDashboard } from '../types/inventoryDashboard'

export class InventoryDashboardApi {
  private static readonly BASE = `${INVENTORY}/dashboard`

  static getInventoryDashboard(): Promise<Result<InventoryDashboard>> {
    return get<Result<InventoryDashboard>>(InventoryDashboardApi.BASE)
  }
}
