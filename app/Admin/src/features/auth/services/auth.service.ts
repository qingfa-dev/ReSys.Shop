import { authRepository } from "../repositories/auth.repository";
import { mapJwtToProfile } from "../mappers/auth.mapper";
import type { ServerResult } from "@/shared/api/types/result.types";
import type { LoginRequest } from "../types/Login.Request.Type";
import type { AuthenticationResponse } from "../types/Login.Response.Type";
import type { UserProfile } from "../types/Login.Response.Type";
import type { ChangePasswordRequest } from "../types/ChangePassword.Request.Type";

export const authService = {
  async login(request: LoginRequest): Promise<ServerResult<AuthenticationResponse>> {
    return authRepository.login(request) as Promise<ServerResult<AuthenticationResponse>>;
  },

  async refresh(request: {
    refreshToken: string;
    rememberMe?: boolean;
  }): Promise<ServerResult<AuthenticationResponse>> {
    return authRepository.refresh(request) as Promise<ServerResult<AuthenticationResponse>>;
  },

  async logout(): Promise<ServerResult<void>> {
    return authRepository.logout();
  },

  async getProfile(): Promise<ServerResult<Partial<UserProfile>>> {
    const result = await authRepository.getProfile();
    if (!result.isSuccess) return result as ServerResult<Partial<UserProfile>>;
    const value = result.value as Record<string, unknown>;
    return {
      ...result,
      value: {
        id: String(value.id || value.Id || ""),
        email: String(value.email || value.Email || ""),
        fullName: String(value.fullName || value.FullName || ""),
        roles: Array.isArray(value.roles) ? value.roles.map(String) : [],
      } as Partial<UserProfile>,
    };
  },

  async updateProfile(data: Record<string, unknown>): Promise<ServerResult<void>> {
    return authRepository.updateProfile(data);
  },

  async changePassword(data: ChangePasswordRequest): Promise<ServerResult<void>> {
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
