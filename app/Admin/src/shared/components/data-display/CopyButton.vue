<script setup lang="ts">
import { ref } from 'vue'

const props = withDefaults(defineProps<{
  value: string
  label?: string
  icon?: string
  variant?: 'button' | 'link'
}>(), {
  icon: 'pi pi-copy',
  variant: 'link',
})

const copied = ref(false)

async function copy() {
  try {
    await navigator.clipboard.writeText(props.value)
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 2000)
  } catch {
    // clipboard API unavailable
  }
}
</script>

<template>
  <button
    type="button"
    :title="label ?? 'Copy'"
    class="inline-flex items-center gap-1 border-none bg-transparent cursor-pointer transition-opacity hover:opacity-70"
    :class="{ 'p-button p-button-text p-button-sm p-button-rounded': variant === 'button' }"
    style="color: var(--p-text-muted-color)"
    @click="copy"
  >
    <i :class="copied ? 'pi pi-check' : icon" class="text-xs" :style="{ color: copied ? 'var(--p-green-500)' : '' }" />
  </button>
</template>
