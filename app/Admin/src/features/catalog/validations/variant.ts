import { z } from 'zod'

export const variantSku = z.string()
  .min(1, 'SKU is required.')
  .max(255, 'SKU must not exceed 255 characters.')
  .refine((s) => s.trim().length > 0, 'SKU is required.')

export const variantPosition = z.number()
  .int('Position must be an integer.')
  .min(-1, 'Position must be at least -1.')
  .default(0)

export const variantIsMaster = z.boolean().default(false)

export const variantTrackInventory = z.boolean().default(true)

export const variantWeight = z.number()
  .min(0, 'Weight must be at least 0.')
  .nullable().optional().default(null)

export const variantWeightUnit = z.string()
  .max(50, 'Weight unit must not exceed 50 characters.')
  .nullable().optional().default(null)

export const variantHeight = z.number()
  .min(0, 'Height must be at least 0.')
  .nullable().optional().default(null)

export const variantWidth = z.number()
  .min(0, 'Width must be at least 0.')
  .nullable().optional().default(null)

export const variantDepth = z.number()
  .min(0, 'Depth must be at least 0.')
  .nullable().optional().default(null)

export const variantDimensionsUnit = z.string()
  .max(50, 'Dimensions unit must not exceed 50 characters.')
  .nullable().optional().default(null)

export const variantPrice = z.number()
  .min(0, 'Price must be at least 0.')
  .nullable().optional().default(null)

export const variantCostPrice = z.number()
  .min(0, 'Cost price must be at least 0.')
  .nullable().optional().default(null)

export const variantCostCurrency = z.string()
  .max(3, 'Cost currency must be a 3-letter code.')
  .nullable().optional().default(null)

export const variantSchema = z.object({
  sku: variantSku,
  position: variantPosition,
  isMaster: variantIsMaster,
  trackInventory: variantTrackInventory,
  weight: variantWeight,
  weightUnit: variantWeightUnit,
  height: variantHeight,
  width: variantWidth,
  depth: variantDepth,
  dimensionsUnit: variantDimensionsUnit,
  price: variantPrice,
  costPrice: variantCostPrice,
  costCurrency: variantCostCurrency,
})

export type VariantForm = z.infer<typeof variantSchema>
