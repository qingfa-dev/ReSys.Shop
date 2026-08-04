<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'
import ScrollToTop from '@/shared/components/ScrollToTop.vue'
import { useTheme } from '@/shared/composables/useTheme'
import { useSearch } from '@/features/catalog/composables/useSearch'
import SearchOverlay from '@/features/catalog/components/SearchOverlay.vue'

// Init: Apply stored theme preference and register OS dark-mode listener.
useTheme()
const toast = useToast()
setNotifyToast(toast)

const search = useSearch()

// Trigger: Global Ctrl/Cmd+K opens the search overlay.
function onGlobalKeyDown(e: KeyboardEvent): void {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
    e.preventDefault()
    search.open()
  }
}

onMounted(() => document.addEventListener('keydown', onGlobalKeyDown))
onUnmounted(() => document.removeEventListener('keydown', onGlobalKeyDown))
</script>
<template>
  <Toast />
  <ScrollToTop />
  <SearchOverlay />
  <router-view />
</template>
