<script setup lang="ts">
import { useLayout } from '@/shared/composables/useLayout';
import { computed } from 'vue';
import AppFooter from '../ui/AppFooter.vue';
import AppSidebar from './AppSidebar.vue';
import AppTopbar from './AppTopbar.vue';

const { layoutConfig, layoutState, hideMobileMenu } = useLayout();

const containerClass = computed<Record<string, boolean | undefined>>(() => {
    return {
        'layout-overlay': layoutConfig.menuMode === 'overlay',
        'layout-static': layoutConfig.menuMode === 'static',
        'layout-overlay-active': layoutState.overlayMenuActive,
        'layout-mobile-active': layoutState.mobileMenuActive,
        'layout-static-inactive': layoutState.staticMenuInactive
    };
});
</script>

<template>
    <div class="layout-wrapper" :class="containerClass">
        <AppTopbar />
        <AppSidebar />
        <div class="layout-main-container">
            <div class="layout-main">
                <router-view />
            </div>
            <AppFooter />
        </div>
        <div class="layout-mask animate-fadein" @click="hideMobileMenu" />
    </div>
    <Toast />
</template>
