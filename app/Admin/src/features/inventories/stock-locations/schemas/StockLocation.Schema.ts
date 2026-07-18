import { z } from 'zod'

export const StockLocationSchema = z.object({
  name: z.string().min(1, 'Location name is required').max(100, 'Location name must not exceed 100 characters'),
  code: z.string().min(1, 'Location code is required').max(50, 'Location code must not exceed 50 characters').regex(/^[A-Z0-9_-]+$/, 'Code may contain only uppercase letters, numbers, hyphens, underscores'),
  type: z.number().int('Type must be a whole number').min(0, 'Type is required'),
  isDefault: z.boolean().default(false),
  active: z.boolean().default(true),
  address1: z.string().min(1, 'Address is required').max(200, 'Address must not exceed 200 characters'),
  address2: z.string().max(200, 'Address must not exceed 200 characters').optional(),
  city: z.string().min(1, 'City is required').max(100, 'City must not exceed 100 characters'),
  zipCode: z.string().min(1, 'ZIP code is required').max(20, 'ZIP code must not exceed 20 characters'),
  countryCode: z.string().length(2, 'Country code must be 2 characters'),
  stateCode: z.string().max(10, 'State code must not exceed 10 characters').optional(),
  phone: z.string().max(30, 'Phone must not exceed 30 characters').optional(),
  backorderableDefault: z.boolean().optional().default(false),
  propagateAllVariants: z.boolean().optional().default(false),
  lowStockThreshold: z.number().int().min(0).optional(),
  notifyOnLowStock: z.boolean().optional().default(false),
  position: z.number().int().min(0).optional().default(0),
})

export type StockLocationParameters = z.infer<typeof StockLocationSchema>
