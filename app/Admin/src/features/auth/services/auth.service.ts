import { authRepository } from "../repository/auth.repository";
import { mapAuthResponse, mapJwtToProfile } from "../mapper/auth.mapper";
import type { ServerResult } from "@/shared/api/types/result.types";
import type { LoginRequest } from "../types/auth.request.types";
import type { AuthenticationResponse } from "../types/auth.response.types";
import type { UserProfile } from "../types/auth.domain.types";
import type { ChangePasswordFormData } from "../types/auth.model.types";

function handleResult<T, R>(result: ServerResult<T>, mapper: (data: T) => R): ServerResult<R> {
  if (result.isSuccess) {
    return { ...result, value: mapper(result.value) };
  }
  return result as unknown as ServerResult<R>;
}

export const authService = {
  async login(request: LoginRequest): Promise<ServerResult<AuthenticationResponse>> {
    const result = await authRepository.login(request);
    return handleResult(result, mapAuthResponse);
  },

  async refresh(request: {
    refreshToken: string;
    rememberMe?: boolean;
  }): Promise<ServerResult<AuthenticationResponse>> {
    const result = await authRepository.refresh(request);
    return handleResult(result, mapAuthResponse);
  },

  async logout(): Promise<ServerResult<void>> {
    return authRepository.logout();
  },

  async getProfile(): Promise<ServerResult<Partial<UserProfile>>> {
    const result = await authRepository.getProfile();
    return handleResult(result, (data) => ({
      id: String(data.id || ""),
      email: String(data.email || ""),
      fullName: String(data.fullName || data.full_name || ""),
      roles: Array.isArray(data.roles) ? data.roles.map(String) : [],
    }));
  },

  async updateProfile(data: Record<string, unknown>): Promise<ServerResult<void>> {
    return authRepository.updateProfile(data);
  },

  async changePassword(data: ChangePasswordFormData): Promise<ServerResult<void>> {
    return authRepository.changePassword(data);
  },

  getProfileFromToken(token: string): UserProfile | null {
    try {
      const claims = JSON.parse(atob(token.split(".")[1] as string));
      return mapJwtToProfile(claims) as UserProfile;
    } catch {
      return null;
    }
  },
};
