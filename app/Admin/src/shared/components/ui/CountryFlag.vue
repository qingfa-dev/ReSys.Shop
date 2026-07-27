<script setup lang="ts">
interface Props {
  country: {
    name: string
    code: string
  }
}

defineProps<Props>()
</script>

<template>
  <div class="flex items-center gap-2">
    <img
      :alt="country.name"
      src="https://primefaces.org/cdn/primevue/images/flag/flag_placeholder.png"
      :class="`fi fi-${country.code.toLowerCase()}`"
      style="width: 18px"
    />
    <span>{{ country.name }}</span>
  </div>
</template>
