<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { usePreferences } from '@/shared/composables/usePreferences'
import { NotificationApi } from '../services/notificationApi'
import type { NotificationPreferences } from '../types'

usePageTitle('Preferences')

// Preferences: Currency/language persist to localStorage through the shared
// singleton's watcher.
const { preferences } = usePreferences()
const notify = useNotify()

// Receipts: Email-receipt toggle maps to the enableEmail notification channel —
// profileStore has no preferences update path (UpdateProfileRequest carries
// names only), so the notification API is the persistence home for it.
const notif = ref<NotificationPreferences | null>(null)
const prefsError = ref<string | null>(null)
const emailReceipts = computed<boolean>({
  get: () => notif.value?.enableEmail ?? false,
  set: (value: boolean) => {
    if (notif.value) notif.value.enableEmail = value
  },
})

// Options: Select choices mirror the usePreferences currency/language unions.
const currencyOptions = [
  { label: 'US Dollar (USD)', value: 'USD' },
  { label: 'Euro (EUR)', value: 'EUR' },
  { label: 'Vietnamese Dong (VND)', value: 'VND' },
]
const localeOptions = [
  { label: 'English', value: 'en' },
  { label: 'Tiếng Việt', value: 'vi' },
]

// Save: Persist the full channel set so toggles made elsewhere are preserved;
// currency/language persist automatically through their composables.
async function onSave(): Promise<void> {
  prefsError.value = null
  if (!notif.value) return
  const result = await NotificationApi.updateNotificationPreferences(notif.value)
  if (result.isSuccess) {
    notif.value = result.value
    notify.success('Preferences saved')
  } else {
    prefsError.value = result.message ?? 'Could not save preferences'
  }
}

onMounted(async () => {
  // Load: Seed the channel set so the receipt toggle reflects real state.
  const result = await NotificationApi.getNotificationPreferences()
  if (result.isSuccess) notif.value = result.value
})
</script>

<template>
  <!-- Section: Content Card — display preferences with a save action -->
  <Card>
    <template #title>Preferences</template>
    <template #content>
      <div class="flex flex-col gap-5">
        <!-- Section: Form Fields — currency, locale and receipts -->
        <div class="grid max-w-md grid-cols-1 gap-5 sm:grid-cols-2">
          <div>
            <Label for="pref-currency" class="mb-1 block text-sm font-medium">Currency</Label>
            <Select
              id="pref-currency"
              v-model="preferences.currency"
              :options="currencyOptions"
              optionLabel="label"
              optionValue="value"
              class="w-full"
            />
          </div>
          <div>
            <Label for="pref-locale" class="mb-1 block text-sm font-medium">Language</Label>
            <Select
              id="pref-locale"
              v-model="preferences.language"
              :options="localeOptions"
              optionLabel="label"
              optionValue="value"
              class="w-full"
            />
          </div>
        </div>
        <div class="flex items-center justify-between gap-4 border-t border-surface-100 pt-4">
          <div>
            <div class="font-medium">Email receipts</div>
            <p class="mt-0.5 text-sm text-surface-500">Receive a receipt by email for every completed order.</p>
          </div>
          <ToggleButton
            v-model="emailReceipts"
            onLabel="On"
            offLabel="Off"
          />
        </div>

        <!-- Feedback: Inline message for API errors -->
        <Message v-if="prefsError" severity="error" :closable="false">{{ prefsError }}</Message>

        <!-- Section: Action Footer — persists the receipt channel -->
        <div>
          <Button label="Save" icon="pi pi-check" @click="onSave" />
        </div>
      </div>
    </template>
  </Card>
</template>
