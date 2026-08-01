import { z } from 'zod'

export const variantPriceAmount = z.number()
  .min(0, 'Price amount must be greater than or equal to zero.')
  .optional()

export const variantPriceCurrency = z.string()
  .min(1, 'Currency is required.')
  .max(3, 'Currency cannot exceed 3 characters.')

export const variantPriceCompareAtAmount = z.number()
  .min(0, 'Compare at amount must be greater than or equal to zero.')
  .optional()

export const variantPriceCountryIso = z.string()
  .max(2, 'Country ISO code must be 2 characters.')
  .optional()

export const variantPriceSchema = z.object({
  amount: variantPriceAmount,
  currency: variantPriceCurrency,
  compareAtAmount: variantPriceCompareAtAmount,
  countryIso: variantPriceCountryIso,
})

export type VariantPriceForm = z.infer<typeof variantPriceSchema>
