import { shippingRateRepository } from '../api/shipping-rate.api'

export const shippingRateService = {
  list: shippingRateRepository.list.bind(shippingRateRepository),
  getById: shippingRateRepository.getById.bind(shippingRateRepository),
  create: shippingRateRepository.create.bind(shippingRateRepository),
  update: shippingRateRepository.update.bind(shippingRateRepository),
  delete: shippingRateRepository.delete,
}
