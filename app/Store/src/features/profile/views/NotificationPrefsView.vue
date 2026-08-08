<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { NotificationApi } from '../services/notificationApi'

usePageTitle('Notification Preferences')
const notify = useNotify()

const loading = ref(true)
const saving = ref(false)

const enableEmail = ref(false)
const enableSms = ref(false)
const enableNewsfeeds = ref(false)

// Bootstrap: Load notification preferences on mount
onMounted(async () => {
  loading.value = true
  const result = await NotificationApi.getNotificationPreferences()
  if (result.isSuccess && result.value) {
    enableEmail.value = result.value.enableEmail
    enableSms.value = result.value.enableSms
    enableNewsfeeds.value = result.value.enableNewsfeeds
  }
  loading.value = false
})

// Persist: Save current toggle state to server
async function save(): Promise<void> {
  saving.value = true
  const result = await NotificationApi.updateNotificationPreferences({
    enableEmail: enableEmail.value,
    enableSms: enableSms.value,
    enableNewsfeeds: enableNewsfeeds.value,
  })
  if (result.isSuccess) {
    notify.success('Preferences saved')
  } else {
    notify.error(result.message || 'Failed to save')
  }
  saving.value = false
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation and title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Notifications' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Notification Preferences</h1>

    <!-- Section: Loading State — skeleton placeholder -->
    <Card v-if="loading">
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
        </div>
      </template>
    </Card>

    <!-- Section: Content Card — notification toggle switches -->
    <Card v-else>
      <template #content>
        <div class="space-y-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="font-medium text-neutral-900">Order updates</p>
              <p class="text-sm text-neutral-500">Get notified about order status changes</p>
            </div>
            <input
              v-model="enableEmail"
              type="checkbox"
              class="h-5 w-5 rounded border-neutral-300"
            />
          </div>

          <div class="flex items-center justify-between">
            <div>
              <p class="font-medium text-neutral-900">New arrivals</p>
              <p class="text-sm text-neutral-500">Be the first to know about new products</p>
            </div>
            <input
              v-model="enableSms"
              type="checkbox"
              class="h-5 w-5 rounded border-neutral-300"
            />
          </div>

          <div class="flex items-center justify-between">
            <div>
              <p class="font-medium text-neutral-900">Marketing</p>
              <p class="text-sm text-neutral-500">Receive promotions and special offers</p>
            </div>
            <input
              v-model="enableNewsfeeds"
              type="checkbox"
              class="h-5 w-5 rounded border-neutral-300"
            />
          </div>
        </div>

        <!-- Section: Action Footer — save button -->
        <Button
          label="Save"
          class="mt-6"
          :loading="saving"
          @click="save"
        />
      </template>
    </Card>
  </div>
</template>
