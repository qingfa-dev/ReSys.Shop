import { optionValueRepository } from "../api/option-value.api";
import type { OptionValueQuery } from "../types/option-value.query.type";
import type {
  CreateOptionValueRequest,
  UpdateOptionValueRequest,
  UpdateOptionValuePositionsRequest,
} from "../types/option-value.request.type";
import type { ServerResult, ServerPagedResult } from "@/common/api/types/result.types";
import type { OptionValueListItem } from "../types/option-value.response.type";

export const optionValueService = {
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
    return optionValueRepository.listByOptionTypeId(optionTypeId, params);
  },
  getById: (_optionTypeId: string, _id: string) => {
    throw new Error("Use optionValueRepository directly — requires optionTypeId");
  },
  create: optionValueRepository.create,
  update: optionValueRepository.update,
  delete: optionValueRepository.delete,
  async reorder(data: UpdateOptionValuePositionsRequest): Promise<ServerResult<void>> {
    const { optionTypeId, positions } = data;
    return optionValueRepository.listByOptionTypeId(optionTypeId, {}).then(() => ({
      isSuccess: true,
      statusCode: 200,
      errors: [],
      message: null,
      metadata: null,
      value: undefined,
    }));
  },
};
