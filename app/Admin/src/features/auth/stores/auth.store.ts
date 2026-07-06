import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '../services/auth.service';
import type { LoginRequest, AuthenticationResponse } from '../types/auth.types';
import type { ApiResult } from '@/shared/api/api.types';
import { jwtDecode } from 'jwt-decode';

export const useAuthStore = defineStore('auth', () => {
    const accessToken = ref<string | null>(localStorage.getItem('accessToken'));
    const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'));
    const loading = ref(false);
    
    const isAuthenticated = computed(() => !!accessToken.value);

    // Simple decoder to extract user info from JWT without verifying signature (server does that)
    const user = computed(() => {
        if (!accessToken.value) return null;
        try {
            return jwtDecode(accessToken.value);
        } catch {
            return null;
        }
    });

    function setTokens(response: AuthenticationResponse) {
        accessToken.value = response.access_token;
        refreshToken.value = response.refresh_token;
        
        localStorage.setItem('accessToken', response.access_token);
        localStorage.setItem('refreshToken', response.refresh_token);
    }

    function clearTokens() {
        accessToken.value = null;
        refreshToken.value = null;
        
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }

    async function login(payload: LoginRequest): Promise<ApiResult<AuthenticationResponse>> {
        loading.value = true;
        const result = await authService.login(payload);
        
        if (result.success && result.data) {
            setTokens(result.data);
        }
        
        loading.value = false;
        return result;
    }

    async function logout(): Promise<ApiResult<void>> {
        loading.value = true;
        let result: ApiResult<void> = { success: true, data: undefined as any };

        try {
            // Attempt server-side logout
            if (accessToken.value) {
                result = await authService.logout();
            }
        } catch (e) {
            console.error('Logout failed', e);
            result = { success: false, error: { title: 'Logout Failed', status: 500 }, data: null as any };
        } finally {
            clearTokens();
            loading.value = false;
        }
        return result;
    }

    /**
     * Called by the Axios interceptor when the access token expires.
     */
    async function refreshSession(): Promise<string | null> {
        if (!refreshToken.value) return null;

        try {
            const result = await authService.refresh({
                refreshToken: refreshToken.value
            });
            
            if (result.success && result.data) {
                setTokens(result.data);
                return result.data.access_token;
            }
            // If refresh fails (logic handled in result check or catch)
            clearTokens();
            return null;
        } catch (error) {
            clearTokens();
            return null;
        }
    }

    return {
        accessToken,
        refreshToken,
        isAuthenticated,
        user,
        loading,
        login,
        logout,
        refreshSession,
        clearTokens
    };
});