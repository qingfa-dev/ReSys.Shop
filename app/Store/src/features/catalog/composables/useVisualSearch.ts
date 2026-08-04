import { onUnmounted, ref } from 'vue'
import { searchByImage } from '../services/searchByImageApi'
import type { SearchByImageResponse } from '../types/searchByImage'

export type VisualSearchState = 'empty' | 'upload' | 'loading' | 'results'

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024 // 10 MB

export interface ValidationError {
  type: 'type' | 'size'
  message: string
}

export function useVisualSearch() {
  const state = ref<VisualSearchState>('empty')
  const selectedFile = ref<File | null>(null)
  const previewUrl = ref<string | null>(null)
  const results = ref<SearchByImageResponse[]>([])
  const error = ref<string | null>(null)
  const validationError = ref<ValidationError | null>(null)
  const isDragging = ref(false)

  // Cleanup: Revoke the object URL when the owning component unmounts
  onUnmounted(() => {
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
  })

  function validateFile(file: File): ValidationError | null {
    if (!ALLOWED_TYPES.includes(file.type)) {
      return { type: 'type', message: 'Please select a JPEG, PNG, or WebP image.' }
    }
    if (file.size > MAX_SIZE) {
      return { type: 'size', message: 'Image must be under 10 MB.' }
    }
    return null
  }

  async function selectFile(file: File): Promise<void> {
    const validationErr = validateFile(file)
    if (validationErr) {
      validationError.value = validationErr
      return
    }
    validationError.value = null
    selectedFile.value = file
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = URL.createObjectURL(file)
    state.value = 'upload'
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

  async function search(topK = 20): Promise<void> {
    if (!selectedFile.value) return
    state.value = 'loading'
    error.value = null
    try {
      const result = await searchByImage(selectedFile.value, topK)
      if (result.isSuccess) {
        results.value = result.items
        state.value = 'results'
      } else {
        error.value = result.message ?? 'Search failed. Please try again.'
        state.value = 'upload'
      }
    } catch {
      error.value = 'Search failed. Please try again.'
      state.value = 'upload'
    }
  }

  return { state, selectedFile, previewUrl, results, error, validationError, isDragging, validateFile, selectFile, reset, search }
}
