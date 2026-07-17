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
  phoneNumber?: string | null;
  emailConfirmed?: boolean;
  phoneNumberConfirmed?: boolean;
  accessFailedCount?: number;
  lockoutEnd?: string | null;
  lastSignInAtUtc?: string | null;
  lastIpAddress?: string | null;
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
