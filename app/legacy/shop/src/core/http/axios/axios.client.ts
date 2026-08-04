import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'
const API_TIMEOUT = 30000

const axiosConfig: AxiosRequestConfig = {
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
  withCredentials: false,
}

const axiosInstance: AxiosInstance = axios.create(axiosConfig)

export { axiosInstance }
export type { AxiosInstance, AxiosRequestConfig }
