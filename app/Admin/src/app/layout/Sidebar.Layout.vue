<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useRouter } from 'vue-router'
import AppMenu from './Menu.Layout.vue'

const authStore = useAuthStore()
const router = useRouter()

const user = computed(() => authStore.user as Record<string, unknown> | null)
const userDisplayName = computed(() => (user.value?.name as string) || 'Admin')
const userEmail = computed(() => (user.value?.email as string) || '')
const userInitials = computed(() => {
  const name = userDisplayName.value
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

function logout() {
  authStore.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="layout-sidebar">
    <div class="layout-sidebar-content">
      <AppMenu />
    </div>
    <div class="layout-sidebar-footer">
      <div class="flex items-center gap-3 px-4 py-3 border-t border-surface-200 dark:border-surface-700">
        <Avatar :label="userInitials" shape="circle" size="normal" class="bg-primary text-primary-contrast shrink-0" />
        <div class="flex flex-col min-w-0 flex-1">
          <span class="text-sm font-semibold truncate">{{ userDisplayName }}</span>
          <span class="text-xs text-surface-500 truncate">{{ userEmail }}</span>
        </div>
        <Button icon="pi pi-sign-out" severity="secondary" text rounded size="small" @click="logout" v-tooltip.left="'Logout'" />
      </div>
    </div>
  </div>
</template>
