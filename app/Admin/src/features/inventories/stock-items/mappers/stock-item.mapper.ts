import type { StockItem } from '../../types/StockItem.Response.Type'

export function mapStockItem<T extends StockItem>(data: T): T {
  return data
}
