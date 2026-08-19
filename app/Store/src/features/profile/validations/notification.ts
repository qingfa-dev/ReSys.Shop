import { z } from 'zod'

export const NotificationPreferencesSchema = z.object({
  enableSms: z.boolean(),
  enableEmail: z.boolean(),
  enableNewsfeeds: z.boolean(),
})
