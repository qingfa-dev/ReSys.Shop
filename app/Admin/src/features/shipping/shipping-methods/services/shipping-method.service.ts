import { shippingMethodRepository } from '../api/shipping-method.api'

export const shippingMethodService = {
  list: shippingMethodRepository.list.bind(shippingMethodRepository),
  getById: shippingMethodRepository.getById.bind(shippingMethodRepository),
  create: shippingMethodRepository.create.bind(shippingMethodRepository),
  update: shippingMethodRepository.update.bind(shippingMethodRepository),
  delete: shippingMethodRepository.delete,
  activate: shippingMethodRepository.activate,
  deactivate: shippingMethodRepository.deactivate,
}
