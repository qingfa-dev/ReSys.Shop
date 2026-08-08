import { z } from 'zod'

export const WishlistListItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  isPrivate: z.boolean(),
  itemCount: z.number().int().min(0),
})

export const WishedItemSchema = z.object({
  id: z.string(),
  variantId: z.string(),
  quantity: z.number().int().min(1),
  addedAtUtc: z.string(),
})

export const WishlistDetailSchema = z.object({
  id: z.string(),
  name: z.string(),
  isPrivate: z.boolean(),
  itemCount: z.number().int().min(0),
  token: z.string(),
  isDefault: z.boolean(),
  wishedItems: z.array(WishedItemSchema),
})

export const CreateWishlistRequestSchema = z.object({
  name: z.string().min(1).max(200),
  isPrivate: z.boolean(),
})

export const UpdateWishlistRequestSchema = z.object({
  name: z.string().min(1).max(200).optional(),
  isPrivate: z.boolean().optional(),
  isDefault: z.boolean().optional(),
})

export const AddWishlistItemRequestSchema = z.object({
  variantId: z.string(),
  quantity: z.number().int().min(1),
})
