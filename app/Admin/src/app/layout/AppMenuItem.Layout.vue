<script setup lang="ts">
import { useLayout } from '@/app/layout/composables/layout.composable'
import { computed } from 'vue'

export interface MenuItem {
  label?: string
  icon?: string
  to?: string
  url?: string
  target?: string
  path?: string
  class?: string
  disabled?: boolean
  visible?: boolean
  separator?: boolean
  command?: (args: { originalEvent: Event; item: MenuItem }) => void
  items?: MenuItem[]
}

const { layoutState, isDesktop } = useLayout()

const props = defineProps<{
  item: MenuItem
  root?: boolean
  parentPath?: string
  index?: number
}>()

const fullPath = computed(() =>
  props.item.path ? (props.parentPath ? props.parentPath + props.item.path : props.item.path) : null,
)

const isActive = computed(() => {
  return props.item.path
    ? layoutState.activePath?.startsWith(fullPath.value!)
    : layoutState.activePath === props.item.to
})

const itemClick = (event: Event, item: MenuItem) => {
  if (item.disabled) {
    event.preventDefault()
    return
  }

  if (item.command) {
    item.command({ originalEvent: event, item })
  }

  if (item.items) {
    if (isActive.value) {
      layoutState.activePath = layoutState.activePath?.replace(item.path || '', '') || null
    } else {
      layoutState.activePath = fullPath.value
      layoutState.menuHoverActive = true
    }
  } else {
    layoutState.overlayMenuActive = false
    layoutState.mobileMenuActive = false
    layoutState.menuHoverActive = false
  }
}

const onMouseEnter = () => {
  if (isDesktop() && props.root && props.item.items && layoutState.menuHoverActive) {
    layoutState.activePath = fullPath.value
  }
}
</script>

<template>
  <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': isActive }">
    <div v-if="root && item.visible !== false" class="layout-menuitem-root-text">{{ item.label }}</div>
    <a
      v-if="(!item.to || item.items) && item.visible !== false"
      :href="item.url"
      @click="itemClick($event, item)"
      :class="item.class"
      :target="item.target"
      tabindex="0"
      @mouseenter="onMouseEnter"
    >
      <i :class="item.icon" class="layout-menuitem-icon" />
      <span class="layout-menuitem-text">{{ item.label }}</span>
      <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items" />
    </a>
    <router-link
      v-if="item.to && !item.items && item.visible !== false"
      @click="itemClick($event, item)"
      exactActiveClass="active-route"
      :class="item.class"
      tabindex="0"
      :to="item.to"
      @mouseenter="onMouseEnter"
    >
      <i :class="item.icon" class="layout-menuitem-icon" />
      <span class="layout-menuitem-text">{{ item.label }}</span>
      <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items" />
    </router-link>
    <Transition v-if="item.items && item.visible !== false" name="layout-submenu">
      <ul v-show="root ? true : isActive" class="layout-submenu">
        <AppMenuItem
          v-for="(child, childIdx) in item.items"
          :key="(child.label || '') + '_' + (child.to || child.path || childIdx)"
          :item="child"
          :root="false"
          :parent-path="fullPath || undefined"
        />
      </ul>
    </Transition>
  </li>
</template>
