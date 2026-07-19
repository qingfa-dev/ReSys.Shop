import { productOptionTypeApi } from '../api/product-option-type.api'

export const productOptionTypeService = {
  getOptionTypes: productOptionTypeApi.getOptionTypes,
  syncOptionTypes: productOptionTypeApi.syncOptionTypes,
}
