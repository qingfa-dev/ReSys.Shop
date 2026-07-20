import { authRepository } from "../api/auth.api";
import { mapJwtToProfile } from "../mappers/auth.mapper";
import type { ServerResult } from "@/common/api/types/result.types";
import type { LoginRequest } from "../types/login.request.type";
import type { UserProfile } from "../types/login.response.type";
import type { AuthSession } from "../types/auth.model.type";
import type { ChangePasswordRequest } from "../types/change-password.request.type";

export const authService = {
  async login(request: LoginRequest): Promise<ServerResult<AuthSession>> {
    return authRepository.login(request);
  },

  async refresh(request: {
    refreshToken: string;
    rememberMe?: boolean;
  }): Promise<ServerResult<AuthSession>> {
    return authRepository.refresh(request);
  },

  async logout(): Promise<ServerResult<void>> {
    return authRepository.logout();
  },

  async getProfile(): Promise<ServerResult<Partial<UserProfile>>> {
    return authRepository.getProfile();
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
