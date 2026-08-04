import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type { OptionValueListItem } from '../models/option-value.response'
import type { OptionValueParameters } from '../models/option-value.parameters'
import type { UpdateOptionValueRequest } from '../models/option-value.request'
import type { OptionValueQuery } from '../types/option-value.query'
import { OptionValueMapper } from "./option-value.mapper";

export const optionValueRepository = {
  listByOptionTypeId: async (
    optionTypeId: string,
    params?: ServerQueryingParameters,
  ): Promise<ServerPagedResult<OptionValueListItem>> => {
    const res = await apiClient.get(`${CATALOG}/option-types/${optionTypeId}/values`, { params });
    const result = res.data as ServerPagedResult<OptionValueListItem>;
    return { ...result, items: result.items?.map(OptionValueMapper.toListItem) ?? [] };
  },

  create: async (
    optionTypeId: string,
    data: OptionValueParameters,
  ): Promise<ServerResult<OptionValueListItem>> => {
    const res = await apiClient.post(`${CATALOG}/option-types/${optionTypeId}/values`, data);
    const result = res.data as ServerResult<OptionValueListItem>;
    if (result.value) result.value = OptionValueMapper.toListItem(result.value)
    return result;
  },

  update: async (
    optionTypeId: string,
    valueId: string,
    data: UpdateOptionValueRequest,
  ): Promise<ServerResult<OptionValueListItem>> => {
    const res = await apiClient.put(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`, data);
    const result = res.data as ServerResult<OptionValueListItem>;
    if (result.value) result.value = OptionValueMapper.toListItem(result.value)
    return result;
  },

  delete: (optionTypeId: string, valueId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`)
      .then((res) => res.data as ServerResult<void>),

  async list(query: OptionValueQuery): Promise<ServerPagedResult<OptionValueListItem>> {
    const { optionTypeId, ...params } = query;
    if (!optionTypeId) {
      return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 0, totalCount: 0 };
    }
    return this.listByOptionTypeId(optionTypeId, params);
  },

  async reorder(data: { optionTypeId: string; positions: Array<{ id: string; position: number }> }): Promise<ServerResult<void>> {
    const { optionTypeId } = data;
    return this.listByOptionTypeId(optionTypeId, {}).then(() => ({
      isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined,
    }));
  },
};
