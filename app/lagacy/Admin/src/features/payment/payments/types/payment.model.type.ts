import type { PaymentListItem, PaymentDetail } from './payment.response.type'
import { PaymentStateMap } from '@/shared/utils/enums'
import { decimalToDisplay } from '@/shared/utils/currency'

export interface PaymentListItemModel extends PaymentListItem {
  amountDisplay: string
  statusLabel: string
}

export interface PaymentDetailModel extends PaymentDetail {
  amountDisplay: string
  statusLabel: string
}

export function toPaymentListItemModel(dto: PaymentListItem): PaymentListItemModel {
  return {
    ...dto,
    amountDisplay: decimalToDisplay(dto.amount),
    statusLabel: PaymentStateMap[dto.status] ?? 'Unknown',
  }
}

export function toPaymentDetailModel(dto: PaymentDetail): PaymentDetailModel {
  return {
    ...dto,
    amountDisplay: decimalToDisplay(dto.amount),
    statusLabel: PaymentStateMap[dto.status] ?? 'Unknown',
  }
}
