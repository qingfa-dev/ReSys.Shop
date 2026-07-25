<script setup lang="ts">
import UserForm from '../components/UserForm.vue'
import UserRoleManager from '../components/UserRoleManager.vue'
import UserPermissionManager from '../components/UserPermissionManager.vue'
</script>

<template>
  <UserForm user-type="staff">
    <template #after-form="{ userId }">
      <div class="grid">
        <div class="col-6">
          <UserRoleManager :user-id="userId" />
        </div>
        <div class="col-6">
          <UserPermissionManager :user-id="userId" />
        </div>
      </div>
    </template>
  </UserForm>
</template>
