import { z } from 'zod'

export const paymentMethodName = z.string()
  .min(1, 'Name is required.')
  .max(255, 'Name cannot exceed 255 characters.')

export const paymentMethodCode = z.string()
  .regex(/^[a-zA-Z0-9_-]+$/, 'Code may only contain letters, numbers, underscores, and hyphens.')
  .max(50, 'Code cannot exceed 50 characters.')

export const paymentMethodProviderKey = z.string()
  .min(1, 'Provider key is required.')
  .max(50, 'Provider key cannot exceed 50 characters.')

export const paymentMethodDescription = z.string()
  .max(1000, 'Description cannot exceed 1000 characters.')
  .optional()

export const paymentMethodDisplayOn = z.enum(['Both', 'Frontend', 'Backend'], {
  message: 'Display on must be one of Both, Frontend, or Backend.',
})

export const paymentMethodPosition = z.number()
  .int()
  .min(0, 'Position must be at least 0.')
  .max(9999, 'Position cannot exceed 9999.')

export const paymentMethodActive = z.boolean()
export const paymentMethodWebhookEnabled = z.boolean()
export const paymentMethodAutoCapture = z.boolean()

export const paymentMethodSchema = z.object({
  name: paymentMethodName,
  code: paymentMethodCode,
  providerKey: paymentMethodProviderKey,
  description: paymentMethodDescription,
  displayOn: paymentMethodDisplayOn,
  position: paymentMethodPosition,
  active: paymentMethodActive,
  webhookEnabled: paymentMethodWebhookEnabled,
  autoCapture: paymentMethodAutoCapture,
})

export type PaymentMethodForm = z.infer<typeof paymentMethodSchema>

export const paymentMethodUpdateSchema = paymentMethodSchema.partial()

export type PaymentMethodUpdateForm = z.infer<typeof paymentMethodUpdateSchema>
