import { z } from 'zod'

export const ShippingRateFields = {
  Required: {
    id: z.string(),
    name: z.string(),
    carrier: z.string(),
    price: z.number(),
    estimatedDays: z.number(),
    trackingEnabled: z.boolean(),
  },
  Optional: {
    description: z.string().optional(),
    restrictions: z.string().optional(),
  },
} as const

export const ShippingRateSchema = z.object({
  ...ShippingRateFields.Required,
  ...ShippingRateFields.Optional,
})

export type ShippingRateSchemaType = z.infer<typeof ShippingRateSchema>

export const ShipmentFields = {
  Required: {
    id: z.string(),
    orderId: z.string(),
    status: z.enum(['pending', 'label_created', 'in_transit', 'delivered', 'exception']),
  },
  Optional: {
    trackingNumber: z.string().optional(),
    trackingUrl: z.string().optional(),
    carrier: z.string().optional(),
    estimatedDelivery: z.string().optional(),
    deliveredAt: z.string().optional(),
  },
} as const

export const ShipmentSchema = z.object({
  ...ShipmentFields.Required,
  ...ShipmentFields.Optional,
})

export type ShipmentSchemaType = z.infer<typeof ShipmentSchema>