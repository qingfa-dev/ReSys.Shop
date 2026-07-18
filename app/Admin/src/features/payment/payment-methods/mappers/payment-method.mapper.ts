import type { PaymentMethodListItem, PaymentMethodDetail } from '../types/payment-method.response.type'
import { toPaymentMethodListItemModel, toPaymentMethodDetailModel, type PaymentMethodListItemModel, type PaymentMethodDetailModel } from '../types/payment-method.model.type'

export function mapPaymentMethodListItem(dto: PaymentMethodListItem): PaymentMethodListItemModel {
  return toPaymentMethodListItemModel(dto)
}

export function mapPaymentMethodDetail(dto: PaymentMethodDetail): PaymentMethodDetailModel {
  return toPaymentMethodDetailModel(dto)
}
