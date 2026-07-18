import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerPagedResult, ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type {
  CreateOptionTypeRequest,
  UpdateOptionTypeRequest,
} from "../types/OptionType.Request.Type";
import type {
  OptionTypeListItem,
  OptionTypeDetail,
} from "../../products/option-types/types/ProductOptionType.Response.Type";

export const optionTypeRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerPagedResult<OptionTypeListItem>> =>
    apiClient
      .get(`${CATALOG}/option-types`, { params })
      .then((res) => res.data as ServerPagedResult<OptionTypeListItem>),

  getById: (id: string): Promise<ServerResult<OptionTypeDetail>> =>
    apiClient
      .get(`${CATALOG}/option-types/${id}`)
      .then((res) => res.data as ServerResult<OptionTypeDetail>),

  create: (data: CreateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> =>
    apiClient
      .post(`${CATALOG}/option-types`, data)
      .then((res) => res.data as ServerResult<OptionTypeDetail>),

  update: (id: string, data: UpdateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> =>
    apiClient
      .put(`${CATALOG}/option-types/${id}`, data)
      .then((res) => res.data as ServerResult<OptionTypeDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/option-types/${id}`).then((res) => res.data as ServerResult<void>),
};
