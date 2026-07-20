<script setup lang="ts">
import { shallowRef, watch } from 'vue'
import { useToast } from 'primevue/usetoast'

const toast = useToast()
const toastBus = shallowRef<{ severity?: string; summary?: string; detail?: string; life?: number } | null>(null)

watch(toastBus, (newValue) => {
  if (newValue) {
    toast.add(newValue)
    toastBus.value = null
  }
})
</script>

<template>
  <router-view />
  <Toast />
  <ConfirmDialog />
</template>
