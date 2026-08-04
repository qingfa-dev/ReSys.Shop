<script setup lang="ts">
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'
import ScrollToTop from '@/shared/components/ScrollToTop.vue'

const toast = useToast()
setNotifyToast(toast)
</script>
<template>
  <Toast />
  <ScrollToTop />
  <router-view />
</template>
