<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRouter, useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StatCard from '@/shared/components/data/StatCard.vue'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()

interface Metric {
  label: string
  value: string | number
  icon: string
  color: 'primary' | 'green' | 'orange' | 'red' | 'blue'
}

const hero = {
  total: 1247,
  active: 892,
  inactive: 355,
  trend: '+5',
}

const activePercent = Math.round((hero.active / hero.total) * 100)

const metrics: Metric[] = [
  { label: t('catalog.taxonomies.titles.list'), value: '8', icon: 'pi pi-sitemap', color: 'primary' },
  { label: t('catalog.option_types.titles.list'), value: '24', icon: 'pi pi-list', color: 'blue' },
  { label: t('catalog.dashboard.catalog_coverage'), value: '78%', icon: 'pi pi-percentage', color: 'green' },
  { label: t('catalog.dashboard.needs_attention'), value: '23', icon: 'pi pi-exclamation-triangle', color: 'orange' },
]

const quickActions = [
  { label: t('catalog.dashboard.add_product'), icon: 'pi pi-plus', route: { name: 'catalog.products.create' } },
  { label: t('catalog.dashboard.import_csv'), icon: 'pi pi-upload', route: { name: 'catalog.products.create' } },
  { label: t('catalog.dashboard.manage_categories'), icon: 'pi pi-sitemap', route: { name: 'catalog.taxonomies.list' } },
]

const recentProducts = [
  { name: 'Vintage Denim Jacket', sku: 'VDJ-001', time: '2 hours ago' },
  { name: 'Organic Cotton T-Shirt', sku: 'OCT-002', time: '5 hours ago' },
  { name: 'Handwoven Basket Bag', sku: 'HBB-003', time: '1 day ago' },
  { name: 'Merino Wool Sweater', sku: 'MWS-004', time: '2 days ago' },
  { name: 'Ceramic Mug Set', sku: 'CMS-005', time: '3 days ago' },
]

const attentionItems = [
  { reason: 'No primary image', count: 8 },
  { reason: 'Missing category', count: 6 },
  { reason: 'Out of stock', count: 5 },
  { reason: 'No price set', count: 4 },
]
</script>

<template>
  <div>
    <PageHeader
      :title="t('catalog.dashboard.title')"
      :subtitle="t('catalog.dashboard.description')"
      :icon="route.meta?.icon as string | undefined"
    >
      <template #actions>
        <Button :label="t('catalog.dashboard.add_product')" icon="pi pi-plus" size="small" @click="router.push({ name: 'catalog.products.create' })" />
      </template>
    </PageHeader>

    <div class="mb-6 rounded-lg border-l-4 border-emerald-500 bg-stone-50 p-6 dark:border-emerald-400 dark:bg-slate-800">
      <div class="flex items-baseline gap-2">
        <span class="font-display text-4xl leading-none text-surface-900 dark:text-surface-0">{{ hero.total.toLocaleString() }}</span>
        <span class="text-sm text-surface-500">{{ t('catalog.dashboard.total_products').toLowerCase() }}</span>
      </div>
      <p class="mt-4 flex items-center gap-2 text-sm text-surface-500">
        <span class="flex items-center gap-1 text-emerald-600 dark:text-emerald-400">
          <i class="pi pi-arrow-up text-xs" />{{ hero.trend }}%
        </span>
        {{ t('catalog.dashboard.this_month') }}
      </p>
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

    <div class="grid gap-6 lg:grid-cols-2">
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

      <div>
        <p class="mb-3 text-xs font-semibold uppercase tracking-wider text-surface-400">{{ t('catalog.dashboard.needs_attention') }}</p>
        <div v-if="attentionItems.length" class="divide-y divide-surface-200 rounded-lg border border-surface-200 dark:divide-surface-700 dark:border-surface-700">
          <div
            v-for="item in attentionItems"
            :key="item.reason"
            class="flex items-center justify-between px-4 py-3 transition-colors hover:bg-surface-50 dark:hover:bg-surface-800"
          >
            <p class="text-sm text-surface-700 dark:text-surface-200">{{ item.reason }}</p>
            <span class="rounded-full bg-orange-100 px-2 py-0.5 text-xs font-medium text-orange-700 dark:bg-orange-400/10 dark:text-orange-400">{{ item.count }}</span>
          </div>
        </div>
        <p v-else class="rounded-lg border border-dashed border-surface-300 px-4 py-8 text-center text-sm text-surface-400">
          {{ t('catalog.dashboard.attention_empty') }}
        </p>
      </div>
    </div>
  </div>
</template>
