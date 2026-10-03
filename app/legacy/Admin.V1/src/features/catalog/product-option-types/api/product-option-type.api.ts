import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerResult } from "@/common/api/types/result.types";
import type { ProductOptionTypeItem } from "../models/product-option-type.response";
import { ProductOptionTypeMapper } from "./product-option-type.mapper";

export const productOptionTypeApi = {
  getOptionTypes: async (productId: string): Promise<ServerResult<ProductOptionTypeItem[]>> => {
    const res = await apiClient.get(`${CATALOG}/products/${productId}/option-types`);
    const raw = res.data as ServerResult<Record<string, unknown>[]>;
    if (raw.isSuccess && raw.value) {
      return { ...raw, value: raw.value.map(ProductOptionTypeMapper.toOptionTypeDetail) };
    }
    return raw as unknown as ServerResult<ProductOptionTypeItem[]>;
  },

  syncOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds }).then((res) => res.data as ServerResult<void>),

  assignOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/products/${productId}/option-types/assign`, { optionTypeIds }).then((res) => res.data as ServerResult<void>),

  revokeOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${productId}/option-types/revoke`, { data: { optionTypeIds } }).then((res) => res.data as ServerResult<void>),
};
