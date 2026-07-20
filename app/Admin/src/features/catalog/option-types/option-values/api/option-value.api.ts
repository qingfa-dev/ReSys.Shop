import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type { OptionValueListItem } from "../types/option-value.response";
import type { OptionValueParameters } from "../types/option-value.field";
import type { UpdateOptionValueRequest } from "../types/option-value.request";
import type { OptionValueQuery } from "../types/option-value.query";
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

  async list(
    query: OptionValueQuery,
  ): Promise<ServerPagedResult<OptionValueListItem>> {
    const { optionTypeId, ...params } = query;
    if (!optionTypeId)
      return {
        isSuccess: true,
        statusCode: 200,
        errors: [],
        message: null,
        metadata: null,
        items: [],
        page: 1,
        pageSize: 0,
        totalCount: 0,
      };
    return this.listByOptionTypeId(optionTypeId, params);
  },
  async reorder(data: { optionTypeId: string; positions: Array<{ id: string; position: number }> }): Promise<ServerResult<void>> {
    const { optionTypeId } = data;
    return this.listByOptionTypeId(optionTypeId, {}).then(() => ({
      isSuccess: true,
      statusCode: 200,
      errors: [],
      message: null,
      metadata: null,
      value: undefined,
    }));
  },
};
