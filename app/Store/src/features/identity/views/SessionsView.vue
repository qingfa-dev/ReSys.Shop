<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import EmptyState from '@/shared/components/EmptyState.vue'
import { useNotify } from '@/shared/composables/useNotify'
import { formatDateTimeUtc } from '@/shared/utils/date'
import type { SessionInfo } from '../types/auth'
import { useAuthStore } from '../stores/authStore'
import * as sessionApi from '../services/sessionApi'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const store = useAuthStore()

const sessions = ref<SessionInfo[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const revokeCurrentLoading = ref(false)
const revokeAllLoading = ref(false)

const isMobile = computed(() =>
  /Mobi|Android|iPhone|iPad|iPod/i.test(typeof navigator === 'undefined' ? '' : navigator.userAgent),
)

function deviceIcon(): string {
  return isMobile.value ? 'pi pi-mobile' : 'pi pi-desktop'
}

async function loadSessions(): Promise<void> {
  loading.value = true
  error.value = null
  try {
    const result = await sessionApi.getSessions()
    if (result.isSuccess) {
      sessions.value = result.value
    } else {
      sessions.value = []
      error.value = result.message ?? result.errors[0]?.message ?? 'Unable to load sessions.'
    }
  } catch {
    sessions.value = []
    error.value = 'Unable to load sessions.'
  } finally {
    loading.value = false
  }
}

async function revokeCurrent(): Promise<void> {
  revokeCurrentLoading.value = true
  try {
    const result = await sessionApi.revokeCurrentDevice()
    if (!result.isSuccess) {
      notify.error('Logout failed', result.message ?? 'Unable to log out of this device.')
      return
    }
    notify.success('Logged out', 'This device has been signed out.')
    await store.logout()
    router.replace('/login')
  } finally {
    revokeCurrentLoading.value = false
  }
}

function requestLogoutAll(): void {
  confirm.require({
    message: 'This will sign you out of every device where you are currently logged in. Continue?',
    header: 'Log out of all devices',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Log Out All',
    acceptClass: 'p-button-danger',
    accept: async () => {
      revokeAllLoading.value = true
      try {
        const result = await sessionApi.revokeAll()
        if (!result.isSuccess) {
          notify.error('Logout failed', result.message ?? 'Unable to log out of all devices.')
          return
        }
        notify.success('Logged out', 'All devices have been signed out.')
        await store.logout()
        router.replace('/login')
      } finally {
        revokeAllLoading.value = false
      }
    },
  })
}

onMounted(loadSessions)
</script>

<template>
  <div>
    <!-- Section: Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-900">Sessions</h1>
      <p class="text-sm text-gray-500 mt-1">Manage the devices signed in to your account.</p>
    </div>

    <!-- Section: Error -->
    <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>

    <!-- Section: Session Content (suppressed while an error banner is shown) -->
    <template v-else>
      <!-- Section: Loading -->
      <div v-if="loading" class="space-y-4">
        <Skeleton v-for="i in 2" :key="i" height="8rem" class="rounded-xl" />
      </div>

      <!-- Section: Session List -->
      <template v-else-if="sessions.length > 0">
      <ul class="space-y-4">
        <li
          v-for="session in sessions"
          :key="session.id"
          class="bg-white rounded-xl border border-gray-200 p-6 flex flex-wrap items-center gap-4"
        >
          <div class="flex items-center gap-4 min-w-0 flex-1">
            <span class="inline-flex items-center justify-center w-11 h-11 rounded-full bg-gray-100 text-gray-500 shrink-0">
              <i :class="deviceIcon()" class="text-lg" />
            </span>
            <div class="min-w-0">
              <div class="flex flex-wrap items-center gap-2">
                <p class="font-medium text-gray-900">{{ session.deviceName }}</p>
                <span
                  v-if="session.isCurrent"
                  class="inline-flex items-center gap-1 rounded-full bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-600"
                >
                  <i class="pi pi-check text-[10px]" />
                  Current
                </span>
              </div>
              <p class="text-sm text-gray-500 mt-0.5">
                IP {{ session.ipAddress || '—' }}
                <span class="mx-1">·</span>
                Last active {{ formatDateTimeUtc(session.lastActivityAt) }}
              </p>
            </div>
          </div>
          <Button
            v-if="session.isCurrent"
            label="Log Out This Device"
            severity="secondary"
            outlined
            size="small"
            icon="pi pi-sign-out"
            :loading="revokeCurrentLoading"
            @click="revokeCurrent"
          />
        </li>
      </ul>

      <!-- Section: Danger zone -->
      <div class="mt-8 bg-red-50 rounded-xl border border-red-200 p-6">
        <h3 class="text-base font-semibold text-gray-900">Log out everywhere</h3>
        <p class="text-sm text-gray-600 mt-1">
          This signs you out of this device and every other device signed in to your account.
        </p>
        <Button
          label="Log Out of All Devices"
          severity="danger"
          icon="pi pi-sign-out"
          class="mt-4"
          :loading="revokeAllLoading"
          @click="requestLogoutAll"
        />
      </div>
    </template>

      <!-- Section: Empty state -->
      <EmptyState v-else icon="pi pi-shield" message="No active sessions." actionLabel="Sign in" actionTo="/login" />
    </template>
  </div>
</template>
