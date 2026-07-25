<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter, useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { StatCard, LoadingSkeleton, ErrorState, ListLayout } from '@/shared/components'
import Button from 'primevue/button'
import { CatalogDashboardApi } from '../api'
import type { CatalogDashboardResponse } from '../types'
import { ROUTE } from '../routes'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()

const data = ref<CatalogDashboardResponse | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const hero = computed(() => {
  const d = data.value!
  return {
    total: d.totalProducts,
    active: d.activeProducts,
    inactive: d.totalProducts - d.activeProducts,
  }
})

const activePercent = computed(() => {
  if (!data.value) return 0
  return Math.round((data.value.activeProducts / data.value.totalProducts) * 100)
})

const metrics = computed(() => [
  { label: t('catalog.dashboard.total_products'), value: data.value?.totalProducts ?? 0, icon: 'pi pi-box', color: 'primary' as const },
  { label: 'Variants', value: data.value?.totalVariants ?? 0, icon: 'pi pi-copy', color: 'blue' as const },
  { label: 'Taxonomies', value: data.value?.totalTaxonomies ?? 0, icon: 'pi pi-sitemap', color: 'green' as const },
  { label: 'Taxons', value: data.value?.totalTaxons ?? 0, icon: 'pi pi-tag', color: 'orange' as const },
])

const quickActions = [
  { label: t('catalog.dashboard.add_product'), icon: 'pi pi-plus', route: { name: ROUTE.PRODUCTS.CREATE } },
  { label: t('catalog.dashboard.import_csv'), icon: 'pi pi-upload', route: { name: ROUTE.PRODUCTS.CREATE } },
  { label: t('catalog.dashboard.manage_categories'), icon: 'pi pi-sitemap', route: { name: ROUTE.TAXONOMIES.LIST } },
]

const recentProducts = computed(() =>
  data.value?.recentProducts.map(p => ({
    name: p.name,
    sku: p.slug,
    time: formatRelativeTime(p.createdAtUtc),
  })) ?? [],
)

function formatRelativeTime(utc: string): string {
  const diff = Date.now() - new Date(utc).getTime()
  const minutes = Math.floor(diff / 60000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  if (days < 30) return `${days}d ago`
  return new Date(utc).toLocaleDateString()
}

async function fetchDashboard() {
  loading.value = true
  error.value = null
  try {
    const result = await CatalogDashboardApi.get()
    if (result.isSuccess) {
      data.value = result.value
    } else {
      error.value = result.message ?? t('catalog.dashboard.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    error.value = t('catalog.dashboard.messages.load_failed')
  }
  loading.value = false
}

onMounted(fetchDashboard)
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('catalog.dashboard.title')"
        :subtitle="t('catalog.dashboard.description')"
        :icon="route.meta?.icon as string | undefined"
      >
        <template #actions>
          <Button :label="t('catalog.dashboard.add_product')" icon="pi pi-plus" size="small" @click="router.push({ name: ROUTE.PRODUCTS.CREATE })" />
        </template>
      </PageHeader>
    </template>

    <LoadingSkeleton v-if="loading" rows="4" columns="4" />

    <ErrorState
      v-else-if="error"
      :title="error"
      @retry="fetchDashboard"
    />

    <template v-else>
      <div class="mb-6 rounded-lg border-l-4 border-emerald-500 bg-stone-50 p-6 dark:border-emerald-400 dark:bg-slate-800">
        <div class="flex items-baseline gap-2">
          <span class="font-display text-4xl leading-none text-surface-900 dark:text-surface-0">{{ hero.total.toLocaleString() }}</span>
          <span class="text-sm text-surface-500">{{ t('catalog.dashboard.total_products').toLowerCase() }}</span>
        </div>
        <div class="mt-4">
          <div class="flex h-2 w-full overflow-hidden rounded-full bg-stone-300 dark:bg-slate-600">
            <div class="h-full rounded-full bg-emerald-500 transition-all" :style="{ width: activePercent + '%' }" />
          </div>
          <div class="mt-2 flex items-center gap-4 text-xs text-surface-500">
            <span class="flex items-center gap-1.5">
              <span class="inline-block h-2 w-2 rounded-full bg-emerald-500" />
              {{ hero.active.toLocaleString() }} {{ t('catalog.dashboard.active') }} ({{ activePercent }}%)
            </span>
            <span class="flex items-center gap-1.5">
              <span class="inline-block h-2 w-2 rounded-full bg-stone-300 dark:bg-slate-600" />
              {{ hero.inactive.toLocaleString() }} {{ t('catalog.dashboard.inactive') }} ({{ 100 - activePercent }}%)
            </span>
          </div>
        </div>
      </div>

      <div class="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard v-for="m in metrics" :key="m.label" :label="m.label" :value="m.value" :icon="m.icon" :color="m.color" />
      </div>

      <div class="mb-8">
        <p class="mb-3 text-xs font-semibold uppercase tracking-wider text-surface-400">{{ t('catalog.dashboard.quick_actions') }}</p>
        <div class="flex flex-wrap gap-3">
          <Button
            v-for="a in quickActions"
            :key="a.label"
            :label="a.label"
            :icon="a.icon"
            outlined
            size="small"
            @click="router.push(a.route)"
          />
        </div>
      </div>

      <div>
        <p class="mb-3 text-xs font-semibold uppercase tracking-wider text-surface-400">{{ t('catalog.dashboard.recently_updated') }}</p>
        <div v-if="recentProducts.length" class="divide-y divide-surface-200 rounded-lg border border-surface-200 dark:divide-surface-700 dark:border-surface-700">
          <div
            v-for="p in recentProducts"
            :key="p.sku"
            class="flex items-center justify-between px-4 py-3 transition-colors hover:bg-surface-50 dark:hover:bg-surface-800"
          >
            <div>
              <p class="text-sm font-medium text-surface-900 dark:text-surface-0">{{ p.name }}</p>
              <p class="text-xs text-surface-400">{{ p.sku }}</p>
            </div>
            <span class="text-xs text-surface-400">{{ p.time }}</span>
          </div>
        </div>
        <p v-else class="rounded-lg border border-dashed border-surface-300 px-4 py-8 text-center text-sm text-surface-400">
          {{ t('catalog.dashboard.recent_empty') }}
        </p>
      </div>
    </template>
  </ListLayout>
</template>
