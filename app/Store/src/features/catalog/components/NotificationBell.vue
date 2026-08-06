<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/features/identity/stores/authStore'

const auth = useAuthStore()
const op = ref()

interface Notification {
  id: string
  type: 'order' | 'promotion' | 'system'
  title: string
  message: string
  createdAt: string
}

// Sample: Replace with real API call when backend endpoint is available.
const notifications = ref<Notification[]>([
  {
    id: '1',
    type: 'order',
    title: 'Order shipped',
    message: 'Your order #1234 has been shipped',
    createdAt: new Date(Date.now() - 3600000).toISOString(),
  },
  {
    id: '2',
    type: 'promotion',
    title: 'Flash sale',
    message: '20% off all electronics today only',
    createdAt: new Date(Date.now() - 7200000).toISOString(),
  },
])

// Label: Mark all notifications as read.
function markAllRead() {
  notifications.value = []
}

// Label: Resolve icon class for notification type.
function getIcon(type: string) {
  const icons: Record<string, string> = {
    order: 'pi pi-shopping-bag',
    promotion: 'pi pi-tag',
    system: 'pi pi-info-circle',
  }
  return icons[type] ?? 'pi pi-bell'
}

// Label: Format date as relative time string.
function timeAgo(date: string) {
  const seconds = Math.floor((Date.now() - new Date(date).getTime()) / 1000)
  if (seconds < 60) return 'just now'
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`
  return `${Math.floor(seconds / 86400)}d ago`
}
</script>
<template>
  <!-- Section: Notification Bell — toggle button for notification dropdown -->
  <div v-if="auth.isAuthenticated">
    <button
      class="relative p-2 text-stone-500 hover:text-teal-700 transition-colors"
      aria-label="Notifications"
      @click="(e: MouseEvent) => op?.toggle(e)"
    >
      <i class="pi pi-bell text-xl" />
    </button>
    <!-- Section: Notification Dropdown — list of recent notifications -->
    <Popover ref="op">
      <div class="w-80">
        <div class="flex items-center justify-between mb-3">
          <h3 class="font-semibold">Notifications</h3>
          <Button text size="small" label="Mark all read" @click="markAllRead" />
        </div>
        <div v-if="notifications.length === 0" class="text-center py-4 text-stone-500">
          No notifications
        </div>
        <div v-else class="space-y-2 max-h-64 overflow-y-auto">
          <div v-for="notif in notifications" :key="notif.id" class="flex gap-3 p-2 rounded-lg hover:bg-stone-50">
            <i :class="getIcon(notif.type)" class="text-teal-600 mt-0.5" />
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-stone-900 truncate">{{ notif.title }}</p>
              <p class="text-xs text-stone-500 truncate">{{ notif.message }}</p>
              <p class="text-xs text-stone-400 mt-1">{{ timeAgo(notif.createdAt) }}</p>
            </div>
          </div>
        </div>
        <router-link to="/account/notifications" class="block text-center text-sm text-teal-600 hover:underline mt-3 pt-3 border-t">
          View all
        </router-link>
      </div>
    </Popover>
  </div>
</template>
