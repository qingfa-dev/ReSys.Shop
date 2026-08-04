export const env = {
  apiUrl: import.meta.env.VITE_API_URL,
  baseUrl: import.meta.env.BASE_URL,
  appVersion: __APP_VERSION__,
  isDev: import.meta.env.DEV,
  isProd: import.meta.env.PROD,
} as const
