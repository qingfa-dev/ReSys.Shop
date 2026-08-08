import { z } from 'zod'

// Schema: Taxonomy list item from GET /api/storefront/taxonomies
// Backend: StoreTaxonomyListItemResponse extends TaxonomyListItemResponse
export const TaxonomyListItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable().optional().default(null),
  position: z.number().int().min(0).optional().default(0),
  taxonsCount: z.number().int().optional().default(0),
})

// Schema: Taxon breadcrumb item nested inside StoreTaxonListItemResponse
export const TaxonBreadcrumbItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  permalink: z.string(),
})

// Schema: Taxon list item from GET /api/storefront/taxons/all
// Backend: StoreTaxonListItemResponse extends TaxonListItemResponse
export const TaxonListItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable().optional().default(null),
  description: z.string().nullable().optional().default(null),
  position: z.number().int().min(0).optional().default(0),
  parentId: z.string().nullable().optional().default(null),
  taxonomyId: z.string().optional().default(''),
  parentName: z.string().nullable().optional().default(null),
  taxonomyName: z.string().nullable().optional().default(null),
  depth: z.number().int().min(0).optional().default(0),
  taxonRuleCount: z.number().int().nullable().optional().default(0),
  productCount: z.number().int().nullable().optional().default(0),
  childrenCount: z.number().int().nullable().optional().default(0),
  permalink: z.string().optional().default(''),
  prettyName: z.string().optional().default(''),
  slug: z.string().optional().default(''),
  imageUrl: z.string().nullable().optional().default(null),
  breadcrumb: z.array(TaxonBreadcrumbItemSchema).optional().default([]),
})
