<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import Badge from 'primevue/badge'
import type { MenuItem } from '@/app/config/admin-menu.config'
import { isRouteActive } from '@/app/config/route-matcher'

defineOptions({ name: 'AppMenuItem' })

const route = useRoute()
const { layoutState } = useLayout()

const props = defineProps<{
  item: MenuItem
  index?: number
  root?: boolean
}>()

const active = ref(false)

const isActive = computed(() => {
  return isRouteActive(props.item, route.path, route.name)
})

watch(() => route.path, (newPath) => {
  if (isRouteActive(props.item, newPath, route.name)) active.value = true
}, { immediate: true })

function itemClick(event: Event, item: MenuItem) {
  if (item.disabled) {
    event.preventDefault()
    return
  }
  if (item.command) {
    item.command({ originalEvent: event, item })
  }
  if (item.items) {
    active.value = !active.value
  }
  if (item.to || item.url) {
    layoutState.overlayMenuActive = false
    layoutState.mobileMenuActive = false
  }
}
</script>

<template>
  <template v-if="item.visible !== false">
    <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': active || isActive }">
      <div v-if="root" class="layout-menuitem-root-text">
        {{ item.label }}
        <span v-if="item.badge" class="ml-auto">
          <Badge :value="item.badge" severity="info" size="small" />
        </span>
      </div>

      <a
        v-if="!root && item.items"
        :href="item.url"
        @click="itemClick($event, item)"
        :class="[item.class, { 'active-route': isActive }]"
        :target="item.target"
        tabindex="0"
      >
        <i v-if="item.icon" :class="item.icon" class="layout-menuitem-icon" />
        <span class="layout-menuitem-text">{{ item.label }}</span>
        <span v-if="item.badge" class="ml-auto">
          <Badge :value="item.badge" severity="info" size="small" />
        </span>
        <i class="pi pi-fw pi-angle-down layout-submenu-toggler" />
      </a>

      <router-link
        v-if="!root && item.to && !item.items"
        @click="itemClick($event, item)"
        exactActiveClass="active-route"
        :class="[item.class]"
        tabindex="0"
        :to="item.to"
      >
        <i v-if="item.icon" :class="item.icon" class="layout-menuitem-icon" />
        <span class="layout-menuitem-text">{{ item.label }}</span>
        <span v-if="item.badge" class="ml-auto">
          <Badge :value="item.badge" severity="info" size="small" />
        </span>
      </router-link>

      <Transition v-if="item.items" name="layout-submenu">
        <ul v-show="root ? true : active || isActive" class="layout-submenu">
          <AppMenuItem
            v-for="(child, ci) in item.items"
            :key="child.label + '_' + ci"
            :item="child"
            :index="ci"
            :root="false"
          />
        </ul>
      </Transition>
    </li>
  </template>
</template>
