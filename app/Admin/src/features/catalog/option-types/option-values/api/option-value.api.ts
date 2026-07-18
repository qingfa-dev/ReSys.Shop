import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerPagedResult, ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type { OptionValueListItem } from "../types/OptionValue.Response.Type";

export const optionValueRepository = {
  listByOptionTypeId: (
    optionTypeId: string,
    params?: ServerQueryingParameters,
  ): Promise<ServerPagedResult<OptionValueListItem>> =>
    apiClient
      .get(`${CATALOG}/option-types/${optionTypeId}/values`, { params })
      .then((res) => res.data as ServerPagedResult<OptionValueListItem>),

  create: (
    optionTypeId: string,
    data: { name: string; presentation: string; position?: number },
  ): Promise<ServerResult<OptionValueListItem>> =>
    apiClient
      .post(`${CATALOG}/option-types/${optionTypeId}/values`, data)
      .then((res) => res.data as ServerResult<OptionValueListItem>),

  update: (
    optionTypeId: string,
    valueId: string,
    data: { name?: string; presentation?: string; position?: number },
  ): Promise<ServerResult<OptionValueListItem>> =>
    apiClient
      .put(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`, data)
      .then((res) => res.data as ServerResult<OptionValueListItem>),

  delete: (optionTypeId: string, valueId: string): Promise<ServerResult<void>> =>
    apiClient
      .delete(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`)
      .then((res) => res.data as ServerResult<void>),
};
