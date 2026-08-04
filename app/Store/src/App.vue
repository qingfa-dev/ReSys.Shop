<script setup lang="ts">
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'
import ScrollToTop from '@/shared/components/ScrollToTop.vue'
import { useTheme } from '@/shared/composables/useTheme'

// Init: Apply stored theme preference and register OS dark-mode listener.
useTheme()
const toast = useToast()
setNotifyToast(toast)
</script>
<template>
  <Toast />
  <ScrollToTop />
  <router-view />
</template>
