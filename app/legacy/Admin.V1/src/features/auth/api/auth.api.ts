import apiClient from "@/common/api/http/api.client";
import type { ServerResult } from "@/common/api/types/result.types";
import type { LoginResponse, UserProfile } from "../types/login.response";
import type { LoginRequest } from "../types/login.request";
import type { ChangePasswordRequest } from "../types/change-password.request";
import type {
  RefreshTokenRequest,
  UpdateProfileRequest,
  AuthProfileResponse,
} from "../types/auth.request";
import type { AuthSession } from '../models/auth.model';
import { mapAuthSession } from "../../identity/models/identity.mapper";
import { mapJwtToProfile, mapProfileResponse, mapSessionResponse } from "../models/auth.mapper";

const BASE_URL = "/store/identity/auth";

function path(sub: string): string {
  return `${BASE_URL}/${sub}`;
}

async function fetchSession(): Promise<{ id: string; roles: string[]; permissions: string[] } | null> {
  try {
    const res = await apiClient.get(path("profile"));
    const data = res.data as ServerResult<AuthProfileResponse>;
    if (data.isSuccess && data.value) {
      return mapSessionResponse(data.value);
    }
  } catch (e) {
    console.error('[Auth] Failed to fetch session:', e)
  }
  return null;
}

export const authRepository = {
  getProfileFromToken(token: string): UserProfile | null {
    try {
      const claims = JSON.parse(atob(token.split(".")[1] as string));
      return mapJwtToProfile(claims) as UserProfile;
    } catch {
      return null;
    }
  },

  async login(request: LoginRequest): Promise<ServerResult<AuthSession>> {
    const res = await apiClient.post(path("login/password"), request);
    const result = res.data as ServerResult<LoginResponse>;
    if (!result.isSuccess) return result as unknown as ServerResult<AuthSession>;
    const session = await fetchSession();
    return mapAuthSession(result, session);
  },

  async refresh(request: RefreshTokenRequest): Promise<ServerResult<AuthSession>> {
    const res = await apiClient.post(path("sessions/refresh"), request);
    const result = res.data as ServerResult<LoginResponse>;
    if (!result.isSuccess) return result as unknown as ServerResult<AuthSession>;
    const session = await fetchSession();
    return mapAuthSession(result, session);
  },

  async logout(): Promise<ServerResult<void>> {
    const res = await apiClient.post(path("logout"), {});
    return res.data as ServerResult<void>;
  },

  async getProfile(): Promise<ServerResult<Partial<UserProfile>>> {
    const res = await apiClient.get(path("profile"));
    const result = res.data as ServerResult<AuthProfileResponse>;
    if (!result.isSuccess) return result as unknown as ServerResult<Partial<UserProfile>>;
    return {
      ...result,
      value: mapProfileResponse(result.value),
    };
  },

  async updateProfile(data: UpdateProfileRequest): Promise<ServerResult<void>> {
    const res = await apiClient.put(path("profile"), data);
    return res.data as ServerResult<void>;
  },

  async changePassword(data: ChangePasswordRequest): Promise<ServerResult<void>> {
    const res = await apiClient.post(path("password/change"), {
      currentPassword: data.currentPassword,
      newPassword: data.newPassword,
      confirmNewPassword: data.confirmNewPassword,
    });
    return res.data as ServerResult<void>;
  },
};
