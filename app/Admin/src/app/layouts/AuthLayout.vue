<script setup lang="ts">
import AuthShell from '@forms/AuthLayout.vue'

interface Props {
  title: string
  subtitle?: string
}
defineProps<Props>()
</script>
<template>
  <AuthShell :title="title" :subtitle="subtitle">
    <router-view />
  </AuthShell>
</template>
