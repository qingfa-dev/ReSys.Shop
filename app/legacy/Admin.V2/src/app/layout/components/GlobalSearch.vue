<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'

const router = useRouter()
const searchQuery = ref('')

function onSearch() {
  if (searchQuery.value.trim()) {
    router.push({ name: 'reports.dashboard', query: { search: searchQuery.value } })
  }
}
</script>

<template>
  <div class="hidden md:flex items-center flex-1 max-w-md mx-4">
    <span class="p-input-icon-left w-full">
      <i class="pi pi-search" />
      <InputText
        v-model="searchQuery"
        placeholder="Search..."
        class="w-full !bg-surface-100 dark:!bg-surface-800 !border-transparent"
        @keyup.enter="onSearch"
      />
    </span>
  </div>
</template>
