import type { MenuItem } from './admin-menu.config'
import type { RouteLocationRaw } from 'vue-router'

function isRouteMatch(
  target: RouteLocationRaw | undefined,
  path: string,
  name: string | symbol | null | undefined,
): boolean {
  if (!target) return false
  if (typeof target === 'string') return target === path
  if (typeof target === 'object' && 'name' in target) {
    return name === target.name
  }
  return false
}

export function isRouteActive(
  item: MenuItem,
  path: string,
  name: string | symbol | null | undefined,
): boolean {
  if (isRouteMatch(item.to, path, name)) return true
  if (item.items) {
    return item.items.some((child) => isRouteMatch(child.to, path, name)
      || (child.items ? child.items.some((sub) => isRouteMatch(sub.to, path, name)) : false))
  }
  return false
}
