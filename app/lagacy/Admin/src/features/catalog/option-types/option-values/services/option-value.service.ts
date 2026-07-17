import { catalogApi } from '../../../services/catalog.api'

export const optionValueService = {
  list: catalogApi.optionTypes.listValues,
  getById: (_optionTypeId: string, _id: string) => {
    throw new Error('Use catalogApi.optionTypes directly — requires optionTypeId')
  },
  create: catalogApi.optionTypes.createValue,
  update: catalogApi.optionTypes.updateValue,
  delete: catalogApi.optionTypes.deleteValue,
}
