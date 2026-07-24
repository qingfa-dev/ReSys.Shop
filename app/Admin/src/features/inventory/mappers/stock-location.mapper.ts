import type { CreateStockLocationForm, UpdateStockLocationForm } from '../schemas'
import type { CreateStockLocationRequest, UpdateStockLocationRequest } from '../types'

export class StockLocationFormMapper {
  static toCreate(form: CreateStockLocationForm): CreateStockLocationRequest { return form }
  static toUpdate(form: UpdateStockLocationForm): UpdateStockLocationRequest { return form }
}
