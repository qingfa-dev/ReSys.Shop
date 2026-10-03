import type { CreateStockItemForm, UpdateStockItemForm } from '../schemas'
import type { CreateStockItemRequest, UpdateStockItemRequest } from '../types'

export class StockItemFormMapper {
  static toCreate(form: CreateStockItemForm): CreateStockItemRequest { return form }
  static toUpdate(form: UpdateStockItemForm): UpdateStockItemRequest { return form }
}
