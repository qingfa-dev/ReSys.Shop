<script setup lang="ts">
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'

const toast = useToast()
setNotifyToast(toast)
</script>
<template>
  <Toast />
  <router-view />
</template>
