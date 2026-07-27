import { ref, onBeforeUnmount } from 'vue'

export function useFilePreview() {
  const previewUrl = ref<string | null>(null)
  let objectUrl: string | null = null

  function createPreview(file: File): string {
    revokePreview()
    objectUrl = URL.createObjectURL(file)
    previewUrl.value = objectUrl
    return objectUrl
  }

  function revokePreview(): void {
    if (objectUrl) {
      URL.revokeObjectURL(objectUrl)
      objectUrl = null
    }
    previewUrl.value = null
  }

  onBeforeUnmount(() => {
    revokePreview()
  })

  return {
    previewUrl,
    createPreview,
    revokePreview,
  }
}
