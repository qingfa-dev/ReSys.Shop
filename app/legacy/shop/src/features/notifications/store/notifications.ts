import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Notification, NotificationPreferences } from '../types'

export const useNotificationsStore = defineStore('notifications', () => {
    const notifications = ref<Notification[]>([])
    const preferences = ref<NotificationPreferences | null>(null)
    const unreadCount = ref(0)

    const unreadNotifications = computed(() =>
        notifications.value.filter(n => !n.read)
    )

    function markAsRead(notificationId: string) {
        const notif = notifications.value.find(n => n.id === notificationId)
        if (notif) {
            notif.read = true
            unreadCount.value = Math.max(0, unreadCount.value - 1)
        }
    }

    function markAllAsRead() {
        notifications.value.forEach(n => n.read = true)
        unreadCount.value = 0
    }

    function addNotification(notification: Notification) {
        notifications.value.unshift(notification)
        if (!notification.read) {
            unreadCount.value++
        }
    }

    function removeNotification(id: string) {
        const notification = notifications.value.find(n => n.id === id)
        if (notification && !notification.read) {
            unreadCount.value = Math.max(0, unreadCount.value - 1)
        }
        notifications.value = notifications.value.filter(n => n.id !== id)
    }

    function clearAll() {
        notifications.value = []
        unreadCount.value = 0
    }

    return {
        notifications,
        preferences,
        unreadCount,
        unreadNotifications,
        markAsRead,
        markAllAsRead,
        addNotification,
        removeNotification,
        clearAll,
    }
})
