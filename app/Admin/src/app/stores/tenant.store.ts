import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useTenantStore = defineStore('tenant', () => {
  const currentTenantId = ref<string | null>(null)
  function setTenant(id: string) {
    currentTenantId.value = id
  }
  return { currentTenantId, setTenant }
})
