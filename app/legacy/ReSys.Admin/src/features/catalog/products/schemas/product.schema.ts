import { z } from 'zod';

export const ManageClassificationsSchema = z.object({
  taxonIds: z.array(z.string().uuid()),
  mainTaxonId: z.string().uuid().optional().nullable(),
});

export type ManageClassificationsFormData = z.infer<typeof ManageClassificationsSchema>;
