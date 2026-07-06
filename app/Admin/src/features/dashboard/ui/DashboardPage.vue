<template>
  <div>
    <AppPageHeader title="Dashboard" subtitle="Overview of your store" />
    <div v-if="user.isLoading.value" class="p-8">
      <AppLoadingState />
    </div>
    <div v-else-if="user.error.value" class="p-8">
      <AppErrorState :message="String(user.error.value)" :on-retry="user.refetch" />
    </div>
    <div v-else class="grid grid-cols-1 gap-4 md:grid-cols-3">
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Signed in as</p>
        <p class="text-lg font-semibold">{{ user.data.value?.displayName ?? '—' }}</p>
      </div>
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Roles</p>
        <p class="text-lg font-semibold">{{ (user.data.value?.roles ?? []).length }}</p>
      </div>
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Permissions</p>
        <p class="text-lg font-semibold">{{ (user.data.value?.permissions ?? []).length }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useCurrentUser } from '@/features/auth'

const user = useCurrentUser()
</script>
