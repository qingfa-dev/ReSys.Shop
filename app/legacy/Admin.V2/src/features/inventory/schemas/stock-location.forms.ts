import { z } from 'zod'
import type { TFunction } from './stock-location.fields'
import { StockLocationFields } from './stock-location.fields'

export class StockLocationForms {
  private f: StockLocationFields
  constructor(private t: TFunction) { this.f = new StockLocationFields(t) }
  create() { return z.object({ name: this.f.name(), code: this.f.code(), address1: this.f.address1(), address2: this.f.address2(), city: this.f.city(), state: this.f.state(), postalCode: this.f.postalCode(), country: this.f.country(), phone: this.f.phone(), isDefault: this.f.isDefault(), isActive: this.f.isActive() }) }
  update() { return this.create() }
}
export type CreateStockLocationForm = z.input<ReturnType<StockLocationForms['create']>>
export type UpdateStockLocationForm = z.input<ReturnType<StockLocationForms['update']>>
