import { z } from 'zod'

export function createStateSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('location.validation.name.required')).max(100, t('location.validation.name.max_length')),
  abbreviation: z.string().min(1, t('location.validation.abbreviation.required')).max(10, t('location.validation.abbreviation.max_length')),
  countryId: z.string().uuid(t('location.validation.country.invalid')).min(1, t('location.validation.country.required')),
  isActive: z.boolean().default(true),
})
}

export type StateParameters = z.infer<ReturnType<typeof createStateSchema>>
