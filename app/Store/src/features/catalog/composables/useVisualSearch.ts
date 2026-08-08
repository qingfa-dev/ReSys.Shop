import { onUnmounted, ref } from 'vue'
import { SearchByImageApi } from '../services/searchByImageApi'
import type { SearchByImageResponse } from '../types/searchByImage'

export type VisualSearchState = 'empty' | 'upload' | 'loading' | 'results'

// Guard: Restrict to browser-supported image formats for embedding API
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
// Guard: 10 MB limit prevents embedding API timeout on oversized images
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

  // Release: Revoke object URL on unmount to prevent memory leak
  onUnmounted(() => {
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
  })

  function validateFile(file: File): ValidationError | null {
    // Validate: File MIME type against allowed image formats
    if (!ALLOWED_TYPES.includes(file.type)) {
      return { type: 'type', message: 'Please select a JPEG, PNG, or WebP image.' }
    }
    // Validate: File size against 10 MB limit
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
    // Release: Revoke previous preview URL before creating new one
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    // Cache: Create object URL for image preview — revoked on reset
    previewUrl.value = URL.createObjectURL(file)
    state.value = 'upload'
  }

  function reset(): void {
    // Release: Revoke object URL to prevent memory leak
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    // Reset: All state to initial values for fresh search session
    selectedFile.value = null
    previewUrl.value = null
    results.value = []
    error.value = null
    validationError.value = null
    state.value = 'empty'
  }

  async function search(topK = 20, model?: string): Promise<void> {
    // Guard: Require selected file before attempting visual search
    if (!selectedFile.value) return
    state.value = 'loading'
    error.value = null
    try {
      // Call: Catalog API visual search endpoint with image and parameters
      const result = await SearchByImageApi.searchByImage(selectedFile.value, topK, model)
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
