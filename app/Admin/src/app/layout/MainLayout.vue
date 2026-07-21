<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'
import { computed, watch, ref, onUnmounted } from 'vue'
import { RouterView } from 'vue-router'
import AppTopbar from './TopbarLayout.vue'
import AppSidebar from './SidebarLayout.vue'
import AppFooter from './FooterLayout.vue'
import AppBreadcrumb from './BreadcrumbLayout.vue'

const { layoutConfig, layoutState, hideMobileMenu } = useLayout()

const containerClass = computed(() => ({
  'layout-overlay': layoutConfig.menuMode === 'overlay',
  'layout-static': layoutConfig.menuMode === 'static',
  'layout-static-inactive': layoutState.staticMenuInactive && layoutConfig.menuMode === 'static',
  'layout-overlay-active': layoutState.overlayMenuActive,
  'layout-mobile-active': layoutState.mobileMenuActive,
}))

const outsideClickListener = ref<((event: MouseEvent) => void) | null>(null)

watch(() => layoutState.mobileMenuActive, (newVal) => {
  if (newVal) {
    bindOutsideClickListener()
  } else {
    unbindOutsideClickListener()
  }
})

function bindOutsideClickListener() {
  if (!outsideClickListener.value) {
    outsideClickListener.value = (event: MouseEvent) => {
      if (isOutsideClicked(event)) {
        hideMobileMenu()
      }
    }
    document.addEventListener('click', outsideClickListener.value)
  }
}

function unbindOutsideClickListener() {
  if (outsideClickListener.value) {
    document.removeEventListener('click', outsideClickListener.value)
    outsideClickListener.value = null
  }
}

onUnmounted(() => {
  unbindOutsideClickListener()
})

function isOutsideClicked(event: MouseEvent) {
  const sidebarEl = document.querySelector('.layout-sidebar')
  const topbarEl = document.querySelector('.layout-menu-button')
  return !(
    sidebarEl?.isSameNode(event.target as Node)
    || sidebarEl?.contains(event.target as Node)
    || topbarEl?.isSameNode(event.target as Node)
    || topbarEl?.contains(event.target as Node)
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
        <RouterView v-slot="{ Component, route }">
          <Transition name="layout-main" mode="out-in">
            <component :is="Component" :key="route.path" />
          </Transition>
        </RouterView>
      </div>
      <AppFooter />
    </div>
    <div class="layout-mask" @click="hideMobileMenu" />
  </div>
</template>
