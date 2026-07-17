import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useToast } from '@/shared/composables/toast.use'
import { stateService } from '../services/state.service'
import type { State } from '../types/State.Response.Type'
import type { CreateStateRequest, UpdateStateRequest } from '../types/State.Request.Type'

export const useStateStore = defineStore('state', () => {
  const { showToast } = useToast()
  const items = ref<State[]>([])
  const loading = ref(false)
  const submitting = ref(false)
  const totalRecords = ref(0)

  async function fetchStates(params?: Record<string, unknown>) {
    loading.value = true
    const result = await stateService.list(params)
    if (result.isSuccess) {
      items.value = result.items
      totalRecords.value = result.totalCount ?? result.items.length
    } else {
      showToast('error', 'Error', result.errors?.[0]?.message || 'Failed to load states')
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

  async function createState(data: CreateStateRequest) {
    submitting.value = true
    const result = await stateService.create(data)
    if (result.isSuccess) {
      showToast('success', 'Created', 'State created successfully')
      await fetchStates()
    }
    submitting.value = false
    return result
  }

  async function updateState(id: string, data: UpdateStateRequest) {
    submitting.value = true
    const result = await stateService.update(id, data)
    if (result.isSuccess) {
      showToast('success', 'Updated', 'State updated successfully')
      await fetchStates()
    }
    submitting.value = false
    return result
  }

  async function deleteState(id: string) {
    loading.value = true
    const result = await stateService.delete(id)
    if (result.isSuccess) {
      showToast('success', 'Deleted', 'State removed successfully')
      items.value = items.value.filter(i => i.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  return { items, loading, submitting, totalRecords, fetchStates, fetchStateById, createState, updateState, deleteState }
})
