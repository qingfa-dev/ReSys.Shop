import { z } from 'zod'

export function createAddressSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    firstName: z.string().min(1, t('ordering.validation.first_name.required')).max(100, t('ordering.validation.first_name.max_length')),
    lastName: z.string().min(1, t('ordering.validation.last_name.required')).max(100, t('ordering.validation.last_name.max_length')),
    address1: z.string().min(1, t('ordering.validation.address.required')).max(200, t('ordering.validation.address.max_length')),
    address2: z.string().max(200, t('ordering.validation.address.max_length')).optional(),
    city: z.string().min(1, t('ordering.validation.city.required')).max(100, t('ordering.validation.city.max_length')),
    zipCode: z.string().min(1, t('ordering.validation.zip.required')).max(20, t('ordering.validation.zip.max_length')),
    countryCode: z.string().length(2, t('ordering.validation.country_code.length')),
    stateCode: z.string().max(10, t('ordering.validation.state_code.max_length')).optional(),
    phone: z.string().max(30, t('ordering.validation.phone.max_length')).optional(),
    company: z.string().max(100, t('ordering.validation.company.max_length')).optional(),
  })
}

export function createLineItemSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    variantId: z.string().uuid(t('ordering.validation.variant.invalid')),
    quantity: z.number().int(t('ordering.validation.quantity.whole')).min(1, t('ordering.validation.quantity.min_one')),
  })
}

export function createOrderSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  const addressSchema = createAddressSchema(t)
  return z.object({
    email: z.string().email(t('ordering.validation.email.invalid')).min(1, t('ordering.validation.email.required')),
    currency: z.string().length(3, t('ordering.validation.currency.length')).default('USD'),
    lineItems: z.array(createLineItemSchema(t)).min(1, t('ordering.validation.items.min_one')),
    shippingAddress: addressSchema.optional(),
    billingAddress: addressSchema.optional(),
  })
}

export type OrderParameters = z.infer<ReturnType<typeof createOrderSchema>>
