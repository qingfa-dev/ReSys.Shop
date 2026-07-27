<script setup lang="ts">
import { useLayout } from '@/shared/composables/useLayout';
import { computed } from 'vue';

const { layoutState, isDesktop } = useLayout();

interface MenuItemParameters {
  label?: string
  icon?: string
  to?: string
  path?: string
  url?: string
  target?: string
  class?: string
  disabled?: boolean
  visible?: boolean
  items?: MenuItemParameters[]
  command?: (args: { originalEvent: Event; item: MenuItemParameters }) => void
}

interface Props {
  item?: MenuItemParameters
  root?: boolean
  parentPath?: string | null
}

const props = withDefaults(defineProps<Props>(), {
    item: () => ({}),
    root: true,
    parentPath: null
});

const fullPath = computed(() => (props.item.path ? (props.parentPath ? props.parentPath + props.item.path : props.item.path) : ''));

const isActive = computed(() => {
    return props.item.path ? layoutState.activePath?.startsWith(fullPath.value) : layoutState.activePath === props.item.to;
});

const itemClick = (event: Event, item: MenuItemParameters): void => {
    if (item.disabled) {
        event.preventDefault();
        return;
    }

    if (item.command) {
        item.command({ originalEvent: event, item: item });
    }

    if (item.items) {
        if (isActive.value) {
            layoutState.activePath = layoutState.activePath?.replace(item.path ?? '', '') ?? null;
        } else {
            layoutState.activePath = fullPath.value;
            layoutState.menuHoverActive = true;
        }
    } else {
        layoutState.overlayMenuActive = false;
        layoutState.mobileMenuActive = false;
        layoutState.menuHoverActive = false;
    }
};

const onMouseEnter = () => {
    if (isDesktop() && props.root && props.item.items && layoutState.menuHoverActive) {
        layoutState.activePath = fullPath.value;
    }
};
</script>

<template>
    <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': isActive }">
        <div v-if="root && item.visible !== false" class="layout-menuitem-root-text">{{ item.label }}</div>
        <a v-if="(!item.to || item.items) && item.visible !== false" :href="item.url" @click="itemClick($event, item)" :class="item.class" :target="item.target" tabindex="0" @mouseenter="onMouseEnter">
            <i :class="item.icon" class="layout-menuitem-icon" />
            <span class="layout-menuitem-text">{{ item.label }}</span>
            <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items" />
        </a>
        <router-link v-if="item.to && !item.items && item.visible !== false" @click="itemClick($event, item)" exactActiveClass="active-route" :class="item.class" tabindex="0" :to="item.to" @mouseenter="onMouseEnter">
            <i :class="item.icon" class="layout-menuitem-icon" />
            <span class="layout-menuitem-text">{{ item.label }}</span>
            <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items" />
        </router-link>
        <Transition v-if="item.items && item.visible !== false" name="layout-submenu">
            <ul v-show="root ? true : isActive" class="layout-submenu">
                <app-menu-item v-for="child in item.items" :key="child.label + '_' + (child.to || child.path)" :item="child" :root="false" :parentPath="fullPath" />
            </ul>
        </Transition>
    </li>
</template>
