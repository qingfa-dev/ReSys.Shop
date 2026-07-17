import { z } from 'zod'

export const ManageClassificationsSchema = z.object({
  taxonIds: z.array(z.string().uuid('Invalid taxon ID')).min(1, 'At least one taxon must be selected'),
  mainTaxonId: z.string().uuid('Invalid taxon ID').optional().nullable(),
})

export type ManageClassificationsParameters = z.infer<typeof ManageClassificationsSchema>
