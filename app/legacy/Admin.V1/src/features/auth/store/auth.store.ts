import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authRepository } from '../api/auth.api';
import type { LoginRequest } from '../types/login.request';
import type { AuthSession } from '../models/auth.model';
import type { ServerResult } from '@/common/api/types/result.types';
import { jwtDecode } from 'jwt-decode';

export const useAuthStore = defineStore('auth', () => {
    const accessToken = ref<string | null>(localStorage.getItem('accessToken'));
    const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'));
    const loading = ref(false);
    const session = ref<AuthSession | null>(null);
    
    const isAuthenticated = computed(() => !!accessToken.value);

    const user = computed(() => {
        if (!accessToken.value) return null;
        try {
            return jwtDecode(accessToken.value);
        } catch {
            return null;
        }
    });

    const permissions = computed<string[]>(() => {
        const sessionPerms = session.value?.user?.permissions
        if (sessionPerms && sessionPerms.length > 0) return sessionPerms
        const decoded = user.value as Record<string, unknown> | null
        const jwtPerms = decoded?.permissions || decoded?.permission
        return Array.isArray(jwtPerms) ? jwtPerms.map(String) : []
    })

    function setTokens(response: { accessToken: string; refreshToken: string }) {
        accessToken.value = response.accessToken;
        refreshToken.value = response.refreshToken;
        
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
    }

    function clearTokens() {
        accessToken.value = null;
        refreshToken.value = null;
        session.value = null;
        
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }

    async function login(payload: LoginRequest): Promise<ServerResult<AuthSession>> {
        loading.value = true;
        const result = await authRepository.login(payload);
        
        if (result.isSuccess) {
            setTokens(result.value);
            session.value = result.value;
        }
        
        loading.value = false;
        return result;
    }

    async function logout(): Promise<ServerResult<void>> {
        loading.value = true;
        let result: ServerResult<void> = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined as never };

        try {
            if (accessToken.value) {
                result = await authRepository.logout();
            }
        } catch (e) {
            console.error('Logout failed', e);
            result = { isSuccess: false, statusCode: 500, errors: [{ code: 'logout_failed', message: 'Logout Failed', type: 0, metadata: null }], message: 'Logout Failed', metadata: null, value: null as never };
        } finally {
            clearTokens();
            loading.value = false;
        }
        return result;
    }

    async function refreshSession(): Promise<string | null> {
        if (!refreshToken.value) return null;

        try {
            const result = await authRepository.refresh({
                refreshToken: refreshToken.value
            });
            
            if (result.isSuccess) {
                setTokens(result.value);
                session.value = result.value;
                return result.value.accessToken;
            }
            clearTokens();
            return null;
        } catch (error) {
      console.error(error);
            clearTokens();
            return null;
        }
    }

    return {
        accessToken,
        refreshToken,
        isAuthenticated,
        user,
        session,
        permissions,
        loading,
        login,
        logout,
        refreshSession,
        clearTokens
    };
});
