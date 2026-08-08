<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { SessionApi } from '../services'
import { useNotify } from '@/shared/composables/useNotify'
import type { SessionInfo } from '../types'

usePageTitle('Sessions')
const notify = useNotify()
const sessions = ref<SessionInfo[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  loading.value = true
  const result = await SessionApi.getSessions()
  if (result.isSuccess && result.value) {
    sessions.value = result.value
  } else {
    error.value = 'Failed to load sessions'
  }
  loading.value = false
})

async function onRevokeAll(): Promise<void> {
  const result = await SessionApi.revokeAll()
  if (result.isSuccess) {
    sessions.value = sessions.value.filter(s => s.isCurrent)
    notify.success('All other sessions revoked')
  } else {
    notify.error('Failed to revoke sessions')
  }
}

function deviceIcon(session: SessionInfo): string {
  const name = session.deviceName.toLowerCase()
  if (name.includes('phone') || name.includes('iphone')) return 'pi pi-mobile'
  if (name.includes('tablet') || name.includes('ipad')) return 'pi pi-tablet'
  return 'pi pi-desktop'
}

function formatDate(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}
</script>
<template>
  <!-- Section: Page Header -->
  <div>
    <h1 class="text-2xl font-bold text-neutral-900 mb-6">Active Sessions</h1>

    <!-- Section: Loading State -->
    <div v-if="loading" class="space-y-3">
      <Skeleton v-for="i in 3" :key="i" height="4rem" />
    </div>

    <!-- Section: Error State -->
    <div v-else-if="error" class="text-center py-8">
      <p class="text-neutral-500 mb-4">{{ error }}</p>
      <Button label="Retry" severity="secondary" outlined @click="loading = true; error = null; onMounted(() => {})" />
    </div>

    <!-- Section: Session List -->
    <div v-else class="space-y-3">
      <div v-for="session in sessions" :key="session.id" class="flex items-center justify-between p-4 bg-white rounded-lg border border-neutral-200">
        <div class="flex items-center gap-3">
          <i :class="deviceIcon(session)" class="text-xl text-neutral-500" />
          <div>
            <p class="text-sm font-medium text-neutral-900">{{ session.deviceName }}</p>
            <p class="text-xs text-neutral-500">{{ session.ipAddress }} &middot; {{ formatDate(session.lastActivityAt) }}</p>
          </div>
        </div>
        <Tag v-if="session.isCurrent" value="Current" severity="info" />
        <Button v-else label="Revoke" severity="danger" outlined size="small" @click="notify.info('Revoke single session not yet available')" />
      </div>
    </div>

    <!-- Section: Revoke All -->
    <div v-if="sessions.length > 1 && !loading" class="mt-6">
      <Button label="Revoke All Other Sessions" severity="danger" text size="small" @click="onRevokeAll" />
    </div>
  </div>
</template>
