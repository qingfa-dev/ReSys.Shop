import type { PaymentListItem, PaymentDetail } from '../types/payment.response.type'
import { toPaymentListItemModel, toPaymentDetailModel, type PaymentListItemModel, type PaymentDetailModel } from '../types/payment.model.type'

export function mapPaymentListItem(dto: PaymentListItem): PaymentListItemModel {
  return toPaymentListItemModel(dto)
}

export function mapPaymentDetail(dto: PaymentDetail): PaymentDetailModel {
  return toPaymentDetailModel(dto)
}
