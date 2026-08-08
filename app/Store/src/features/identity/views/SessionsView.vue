<script setup lang="ts">
import { ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'

usePageTitle('Active Sessions')
const auth = useAuthStore()
</script>
<template>
  <div class="max-w-2xl mx-auto py-16 px-4">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Active Sessions' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Active Sessions</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="4rem" />
          <Skeleton width="100%" height="4rem" />
          <Skeleton width="100%" height="4rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">Sessions list with device info, revoke buttons, and active session indicator will be implemented here.</Message>
  </div>
</template>
