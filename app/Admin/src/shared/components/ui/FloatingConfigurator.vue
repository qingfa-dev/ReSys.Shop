<script setup lang="ts">
import AppConfigurator from '../layout/AppConfigurator.vue';
import { useLayout } from '@/shared/composables/useLayout';
import Moon from '@primeicons/vue/moon'
import Sun from '@primeicons/vue/sun'
import Palette from '@primeicons/vue/palette'

const { toggleDarkMode, isDarkTheme } = useLayout();
</script>

<template>
    <div class="fixed flex gap-4 top-8 right-8">
        <Button type="button" @click="toggleDarkMode" rounded severity="secondary">
            <Moon v-if="isDarkTheme" />
            <Sun v-else />
        </Button>
        <div class="relative">
            <Button
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
                type="button"
                rounded
            >
                <Palette />
            </Button>
            <AppConfigurator />
        </div>
    </div>
</template>
