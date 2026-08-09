<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'
import { useSearch } from '@/features/catalog/composables/useSearch'

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

onMounted(() => {
  // Setup: Register the global Ctrl/Cmd+K shortcut listener.
  document.addEventListener('keydown', onGlobalKeyDown)
})
onUnmounted(() => document.removeEventListener('keydown', onGlobalKeyDown))
</script>

<template>
  <div>
    <Toast position="bottom-right" />
    <ConfirmDialog />
    <RouterView />
  </div>
</template>
