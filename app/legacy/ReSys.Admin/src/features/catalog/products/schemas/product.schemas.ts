import { z } from 'zod';

export const createProductSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200, 'Name is too long'),
  slug: z.string().min(1, 'Slug is required').max(200, 'Slug is too long')
    .regex(/^[a-z0-9-]+$/, 'Slug must contain only lowercase letters, numbers, and hyphens'),
  description: z.string().optional(),
  price: z.number().min(0, 'Price must be non-negative'),
  sku: z.string().optional(),
  is_active: z.boolean().default(true),
  is_visible: z.boolean().default(true),
  weight: z.number().min(0).optional().nullable(),
  height: z.number().min(0).optional().nullable(),
  width: z.number().min(0).optional().nullable(),
  depth: z.number().min(0).optional().nullable(),
  brand: z.string().optional().nullable(),
  meta_title: z.string().max(60, 'Meta title should be under 60 chars').optional().nullable(),
  meta_description: z.string().max(160, 'Meta description should be under 160 chars').optional().nullable(),
  meta_keywords: z.string().optional().nullable(),
});

export const updateProductSchema = createProductSchema.partial();

export type CreateProductInput = z.infer<typeof createProductSchema>;
export type UpdateProductInput = z.infer<typeof updateProductSchema>;
