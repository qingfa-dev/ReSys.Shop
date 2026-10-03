import { z } from 'zod'

export const optionValueOptionTypeId = z.string()
  .min(1, 'Option type is required.')

export const optionValueName = z.string()
  .min(1, 'Option value name is required.')
  .max(100, 'Option value name must not exceed 100 characters.')

export const optionValuePresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(100, 'Presentation must not exceed 100 characters.')

export const optionValuePosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const optionValueSchema = z.object({
  optionTypeId: optionValueOptionTypeId,
  name: optionValueName,
  presentation: optionValuePresentation,
  position: optionValuePosition,
})

export type OptionValueForm = z.infer<typeof optionValueSchema>
