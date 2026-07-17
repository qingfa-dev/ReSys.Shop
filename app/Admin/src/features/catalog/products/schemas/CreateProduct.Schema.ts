import { z } from 'zod'

export const CreateProductSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200, 'Name must not exceed 200 characters'),
  slug: z.string().min(1, 'Slug is required').max(200, 'Slug must not exceed 200 characters').regex(/^[a-z0-9-]+$/, 'Slug may only contain lowercase letters, numbers, and hyphens'),
  description: z.string().optional(),
  price: z.number().min(0, 'Price must be non-negative'),
  sku: z.string().max(100, 'SKU must not exceed 100 characters').optional(),
  availableOn: z.string().optional(),
  discontinueOn: z.string().optional(),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0, 'Weight must be non-negative').optional().nullable(),
  height: z.number().min(0, 'Height must be non-negative').optional().nullable(),
  width: z.number().min(0, 'Width must be non-negative').optional().nullable(),
  depth: z.number().min(0, 'Depth must be non-negative').optional().nullable(),
  metaTitle: z.string().max(60, 'Meta title must not exceed 60 characters').optional().nullable(),
  metaDescription: z.string().max(160, 'Meta description must not exceed 160 characters').optional().nullable(),
  metaKeywords: z.string().max(255, 'Meta keywords must not exceed 255 characters').optional().nullable(),
})

export type CreateProductParameters = z.infer<typeof CreateProductSchema>
