import axios, {
  type AxiosInstance,
  type AxiosError,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import type { ServerResult } from "../types/result.types";
import { parseApiError } from "../utils/api.utils";
import { refreshTokens } from "./refresh-handler";
import { toCamelCaseKeys } from "@/shared/mapper/mapper.utils";

const apiBaseUrl = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api` : "/api";

const apiClient: AxiosInstance = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    "Content-Type": "application/json",
  },
  paramsSerializer: {
    indexes: null,
  },
});

apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem("accessToken");
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  },
);

apiClient.interceptors.response.use(
  (response) => {
    if (response.data && typeof response.data === 'object') {
      response.data = toCamelCaseKeys(response.data as Record<string, unknown>)
    }
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    const apiError = parseApiError(error);

    if (apiError.statusCode === 401 && originalRequest && !originalRequest._retry) {
      console.warn("Session expired. Attempting to refresh token...");

      if (originalRequest.url?.includes("/auth/session/refresh")) {
        return Promise.resolve({
          data: {
            isSuccess: false,
            statusCode: 401,
            errors: [
              {
                code: "UNAUTHORIZED",
                message: apiError.detail || "Unauthorized",
                type: 0,
                metadata: null,
              },
            ],
            message: apiError.title,
            metadata: null,
            value: null,
          } as ServerResult<null>,
        } as AxiosResponse);
      }

      originalRequest._retry = true;

      const refreshed = await refreshTokens();
      if (refreshed) {
        const newToken = localStorage.getItem("accessToken");
        if (newToken && originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
        }
        return apiClient(originalRequest);
      }
    }

    return Promise.resolve({
      data: {
        isSuccess: false,
        statusCode: apiError.statusCode,
        errors: [
          {
            code: apiError.errorCode || "ERROR",
            message: apiError.detail || apiError.title || "Request failed",
            type: 0,
            metadata: null,
          },
        ],
        message: apiError.title,
        metadata: null,
        value: null,
      } as ServerResult<null>,
    } as AxiosResponse);
  },
);

export default apiClient;
