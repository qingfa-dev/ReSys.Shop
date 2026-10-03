export interface RecentMovementData {
  id: string
  variantSku: string
  locationName: string
  quantity: number
  direction: string
  createdAt: string
}

export interface InventoryDashboardResponse {
  totalStockItems: number
  totalLocations: number
  lowStockCount: number
  outOfStockCount: number
  totalReservedQuantity: number
  totalTransfersPending: number
  recentMovements: RecentMovementData[]
}
