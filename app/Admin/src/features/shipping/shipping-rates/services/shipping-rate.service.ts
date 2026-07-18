import { shippingRateRepository } from '../api/shipping-rate.api'

export const shippingRateService = {
  list: shippingRateRepository.list,
  getById: shippingRateRepository.getById,
  create: shippingRateRepository.create,
  update: shippingRateRepository.update,
  delete: shippingRateRepository.delete,
}
