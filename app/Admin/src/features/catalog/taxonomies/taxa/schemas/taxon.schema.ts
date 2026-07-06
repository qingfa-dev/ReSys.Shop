import { z } from 'zod';

export const TaxonRuleSchema = z.object({
  type: z.string().min(1, 'Rule type is required'),
  value: z.string().min(1, 'Value is required'),
  match_policy: z.string().min(1, 'Match policy is required'),
  property_name: z.string().optional().nullable(),
});

export type TaxonRuleFormData = z.infer<typeof TaxonRuleSchema>;

export const TaxonSchema = z.object({
  taxonomy_id: z.string().uuid('Taxonomy is required'),
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  description: z.string().max(500).optional().nullable(),
  slug: z.string().min(1, 'Slug is required').max(100),
  position: z.number().int().min(0).default(0),
  hide_from_nav: z.boolean().default(false),
  image_url: z.string().optional().nullable(),
  square_image_url: z.string().optional().nullable(),
  parent_id: z.string().uuid().optional().nullable(),
  automatic: z.boolean().default(false),
  rules_match_policy: z.enum(['all', 'any']).default('all'),
  sort_order: z.string().default('manual'),
  meta_title: z.string().max(100).optional().nullable(),
  meta_description: z.string().max(255).optional().nullable(),
  meta_keywords: z.string().max(255).optional().nullable(),
});

export type TaxonFormData = z.infer<typeof TaxonSchema>;