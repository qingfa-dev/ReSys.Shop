import { z } from 'zod'
import type { TFunction } from './variant.fields'
import { VariantFields } from './variant.fields'
export class VariantForms {
  private f: VariantFields
  constructor(private t: TFunction) { this.f = new VariantFields(t) }
  create() { return z.object({ sku: this.f.sku(), position: this.f.position(), trackInventory: this.f.trackInventory(), weight: this.f.weight(), weightUnit: this.f.weightUnit(), height: this.f.height(), width: this.f.width(), depth: this.f.depth(), dimensionsUnit: this.f.dimensionsUnit(), price: this.f.price(), costPrice: this.f.costPrice(), costCurrency: this.f.costCurrency(), isMaster: this.f.isMaster() }) }
  update() { return this.create() }
}
export type CreateVariantForm = z.input<ReturnType<VariantForms['create']>>
export type UpdateVariantForm = z.input<ReturnType<VariantForms['update']>>
