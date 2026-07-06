<template>
  <AppDrawer :visible="visible" :title="user?.displayName ?? 'User'" @update:visible="$emit('update:visible', $event)">
    <div v-if="user" class="flex flex-col gap-2 text-sm">
      <p><span class="text-color-secondary">Email:</span> {{ user.email }}</p>
      <p><span class="text-color-secondary">Status:</span> <UserStatusBadge :status="user.status" /></p>
      <p><span class="text-color-secondary">Roles:</span> {{ user.roles.join(', ') || '—' }}</p>
      <p><span class="text-color-secondary">Created:</span> {{ user.createdAt }}</p>
      <p><span class="text-color-secondary">Updated:</span> {{ user.updatedAt }}</p>
    </div>
  </AppDrawer>
</template>

<script setup lang="ts">
import type { User } from '../model/user.types'
import UserStatusBadge from './UserStatusBadge.vue'

defineProps<{ visible: boolean; user: User | null }>()
defineEmits<{ 'update:visible': [v: boolean] }>()
</script>
