import { ref } from 'vue'

export function useModalService() {
  const isOpen = ref(false)
  const modalData = ref<unknown>(null)

  function open(data?: unknown): void {
    modalData.value = data ?? null
    isOpen.value = true
  }

  function close(): void {
    isOpen.value = false
  }

  function toggle(): void {
    isOpen.value = !isOpen.value
  }

  return { isOpen, modalData, open, close, toggle }
}
