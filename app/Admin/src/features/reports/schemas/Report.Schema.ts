import { z } from 'zod'

export function createDashboardQuerySchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  from: z.string().optional(),
  to: z.string().optional(),
})
}

export type DashboardQueryParameters = z.infer<ReturnType<typeof createDashboardQuerySchema>>
