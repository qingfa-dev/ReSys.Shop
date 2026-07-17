<script setup lang="ts">
import { useLayout } from './composables/layout.composable'

interface Preset {
  label: string
  value: string
}

interface ColorOption {
  label: string
  value: string
}

const { layoutConfig, layoutState, changeMenuMode, toggleConfigSidebar } = useLayout()

const presets: Preset[] = [
  { label: 'Aura', value: 'Aura' },
  { label: 'Lara', value: 'Lara' },
  { label: 'Nora', value: 'Nora' },
]

const surfaceOptions: ColorOption[] = [
  { label: 'Slate', value: 'slate' },
  { label: 'Gray', value: 'gray' },
  { label: 'Zinc', value: 'zinc' },
  { label: 'Neutral', value: 'neutral' },
  { label: 'Stone', value: 'stone' },
  { label: 'Soho', value: 'soho' },
  { label: 'Viva', value: 'viva' },
  { label: 'Owl', value: 'owl' },
]

const primaryColors: ColorOption[] = [
  { label: 'Emerald', value: 'emerald' },
  { label: 'Green', value: 'green' },
  { label: 'Lime', value: 'lime' },
  { label: 'Orange', value: 'orange' },
  { label: 'Amber', value: 'amber' },
  { label: 'Yellow', value: 'yellow' },
  { label: 'Teal', value: 'teal' },
  { label: 'Cyan', value: 'cyan' },
  { label: 'Sky', value: 'sky' },
  { label: 'Blue', value: 'blue' },
  { label: 'Indigo', value: 'indigo' },
  { label: 'Violet', value: 'violet' },
  { label: 'Purple', value: 'purple' },
  { label: 'Fuchsia', value: 'fuchsia' },
  { label: 'Pink', value: 'pink' },
  { label: 'Rose', value: 'rose' },
]

const menuModes = [
  { label: 'Static', value: 'static', icon: 'pi pi-bars' },
  { label: 'Overlay', value: 'overlay', icon: 'pi pi-window-maximize' },
]
</script>

<template>
  <div class="layout-config-sidebar" :class="{ 'layout-config-sidebar-active': layoutState.configSidebarVisible }">
    <div class="layout-config-sidebar-header">
      <span class="text-lg font-semibold">Theme Config</span>
      <button class="p-link layout-config-close" @click="toggleConfigSidebar">
        <i class="pi pi-times"></i>
      </button>
    </div>

    <div class="layout-config-sidebar-content">
      <div class="config-section">
        <h4>Preset</h4>
        <div class="config-options">
          <button
            v-for="preset in presets"
            :key="preset.value"
            class="config-option"
            :class="{ 'config-option-active': layoutConfig.preset === preset.value }"
            @click="layoutConfig.preset = preset.value"
          >
            {{ preset.label }}
          </button>
        </div>
      </div>

      <div class="config-section">
        <h4>Primary Color</h4>
        <div class="config-colors">
          <button
            v-for="color in primaryColors"
            :key="color.value"
            class="config-color"
            :style="{ backgroundColor: `var(--p-${color.value}-500)` }"
            :class="{ 'config-color-active': layoutConfig.primary === color.value }"
            :title="color.label"
            @click="layoutConfig.primary = color.value"
          />
        </div>
      </div>

      <div class="config-section">
        <h4>Surface</h4>
        <div class="config-colors">
          <button
            v-for="surface in surfaceOptions"
            :key="surface.value"
            class="config-color"
            :style="{ backgroundColor: `var(--p-${surface.value}-500)` }"
            :class="{ 'config-color-active': layoutConfig.surface === surface.value }"
            :title="surface.label"
            @click="layoutConfig.surface = surface.value"
          />
        </div>
      </div>

      <div class="config-section">
        <h4>Menu Mode</h4>
        <div class="config-options">
          <button
            v-for="mode in menuModes"
            :key="mode.value"
            class="config-option"
            :class="{ 'config-option-active': layoutConfig.menuMode === mode.value }"
            @click="changeMenuMode(mode.value)"
          >
            <i :class="mode.icon"></i>
            {{ mode.label }}
          </button>
        </div>
      </div>
    </div>

    <div class="layout-config-sidebar-footer">
      <p class="text-xs text-surface-500">Changes apply immediately</p>
    </div>
  </div>
</template>
