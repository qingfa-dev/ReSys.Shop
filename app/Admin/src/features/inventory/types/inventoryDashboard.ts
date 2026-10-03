export interface RecentMovementData {
  id: string
  quantity: number
  action: string | null
  reason: string | null
  createdAtUtc: string
}

export interface InventoryDashboard {
  totalSkusTracked: number
  inStockCount: number
  outOfStockCount: number
  lowStockCount: number
  stockLocationCount: number
  itemsPerLocationAverage: number
  recentMovements: RecentMovementData[]
}
