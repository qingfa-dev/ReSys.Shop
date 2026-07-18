import { paymentMethodRepository } from '../api/payment-method.api'

export const paymentMethodService = {
  list: paymentMethodRepository.list,
  getById: paymentMethodRepository.getById,
  create: paymentMethodRepository.create,
  update: paymentMethodRepository.update,
  delete: paymentMethodRepository.delete,
  activate: paymentMethodRepository.activate,
  deactivate: paymentMethodRepository.deactivate,
}
