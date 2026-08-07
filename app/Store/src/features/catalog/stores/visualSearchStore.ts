import { defineStore } from 'pinia'
import { ref } from 'vue'
import { SearchByImageApi } from '../services/searchByImageApi'
import type { SearchByImageResponse, VisualSearchModel } from '../types'

type VisualSearchState = 'empty' | 'upload' | 'loading' | 'results'

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024

export const useVisualSearchStore = defineStore('visualSearch', () => {
  const state = ref<VisualSearchState>('empty')
  const selectedFile = ref<File | null>(null)
  const previewUrl = ref<string | null>(null)
  const selectedModelId = ref<string | null>(null)
  const availableModels = ref<VisualSearchModel[]>([])
  const results = ref<SearchByImageResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const validationError = ref<string | null>(null)

  function validateFile(file: File): boolean {
    if (!ALLOWED_TYPES.includes(file.type)) {
      validationError.value = 'Invalid file type. Use JPEG, PNG, or WebP.'
      return false
    }
    if (file.size > MAX_SIZE) {
      validationError.value = 'File exceeds 10 MB limit.'
      return false
    }
    validationError.value = null
    return true
  }

  function selectFile(file: File): void {
    if (!validateFile(file)) return
    selectedFile.value = file
    previewUrl.value = URL.createObjectURL(file)
    state.value = 'upload'
  }

  async function search(topK?: number, model?: string): Promise<void> {
    if (!selectedFile.value) return
    state.value = 'loading'
    loading.value = true
    error.value = null
    const result = await SearchByImageApi.searchByImage(selectedFile.value, topK ?? 20, model ?? selectedModelId.value ?? undefined)
    if (result.isSuccess) {
      results.value = result.items
      state.value = 'results'
    } else {
      error.value = result.message ?? 'Visual search failed'
      state.value = 'upload'
    }
    loading.value = false
  }

  async function loadModels(): Promise<void> {
    const result = await SearchByImageApi.getVisualSearchModels()
    if (result.isSuccess) availableModels.value = result.value
  }

  function reset(): void {
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    selectedFile.value = null
    previewUrl.value = null
    results.value = []
    error.value = null
    validationError.value = null
    state.value = 'empty'
  }

  return {
    state, selectedFile, previewUrl, selectedModelId, availableModels, results,
    loading, error, validationError,
    validateFile, selectFile, search, loadModels, reset,
  }
})
