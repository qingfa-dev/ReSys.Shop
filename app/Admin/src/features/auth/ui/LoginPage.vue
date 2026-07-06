<template>
  <div class="flex min-h-screen items-center justify-center bg-surface-50 p-4">
    <div class="w-full max-w-sm rounded-lg bg-white p-6 shadow">
      <h1 class="mb-4 text-center text-2xl font-semibold">Sign in</h1>
      <LoginForm />
    </div>
  </div>
</template>

<script setup lang="ts">
import LoginForm from './LoginForm.vue'
</script>
