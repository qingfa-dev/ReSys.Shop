import type { StockLocation } from '../types/StockLocation.Response.Type'

export function mapStockLocation<T extends StockLocation>(data: T): T {
  return data
}
