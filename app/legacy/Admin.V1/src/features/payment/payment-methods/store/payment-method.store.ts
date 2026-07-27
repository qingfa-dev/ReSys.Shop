import { defineStore } from 'pinia'
import { ref } from 'vue'
import { paymentMethodRepository } from '../api/payment-method.api'
import type { PaymentMethodListItemModel, PaymentMethodDetailModel } from '../types/payment-method.model'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export const usePaymentMethodStore = defineStore('paymentMethod', () => {
  const items = ref<PaymentMethodListItemModel[]>([])
  const current = ref<PaymentMethodDetailModel | null>(null)
  const loading = ref(false)
  const totalRecords = ref(0)

  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 20, sort: ['position'] })

  async function fetchItems(params?: ServerQueryingParameters) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await paymentMethodRepository.list(query.value)
    if (result.isSuccess) {
      items.value = result.items
      totalRecords.value = result.totalCount
    }
    loading.value = false
    return result
  }

  async function fetchById(id: string) {
    const result = await paymentMethodRepository.getById(id)
    if (result.isSuccess) current.value = result.value
    return result
  }

  return { items, current, loading, totalRecords, query, fetchItems, fetchById }
})
