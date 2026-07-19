import { paymentMethodRepository } from '../api/payment-method.api'

export const paymentMethodService = {
  list: paymentMethodRepository.list.bind(paymentMethodRepository),
  getById: paymentMethodRepository.getById.bind(paymentMethodRepository),
  create: paymentMethodRepository.create.bind(paymentMethodRepository),
  update: paymentMethodRepository.update.bind(paymentMethodRepository),
  delete: paymentMethodRepository.delete,
  activate: paymentMethodRepository.activate,
  deactivate: paymentMethodRepository.deactivate,
}
