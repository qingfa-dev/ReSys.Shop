import type { Result, PagedResult } from '@/shared/models'

export type AsyncResult<T> = Promise<Result<T>>
export type AsyncPagedResult<T> = Promise<PagedResult<T>>

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

export interface SoftDeletable {
  deletedAt: string | null
}
