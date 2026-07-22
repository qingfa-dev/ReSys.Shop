<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ password: string }>()

const rules = computed(() => [
  { key: 'minLength', label: 'At least 8 characters', met: (props.password?.length ?? 0) >= 8 },
  { key: 'uppercase', label: 'At least one uppercase letter', met: /[A-Z]/.test(props.password ?? '') },
  { key: 'lowercase', label: 'At least one lowercase letter', met: /[a-z]/.test(props.password ?? '') },
  { key: 'digit', label: 'At least one digit', met: /[0-9]/.test(props.password ?? '') },
  { key: 'special', label: 'At least one special character', met: /[^A-Za-z0-9]/.test(props.password ?? '') },
])

const metCount = computed(() => rules.value.filter((r) => r.met).length)
const strengthColor = computed(() => {
  if (metCount.value <= 2) return 'var(--p-red-500)'
  if (metCount.value <= 4) return 'var(--p-amber-500)'
  return 'var(--p-green-500)'
})
</script>

<template>
  <div v-if="password" class="mt-2">
    <div class="flex gap-1 mb-2">
      <div
        v-for="i in rules.length"
        :key="i"
        class="h-1 flex-1 rounded-full transition-colors duration-200"
        :style="{ backgroundColor: i <= metCount ? strengthColor : 'var(--p-surface-200)' }"
      />
    </div>

    <ul class="space-y-1 text-sm">
      <li
        v-for="rule in rules"
        :key="rule.key"
        class="flex items-center gap-2"
        :class="rule.met ? 'text-green-600 dark:text-green-400' : 'text-muted-color'"
      >
        <i :class="rule.met ? 'pi pi-check-circle' : 'pi pi-circle'" class="text-xs" />
        {{ rule.label }}
      </li>
    </ul>
  </div>
</template>
