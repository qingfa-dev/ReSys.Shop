import type { StockTransfer } from '../../types/StockTransfer.Response.Type'

export function mapStockTransfer<T extends StockTransfer>(data: T): T {
  return data
}
