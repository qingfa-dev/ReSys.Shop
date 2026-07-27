import { onMounted, onUnmounted } from 'vue'
import { useNotificationStore } from '@/stores/useNotificationStore'

export function useNotification() {
  const store = useNotificationStore()

  onMounted(() => {
    store.startPolling(30000)
  })

  onUnmounted(() => {
    store.stopPolling()
  })

  return store
}
