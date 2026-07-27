import { z } from 'zod'
import type { TFunction } from './stock-location.fields'
import { StockItemFields } from './stock-item.fields'

export class StockItemForms {
  private f: StockItemFields
  constructor(private t: TFunction) { this.f = new StockItemFields(t) }
  create() { return z.object({ variantId: this.f.variantId(), locationId: this.f.locationId(), quantity: this.f.quantity(), lowStockThreshold: this.f.lowStockThreshold() }) }
  update() { return z.object({ variantId: this.f.variantId(), locationId: this.f.locationId(), quantity: this.f.quantity(), lowStockThreshold: this.f.lowStockThreshold() }) }
}
export type CreateStockItemForm = z.input<ReturnType<StockItemForms['create']>>
export type UpdateStockItemForm = z.input<ReturnType<StockItemForms['update']>>
