import { z } from 'zod'

export const VariantSchema = z.object({
  sku: z.string().min(1, 'SKU is required').max(100, 'SKU must not exceed 100 characters'),
  barcode: z.string().max(50, 'Barcode must not exceed 50 characters').optional(),
  price: z.number().min(0, 'Price must be non-negative').default(0),
  compareAtPrice: z.number().min(0, 'Compare-at price must be non-negative').optional().nullable(),
  costPrice: z.number().min(0, 'Cost price must be non-negative').optional().nullable(),
  position: z.number().int('Position must be a whole number').min(0, 'Position must be non-negative').default(0),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0, 'Weight must be non-negative').optional().nullable(),
  height: z.number().min(0, 'Height must be non-negative').optional().nullable(),
  width: z.number().min(0, 'Width must be non-negative').optional().nullable(),
  depth: z.number().min(0, 'Depth must be non-negative').optional().nullable(),
  optionValueIds: z.array(z.string().uuid()).optional(),
})

export type VariantParameters = z.infer<typeof VariantSchema>
