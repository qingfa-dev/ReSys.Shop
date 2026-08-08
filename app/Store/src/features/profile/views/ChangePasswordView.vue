<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

usePageTitle('Change Password')
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Change Password' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Change Password</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Current password, new password, and confirm password form with validation will be implemented here.
    </Message>
  </div>
</template>
