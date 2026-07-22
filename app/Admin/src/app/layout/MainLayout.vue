<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'
import { computed } from 'vue'
import { RouterView } from 'vue-router'
import AppTopbar from './TopbarLayout.vue'
import AppSidebar from './SidebarLayout.vue'
import AppFooter from './FooterLayout.vue'
import AppBreadcrumb from './BreadcrumbLayout.vue'

const { layoutConfig, layoutState, hideMobileMenu } = useLayout()

const clickOutsideExcept = '.layout-menu-button' as const

const containerClass = computed(() => ({
  'layout-overlay': layoutConfig.menuMode === 'overlay',
  'layout-static': layoutConfig.menuMode === 'static',
  'layout-static-inactive': layoutState.staticMenuInactive && layoutConfig.menuMode === 'static',
  'layout-overlay-active': layoutState.overlayMenuActive,
  'layout-mobile-active': layoutState.mobileMenuActive,
}))
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <AppTopbar />
    <div v-click-outside:[clickOutsideExcept]="hideMobileMenu">
      <AppSidebar />
    </div>
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
