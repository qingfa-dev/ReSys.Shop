export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface InventoryItem {
  id: string
  productId: string
  quantity: number
  reserved: number
  available: number
  warehouse: string
  lowStockThreshold: number
}

export interface StockStatus {
  inStock: boolean
  lowStock: boolean
  outOfStock: boolean
  quantity: number
}
