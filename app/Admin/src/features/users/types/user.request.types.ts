import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

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
