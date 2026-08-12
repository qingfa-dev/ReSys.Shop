import { z } from 'zod'

// Validate: Zod schemas mirror server API response contracts for runtime type safety
const StockLocationInfoSchema = z.object({
  stockLocationId: z.string(),
  stockLocationName: z.string().nullable(),
  countOnHand: z.number().int().min(0),
  reservedCount: z.number().int().min(0),
  availableCount: z.number().int().min(0),
  backorderable: z.boolean(),
})

const VariantStockInfoSchema = z.object({
  totalOnHand: z.number().int().min(0),
  totalReserved: z.number().int().min(0),
  totalAvailable: z.number().int().min(0),
  backorderable: z.boolean(),
  locations: z.array(StockLocationInfoSchema),
})

const VariantOptionValueSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
  optionTypeId: z.string(),
  optionTypeName: z.string().nullable(),
})

const ProductImageSchema = z.object({
  id: z.string(),
  url: z.string(),
  alt: z.string().nullable(),
  position: z.number().int().min(0),
})

const VariantPriceSchema = z.object({
  id: z.string(),
  amount: z.number().nullable(),
  currency: z.string(),
  compareAtAmount: z.number().nullable(),
  countryIso: z.string().nullable(),
})

const ProductVariantSchema = z.object({
  id: z.string(),
  sku: z.string().nullable(),
  isMaster: z.boolean(),
  price: z.number().nullable(),
  currency: z.string().nullable(),
  optionValues: z.array(VariantOptionValueSchema),
  images: z.array(ProductImageSchema),
  prices: z.array(VariantPriceSchema),
  stock: VariantStockInfoSchema,
})

export const ProductListItemSchema = z.object({
  id: z.string(),
  masterVariantId: z.string(),
  name: z.string(),
  status: z.string(),
  description: z.string().nullable(),
  slug: z.string(),
  styleCode: z.string().nullable(),
  seasonName: z.string().nullable(),
  materialComposition: z.string().nullable(),
  careInstructions: z.string().nullable(),
  fitNotes: z.string().nullable(),
  department: z.string().nullable(),
  genderTarget: z.string().nullable(),
  variantsCount: z.number().int().min(0),
  availableOn: z.string().nullable(),
  masterVariant: ProductVariantSchema.nullable(),
  classifications: z.array(z.any()),
})

export const ProductDetailSchema = ProductListItemSchema.extend({
  variants: z.array(ProductVariantSchema),
})

// Validate: Search form — restricts sort to known fields to prevent injection
export const ProductSearchFormSchema = z.object({
  search: z.string().optional(),
  sort: z.enum(['-CreatedAtUtc', 'Price', '-Price', 'Name', '-Name']).optional(),
})

export type ProductSearchForm = z.infer<typeof ProductSearchFormSchema>
