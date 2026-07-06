<template>
  <Tag :value="label" :severity="severity" />
</template>

<script setup lang="ts">
import { computed } from 'vue'

type Tone = 'success' | 'info' | 'warn' | 'danger' | 'secondary'

const props = withDefaults(
  defineProps<{ label: string; tone?: Tone }>(),
  { tone: 'info' },
)

const severity = computed(() => props.tone)
</script>
