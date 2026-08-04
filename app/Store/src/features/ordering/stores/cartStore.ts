import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useCartStore = defineStore('cart', () => {
  const items = ref<Array<{ id: string; quantity: number }>>([])
  const itemCount = computed(() => items.value.reduce((sum, i) => sum + i.quantity, 0))

  return { items, itemCount }
})
