import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { Notification } from '@/shared/types'

export const useNotificationStore = defineStore('notification', () => {
  const items = ref<Notification[]>([])
  let pollingTimer: ReturnType<typeof setInterval> | null = null

  const unreadCount = computed(() => items.value.filter((n) => !n.isRead).length)

  const recentItems = computed(() => items.value.slice(-5).reverse())

  function fetch() {}

  function markRead(_id: string) {}

  function markAllRead() {}

  function startPolling(intervalMs: number) {
    if (pollingTimer !== null) return
    pollingTimer = setInterval(() => {
      fetch()
    }, intervalMs)
  }

  function stopPolling() {
    if (pollingTimer !== null) {
      clearInterval(pollingTimer)
      pollingTimer = null
    }
  }

  return {
    items: readonly(items),
    unreadCount,
    recentItems,
    fetch,
    markRead,
    markAllRead,
    startPolling,
    stopPolling,
  }
})
