import { z } from 'zod'

export const taxonTaxonomyId = z.string()
  .min(1, 'Taxonomy is required.')

export const taxonParentId = z.string()
  .nullable()
  .optional()

export const taxonName = z.string()
  .min(1, 'Taxon name is required.')
  .max(255, 'Taxon name must not exceed 255 characters.')

export const taxonPresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(255, 'Presentation must not exceed 255 characters.')

export const taxonSlug = z.string()
  .min(1, 'Slug is required.')
  .max(255, 'Slug must not exceed 255 characters.')
  .regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, 'Slug must be lowercase alphanumeric with hyphens.')

export const taxonDescription = z.string()
  .max(2000, 'Description must not exceed 2000 characters.')
  .nullable()
  .optional()

export const taxonPosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const taxonMetaTitle = z.string()
  .max(100, 'Meta title must not exceed 100 characters.')
  .nullable()
  .optional()

export const taxonMetaDescription = z.string()
  .max(255, 'Meta description must not exceed 255 characters.')
  .nullable()
  .optional()

export const taxonMetaKeywords = z.string()
  .max(255, 'Meta keywords must not exceed 255 characters.')
  .nullable()
  .optional()

export const taxonImageUrl = z.string()
  .nullable()
  .optional()

export const taxonSquareImageUrl = z.string()
  .nullable()
  .optional()

export const taxonAutomatic = z.boolean()

export const taxonRulesMatchPolicy = z.string()
  .min(1, 'Rules match policy is required.')

export const taxonSortOrder = z.string()
  .min(1, 'Sort order is required.')

export const taxonHideFromNav = z.boolean()

export const taxonSchema = z.object({
  taxonomyId: taxonTaxonomyId,
  parentId: taxonParentId,
  name: taxonName,
  presentation: taxonPresentation,
  slug: taxonSlug,
  description: taxonDescription,
  position: taxonPosition,
  metaTitle: taxonMetaTitle,
  metaDescription: taxonMetaDescription,
  metaKeywords: taxonMetaKeywords,
  imageUrl: taxonImageUrl,
  squareImageUrl: taxonSquareImageUrl,
  automatic: taxonAutomatic,
  rulesMatchPolicy: taxonRulesMatchPolicy,
  sortOrder: taxonSortOrder,
  hideFromNav: taxonHideFromNav,
})

export type TaxonForm = z.infer<typeof taxonSchema>
