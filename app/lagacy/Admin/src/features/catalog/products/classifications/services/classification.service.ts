import { productClassificationApi } from '../api/product-classification.api'

export const classificationService = {
  getClassifications: productClassificationApi.getClassifications,
  syncClassifications: productClassificationApi.syncClassifications,
}
