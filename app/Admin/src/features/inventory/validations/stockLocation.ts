import { z } from 'zod'

export const stockLocationName = z.string()
  .min(1, 'Name is required.')
  .max(255, 'Name cannot exceed 255 characters.')

export const stockLocationCode = z.string()
  .optional()

export const stockLocationCity = z.string()
  .optional()

export const stockLocationPostalCode = z.string()
  .optional()

export const stockLocationPhone = z.string()
  .optional()

export const stockLocationPosition = z.number()
  .int()
  .min(0, 'Position must be at least 0.')

export const stockLocationActive = z.boolean()

export const stockLocationSchema = z.object({
  name: stockLocationName,
  code: stockLocationCode,
  city: stockLocationCity,
  postalCode: stockLocationPostalCode,
  phone: stockLocationPhone,
  position: stockLocationPosition,
  active: stockLocationActive,
})

export type StockLocationForm = z.infer<typeof stockLocationSchema>
