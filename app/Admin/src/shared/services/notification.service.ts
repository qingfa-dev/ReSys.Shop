import { reactive } from 'vue'

export interface AppNotification {
  id: string
  type: 'info' | 'success' | 'warning' | 'error'
  title: string
  message: string
  timestamp: Date
  read: boolean
}

export const notificationState = reactive({
  notifications: [] as AppNotification[],
})

export function useNotificationService() {
  let nextId = 1

  function addNotification(type: AppNotification['type'], title: string, message: string): void {
    notificationState.notifications.unshift({
      id: `notif-${nextId++}`,
      type,
      title,
      message,
      timestamp: new Date(),
      read: false,
    })
    if (notificationState.notifications.length > 50) {
      notificationState.notifications.pop()
    }
  }

  function markAsRead(id: string): void {
    const notif = notificationState.notifications.find(n => n.id === id)
    if (notif) notif.read = true
  }

  function markAllAsRead(): void {
    notificationState.notifications.forEach(n => (n.read = true))
  }

  function clearAll(): void {
    notificationState.notifications.length = 0
  }

  const unreadCount = computed(() => notificationState.notifications.filter(n => !n.read).length)

  return {
    notificationState,
    addNotification,
    markAsRead,
    markAllAsRead,
    clearAll,
    unreadCount,
  }
}
