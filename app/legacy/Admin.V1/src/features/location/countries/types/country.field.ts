import { z } from 'zod'

export function createCountrySchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('location.validation.name.required')).max(100, t('location.validation.name.max_length')),
  isoCode: z.string().length(2, t('location.validation.iso_code.length')).toUpperCase(),
  callingCode: z.string().max(10, t('location.validation.calling_code.max_length')).default(''),
  isActive: z.boolean().default(true),
})
}

export type CountryParameters = z.infer<ReturnType<typeof createCountrySchema>>
