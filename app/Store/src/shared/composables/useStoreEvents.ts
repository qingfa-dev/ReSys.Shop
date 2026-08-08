type EventHandler = (event: Record<string, unknown>) => void

// Cache: Module-level event bus — singleton shared across all stores and composables
const handlers = new Map<string, Set<EventHandler>>()

export function emit(event: Record<string, unknown>): void {
  const type = event.type as string
  // Guard: Skip emission if event type is missing
  if (!type) return
  const typeHandlers = handlers.get(type)
  if (typeHandlers) {
    typeHandlers.forEach(h => h(event))
  }
}

export function on(type: string, handler: EventHandler): () => void {
  // Subscribe: Register handler for event type — returns unsubscribe function
  if (!handlers.has(type)) {
    handlers.set(type, new Set())
  }
  handlers.get(type)!.add(handler)
  return () => {
    handlers.get(type)?.delete(handler)
  }
}

export function off(type: string, handler: EventHandler): void {
  handlers.get(type)?.delete(handler)
}

export function reset(): void {
  // Reset: Clear all handlers — used in test teardown to prevent state leakage
  handlers.clear()
}
