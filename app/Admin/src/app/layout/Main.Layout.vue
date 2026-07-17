<script setup lang="ts">
import { useLayout } from './composables/layout.composable'
import { computed, watch, ref } from 'vue'
import { RouterView } from 'vue-router'
import ConfirmDialog from 'primevue/confirmdialog'
import AppTopbar from './Topbar.Layout.vue'
import AppFooter from './Footer.Layout.vue'
import AppSidebar from './Sidebar.Layout.vue'
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue'
import FloatingConfigurator from './components/FloatingConfigurator.Component.vue'

const { layoutConfig, layoutState, isDarkTheme, hideMobileMenu } = useLayout()

const containerClass = computed(() => ({
  'layout-overlay': layoutConfig.menuMode === 'overlay',
  'layout-static': layoutConfig.menuMode === 'static',
  'layout-static-inactive': layoutState.staticMenuInactive && layoutConfig.menuMode === 'static',
  'layout-overlay-active': layoutState.overlayMenuActive,
  'layout-mobile-active': layoutState.mobileMenuActive,
  'layout-dark': isDarkTheme.value,
}))

const outsideClickListener = ref<((event: MouseEvent) => void) | null>(null)

watch(() => layoutState.mobileMenuActive, (newVal) => {
  if (newVal) {
    bindOutsideClickListener()
  } else {
    unbindOutsideClickListener()
  }
})

const bindOutsideClickListener = () => {
  if (!outsideClickListener.value) {
    outsideClickListener.value = (event: MouseEvent) => {
      if (isOutsideClicked(event)) {
        hideMobileMenu()
      }
    }
    document.addEventListener('click', outsideClickListener.value)
  }
}

const unbindOutsideClickListener = () => {
  if (outsideClickListener.value) {
    document.removeEventListener('click', outsideClickListener.value)
    outsideClickListener.value = null
  }
}

const isOutsideClicked = (event: MouseEvent) => {
  const sidebarEl = document.querySelector('.layout-sidebar')
  const topbarEl = document.querySelector('.layout-menu-button')
  return !(
    sidebarEl?.isSameNode(event.target as Node) ||
    sidebarEl?.contains(event.target as Node) ||
    topbarEl?.isSameNode(event.target as Node) ||
    topbarEl?.contains(event.target as Node)
  )
}
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <AppTopbar />
    <AppSidebar />
    <div class="layout-main-container">
      <div class="layout-main">
        <AppBreadcrumb />
        <router-view />
      </div>
      <AppFooter />
    </div>
    <FloatingConfigurator />
    <div class="layout-mask animate-fadein" @click="hideMobileMenu" />
  </div>
  <ConfirmDialog />
</template>

<style lang="scss" scoped>
:global(.p-toast.p-component.p-toast-top-right),
:global(.p-toast.p-component.p-toast-top-left),
:global(.p-toast.p-component.p-toast-top-center) {
  top: 5rem;
}
</style>
