<script setup lang="ts">
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import UserListTable from '../components/UserListTable.vue'

const route = useRoute()
</script>

<template>
  <div>
    <PageHeader title="Staff" :icon="route.meta?.icon as string | undefined" subtitle="Manage staff accounts" />
    <UserListTable user-type="staff" />
  </div>
</template>
