import { z } from 'zod'

export const TaxonBreadcrumbItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  permalink: z.string(),
})

export const TaxonListItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  permalink: z.string(),
  depth: z.number().int().min(0),
  slug: z.string(),
  presentation: z.string().nullable(),
  taxonomyId: z.string(),
  parentId: z.string().nullable(),
  position: z.number().int().min(0),
  imageUrl: z.string().nullable(),
  taxonCount: z.number().int().nullable(),
  childrenCount: z.number().int().nullable(),
  prettyName: z.string(),
  breadcrumb: z.array(TaxonBreadcrumbItemSchema),
})

export const TaxonomySchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
})

const TaxonTreeNodeSchema: z.ZodType<any> = z.lazy(() =>
  z.object({
    id: z.string(),
    name: z.string(),
    presentation: z.string().nullable(),
    permalink: z.string(),
    depth: z.number().int().min(0),
    hasChildren: z.boolean(),
    children: z.array(TaxonTreeNodeSchema),
  })
)

export const TaxonomyGroupSchema = z.object({
  taxonomy: z.object({
    id: z.string(),
    name: z.string(),
    presentation: z.string().nullable(),
    position: z.number().int().min(0),
  }),
  tree: z.array(TaxonTreeNodeSchema),
})
