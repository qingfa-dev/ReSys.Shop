<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { getNotificationPreferences } from '@/features/profile/services/notificationApi'
import type { NotificationPreferences } from '@/features/profile/types/notification'

const auth = useAuthStore()
const op = ref()
const preferences = ref<NotificationPreferences>({
  enableSms: false,
  enableEmail: false,
  enableNewsfeeds: false,
})

// Map: Channel rows dictated by the flat three-boolean preferences contract.
const channels: { key: keyof NotificationPreferences; label: string }[] = [
  { key: 'enableSms', label: 'SMS Notifications' },
  { key: 'enableEmail', label: 'Email Notifications' },
  { key: 'enableNewsfeeds', label: 'Newsletter' },
]

// Trigger: Fetch notification preferences on mount for authenticated users.
onMounted(async () => {
  if (!auth.isAuthenticated) return
  const result = await getNotificationPreferences()
  if (result.isSuccess && result.value) {
    preferences.value = result.value
  }
})
</script>
<template>
  <div v-if="auth.isAuthenticated">
    <button
      class="relative p-2 text-stone-500 hover:text-teal-700 transition-colors"
      aria-label="Notifications"
      @click="(e: MouseEvent) => op?.toggle(e)"
    >
      <i class="pi pi-bell text-xl" />
    </button>
    <Popover ref="op">
      <div class="w-72">
        <p class="text-sm font-semibold text-stone-900 mb-3">Notification Preferences</p>
        <div v-for="ch in channels" :key="ch.key" class="flex items-center justify-between py-2">
          <span class="text-sm text-stone-700">{{ ch.label }}</span>
          <i
            :class="preferences[ch.key] ? 'pi pi-check-circle text-teal-600' : 'pi pi-times-circle text-stone-400'"
            class="text-lg"
          />
        </div>
        <div class="mt-3 pt-3 border-t border-stone-100">
          <router-link to="/account/notifications" class="text-sm text-teal-600 hover:text-teal-700 font-medium">Manage Preferences &rarr;</router-link>
        </div>
      </div>
    </Popover>
  </div>
</template>