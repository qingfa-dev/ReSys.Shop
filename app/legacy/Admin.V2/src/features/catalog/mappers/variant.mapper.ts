import type { CreateVariantForm, UpdateVariantForm } from '../schemas'
import type { VariantRequest } from '../types'
export class VariantFormMapper {
  static toCreate(form: CreateVariantForm): VariantRequest { return { sku: form.sku, position: form.position, trackInventory: form.trackInventory ?? true, weight: form.weight ?? null, weightUnit: form.weightUnit ?? null, height: form.height ?? null, width: form.width ?? null, depth: form.depth ?? null, dimensionsUnit: form.dimensionsUnit ?? null, price: form.price ?? null, costPrice: form.costPrice ?? null, costCurrency: form.costCurrency ?? null, isMaster: form.isMaster ?? false } }
  static toUpdate(form: UpdateVariantForm): VariantRequest { return VariantFormMapper.toCreate(form) }
}
