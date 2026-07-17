import { z } from 'zod'

export const DashboardQuerySchema = z.object({
  from: z.string().optional(),
  to: z.string().optional(),
})

export type DashboardQueryParameters = z.infer<typeof DashboardQuerySchema>
