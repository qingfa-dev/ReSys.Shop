<script setup lang="ts">
const visible = defineModel<boolean>({ required: true })

withDefaults(defineProps<{
  header: string
  maxWidth?: string
  closable?: boolean
  dismissableMask?: boolean
}>(), {
  maxWidth: 'max-w-lg',
  closable: true,
  dismissableMask: true,
})
</script>

<template>
  <Dialog
    v-model:visible="visible"
    :header="header"
    :closable="closable"
    :dismissableMask="dismissableMask"
    modal
    :class="maxWidth"
  >
    <slot />
    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </Dialog>
</template>
