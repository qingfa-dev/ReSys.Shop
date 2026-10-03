import type { App } from 'vue'
import { clickOutside } from './clickOutside'
import { autofocus } from './autofocus'

export function createDirectivesPlugin() {
  return {
    install(app: App) {
      app.directive('click-outside', clickOutside)
      app.directive('autofocus', autofocus)
    },
  }
}

export { clickOutside, autofocus }
