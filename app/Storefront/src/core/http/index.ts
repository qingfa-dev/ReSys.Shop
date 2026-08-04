import { axiosInstance } from './axios/axios.client'
import { requestInterceptor } from './interceptors/request.interceptor'
import { responseInterceptor, responseErrorInterceptor } from './interceptors/response.interceptor'

axiosInstance.interceptors.request.use(requestInterceptor, undefined)
axiosInstance.interceptors.response.use(responseInterceptor, responseErrorInterceptor)

export const httpClient = axiosInstance
export { axiosInstance }
