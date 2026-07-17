import { z } from 'zod';

export const TaxonRuleSchema = z.object({
  type: z.string().min(1, 'Rule type is required'),
  value: z.string().min(1, 'Value is required'),
  matchPolicy: z.string().min(1, 'Match policy is required'),
});

export type TaxonRuleFormData = z.infer<typeof TaxonRuleSchema>;

export const TaxonSchema = z.object({
  taxonomyId: z.string().uuid('Taxonomy is required'),
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  description: z.string().max(500).optional().nullable(),
  slug: z.string().min(1, 'Slug is required').max(100),
  position: z.number().int().min(0).default(0),
  hideFromNav: z.boolean().default(false),
  parentId: z.string().uuid().optional().nullable(),
  automatic: z.boolean().default(false),
  rulesMatchPolicy: z.enum(['all', 'any']).default('all'),
  sortOrder: z.string().default('manual'),
  metaTitle: z.string().max(100).optional().nullable(),
  metaDescription: z.string().max(255).optional().nullable(),
  metaKeywords: z.string().max(255).optional().nullable(),
});

export type TaxonFormData = z.infer<typeof TaxonSchema>;