import { z } from 'zod'

export const InventoryItemFields = {
  Required: {
    id: z.string(),
    productId: z.string(),
    quantity: z.number(),
    reserved: z.number(),
    available: z.number(),
    warehouse: z.string(),
    lowStockThreshold: z.number(),
  },
  Optional: {
    lastUpdated: z.string().optional(),
    reorderLevel: z.number().optional(),
  },
} as const

export const InventoryItemSchema = z.object({
  ...InventoryItemFields.Required,
  ...InventoryItemFields.Optional,
})

export type InventoryItemSchemaType = z.infer<typeof InventoryItemSchema>

export const StockStatusFields = {
  Required: {
    inStock: z.boolean(),
    lowStock: z.boolean(),
    outOfStock: z.boolean(),
    quantity: z.number(),
  },
  Optional: {
    restockDate: z.string().optional(),
  },
} as const

export const StockStatusSchema = z.object({
  ...StockStatusFields.Required,
  ...StockStatusFields.Optional,
})

export type StockStatusSchemaType = z.infer<typeof StockStatusSchema>

export const InventoryOperationFields = {
  Required: {
    productId: z.string(),
    quantity: z.number(),
  },
  Optional: {
    reason: z.string().optional(),
    referenceId: z.string().optional(),
  },
} as const

export const ReserveStockSchema = z.object({
  ...InventoryOperationFields.Required,
  ...InventoryOperationFields.Optional,
})

export type ReserveStockSchemaType = z.infer<typeof ReserveStockSchema>

export const UpdateQuantitySchema = z.object({
  quantity: z.number().min(0),
  operation: z.enum(['set', 'increment', 'decrement']).optional(),
})

export type UpdateQuantitySchemaType = z.infer<typeof UpdateQuantitySchema>