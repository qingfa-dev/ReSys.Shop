import type { StockItem, StockItemDetail } from './stock-item.response.type'

export interface StockItemModel extends StockItem {
  countAvailable: number
}

export interface StockItemDetailModel extends StockItemDetail {
  countAvailable: number
}

export function toStockItemModel(dto: StockItem): StockItemModel {
  return { ...dto, countAvailable: dto.countOnHand }
}
export function toStockItemDetailModel(dto: StockItemDetail): StockItemDetailModel {
  return { ...dto, countAvailable: dto.countOnHand }
}
