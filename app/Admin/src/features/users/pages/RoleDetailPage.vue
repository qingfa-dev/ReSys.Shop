<script setup lang="ts">
import RoleForm from '../components/RoleForm.vue'
import RolePermissionManager from '../components/RolePermissionManager.vue'
</script>

<template>
  <RoleForm>
    <template #after-form="{ roleId }">
      <RolePermissionManager :role-id="roleId" />
    </template>
  </RoleForm>
</template>
