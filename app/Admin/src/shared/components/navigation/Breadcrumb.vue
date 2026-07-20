<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const route = useRoute();

const resolveLabel = (key: string): string => {
  if (!key) return '';
  return t(key);
};

const breadcrumbs = computed(() => {
  const matched = route.matched.filter(r => r.meta && r.meta.breadcrumb);
  
  return matched.map((record, index) => {
    const path = record.path || '/';
    
    return {
      label: resolveLabel(record.meta.breadcrumb as string),
      to: record.name ? { name: record.name } : path,
      active: index === matched.length - 1
    };
  });
});
</script>

<template>
  <nav v-if="breadcrumbs.length > 0" class="flex mb-6 text-sm" aria-label="Breadcrumb">
    <ol class="inline-flex items-center space-x-1 md:space-x-3">
      <li v-for="(item, index) in breadcrumbs" :key="index" class="inline-flex items-center">
        <div class="flex items-center">
          <i v-if="index > 0" class="pi pi-chevron-right text-muted-color mx-2 text-xs"></i>
          
          <router-link
            v-if="!item.active"
            :to="item.to"
            class="transition-colors text-muted-color hover:text-primary flex items-center"
          >
            <i v-if="index === 0" class="mr-2 pi pi-home text-sm"></i>
            {{ item.label }}
          </router-link>
          
          <span v-else class="font-bold text-primary flex items-center">
            <i v-if="index === 0" class="mr-2 pi pi-home text-sm"></i>
            {{ item.label }}
          </span>
        </div>
      </li>
    </ol>
  </nav>
</template>
