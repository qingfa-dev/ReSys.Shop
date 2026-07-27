export function throttle<T extends (...args: any[]) => any>(
  fn: T,
  limitMs: number,
): (...args: Parameters<T>) => void {
  let inThrottle = false

  return (...args: Parameters<T>): void => {
    if (!inThrottle) {
      fn(...args)
      inThrottle = true
      setTimeout(() => {
        inThrottle = false
      }, limitMs)
    }
  }
}
