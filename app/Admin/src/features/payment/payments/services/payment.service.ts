import { paymentRepository } from '../api/payment.api'

export const paymentService = {
  list: paymentRepository.list.bind(paymentRepository),
  getById: paymentRepository.getById.bind(paymentRepository),
  capture: paymentRepository.capture,
  void: paymentRepository.void,
  refund(id: string, amount?: number, reason?: string) {
    return paymentRepository.refund(id, amount !== undefined ? { amount, reason } : undefined)
  },
}
