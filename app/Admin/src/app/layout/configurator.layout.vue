<template>
  <Sidebar v-model:visible="visible" position="right" class="w-80">
    <template #header>
      <div class="flex items-center gap-2">
        <i class="pi pi-cog text-xl" />
        <span class="text-lg font-semibold">Configurator</span>
      </div>
    </template>

    <div class="flex flex-col gap-6">
      <div>
        <h3 class="mb-2 text-sm font-medium text-color-secondary">Dark Mode</h3>
        <div class="flex items-center justify-between">
          <span class="text-sm">Toggle dark mode</span>
          <InputSwitch
            :model-value="darkMode"
            @update:model-value="toggleDarkMode"
          />
        </div>
      </div>

      <div>
        <h3 class="mb-2 text-sm font-medium text-color-secondary">Scale</h3>
        <div class="flex items-center gap-2">
          <span class="text-sm text-color-secondary">Small</span>
          <Slider
            v-model="scale"
            :min="12"
            :max="16"
            :step="1"
            class="flex-1"
          />
          <span class="text-sm text-color-secondary">Large</span>
        </div>
        <div class="mt-1 text-center text-xs text-color-secondary">
          {{ scale }}px
        </div>
      </div>

      <div>
        <h3 class="mb-2 text-sm font-medium text-color-secondary">Menu Position</h3>
        <SelectButton
          v-model="menuPosition"
          :options="menuPositionOptions"
          option-label="label"
          option-value="value"
          class="w-full"
        />
      </div>

      <div>
        <h3 class="mb-2 text-sm font-medium text-color-secondary">Animation</h3>
        <div class="flex items-center justify-between">
          <span class="text-sm">Enable ripple</span>
          <InputSwitch v-model="rippleEnabled" />
        </div>
        <div class="mt-2 flex items-center justify-between">
          <span class="text-sm">Animate transitions</span>
          <InputSwitch v-model="animateTransitions" />
        </div>
      </div>
    </div>
  </Sidebar>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useLayout } from '@/app/layout/composables/layout.composable'

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
}>()

const { darkMode, toggleDarkMode } = useLayout()

const scale = ref(14)
const menuPosition = ref('side')
const rippleEnabled = ref(true)
const animateTransitions = ref(true)

const menuPositionOptions = [
  { label: 'Side', value: 'side' },
  { label: 'Top', value: 'top' },
  { label: 'Overlay', value: 'overlay' },
]

watch(
  () => props.visible,
  (v) => emit('update:visible', v),
)
</script>
