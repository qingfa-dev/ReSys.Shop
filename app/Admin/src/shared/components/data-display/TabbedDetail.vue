<script setup lang="ts">
import type { Component } from 'vue'

export interface TabDef {
  label: string
  icon?: string
  value: number | string
  panel: Component
}

const props = withDefaults(defineProps<{
  tabs: TabDef[]
  scrollable?: boolean
}>(), {
  scrollable: true,
})

const activeTab = defineModel<string | number>('activeTab', { required: true })
</script>

<template>
  <Tabs v-model:value="activeTab">
    <TabList :scrollable="scrollable">
      <Tab v-for="tab in tabs" :key="tab.value" :value="tab.value">
        <div class="flex items-center gap-2">
          <i v-if="tab.icon" :class="tab.icon" />
          <span>{{ tab.label }}</span>
        </div>
      </Tab>
    </TabList>
    <TabPanels class="p-6">
      <TabPanel v-for="tab in tabs" :key="tab.value" :value="tab.value">
        <component :is="tab.panel" />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
