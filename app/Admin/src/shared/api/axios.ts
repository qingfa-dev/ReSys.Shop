import axios from 'axios'
import type { AxiosInstance } from 'axios'
import { authInterceptor } from './interceptors/auth'
import { camelCaseInterceptor } from './interceptors/camelcase'
import { errorInterceptor } from './interceptors/error'

const BASE_URL = typeof import.meta !== 'undefined'
  ? import.meta.env?.VITE_API_URL ?? ''
  : ''
const TIMEOUT = 30_000

let _instance: AxiosInstance | null = null

export function createApiClient(): AxiosInstance {
  if (_instance) return _instance

  _instance = axios.create({
    baseURL: BASE_URL,
    timeout: TIMEOUT,
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  })

  _instance.interceptors.request.use(authInterceptor)
  _instance.interceptors.response.use(camelCaseInterceptor, errorInterceptor)

  return _instance
}

export function getApiClient(): AxiosInstance {
  return _instance ?? createApiClient()
}

export function resetApiClient(): void {
  _instance = null
}
