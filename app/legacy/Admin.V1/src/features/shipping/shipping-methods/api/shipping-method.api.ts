import apiClient from "@/common/api/http/api.client";
import { SHIPPING } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type {
  ShippingMethodListItem,
  ShippingMethodDetail,
  CreateShippingMethodRequest,
  UpdateShippingMethodRequest,
} from "../types";
import type {
  ShippingMethodListItemModel,
  ShippingMethodDetailModel,
} from "../types/shipping-method.model";
import { mapValue, mapItems } from "@/common/utils/transform";

function methodsPath(sub?: string): string {
  return `${SHIPPING}/shipping-methods${sub ? `/${sub}` : ""}`;
}

export const shippingMethodRepository = {
  async list(
    params?: ServerQueryingParameters,
  ): Promise<ServerPagedResult<ShippingMethodListItemModel>> {
    const result = await apiClient
      .get(methodsPath(), { params })
      .then((res) => res.data as ServerPagedResult<ShippingMethodListItem>);
    if (result.isSuccess) {
      return mapItems(result, (d) => ({ ...d, statusLabel: d.isActive ? "Active" : "Inactive" }));
    }
    return result as ServerPagedResult<ShippingMethodListItemModel>;
  },

  async getById(id: string): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await apiClient
      .get(methodsPath(id))
      .then((res) => res.data as ServerResult<ShippingMethodDetail>);
    if (result.isSuccess) {
      return mapValue(result, (d) => ({ ...d, statusLabel: d.isActive ? "Active" : "Inactive" }));
    }
    return result as ServerResult<ShippingMethodDetailModel>;
  },

  async create(
    data: CreateShippingMethodRequest,
  ): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await apiClient
      .post(methodsPath(), data)
      .then((res) => res.data as ServerResult<ShippingMethodDetail>);
    if (result.isSuccess) {
      return mapValue(result, (d) => ({ ...d, statusLabel: d.isActive ? "Active" : "Inactive" }));
    }
    return result as ServerResult<ShippingMethodDetailModel>;
  },

  async update(
    id: string,
    data: UpdateShippingMethodRequest,
  ): Promise<ServerResult<ShippingMethodDetailModel>> {
    const result = await apiClient
      .put(methodsPath(id), data)
      .then((res) => res.data as ServerResult<ShippingMethodDetail>);
    if (result.isSuccess) {
      return mapValue(result, (d) => ({ ...d, statusLabel: d.isActive ? "Active" : "Inactive" }));
    }
    return result as ServerResult<ShippingMethodDetailModel>;
  },

  async delete(id: string): Promise<ServerResult<void>> {
    const res = await apiClient.delete(methodsPath(id));
    return res.data as ServerResult<void>;
  },

  async activate(id: string): Promise<ServerResult<void>> {
    const res = await apiClient
      .patch(methodsPath(`${id}/activate`));
    return res.data as ServerResult<void>;
  },

  async deactivate(id: string): Promise<ServerResult<void>> {
    const res = await apiClient
      .patch(methodsPath(`${id}/deactivate`));
    return res.data as ServerResult<void>;
  },
};
