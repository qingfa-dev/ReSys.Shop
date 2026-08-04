import { z } from 'zod'

export const optionTypeName = z.string()
  .min(1, 'Option type name is required.')
  .max(100, 'Option type name must not exceed 100 characters.')

export const optionTypePresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(100, 'Presentation must not exceed 100 characters.')

export const optionTypePosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const optionTypeFilterable = z.boolean()

export const optionTypeSchema = z.object({
  name: optionTypeName,
  presentation: optionTypePresentation,
  position: optionTypePosition,
  filterable: optionTypeFilterable,
})

export type OptionTypeForm = z.infer<typeof optionTypeSchema>
