import { env } from './env'

export const appConfig = {
  title: 'Sakai',
  api: {
    baseUrl: env.apiUrl,
    timeout: 30_000,
  },
} as const
