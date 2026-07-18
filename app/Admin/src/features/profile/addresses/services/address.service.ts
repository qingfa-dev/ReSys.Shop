import { addressApi } from '../api/address.api'

export const addressService = {
  getAll: addressApi.getAll,
  getById: addressApi.getById,
  create: addressApi.create,
  update: addressApi.update,
  delete: addressApi.delete,
}
