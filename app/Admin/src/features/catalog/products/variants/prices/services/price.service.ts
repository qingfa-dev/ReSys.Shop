import { priceApi } from '../api/price.api'

export const priceService = {
  listPrices: priceApi.listPrices,
  setPrice: priceApi.setPrice,
  deletePrice: priceApi.deletePrice,
  syncPrices: priceApi.syncPrices,
}
