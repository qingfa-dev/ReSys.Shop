import { z } from 'zod'

export function createVariantImageSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  alt: z.string().max(500).optional().nullable(),
  role: z.number().int().min(0).max(5).optional(),
})
}

export type VariantImageParameters = z.infer<ReturnType<typeof createVariantImageSchema>>
