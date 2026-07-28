<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useNotify } from '@/shared/composables/useNotify'
import Avatar from 'primevue/avatar'
import Popover from 'primevue/popover'
import Button from 'primevue/button'
import { useAuthStore } from '@/features/auth/stores/authStore'

const authStore = useAuthStore()
const router = useRouter()
const notify = useNotify()

const popover = ref<InstanceType<typeof Popover> | null>(null)

function togglePopover(event: Event) {
  ;(popover.value as any)?.toggle(event)
}

async function handleLogout() {
  await authStore.logout()
  notify.info('Logged out')
  router.replace({ name: 'login' })
}
</script>

<template>
  <div v-if="authStore.isAuthenticated" class="flex align-items-center gap-2">
    <div class="flex align-items-center gap-2 cursor-pointer" @click="togglePopover">
      <Avatar :label="authStore.currentUser?.userName?.charAt(0)?.toUpperCase() ?? '?'" shape="circle" size="large" />
      <span class="font-medium text-color hidden md:inline">{{ authStore.currentUser?.userName ?? 'User' }}</span>
    </div>

    <Popover ref="popover">
      <div class="flex flex-column gap-3" style="min-width: 16rem">
        <div class="flex flex-column gap-1">
          <span class="text-sm text-color-secondary">{{ authStore.currentUser?.userName }}</span>
        </div>
        <router-link to="/profile" class="flex align-items-center gap-2 p-ripple no-underline text-color p-2 border-round surface-hover">
          <i class="pi pi-user" />
          <span>Profile</span>
        </router-link>
        <div class="border-top-1 surface-border" />
        <Button
          label="Logout"
          icon="pi pi-sign-out"
          severity="danger"
          text
          class="logout-btn w-full justify-content-start"
          :disabled="authStore.isLoggingOut"
          @click="handleLogout"
        />
      </div>
    </Popover>
  </div>
</template>
