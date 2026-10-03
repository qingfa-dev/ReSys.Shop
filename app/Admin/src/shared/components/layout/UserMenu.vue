<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useNotify } from '@/shared/composables/useNotify'
import Button from 'primevue/button'
import Popover from 'primevue/popover'
import { useAuthStore } from '@/features/auth/stores/authStore'
import User from '@primeicons/vue/user'
import SignOut from '@primeicons/vue/sign-out'

const authStore = useAuthStore()
const router = useRouter()
const notify = useNotify()

const popover = ref<InstanceType<typeof Popover> | null>(null)

function togglePopover(event: Event) {
  popover.value?.toggle(event)
}

async function handleLogout() {
  await authStore.logout()
  notify.info('Logged out')
  router.replace({ name: 'login' })
}
</script>

<template>
  <button v-if="authStore.isAuthenticated" class="layout-topbar-action" @click="togglePopover">
    <User />
  </button>

  <Popover ref="popover">
    <div class="flex flex-col gap-1 p-2" style="min-width: 14rem">
      <div class="px-3 py-2">
        <span class="font-medium text-color block">{{ authStore.currentUser?.userName ?? 'User' }}</span>
        <span class="text-xs text-color-secondary block">{{ authStore.currentUser?.email }}</span>
      </div>
      <div class="border-top-1 surface-border" />
      <router-link
        to="/customer/profiles"
        class="flex align-items-center gap-2 px-3 py-2 border-round surface-hover no-underline text-color"
      >
        <User class="text-sm" />
        <span class="text-sm">Profile</span>
      </router-link>
      <div class="border-top-1 surface-border" />
      <Button
        severity="danger"
        variant="text"
        class="logout-btn justify-content-start"
        fluid
        :disabled="authStore.isLoggingOut"
        @click="handleLogout"
      >
        <SignOut class="mr-2" />
        Logout
      </Button>
    </div>
  </Popover>
</template>
