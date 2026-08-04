import { ref, onUnmounted } from 'vue'

export function useFilePreview() {
  const previewUrl = ref<string | null>(null)
  const selectedFile = ref<File | null>(null)

  const handleFileChange = (file: File | null) => {
    if (previewUrl.value) {
      URL.revokeObjectURL(previewUrl.value)
    }

    selectedFile.value = file

    if (file) {
      previewUrl.value = URL.createObjectURL(file)
    } else {
      previewUrl.value = null
    }
  }

  const clearPreview = () => {
    handleFileChange(null)
  }

  onUnmounted(() => {
    if (previewUrl.value) {
      URL.revokeObjectURL(previewUrl.value)
    }
  })

  return {
    previewUrl,
    selectedFile,
    handleFileChange,
    clearPreview,
  }
}
