<script setup lang="ts">
type FormLayout = 'vertical' | 'horizontal' | 'inline'

interface Props {
  label: string
  layout?: FormLayout
  helpText?: string
  invalid?: boolean
  required?: boolean
}

withDefaults(defineProps<Props>(), {
  layout: 'vertical',
})
</script>

<template>
  <div v-if="layout === 'vertical'" class="flex flex-col gap-1">
    <label class="text-surface-900 dark:text-surface-0 font-medium">
      {{ label }}<span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    <slot />
    <small v-if="helpText" class="text-muted-color">{{ helpText }}</small>
    <small v-if="invalid" class="text-red-500">This field is required</small>
  </div>
  <div v-else-if="layout === 'horizontal'" class="grid grid-cols-12 gap-4 items-center">
    <label class="col-span-12 md:col-span-2 text-surface-900 dark:text-surface-0 font-medium">
      {{ label }}<span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    <div class="col-span-12 md:col-span-10">
      <slot />
      <small v-if="helpText" class="text-muted-color block mt-1">{{ helpText }}</small>
    </div>
  </div>
  <div v-else class="flex flex-wrap items-start gap-4">
    <label class="sr-only">{{ label }}</label>
    <slot />
  </div>
</template>
