import { z } from 'zod'

export const productName = z.string()
  .min(1, 'Product name is required.')
  .max(255, 'Product name must not exceed 255 characters.')

export const productSlug = z.string()
  .min(1, 'Slug is required.')
  .max(255, 'Slug must not exceed 255 characters.')
  .regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, 'Slug must be lowercase alphanumeric with hyphens.')

export const productDescription = z.string()
  .max(2000, 'Description must not exceed 2000 characters.')
  .nullable()
  .optional()

export const productMetaTitle = z.string()
  .max(100, 'Meta title must not exceed 100 characters.')
  .nullable()
  .optional()

export const productMetaDescription = z.string()
  .max(255, 'Meta description must not exceed 255 characters.')
  .nullable()
  .optional()

export const productMetaKeywords = z.string()
  .max(255, 'Meta keywords must not exceed 255 characters.')
  .nullable()
  .optional()

export const productAvailableOn = z.string()
  .nullable()
  .optional()

export const productDiscontinueOn = z.string()
  .nullable()
  .optional()

export const productTrackInventory = z.boolean()

export const productStyleCode = z.string()
  .max(50, 'Style code must not exceed 50 characters.')
  .nullable()
  .optional()

export const productSeasonName = z.string()
  .max(50, 'Season name must not exceed 50 characters.')
  .nullable()
  .optional()

export const productMaterialComposition = z.string()
  .max(500, 'Material composition must not exceed 500 characters.')
  .nullable()
  .optional()

export const productCareInstructions = z.string()
  .max(500, 'Care instructions must not exceed 500 characters.')
  .nullable()
  .optional()

export const productFitNotes = z.string()
  .max(500, 'Fit notes must not exceed 500 characters.')
  .nullable()
  .optional()

export const productDepartment = z.string()
  .max(50, 'Department must not exceed 50 characters.')
  .nullable()
  .optional()

export const productGenderTarget = z.string()
  .max(20, 'Gender target must not exceed 20 characters.')
  .nullable()
  .optional()

export const productSchema = z.object({
  name: productName,
  slug: productSlug,
  description: productDescription,
  metaTitle: productMetaTitle,
  metaDescription: productMetaDescription,
  metaKeywords: productMetaKeywords,
  availableOn: productAvailableOn,
  discontinueOn: productDiscontinueOn,
  trackInventory: productTrackInventory,
  styleCode: productStyleCode,
  seasonName: productSeasonName,
  materialComposition: productMaterialComposition,
  careInstructions: productCareInstructions,
  fitNotes: productFitNotes,
  department: productDepartment,
  genderTarget: productGenderTarget,
})

export type ProductForm = z.infer<typeof productSchema>
