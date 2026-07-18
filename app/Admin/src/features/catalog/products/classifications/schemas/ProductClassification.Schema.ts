import { z } from 'zod'

export function createManageClassificationsSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  taxonIds: z.array(z.string().uuid(t('catalog.validation.taxon_id.invalid'))).min(1, t('catalog.validation.option_type_ids.min')),
  mainTaxonId: z.string().uuid(t('catalog.validation.taxon_id.invalid')).optional().nullable(),
})
}

export type ManageClassificationsParameters = z.infer<ReturnType<typeof createManageClassificationsSchema>>
