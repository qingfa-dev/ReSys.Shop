import type { ProductListItem } from '../types/product'

export type ProductStatusAction =
  | { kind: 'activate' }
  | { kind: 'discontinue' }
  | { kind: 'none' }

export function statusAction(status: ProductListItem['status']): ProductStatusAction {
  if (status === 'Active') return { kind: 'discontinue' }
  if (status === 'Draft' || status === 'Archived') return { kind: 'activate' }
  return { kind: 'none' }
}
