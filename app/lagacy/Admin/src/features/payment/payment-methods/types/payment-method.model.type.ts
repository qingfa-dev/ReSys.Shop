import type { PaymentMethodListItem, PaymentMethodDetail } from './payment-method.response.type'

export interface PaymentMethodListItemModel extends PaymentMethodListItem {
  statusLabel: string
}

export type PaymentMethodDetailModel = PaymentMethodListItemModel

export function toPaymentMethodListItemModel(dto: PaymentMethodListItem): PaymentMethodListItemModel {
  return {
    ...dto,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
  }
}

export function toPaymentMethodDetailModel(dto: PaymentMethodDetail): PaymentMethodDetailModel {
  return toPaymentMethodListItemModel(dto)
}
