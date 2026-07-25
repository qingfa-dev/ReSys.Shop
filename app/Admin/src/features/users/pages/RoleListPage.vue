<script setup lang="ts">
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import RoleListTable from '../components/RoleListTable.vue'

const route = useRoute()
</script>

<template>
  <div>
    <PageHeader title="Roles" :icon="route.meta?.icon as string | undefined" subtitle="Manage user roles" />
    <RoleListTable />
  </div>
</template>
