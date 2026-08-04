import { onBeforeRouteLeave, onBeforeRouteUpdate, type NavigationGuard } from 'vue-router'

export function useBeforeRouteLeave(guard: NavigationGuard): void {
  onBeforeRouteLeave(guard)
}

export function useBeforeRouteUpdate(guard: NavigationGuard): void {
  onBeforeRouteUpdate(guard)
}
