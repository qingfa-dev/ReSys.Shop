import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '../services/auth.service';
import type { LoginRequest } from '../types/auth.request.types';
import type { AuthenticationResponse } from '../types/auth.response.types';
import type { ServerResult } from '@/shared/api/types/result.types';
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
        accessToken.value = response.accessToken;
        refreshToken.value = response.refreshToken;
        
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
    }

    function clearTokens() {
        accessToken.value = null;
        refreshToken.value = null;
        
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }

    async function login(payload: LoginRequest): Promise<ServerResult<AuthenticationResponse>> {
        loading.value = true;
        const result = await authService.login(payload);
        
        if (result.isSuccess) {
            setTokens(result.value);
        }
        
        loading.value = false;
        return result;
    }

    async function logout(): Promise<ServerResult<void>> {
        loading.value = true;
        let result: ServerResult<void> = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined as any };

        try {
            // Attempt server-side logout
            if (accessToken.value) {
                result = await authService.logout();
            }
        } catch (e) {
            console.error('Logout failed', e);
            result = { isSuccess: false, statusCode: 500, errors: [{ code: 'logout_failed', message: 'Logout Failed', type: 0, metadata: null }], message: 'Logout Failed', metadata: null, value: null as any };
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
            
            if (result.isSuccess) {
                setTokens(result.value);
                return result.value.accessToken;
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
