<template>
  <div>
    <AppPageHeader title="Dashboard" subtitle="Overview of your store" />
    <div v-if="!user" class="p-8">
      <AppLoadingState />
    </div>
    <div v-else class="grid grid-cols-1 gap-4 md:grid-cols-3">
      <Card>
        <template #content>
          <p class="text-sm text-color-secondary">Signed in as</p>
          <p class="text-lg font-semibold">{{ userName }}</p>
        </template>
      </Card>
      <Card>
        <template #content>
          <p class="text-sm text-color-secondary">Roles</p>
          <p class="text-lg font-semibold">{{ rolesCount }}</p>
        </template>
      </Card>
      <Card>
        <template #content>
          <p class="text-sm text-color-secondary">Permissions</p>
          <p class="text-lg font-semibold">{{ permissionsCount }}</p>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { storeToRefs } from 'pinia'

const authStore = useAuthStore()
const { user } = storeToRefs(authStore)
const userData = computed(() => user.value as Record<string, unknown> | null)
const userName = computed(() => String(userData.value?.name ?? userData.value?.sub ?? '—'))
const rolesCount = computed(() => ((userData.value?.roleNames ?? userData.value?.roles) as any[] | undefined)?.length ?? 0)
const permissionsCount = computed(() => ((userData.value?.permissions as any[] | undefined)?.length ?? 0))
</script>
