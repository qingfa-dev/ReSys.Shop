<script setup lang="ts">
import AppConfigurator from '../Configurator.Layout.vue'
import { useLayout } from '../composables/layout.composable'

const { toggleDarkMode, isDarkTheme } = useLayout()
</script>

<template>
  <div class="fixed flex gap-4 top-8 right-8 z-50">
    <Button
      type="button"
      @click="toggleDarkMode"
      rounded
      :icon="isDarkTheme ? 'pi pi-moon' : 'pi pi-sun'"
      severity="secondary"
    />
    <div class="relative">
      <Button
        icon="pi pi-palette"
        v-styleclass="{
          selector: '@next',
          enterFromClass: 'hidden',
          enterActiveClass: 'animate-scalein',
          leaveToClass: 'hidden',
          leaveActiveClass: 'animate-fadeout',
          hideOnOutsideClick: true,
        }"
        type="button"
        rounded
      />
      <AppConfigurator />
    </div>
  </div>
</template>
