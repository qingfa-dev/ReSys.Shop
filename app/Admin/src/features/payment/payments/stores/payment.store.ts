import { defineStore } from 'pinia'
import { ref } from 'vue'
import { paymentService } from '../services/payment.service'
import type { PaymentListItemModel, PaymentDetailModel } from '../types/payment.model.type'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export const usePaymentStore = defineStore('payment', () => {
  const items = ref<PaymentListItemModel[]>([])
  const current = ref<PaymentDetailModel | null>(null)
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 20, sort: ['-createdAtUtc'] })

  async function fetchItems(params?: ServerQueryingParameters) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await paymentService.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount }
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await paymentService.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  async function capture(id: string, amount?: number) {
    return paymentService.capture(id, amount)
  }

  async function voidPayment(id: string) {
    return paymentService.void(id)
  }

  async function refund(id: string, amount: number, reason?: string) {
    return paymentService.refund(id, amount, reason)
  }

  return { items, current, loading, totalRecords, query, fetchItems, fetchById, capture, void: voidPayment, refund }
})
