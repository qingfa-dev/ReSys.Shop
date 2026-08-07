export type StoreEvent =
  | { type: 'auth:login'; userId: string }
  | { type: 'auth:logout' }
  | { type: 'auth:init-done'; userId: string }
  | { type: 'filter:changed' }
  | { type: 'checkout:placed'; orderId: string }
  | { type: 'cart:updated'; itemCount: number }
  | { type: 'profile:deleted' }

type EventHandler<T extends StoreEvent> = (event: T) => void

const listeners = new Map<string, Set<EventHandler<any>>>()

function getListeners<T extends StoreEvent>(type: string): Set<EventHandler<T>> {
  if (!listeners.has(type)) {
    listeners.set(type, new Set())
  }
  return listeners.get(type) as Set<EventHandler<T>>
}

export function emit<T extends StoreEvent>(event: T): void {
  for (const handler of getListeners<T>(event.type)) {
    handler(event)
  }
}

export function on<T extends StoreEvent>(
  type: T['type'],
  handler: EventHandler<T>
): void {
  getListeners<T>(type).add(handler)
}

export function off<T extends StoreEvent>(
  type: T['type'],
  handler: EventHandler<T>
): void {
  getListeners<T>(type).delete(handler)
}
