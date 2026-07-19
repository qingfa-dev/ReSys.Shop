import { z } from 'zod'

export function createStockLocationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('inventory.validation.name.required')).max(100, t('inventory.validation.name.max_length')),
  code: z.string().min(1, t('inventory.validation.code.required')).max(50, t('inventory.validation.code.max_length')).regex(/^[A-Z0-9_-]+$/, t('inventory.validation.code.format')),
  type: z.number().int('Type must be a whole number').min(0, t('inventory.validation.type.required')),
  isDefault: z.boolean().default(false),
  active: z.boolean().default(true),
  address1: z.string().min(1, t('inventory.validation.address.required')).max(200, t('inventory.validation.address.max_length')),
  address2: z.string().max(200, t('inventory.validation.address.max_length')).optional(),
  city: z.string().min(1, t('inventory.validation.city.required')).max(100, t('inventory.validation.city.max_length')),
  zipCode: z.string().min(1, t('inventory.validation.zip.required')).max(20, t('inventory.validation.zip.max_length')),
  countryCode: z.string().length(2, t('inventory.validation.country_code.length')),
  stateCode: z.string().max(10, t('inventory.validation.state_code.max_length')).optional(),
  phone: z.string().max(30, t('inventory.validation.phone.max_length')).optional(),
  backorderableDefault: z.boolean().optional().default(false),
  propagateAllVariants: z.boolean().optional().default(false),
  lowStockThreshold: z.number().int().min(0).optional(),
  notifyOnLowStock: z.boolean().optional().default(false),
  position: z.number().int().min(0).optional().default(0),
})
}

export type StockLocationParameters = z.infer<ReturnType<typeof createStockLocationSchema>>
