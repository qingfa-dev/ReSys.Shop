import { paymentRepository } from '../api/payment.api'

export const paymentService = {
  list: paymentRepository.list,
  getById: paymentRepository.getById,
  capture: paymentRepository.capture,
  void: paymentRepository.void,
  refund(id: string, amount?: number, reason?: string) {
    return paymentRepository.refund(id, amount !== undefined ? { amount, reason } : undefined)
  },
}
