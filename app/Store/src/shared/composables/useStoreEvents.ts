type EventHandler = (event: Record<string, unknown>) => void

const handlers = new Map<string, Set<EventHandler>>()

export function emit(event: Record<string, unknown>): void {
  const type = event.type as string
  if (!type) return
  const typeHandlers = handlers.get(type)
  if (typeHandlers) {
    typeHandlers.forEach(h => h(event))
  }
}

export function on(type: string, handler: EventHandler): () => void {
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
  handlers.clear()
}
