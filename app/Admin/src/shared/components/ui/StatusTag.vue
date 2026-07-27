<script setup lang="ts">
type StatusDomain = 'inventory' | 'order' | 'stock' | 'default'

interface Props {
  status: string
  domain?: StatusDomain
}

withDefaults(defineProps<Props>(), {
  domain: 'default',
})

function getSeverity(status: string, domain: StatusDomain): string {
  const mappings: Record<StatusDomain, Record<string, string>> = {
    inventory: { INSTOCK: 'success', LOWSTOCK: 'warn', OUTOFSTOCK: 'danger' },
    order: { DELIVERED: 'success', CANCELLED: 'danger', PENDING: 'warn', RETURNED: 'info' },
    stock: { INSTOCK: 'success', LOWSTOCK: 'warn', OUTOFSTOCK: 'danger' },
    default: {},
  }
  return mappings[domain]?.[status] || 'info'
}
</script>

<template>
  <Tag :value="status" :severity="getSeverity(status, domain)" />
</template>
