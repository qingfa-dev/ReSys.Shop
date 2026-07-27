<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import ErrorPageShell from '@feedback/ErrorPageShell.vue'

const route = useRoute()
const statusCode = computed(() => route.meta.statusCode as string | number | undefined)
const title = computed(() => route.meta.title as string | undefined)
const description = computed(() => route.meta.description as string | undefined)
const icon = computed(() => route.meta.icon as string | undefined)
const iconBgClass = computed(() => route.meta.iconBgClass as string | undefined)
const image = computed(() => route.meta.image as string | undefined)
const buttonLabel = computed(() => route.meta.buttonLabel as string | undefined)
const buttonTo = computed(() => route.meta.buttonTo as string | undefined)
const gradientColor = computed(() => route.meta.gradientColor as string | undefined)
const links = computed(() => route.meta.links as Array<{ icon: string; title: string; description: string; to: string }> | undefined)
</script>
<template>
  <ErrorPageShell
    :statusCode="statusCode"
    :title="title ?? 'Error'"
    :description="description ?? 'An unexpected error occurred.'"
    :icon="icon ?? 'pi pi-exclamation-circle'"
    :iconBgClass="iconBgClass"
    :image="image"
    :buttonLabel="buttonLabel"
    :buttonTo="buttonTo"
    :gradientColor="gradientColor"
    :links="links"
  />
</template>
