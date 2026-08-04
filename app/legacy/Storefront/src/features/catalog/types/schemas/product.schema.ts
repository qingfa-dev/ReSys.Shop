import { z } from 'zod'

export const ProductImageSchema = z.object({
  url: z.string().url('Invalid image URL'),
  alt: z.string().optional(),
})

export type ProductImage = z.infer<typeof ProductImageSchema>

export const fields = {
  id: {
    Required: z.string().uuid('Invalid ID format'),
    Optional: z.string().uuid('Invalid ID format').optional(),
  },
  name: {
    Required: z.string().min(1, 'Name is required').max(255, 'Name must be less than 255 characters'),
    Optional: z.string().max(255, 'Name must be less than 255 characters').optional(),
  },
  slug: {
    Required: z.string().min(1, 'Slug is required').max(255, 'Slug must be less than 255 characters'),
    Optional: z.string().max(255, 'Slug must be less than 255 characters').optional(),
  },
  description: {
    Required: z.string().min(1, 'Description is required'),
    Optional: z.string().optional(),
  },
  price: {
    Required: z.number().min(0, 'Price must be greater than 0'),
    Optional: z.number().min(0).optional(),
  },
  compareAtPrice: {
    Required: z.number().min(0).optional(),
    Optional: z.number().min(0).optional(),
  },
  images: {
    Required: z.array(z.union([z.string().url('Invalid image URL'), ProductImageSchema])).min(1, 'At least one image is required'),
    Optional: z.array(z.union([z.string().url('Invalid image URL'), ProductImageSchema])).optional(),
  },
  category: {
    Required: z.object({
      id: z.string().uuid(),
      name: z.string().min(1),
      slug: z.string().min(1),
      parentId: z.string().uuid().optional(),
      image: z.string().url().optional(),
    }),
    Optional: z.object({
      id: z.string().uuid(),
      name: z.string().min(1),
      slug: z.string().min(1),
      parentId: z.string().uuid().optional(),
      image: z.string().url().optional(),
    }).optional(),
  },
  tags: {
    Required: z.array(z.string()),
    Optional: z.array(z.string()).optional(),
  },
  variants: {
    Optional: z.array(z.object({
      id: z.string().uuid(),
      productId: z.string().uuid(),
      name: z.string(),
      sku: z.string(),
      price: z.number().min(0),
      options: z.array(z.object({
        name: z.string(),
        value: z.string(),
      })),
      inventory: z.object({
        quantity: z.number().int().min(0),
        trackQuantity: z.boolean(),
        allowBackorder: z.boolean(),
        lowStockThreshold: z.number().int().optional(),
      }),
    })).optional(),
  },
  inventory: {
    Required: z.object({
      quantity: z.number().int().min(0),
      trackQuantity: z.boolean(),
      allowBackorder: z.boolean(),
      lowStockThreshold: z.number().int().optional(),
    }),
    Optional: z.object({
      quantity: z.number().int().min(0),
      trackQuantity: z.boolean(),
      allowBackorder: z.boolean(),
      lowStockThreshold: z.number().int().optional(),
    }).optional(),
  },
  createdAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
  updatedAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
} as const

export const ProductFields = {
  Id: { Required: fields.id.Required, Optional: fields.id.Optional },
  Name: { Required: fields.name.Required, Optional: fields.name.Optional },
  Slug: { Required: fields.slug.Required, Optional: fields.slug.Optional },
  Description: { Required: fields.description.Required, Optional: fields.description.Optional },
  Price: { Required: fields.price.Required, Optional: fields.price.Optional },
  CompareAtPrice: { Required: fields.compareAtPrice.Required, Optional: fields.compareAtPrice.Optional },
  Images: { Required: fields.images.Required, Optional: fields.images.Optional },
  Category: { Required: fields.category.Required, Optional: fields.category.Optional },
  Tags: { Required: fields.tags.Required, Optional: fields.tags.Optional },
  Variants: { Optional: fields.variants.Optional },
  Inventory: { Required: fields.inventory.Required, Optional: fields.inventory.Optional },
  CreatedAt: { Required: fields.createdAt.Required, Optional: fields.createdAt.Optional },
  UpdatedAt: { Required: fields.updatedAt.Required, Optional: fields.updatedAt.Optional },
} as const

export const ProductSchema = z.object({
  id: fields.id.Required,
  name: fields.name.Required,
  slug: fields.slug.Required,
  description: fields.description.Required,
  price: fields.price.Required,
  compareAtPrice: fields.compareAtPrice.Optional,
  images: fields.images.Required,
  category: fields.category.Required,
  tags: fields.tags.Required,
  variants: fields.variants.Optional,
  inventory: fields.inventory.Required,
  createdAt: fields.createdAt.Required,
  updatedAt: fields.updatedAt.Required,
})

export type Product = z.infer<typeof ProductSchema>

export const CategorySchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1).max(255),
  slug: z.string().min(1).max(255),
  parentId: z.string().uuid().optional(),
  image: z.string().url().optional(),
})

export type Category = z.infer<typeof CategorySchema>

export const ProductInventorySchema = z.object({
  quantity: z.number().int().min(0),
  trackQuantity: z.boolean(),
  allowBackorder: z.boolean(),
  lowStockThreshold: z.number().int().optional(),
})

export type ProductInventory = z.infer<typeof ProductInventorySchema>

export const ProductVariantSchema = z.object({
  id: z.string().uuid(),
  productId: z.string().uuid(),
  name: z.string(),
  sku: z.string(),
  price: z.number().min(0),
  options: z.array(z.object({
    name: z.string(),
    value: z.string(),
  })),
  inventory: ProductInventorySchema,
})

export type ProductVariant = z.infer<typeof ProductVariantSchema>

export type VariantOption = { name: string; value: string }

export type ProductSchemaType = Product
export type CategorySchemaType = Category
export type ProductVariantSchemaType = ProductVariant
export type ProductInventorySchemaType = ProductInventory
