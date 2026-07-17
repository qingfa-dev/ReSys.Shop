import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useToast } from '@/shared/composables/toast.use'
import { stateService } from '../services/state.service'
import type { State } from '../types/state.types'

export const useStateStore = defineStore('state', () => {
  const { showToast } = useToast()
  const items = ref<State[]>([])
  const loading = ref(false)
  const submitting = ref(false)
  const totalRecords = ref(0)

  async function fetchStates(params?: Record<string, unknown>) {
    loading.value = true
    const result = await stateService.list(params)
    if (result.success) {
      items.value = result.data
      totalRecords.value = result.meta?.totalCount ?? result.data.length
    } else {
      showToast('error', 'Error', result.error?.detail || 'Failed to load states')
    }
    loading.value = false
    return result
  }

  async function fetchStateById(id: string) {
    loading.value = true
    const result = await stateService.getById(id)
    loading.value = false
    return result
  }

  async function createState(data: import('../types/state.types').StateCreateRequest) {
    submitting.value = true
    const result = await stateService.create(data)
    if (result.success) {
      showToast('success', 'Created', 'State created successfully')
      await fetchStates()
    }
    submitting.value = false
    return result
  }

  async function updateState(id: string, data: import('../types/state.types').StateUpdateRequest) {
    submitting.value = true
    const result = await stateService.update(id, data)
    if (result.success) {
      showToast('success', 'Updated', 'State updated successfully')
      await fetchStates()
    }
    submitting.value = false
    return result
  }

  async function deleteState(id: string) {
    loading.value = true
    const result = await stateService.delete(id)
    if (result.success) {
      showToast('success', 'Deleted', 'State removed successfully')
      items.value = items.value.filter(i => i.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  return { items, loading, submitting, totalRecords, fetchStates, fetchStateById, createState, updateState, deleteState }
})
