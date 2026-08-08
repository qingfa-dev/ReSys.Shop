<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { NotificationApi } from '../services/notificationApi'
import type { NotificationPreferences } from '../types'

usePageTitle('Notifications')

// API: No notification store exists, so the view talks to NotificationApi
// directly (SessionsView precedent) and keeps the draft locally.
const notify = useNotify()

// Draft: Per-channel toggles loaded from the API and persisted on save.
const prefs = ref<NotificationPreferences>({ enableSms: false, enableEmail: false, enableNewsfeeds: false })
const loading = ref(true)
const error = ref<string | null>(null)
const saving = ref(false)

// Channels: Row descriptors driving the preference list.
const channels = [
  { key: 'enableEmail' as const, label: 'Order & account email', description: 'Order confirmations, shipping updates and account security notices.' },
  { key: 'enableSms' as const, label: 'SMS notifications', description: 'Delivery alerts and urgent account messages by text message.' },
  { key: 'enableNewsfeeds' as const, label: 'News & offers', description: 'New arrivals, promotions and personalized recommendations.' },
]

// Load: Fetch the current preference set on mount.
async function loadPrefs(): Promise<void> {
  loading.value = true
  error.value = null
  const result = await NotificationApi.getNotificationPreferences()
  if (result.isSuccess) prefs.value = result.value
  else error.value = result.message ?? 'Could not load notification preferences'
  loading.value = false
}

// Save: Persist the draft channels and toast the outcome.
async function onSave(): Promise<void> {
  saving.value = true
  error.value = null
  const result = await NotificationApi.updateNotificationPreferences(prefs.value)
  saving.value = false
  if (result.isSuccess) {
    prefs.value = result.value
    notify.success('Notification preferences saved')
  } else {
    error.value = result.message ?? 'Could not save notification preferences'
  }
}

onMounted(() => void loadPrefs())
</script>

<template>
  <!-- Section: Content Card — channel toggles with descriptions and a save action -->
  <Card>
    <template #title>Notifications</template>
    <template #content>
      <!-- Section: Loading State -->
      <div v-if="loading" class="flex flex-col gap-3">
        <Skeleton v-for="i in 3" :key="i" height="4rem" />
      </div>

      <!-- Section: Error State -->
      <div v-else-if="error" class="flex flex-col items-center gap-4 py-8">
        <Message severity="error" :closable="false">{{ error }}</Message>
        <Button label="Retry" severity="secondary" outlined @click="loadPrefs" />
      </div>

      <!-- Section: Preference Rows — one toggle per channel -->
      <div v-else class="flex flex-col gap-5">
        <div v-for="channel in channels" :key="channel.key" class="flex items-center justify-between gap-4 border-b border-surface-100 pb-4 last:border-b-0 dark:border-surface-800">
          <div>
            <div class="font-medium">{{ channel.label }}</div>
            <p class="mt-0.5 text-sm text-surface-500">{{ channel.description }}</p>
          </div>
          <ToggleSwitch
            :input-id="`pref-${channel.key}`"
            v-model="prefs[channel.key]"
          />
        </div>

        <!-- Section: Action Footer — persists all channel toggles -->
        <div>
          <Button label="Save" icon="pi pi-check" :loading="saving" @click="onSave" />
        </div>
      </div>
    </template>
  </Card>
</template>
