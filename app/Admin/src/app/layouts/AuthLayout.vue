<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AuthShell from '@forms/AuthLayout.vue'

const route = useRoute()

const title = computed(() => (route.meta.title as string) || '')
const subtitle = computed(() => (route.meta.subtitle as string) || '')
</script>

<template>
  <AuthShell :title="title" :subtitle="subtitle">
    <router-view />
  </AuthShell>
</template>
