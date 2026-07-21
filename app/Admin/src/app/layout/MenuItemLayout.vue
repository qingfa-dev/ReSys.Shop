<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import type { MenuItem } from '@/app/config/admin-menu.config'

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
  if (props.item.to && typeof props.item.to === 'string' && route.path === props.item.to) return true
  if (props.item.to && typeof props.item.to === 'object' && 'name' in props.item.to) {
    return route.name === props.item.to.name
  }
  if (props.item.items) {
    return props.item.items.some(child => {
      if (typeof child.to === 'string' && child.to === route.path) return true
      if (child.to && typeof child.to === 'object' && 'name' in child.to && route.name === child.to.name) return true
      if (child.items) return child.items.some(sub => {
        if (typeof sub.to === 'string' && sub.to === route.path) return true
        if (sub.to && typeof sub.to === 'object' && 'name' in sub.to && route.name === sub.to.name) return true
        return false
      })
      return false
    })
  }
  return false
})

watch(() => route.path, (newPath) => {
  if (props.item.items) {
    const hasActiveChild = props.item.items.some(child => {
      if (typeof child.to === 'string' && child.to === newPath) return true
      if (child.to && typeof child.to === 'object' && 'name' in child.to && route.name === child.to.name) return true
      if (child.items) return child.items.some(sub => {
        if (typeof sub.to === 'string' && sub.to === newPath) return true
        if (sub.to && typeof sub.to === 'object' && 'name' in sub.to && route.name === sub.to.name) return true
        return false
      })
      return false
    })
    if (hasActiveChild) active.value = true
  }
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
