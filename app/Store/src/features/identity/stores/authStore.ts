import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { AuthApi, EmailApi } from "../services";
import {
  getAccessToken,
  setTokens,
  clearTokens,
  hasValidAccessToken,
  getRefreshToken,
} from "../services/tokenService";
import { emit } from "@/shared/composables/useStoreEvents";
import type { AuthUser, LoginRequest, RegisterRequest } from "../types";

export const useAuthStore = defineStore("auth", () => {
  const user = ref<AuthUser | null>(null);
  const status = ref<"idle" | "loading" | "authenticated" | "error">("idle");
  const error = ref<string | null>(null);
  const _initialized = ref(false);

  const isAuthenticated = computed(() => status.value === "authenticated" && user.value !== null);

  async function init(): Promise<void> {
    if (_initialized.value) return;
    _initialized.value = true;
    if (!getAccessToken()) {
      status.value = "idle";
      return;
    }
    status.value = "loading";
    try {
      const result = await AuthApi.getSession();
      if (result.isSuccess && result.value) {
        user.value = {
          userId: result.value.id,
          userName: result.value.userName,
          email: result.value.email,
          roles: result.value.roles,
          permissions: result.value.permissions,
          isAuthenticated: true,
        };
        status.value = "authenticated";
        emit({ type: "auth:login", userId: result.value.id });
      } else {
        clearTokens();
        status.value = "error";
      }
    } catch {
      clearTokens();
      status.value = "error";
    }
    emit({ type: "auth:init-done", userId: user.value?.userId ?? "" });
  }

  async function login(credential: string, password: string): Promise<boolean> {
    status.value = "loading";
    error.value = null;
    const result = await AuthApi.login({ credential, password });
    if (result.isSuccess) {
      setTokens(result.value);
      const session = await AuthApi.getSession();
      if (session.isSuccess && session.value) {
        user.value = {
          userId: session.value.id,
          userName: session.value.userName,
          email: session.value.email,
          roles: session.value.roles,
          permissions: session.value.permissions,
          isAuthenticated: true,
        };
        status.value = "authenticated";
        emit({ type: "auth:login", userId: session.value.id });
        return true;
      }
    }
    error.value = result.message ?? "Login failed";
    status.value = "error";
    return false;
  }

  async function loginWithGoogle(): Promise<void> {
    const result = await AuthApi.getLoginProviders();
    if (result.isSuccess) {
      const google = result.value.find((p) => p.name.toLowerCase().includes("google"));
      if (google) window.location.href = google.url;
    }
  }

  async function register(req: RegisterRequest): Promise<boolean> {
    status.value = "loading";
    error.value = null;
    const result = await AuthApi.register(req);
    if (result.isSuccess) {
      status.value = "idle";
      return true;
    }
    error.value = result.message ?? "Registration failed";
    status.value = "error";
    return false;
  }

  async function logout(revokeAll = false): Promise<void> {
    try {
      await AuthApi.logout({ revokeAll });
    } catch {}
    clearTokens();
    user.value = null;
    status.value = "idle";
    emit({ type: "auth:logout" });
  }

  async function changePassword(current: string, newPwd: string): Promise<boolean> {
    return (await AuthApi.changePassword(current, newPwd)).isSuccess;
  }
  async function forgotPassword(email: string): Promise<boolean> {
    return (await AuthApi.forgotPassword(email)).isSuccess;
  }
  async function resetPassword(token: string, newPwd: string): Promise<boolean> {
    return (await AuthApi.resetPassword(token, newPwd)).isSuccess;
  }
  async function changeEmail(newEmail: string): Promise<boolean> {
    return (await EmailApi.changeEmail(newEmail)).isSuccess;
  }
  async function confirmEmail(token: string): Promise<boolean> {
    return (await EmailApi.confirmEmail(token)).isSuccess;
  }
  async function resendVerification(): Promise<boolean> {
    return (await EmailApi.resendVerification()).isSuccess;
  }

  return {
    user,
    status,
    error,
    isAuthenticated,
    init,
    login,
    loginWithGoogle,
    register,
    logout,
    changePassword,
    forgotPassword,
    resetPassword,
    changeEmail,
    confirmEmail,
    resendVerification,
  };
});
