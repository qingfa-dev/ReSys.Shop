import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerPagedResult, ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type { OptionValueListItem } from "../types/option-value.response.type";
import type { OptionValueParameters } from "../schemas/option-value.schema";
import type { UpdateOptionValueRequest } from "../types/option-value.request.type";
export const optionValueRepository = {
  listByOptionTypeId: async (
    optionTypeId: string,
    params?: ServerQueryingParameters,
  ): Promise<ServerPagedResult<OptionValueListItem>> => {
    return apiClient
      .get(`${CATALOG}/option-types/${optionTypeId}/values`, { params })
      .then((res) => res.data as ServerPagedResult<OptionValueListItem>)
  },

  create: async (
    optionTypeId: string,
    data: OptionValueParameters,
  ): Promise<ServerResult<OptionValueListItem>> => {
    return apiClient
      .post(`${CATALOG}/option-types/${optionTypeId}/values`, data)
      .then((res) => res.data as ServerResult<OptionValueListItem>)
  },

  update: async (
    optionTypeId: string,
    valueId: string,
    data: UpdateOptionValueRequest,
  ): Promise<ServerResult<OptionValueListItem>> => {
    return apiClient
      .put(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`, data)
      .then((res) => res.data as ServerResult<OptionValueListItem>)
  },

  delete: (optionTypeId: string, valueId: string): Promise<ServerResult<void>> =>
    apiClient
      .delete(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`)
      .then((res) => res.data as ServerResult<void>),
};
