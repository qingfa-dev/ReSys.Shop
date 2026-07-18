import axios, { type AxiosInstance } from 'axios'
import { authInterceptor } from './interceptors/auth.interceptor'
import { camelCaseInterceptor } from './interceptors/camelcase.interceptor'
import { errorWrapperInterceptor } from './interceptors/error-wrapper.interceptor'

const apiBaseUrl = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

const apiClient: AxiosInstance = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
  paramsSerializer: { indexes: null },
})

apiClient.interceptors.request.use(authInterceptor)
apiClient.interceptors.response.use(camelCaseInterceptor, errorWrapperInterceptor)

export default apiClient
