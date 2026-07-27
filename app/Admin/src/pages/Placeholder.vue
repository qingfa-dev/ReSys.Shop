<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import PageShell from '@ui/PageShell.vue'
import EmptyState from '@feedback/EmptyState.vue'

const route = useRoute()
const title = computed(() => (route.meta.title as string) || route.name?.toString() || 'Page')
</script>

<template>
  <PageShell :title="title">
    <EmptyState title="Coming Soon" description="This page is under development." />
  </PageShell>
</template>
