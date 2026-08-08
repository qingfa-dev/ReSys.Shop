<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useProfileStore } from '../stores/profileStore'

usePageTitle('My Profile')
const profileStore = useProfileStore()

onMounted(() => profileStore.init())
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'My Profile' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">My Profile</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
          <Skeleton width="50%" height="1rem" />
          <Skeleton width="100%" height="2.5rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Profile form fields with PrimeVue InputText, Calendar, and validation will be implemented here.
    </Message>
  </div>
</template>
