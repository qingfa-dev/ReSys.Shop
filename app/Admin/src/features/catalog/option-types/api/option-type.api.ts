import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerPagedResult, ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type {
  CreateOptionTypeRequest,
  UpdateOptionTypeRequest,
} from "../types/option-type.request.type";
import type {
  OptionTypeListItem,
  OptionTypeDetail,
} from "../../products/option-types/types/product-option-type.response.type";
export const optionTypeRepository = {
  list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<OptionTypeListItem>> => {
    return apiClient
      .get(`${CATALOG}/option-types`, { params })
      .then((res) => res.data as ServerPagedResult<OptionTypeListItem>)
  },

  getById: async (id: string): Promise<ServerResult<OptionTypeListItem>> => {
    return apiClient
      .get(`${CATALOG}/option-types/${id}`)
      .then((res) => res.data as ServerResult<OptionTypeListItem>)
  },

  create: async (data: CreateOptionTypeRequest): Promise<ServerResult<OptionTypeListItem>> => {
    return apiClient
      .post(`${CATALOG}/option-types`, data)
      .then((res) => res.data as ServerResult<OptionTypeListItem>)
  },

  update: async (id: string, data: UpdateOptionTypeRequest): Promise<ServerResult<OptionTypeListItem>> => {
    return apiClient
      .put(`${CATALOG}/option-types/${id}`, data)
      .then((res) => res.data as ServerResult<OptionTypeListItem>)
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/option-types/${id}`).then((res) => res.data as ServerResult<void>),
};
