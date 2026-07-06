import { ref, type Ref } from 'vue'

export function useDisclosure(initial = false): {
  isOpen: Ref<boolean>
  open: () => void
  close: () => void
  toggle: () => void
} {
  const isOpen = ref(initial)
  return {
    isOpen,
    open: () => (isOpen.value = true),
    close: () => (isOpen.value = false),
    toggle: () => (isOpen.value = !isOpen.value),
  }
}
