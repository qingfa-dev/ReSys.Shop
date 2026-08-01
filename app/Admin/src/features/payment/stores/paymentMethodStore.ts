import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { PaymentMethodListItem } from '../types/paymentMethod'
import { PaymentMethodApi } from '../services/paymentMethodApi'

export const usePaymentMethodStore = defineStore('paymentMethods', () => {
  const activePaymentMethods = ref<PaymentMethodListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await PaymentMethodApi.getPaymentMethods({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    if (result.isSuccess) {
      activePaymentMethods.value = result.items
      loaded.value = true
    }
  }

  return { activePaymentMethods, loaded, fetchActive }
})
