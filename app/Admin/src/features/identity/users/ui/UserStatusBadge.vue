<template>
  <AppStatusBadge :label="label" :tone="tone" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { UserStatus } from '../model/user.types'

const props = defineProps<{ status: UserStatus }>()

const label = computed(() => props.status)
const tone = computed<'success' | 'secondary' | 'info' | 'danger'>(() => {
  switch (props.status) {
    case 'active': return 'success'
    case 'inactive': return 'secondary'
    case 'invited': return 'info'
    case 'suspended': return 'danger'
    default: return 'secondary'
  }
})
</script>
