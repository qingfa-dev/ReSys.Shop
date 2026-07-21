import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type {
  CreateOptionTypeRequest,
  OptionTypeDetail,
  OptionTypeListItem,
  UpdateOptionTypeRequest,
} from "../models";
import { OptionTypeMapper } from "./option-type.mapper";

export const optionTypeRepository = {
  list: async (
    params?: ServerQueryingParameters,
  ): Promise<ServerPagedResult<OptionTypeListItem>> => {
    const res = await apiClient.get(`${CATALOG}/option-types`, { params });
    const result = res.data as ServerPagedResult<OptionTypeListItem>;
    return { ...result, items: result.items?.map(OptionTypeMapper.toListItem) ?? [] };
  },

  getById: async (id: string): Promise<ServerResult<OptionTypeDetail>> => {
    const res = await apiClient.get(`${CATALOG}/option-types/${id}`);
    const result = res.data as ServerResult<OptionTypeDetail>;
    if (result.value) result.value = OptionTypeMapper.toDetail(result.value);
    return result;
  },

  create: async (data: CreateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> => {
    const res = await apiClient.post(`${CATALOG}/option-types`, data);
    const result = res.data as ServerResult<OptionTypeDetail>;
    if (result.value) result.value = OptionTypeMapper.toDetail(result.value);
    return result;
  },

  update: async (
    id: string,
    data: UpdateOptionTypeRequest,
  ): Promise<ServerResult<OptionTypeDetail>> => {
    const res = await apiClient.put(`${CATALOG}/option-types/${id}`, data);
    const result = res.data as ServerResult<OptionTypeDetail>;
    if (result.value) result.value = OptionTypeMapper.toDetail(result.value);
    return result;
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/option-types/${id}`).then((res) => res.data as ServerResult<void>),
};
