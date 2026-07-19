import { z } from 'zod'

export function createOptionTypeSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z
    .string()
    .min(1, t('catalog.validation.name.required'))
    .max(100, t('catalog.validation.name.max_length')),
  presentation: z
    .string()
    .min(1, t('catalog.validation.presentation.required'))
    .max(100, t('catalog.validation.presentation.max_length')),
  description: z
    .string()
    .max(500, t('catalog.validation.description.max_length'))
    .optional()
    .nullable(),
  filterable: z.boolean().default(false),
  position: z
    .number()
    .int(t('catalog.validation.position.whole'))
    .min(0, t('catalog.validation.position.min'))
    .default(0),
})
}

export type OptionTypeParameters = z.infer<ReturnType<typeof createOptionTypeSchema>>
