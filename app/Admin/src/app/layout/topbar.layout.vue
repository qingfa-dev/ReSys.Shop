<template>
  <header class="flex h-16 items-center justify-between border-b border-surface-200 bg-surface-0 px-4">
    <div class="flex items-center gap-4">
      <Button
        icon="pi pi-bars"
        text
        rounded
        @click="toggleSidebar"
      />
      <GlobalSearch />
    </div>
    <div class="flex items-center gap-2">
      <Button
        :icon="darkMode ? 'pi pi-moon' : 'pi pi-sun'"
        text
        rounded
        :severity="darkMode ? 'warn' : 'secondary'"
        @click="toggleDarkMode"
      />
      <Button
        icon="pi pi-cog"
        text
        rounded
        severity="secondary"
        @click="configuratorVisible = !configuratorVisible"
      />
      <div class="mx-2 h-6 w-px bg-surface-200" />
      <div class="flex items-center gap-2">
        <Avatar
          icon="pi pi-user"
          shape="circle"
          class="cursor-pointer"
          @click="userMenuVisible = !userMenuVisible"
        />
        <span v-if="!collapsed" class="text-sm font-medium">{{ userName }}</span>
      </div>
    </div>

    <OverlayPanel ref="userMenu" :visible="userMenuVisible" @hide="userMenuVisible = false">
      <div class="flex flex-col gap-1" style="min-width: 180px">
        <Button
          label="Profile"
          icon="pi pi-user-edit"
          text
          class="w-full justify-start"
          @click="navigateToProfile"
        />
        <Button
          label="Sign Out"
          icon="pi pi-sign-out"
          text
          class="w-full justify-start"
          severity="danger"
          @click="signOut"
        />
      </div>
    </OverlayPanel>
  </header>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useLayout } from './composables/layout.composable'
import GlobalSearch from './components/GlobalSearch.vue'

const router = useRouter()
const { darkMode, sidebarCollapsed, toggleDarkMode, toggleSidebar } = useLayout()

const collapsed = computed(() => sidebarCollapsed.value)
const configuratorVisible = ref(false)
const userMenuVisible = ref(false)
const userName = ref('Admin')

function navigateToProfile() {
  userMenuVisible.value = false
}

function signOut() {
  userMenuVisible.value = false
  router.push('/login')
}
</script>
