type EventHandler = (event: Record<string, unknown>) => void | Promise<void>

// Cache: Module-level event bus — singleton shared across all stores and composables
const handlers = new Map<string, Set<EventHandler>>()

// Emit: Dispatch to all handlers. Async handlers are awaited so callers can
// gate navigation on their completion (e.g. cart association before checkout).
export async function emit(event: Record<string, unknown>): Promise<void> {
  const type = event.type as string
  // Guard: Skip emission if event type is missing
  if (!type) return
  const typeHandlers = handlers.get(type)
  if (typeHandlers) {
    await Promise.all([...typeHandlers].map(h => h(event)))
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
