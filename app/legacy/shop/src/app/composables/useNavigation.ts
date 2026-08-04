import { computed } from 'vue'
import { useRoute, type RouteLocationNormalized } from 'vue-router'

export interface NavLink {
  name: string
  path: string
}

const DEFAULT_NAV_LINKS: NavLink[] = [
  { name: 'Home', path: '/' },
  { name: 'Shop', path: '/shop' },
  { name: 'Collections', path: '/collections' },
  { name: 'About', path: '/about' },
]

export function useNavigation(links: NavLink[] = DEFAULT_NAV_LINKS) {
  const route = useRoute()

  const navLinks = computed(() => links)

  function isActive(path: string): boolean {
    return route.path === path
  }

  function isExactActive(path: string): boolean {
    return route.path === path
  }

  return {
    navLinks,
    isActive,
    isExactActive,
    route,
  }
}

export function useBreadcrumbs() {
  const route = useRoute()

  const breadcrumbs = computed(() => {
    const crumbs: { label: string; path: string }[] = []
    const matched = route.matched

    for (const record of matched) {
      if (record.meta?.breadcrumb) {
        crumbs.push({
          label: record.meta.breadcrumb as string,
          path: record.path,
        })
      }
    }

    return crumbs
  })

  return {
    breadcrumbs,
  }
}
