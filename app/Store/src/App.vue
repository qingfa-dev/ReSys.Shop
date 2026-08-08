<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'
import { useTheme } from '@/shared/composables/useTheme'
import { useSearch } from '@/features/catalog/composables/useSearch'

// Init: Apply stored theme preference and register OS dark-mode listener.
const { isDark, init } = useTheme()
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
  init()
  document.addEventListener('keydown', onGlobalKeyDown)
})
onUnmounted(() => document.removeEventListener('keydown', onGlobalKeyDown))
</script>

<template>
  <div :class="{ 'app-dark': isDark }">
    <Toast position="bottom-right" />
    <ConfirmDialog />
    <RouterView />
  </div>
</template>
