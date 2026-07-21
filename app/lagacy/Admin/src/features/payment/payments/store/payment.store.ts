import { defineStore } from 'pinia'
import { ref } from 'vue'
import { paymentRepository } from '../api/payment.api'
import type { PaymentListItemModel, PaymentDetailModel } from '../types/payment.model'
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
    const result = await paymentRepository.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount }
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await paymentRepository.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  async function capture(id: string, amount?: number) {
    return paymentRepository.capture(id, amount)
  }

  async function voidPayment(id: string) {
    return paymentRepository.void(id)
  }

  async function refund(id: string, amount: number, reason?: string) {
    return paymentRepository.refund(id, amount !== undefined ? { amount, reason } : undefined)
  }

  return { items, current, loading, totalRecords, query, fetchItems, fetchById, capture, void: voidPayment, refund }
})
