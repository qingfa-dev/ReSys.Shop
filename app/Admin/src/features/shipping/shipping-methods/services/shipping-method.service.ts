import { shippingMethodRepository } from '../api/shipping-method.api'

export const shippingMethodService = {
  list: shippingMethodRepository.list,
  getById: shippingMethodRepository.getById,
  create: shippingMethodRepository.create,
  update: shippingMethodRepository.update,
  delete: shippingMethodRepository.delete,
  activate: shippingMethodRepository.activate,
  deactivate: shippingMethodRepository.deactivate,
}
