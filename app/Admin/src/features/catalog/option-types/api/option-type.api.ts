import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type {
  OptionTypeDetail,
  OptionTypeListItem,
} from "../types/OptionType.Response.Type";

export const optionTypeRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerResult<OptionTypeListItem[]>> =>
    apiClient
      .get(`${CATALOG}/option-types`, { params })
      .then((res) => res.data as ServerResult<OptionTypeListItem[]>),

  getById: (id: string): Promise<ServerResult<OptionTypeDetail>> =>
    apiClient
      .get(`${CATALOG}/option-types/${id}`)
      .then((res) => res.data as ServerResult<OptionTypeDetail>),

  create: (data: {
    name: string;
    presentation: string;
    filterable?: boolean;
    position?: number;
  }): Promise<ServerResult<OptionTypeDetail>> =>
    apiClient
      .post(`${CATALOG}/option-types`, data)
      .then((res) => res.data as ServerResult<OptionTypeDetail>),

  update: (
    id: string,
    data: Partial<{ name: string; presentation: string; filterable: boolean; position: number }>,
  ): Promise<ServerResult<OptionTypeDetail>> =>
    apiClient
      .put(`${CATALOG}/option-types/${id}`, data)
      .then((res) => res.data as ServerResult<OptionTypeDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/option-types/${id}`).then((res) => res.data as ServerResult<void>),
};
