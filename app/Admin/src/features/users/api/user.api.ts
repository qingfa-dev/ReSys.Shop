import apiClient from "@/common/api/http/api.client";
import { IDENTITY } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type { AdminUserSummary, CustomerSummary } from "../types/user.response.type";
import type {
  CreateAdminUserRequest,
  UpdateAdminUserRequest,
  UpdateUserStatusRequest,
  AssignRoleRequest,
  SyncRolesRequest,
  AssignPermissionRequest,
  SyncPermissionsRequest,
} from "../types/user.request.type";
import type { AdminUserSummaryModel } from "../types/user.model.type";
import { mapValue, mapItems } from "@/common/utils/transform";

export const userRepository = {
  async list(params?: ServerQueryingParameters): Promise<ServerPagedResult<AdminUserSummaryModel>> {
    const res = await apiClient.get(`${IDENTITY}/users`, { params });
    const data = res.data as ServerPagedResult<AdminUserSummary>;
    return mapItems(data, (d) => ({ ...d, hasRole: false, isLocked: false }));
  },

  listCustomers: (params?: ServerQueryingParameters): Promise<ServerPagedResult<CustomerSummary>> =>
    apiClient
      .get(`${IDENTITY}/users`, { params: { ...params, role: "Storefront.Customer" } })
      .then((res) => res.data as ServerPagedResult<CustomerSummary>),

  async getById(id: string): Promise<ServerResult<AdminUserSummaryModel>> {
    const res = await apiClient.get(`${IDENTITY}/users/${id}`);
    const data = res.data as ServerResult<AdminUserSummary>;
    return mapValue(data, (d) => ({ ...d, hasRole: false, isLocked: false }));
  },

  async create(data: CreateAdminUserRequest): Promise<ServerResult<AdminUserSummaryModel>> {
    const res = await apiClient.post(`${IDENTITY}/users`, data);
    const result = res.data as ServerResult<AdminUserSummary>;
    return mapValue(result, (d) => ({ ...d, hasRole: false, isLocked: false }));
  },

  async update(
    id: string,
    data: UpdateAdminUserRequest,
  ): Promise<ServerResult<AdminUserSummaryModel>> {
    const res = await apiClient.put(`${IDENTITY}/users/${id}`, data);
    const result = res.data as ServerResult<AdminUserSummary>;
    return mapValue(result, (d) => ({ ...d, hasRole: false, isLocked: false }));
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${IDENTITY}/users/${id}`).then((res) => res.data as ServerResult<void>),

  updateStatus: (id: string, data: UpdateUserStatusRequest): Promise<ServerResult<void>> =>
    apiClient
      .patch(`${IDENTITY}/users/${id}/status`, data)
      .then((res) => res.data as ServerResult<void>),

  getRoles: (id: string): Promise<ServerResult<string[]>> =>
    apiClient
      .get(`${IDENTITY}/users/${id}/roles`)
      .then((res) => res.data as ServerResult<string[]>),

  assignRole: (id: string, data: AssignRoleRequest): Promise<ServerResult<void>> =>
    apiClient
      .post(`${IDENTITY}/users/${id}/roles/assign`, data)
      .then((res) => res.data as ServerResult<void>),

  revokeRole: (id: string, data: AssignRoleRequest): Promise<ServerResult<void>> =>
    apiClient
      .post(`${IDENTITY}/users/${id}/roles/revoke`, data)
      .then((res) => res.data as ServerResult<void>),

  syncRoles: (id: string, data: SyncRolesRequest): Promise<ServerResult<void>> =>
    apiClient
      .patch(`${IDENTITY}/users/${id}/roles/sync`, data)
      .then((res) => res.data as ServerResult<void>),

  getPermissions: (id: string): Promise<ServerResult<string[]>> =>
    apiClient
      .get(`${IDENTITY}/users/${id}/permissions`)
      .then((res) => res.data as ServerResult<string[]>),

  assignPermission: (id: string, data: AssignPermissionRequest): Promise<ServerResult<void>> =>
    apiClient
      .post(`${IDENTITY}/users/${id}/permissions/assign`, data)
      .then((res) => res.data as ServerResult<void>),

  revokePermission: (id: string, data: AssignPermissionRequest): Promise<ServerResult<void>> =>
    apiClient
      .delete(`${IDENTITY}/users/${id}/permissions/revoke`, { data })
      .then((res) => res.data as ServerResult<void>),

  syncPermissions: (id: string, data: SyncPermissionsRequest): Promise<ServerResult<void>> =>
    apiClient
      .put(`${IDENTITY}/users/${id}/permissions/sync`, data)
      .then((res) => res.data as ServerResult<void>),
};
