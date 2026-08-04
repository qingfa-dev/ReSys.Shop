export type Nullable<T> = T | null

export type Optional<T> = T | undefined

export type Dictionary<T> = Record<string, T>

export interface Identifiable {
  id: string
}

export interface Timestamped {
  createdAt: string
  updatedAt: string
}
