import { z } from 'zod'

export const variantSchema = z.object({
  sku: z.string()
    .min(1, 'SKU is required')
    .max(255, 'SKU must not exceed 255 characters')
    .refine((s) => s.trim().length > 0, 'SKU is required'),
  position: z.number().int().min(-1).default(0),
  isMaster: z.boolean().default(false),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0).nullable().optional().default(null),
  weightUnit: z.string().nullable().optional().default(null),
  height: z.number().min(0).nullable().optional().default(null),
  width: z.number().min(0).nullable().optional().default(null),
  depth: z.number().min(0).nullable().optional().default(null),
  dimensionsUnit: z.string().nullable().optional().default(null),
  price: z.number().min(0).nullable().optional().default(null),
  costPrice: z.number().min(0).nullable().optional().default(null),
  costCurrency: z.string().max(3).nullable().optional().default(null),
})

export type VariantForm = z.infer<typeof variantSchema>
