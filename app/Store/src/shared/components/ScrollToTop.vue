<script setup lang="ts">
import { watch } from 'vue'
import { useRoute } from 'vue-router'
const route = useRoute()
watch(() => route.fullPath, () => window.scrollTo(0, 0))
</script>
<template><div /></template>
