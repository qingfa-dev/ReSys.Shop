import apiClient from '@/shared/api/http/api.client'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { LoginRequest, RefreshRequest, AuthenticationResponse } from '../types/auth.types'

// NOTE: Backend has no admin auth endpoints yet.
// Admin auth uses storefront identity routes as a temporary bridge.
// Full admin auth endpoints should be added to the Identity module.
// See: docs/superpowers/plans/2026-07-16-admin-api-service-correction.md §4.1
const BASE_URL = '/store/identity/auth'

export interface ChangePasswordRequest {
    current_password: string;
    new_password: string;
    confirm_new_password: string;
}

export const authService = {
  /**
   * Authenticates the user with credentials.
   */
    async login(request: LoginRequest): Promise<ApiResult<AuthenticationResponse>> {
        return await apiClient.post(`${BASE_URL}/login/password`, request) as any;
    },

    /**
     * Refreshes the access token using the refresh token.
     * Note: This rotates the refresh token.
     */
    async refresh(request: RefreshRequest): Promise<ApiResult<AuthenticationResponse>> {
        return await apiClient.post(`${BASE_URL}/sessions/refresh`, request) as any;
    },

    /**
     * Logs out the user (invalidates the session on the server).
     */
    async logout(): Promise<ApiResult<void>> {
        return await apiClient.post(`${BASE_URL}/logout`, {}) as any;
    },

    async getProfile(): Promise<ApiResult<any>> {
        return apiClient.get('/account/profile');
    },

    async updateProfile(data: any): Promise<ApiResult<void>> {
        return apiClient.put('/account/profile', data);
    },

    async changePassword(data: ChangePasswordRequest): Promise<ApiResult<void>> {
        return apiClient.post(`${BASE_URL}/password/change`, data);
    }
};