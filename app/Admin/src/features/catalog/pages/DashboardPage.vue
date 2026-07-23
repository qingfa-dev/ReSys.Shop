<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StatCard from '@/shared/components/data/StatCard.vue'

const route = useRoute()
const { t } = useI18n()
const stats = [
  { label: 'Total Products', value: '0', icon: 'pi pi-shopping-bag', color: 'primary' as const },
  { label: 'Active Products', value: '0', icon: 'pi pi-check-circle', color: 'green' as const },
  { label: t('catalog.taxonomies.titles.list'), value: '0', icon: 'pi pi-sitemap', color: 'blue' as const },
  { label: t('catalog.option_types.titles.list'), value: '0', icon: 'pi pi-list', color: 'orange' as const },
]
</script>

<template>
  <div>
    <PageHeader :title="t('catalog.products.titles.list')" :subtitle="t('catalog.products.descriptions.list')" :icon="route.meta?.icon as string | undefined" />
    <div class="grid">
      <div v-for="s in stats" :key="s.label" class="col-12 md:col-6 lg:col-3">
        <StatCard :label="s.label" :value="s.value" :icon="s.icon" :color="s.color" />
      </div>
    </div>
  </div>
</template>
