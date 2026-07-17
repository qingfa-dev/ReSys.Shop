export interface AdminUserSummary {
  id: string;
  email: string;
  userName: string | null;
  firstName: string | null;
  lastName: string | null;
  fullName: string | null;
  roleNames: string[];
  isActive: boolean;
  createdAtUtc: string;
  // Security Fields
  phoneNumber?: string | null;
  emailConfirmed?: boolean;
  phoneNumberConfirmed?: boolean;
  accessFailedCount?: number;
  lockoutEnd?: string | null;
  lastSignInAtUtc?: string | null;
  lastIpAddress?: string | null;
}

export interface RoleSummary {
  id: string;
  name: string;
  displayName: string | null;
  description: string | null;
  priority: number;
  isSystem: boolean;
  isDefault: boolean;
  userCount: number;
}

export interface PermissionSummary {
  identifier: string;
  name: string;
  description: string | null;
  action: string;
}

export interface CustomerSummary {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  fullName: string | null;
  ordersCount: number;
  totalSpent: number;
  isActive: boolean;
  createdAtUtc: string;
}

import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'

export interface UserSearchParams extends ServerQueryingParameters {
  isActive?: boolean
  role?: string
}

export interface CreateAdminUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: string[];
  password?: string;
  phoneNumber?: string;
  emailConfirmed?: boolean;
}

export interface UpdateAdminUserRequest {
  firstName?: string;
  lastName?: string;
  role?: string[];
  isActive?: boolean;
  phoneNumber?: string;
}

export interface ResetPasswordRequest {
  new_password: string;
}

export interface VerifyUserRequest {
  verifyEmail: boolean;
  verifyPhone: boolean;
}

export interface CreateRoleRequest {
  name: string;
  displayName?: string;
  description?: string;
  priority: number;
}

export interface UpdateRoleRequest {
  displayName?: string;
  description?: string;
  priority?: number;
}

export interface UpdateStaffProfileRequest {
  employee_id?: string;
  department?: string;
  position?: string;
}
