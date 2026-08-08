import { z } from 'zod'

// Validate: Wishlist summary for list views
export const WishlistListItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  isPrivate: z.boolean(),
  itemCount: z.number().int().min(0),
})

// Validate: Single wished item within a wishlist detail
export const WishedItemSchema = z.object({
  id: z.string(),
  variantId: z.string(),
  quantity: z.number().int().min(1),
  addedAtUtc: z.string(),
})

// Validate: Full wishlist detail including item array
export const WishlistDetailSchema = z.object({
  id: z.string(),
  name: z.string(),
  isPrivate: z.boolean(),
  itemCount: z.number().int().min(0),
  token: z.string(),
  isDefault: z.boolean(),
  wishedItems: z.array(WishedItemSchema),
})

// Enforce: Name required, privacy flag mandatory for creation
export const CreateWishlistRequestSchema = z.object({
  name: z.string().min(1).max(200),
  isPrivate: z.boolean(),
})

// Enforce: All fields optional for partial update; name has min/max when provided
export const UpdateWishlistRequestSchema = z.object({
  name: z.string().min(1).max(200).optional(),
  isPrivate: z.boolean().optional(),
  isDefault: z.boolean().optional(),
})

// Enforce: Quantity must be at least 1
export const AddWishlistItemRequestSchema = z.object({
  variantId: z.string(),
  quantity: z.number().int().min(1),
})
