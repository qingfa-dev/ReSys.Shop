<script setup lang="ts">
import { DetailLayout } from '@/shared/components'
import RoleForm from '../components/RoleForm.vue'
import RolePermissionManager from '../components/RolePermissionManager.vue'
</script>

<template>
  <DetailLayout>
    <RoleForm>
      <template #after-form="{ roleId }">
        <RolePermissionManager :role-id="roleId" />
      </template>
    </RoleForm>
  </DetailLayout>
</template>
