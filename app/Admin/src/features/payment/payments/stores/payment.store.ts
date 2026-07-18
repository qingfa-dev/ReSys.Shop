import { defineStore } from 'pinia'
import { ref } from 'vue'
import { paymentService } from '../services/payment.service'
import type { PaymentListItem, PaymentDetail } from '../types/Payment.Response.Type'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export const usePaymentStore = defineStore('payment', () => {
  const items = ref<PaymentListItem[]>([])
  const current = ref<PaymentDetail | null>(null)
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
