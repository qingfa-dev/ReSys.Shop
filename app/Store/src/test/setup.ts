// Polyfills: PrimeVue 5 Carousel observes content and scrolls programmatically;
// jsdom provides neither ResizeObserver/IntersectionObserver nor Element.scrollTo.
import { vi } from 'vitest'

class ResizeObserverStub {
  observe(): void {}
  unobserve(): void {}
  disconnect(): void {}
}

class IntersectionObserverStub {
  observe(): void {}
  unobserve(): void {}
  disconnect(): void {}
  takeRecords(): IntersectionObserverEntry[] {
    return []
  }
}

vi.stubGlobal('ResizeObserver', ResizeObserverStub)
vi.stubGlobal('IntersectionObserver', IntersectionObserverStub)

if (!Element.prototype.scrollTo) {
  Element.prototype.scrollTo = (): void => {}
}
