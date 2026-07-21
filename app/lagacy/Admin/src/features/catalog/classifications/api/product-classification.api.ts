import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerResult } from "@/common/api/types/result.types";
import type { ProductClassification } from "../models/classification.response";
import type { SyncClassificationsRequest } from "../models/classification.request";
import { ClassificationMapper } from "./classification.mapper";

export const productClassificationApi = {
  getClassifications: async (productId: string): Promise<ServerResult<ProductClassification[]>> => {
    const res = await apiClient.get(`${CATALOG}/products/${productId}/classifications`);
    const result = res.data as ServerResult<ProductClassification[]>;
    if (result.value) result.value = result.value.map(ClassificationMapper.toClassification)
    return result;
  },

  syncClassifications: (productId: string, data: SyncClassificationsRequest): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data).then((res) => res.data as ServerResult<void>),

  assignClassifications: (productId: string, taxonIds: string[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/products/${productId}/classifications/assign`, { taxonIds }).then((res) => res.data as ServerResult<void>),

  revokeClassifications: (productId: string, taxonIds: string[]): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${productId}/classifications/revoke`, { data: { taxonIds } }).then((res) => res.data as ServerResult<void>),
};
