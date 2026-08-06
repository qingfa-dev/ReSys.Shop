<script setup lang="ts">
import { onMounted, ref } from 'vue'
import * as notificationApi from '../services/notificationApi'
import type { NotificationPreferences } from '../types/notification'
import { DEFAULT_NOTIFICATION_PREFERENCES } from '../types/notification'
import NotificationPreferenceRow from '../components/NotificationPreferenceRow.vue'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

const notify = useNotify()
const { handleError } = useApiErrorHandler()
const prefs = ref<NotificationPreferences>({ ...DEFAULT_NOTIFICATION_PREFERENCES })
const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)

async function loadPrefs(): Promise<void> {
  loading.value = true
  error.value = null
  try {
    const result = await notificationApi.getNotificationPreferences()
    if (result.isSuccess) {
      prefs.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Unable to load notification preferences.'
    }
  } catch {
    error.value = 'Unable to load notification preferences.'
  } finally {
    loading.value = false
  }
}

async function onToggle(key: keyof NotificationPreferences, value: boolean): Promise<void> {
  const previous = prefs.value[key]
  // Optimistic update; revert if the backend rejects it or the request throws.
  prefs.value = { ...prefs.value, [key]: value }
  saving.value = true
  error.value = null
  try {
    const result = await notificationApi.updateNotificationPreferences(prefs.value)
    if (result.isSuccess) {
      prefs.value = result.value
      notify.success('Preferences saved', 'Your notification settings have been updated.')
    } else {
      prefs.value = { ...prefs.value, [key]: previous }
      handleError(new Error(result.message ?? 'Unable to save your notification preferences.'))
    }
  } catch {
    prefs.value = { ...prefs.value, [key]: previous }
    handleError(new Error('Unable to save your notification preferences.'))
  } finally {
    saving.value = false
  }
}

onMounted(loadPrefs)
</script>

<template>
  <div>
    <!-- Section: Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-stone-900">Notifications</h1>
      <p class="text-sm text-stone-500 mt-1">Choose which notifications you want to receive.</p>
    </div>

    <!-- Section: Error -->
    <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>

    <!-- Section: Loading -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Skeleton v-for="i in 3" :key="i" height="6rem" class="rounded-xl" />
    </div>

    <!-- Section: Preference Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <NotificationPreferenceRow
        label="Email notifications"
        description="Order updates, promotions and account alerts sent by email."
        :value="prefs.enableEmail"
        :loading="saving"
        @update:value="(v: boolean) => onToggle('enableEmail', v)"
      />
      <NotificationPreferenceRow
        label="SMS notifications"
        description="Order status and delivery updates sent by text message."
        :value="prefs.enableSms"
        :loading="saving"
        @update:value="(v: boolean) => onToggle('enableSms', v)"
      />
      <NotificationPreferenceRow
        label="Newsfeeds"
        description="Newsletter and product news digests."
        :value="prefs.enableNewsfeeds"
        :loading="saving"
        @update:value="(v: boolean) => onToggle('enableNewsfeeds', v)"
      />
    </div>
  </div>
</template>
