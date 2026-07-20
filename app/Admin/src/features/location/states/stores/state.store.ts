import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useToast } from '@/common/composables/toast.use'
import { useI18n } from 'vue-i18n'
import { stateService } from '../services/state.service'
import type { State } from '../types/state.response.type'
import type { CreateStateRequest, UpdateStateRequest } from '../types/state.request.type'

export const useStateStore = defineStore('state', () => {
  const { showToast } = useToast()
  const { t } = useI18n()
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
      showToast('error', t('common.error'), result.errors?.[0]?.message || t('location.messages.state_load_error'))
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
      showToast('success', t('common.created'), t('location.messages.state_create_success'))
      await fetchStates()
    }
    submitting.value = false
    return result
  }

  async function updateState(id: string, data: UpdateStateRequest) {
    submitting.value = true
    const result = await stateService.update(id, data)
    if (result.isSuccess) {
      showToast('success', t('common.updated'), t('location.messages.state_update_success'))
      await fetchStates()
    }
    submitting.value = false
    return result
  }

  async function deleteState(id: string) {
    loading.value = true
    const result = await stateService.delete(id)
    if (result.isSuccess) {
      showToast('success', t('common.deleted'), t('location.messages.state_delete_success'))
      items.value = items.value.filter(i => i.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  return { items, loading, submitting, totalRecords, fetchStates, fetchStateById, createState, updateState, deleteState }
})
