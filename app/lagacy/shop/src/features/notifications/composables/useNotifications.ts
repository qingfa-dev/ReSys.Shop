import { useNotificationsStore } from '../store/notifications'

export function useNotifications() {
    const store = useNotificationsStore()

    return {
        notifications: store.notifications,
        unreadCount: store.unreadCount,
        unreadNotifications: store.unreadNotifications,
        markAsRead: store.markAsRead,
        markAllAsRead: store.markAllAsRead,
        addNotification: store.addNotification,
        removeNotification: store.removeNotification,
        clearAll: store.clearAll,
    }
}
