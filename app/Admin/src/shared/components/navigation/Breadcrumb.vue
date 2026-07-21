<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

defineOptions({ name: 'AppBreadcrumb' })

const route = useRoute()

interface BreadcrumbItem {
  label: string
  to?: { name?: string; path?: string }
  active: boolean
}

const breadcrumbs = computed<BreadcrumbItem[]>(() => {
  const matched = route.matched.filter(r => r.meta && r.meta.breadcrumb)

  return matched.map((record, index) => ({
    label: (record.meta.breadcrumb as string) || '',
    to: record.name ? { name: record.name as string } : undefined,
    active: index === matched.length - 1,
  }))
})
</script>

<template>
  <nav v-if="breadcrumbs.length > 0" class="flex mb-6 text-sm" aria-label="Breadcrumb">
    <ol class="inline-flex items-center space-x-1 md:space-x-3">
      <li v-for="(item, index) in breadcrumbs" :key="index" class="inline-flex items-center">
        <div class="flex items-center">
          <i v-if="index > 0" class="pi pi-chevron-right text-muted-color mx-2 text-xs" />

          <router-link
            v-if="!item.active && item.to"
            :to="item.to"
            class="transition-colors text-muted-color hover:text-primary flex items-center"
          >
            <i v-if="index === 0" class="mr-2 pi pi-home text-sm" />
            {{ item.label }}
          </router-link>

          <span v-else class="font-semibold text-primary flex items-center">
            <i v-if="index === 0" class="mr-2 pi pi-home text-sm" />
            {{ item.label }}
          </span>
        </div>
      </li>
    </ol>
  </nav>
</template>
