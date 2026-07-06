<template>
  <li>
    <div
      v-ripple
      class="flex cursor-pointer items-center gap-2 rounded-lg p-3 transition-colors duration-200 hover:bg-surface-100"
      :class="[isActive && 'bg-primary-50 text-primary-700']"
      @click="handleClick"
    >
      <i v-if="item.icon" :class="item.icon" class="text-lg" />
      <span v-if="!collapsed" class="text-sm font-medium">{{ item.label }}</span>
      <i
        v-if="item.items && !collapsed"
        class="pi pi-angle-down ml-auto transition-transform duration-200"
        :class="{ 'rotate-180': expanded }"
      />
    </div>
    <ul
      v-if="item.items && !collapsed"
      v-show="expanded"
      class="ml-4 overflow-hidden transition-all duration-200"
    >
      <MenuItemLayout
        v-for="(child, index) in item.items"
        :key="index"
        :item="child"
        :collapsed="collapsed"
      />
    </ul>
  </li>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { MenuItem } from './composables/layout.composable'

const props = defineProps<{
  item: MenuItem
  collapsed: boolean
}>()

const route = useRoute()
const router = useRouter()

const expanded = ref(false)

const isActive = computed(() => {
  if (props.item.to) {
    return route.path === props.item.to
  }
  return false
})

function handleClick() {
  if (props.item.to) {
    router.push(props.item.to)
  }
  if (props.item.items) {
    expanded.value = !expanded.value
  }
}
</script>
