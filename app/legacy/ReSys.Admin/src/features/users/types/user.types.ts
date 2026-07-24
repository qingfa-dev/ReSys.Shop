export interface AdminUserSummary {
  id: string;
  email: string;
  user_name: string | null;
  first_name: string | null;
  last_name: string | null;
  full_name: string | null;
  role_names: string[];
  is_active: boolean;
  created_at: string;
  // Security Fields
  phone_number?: string | null;
  email_confirmed?: boolean;
  phone_number_confirmed?: boolean;
  access_failed_count?: number;
  lockout_end?: string | null;
  last_sign_in_at?: string | null;
  last_ip_address?: string | null;
}

export interface RoleSummary {
  id: string;
  name: string;
  display_name: string | null;
  description: string | null;
  priority: number;
  is_system_role: boolean;
  is_default: boolean;
  user_count: number;
}

export interface PermissionSummary {
  name: string;
  display_name: string;
  description: string | null;
  module: string;
}

export interface CustomerSummary {
  id: string;
  email: string;
  first_name: string | null;
  last_name: string | null;
  full_name: string | null;
  order_count: number;
  total_spent_cents: number;
  is_active: boolean;
  created_at: string;
}

export interface UserSearchParams {
  page?: number;
  page_size?: number;
  search?: string;
  is_active?: boolean;
  role?: string;
  sort_by?: string;
  is_descending?: boolean;
  filter?: string;
}

export interface CreateAdminUserRequest {
  email: string;
  first_name: string;
  last_name: string;
  role: string[];
  password?: string;
}

export interface UpdateAdminUserRequest {
  first_name?: string;
  last_name?: string;
  role?: string[];
  is_active?: boolean;
}

export interface ResetPasswordRequest {
  new_password: string;
}

export interface VerifyUserRequest {
  verify_email: boolean;
  verify_phone: boolean;
}

export interface CreateRoleRequest {
  name: string;
  display_name?: string;
  description?: string;
  priority: number;
}

export interface UpdateRoleRequest {
  display_name?: string;
  description?: string;
  priority?: number;
}

export interface UpdateStaffProfileRequest {
  employee_id?: string;
  department?: string;
  position?: string;
}
