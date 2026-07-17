import type { PaginationMeta } from './result.types'
import type { ParsedApiError } from '../utils/api.utils'

export type ApiResult<T> =
  | { data: T; meta?: PaginationMeta; success: true; error?: never }
  | { data: null; success: false; error: ParsedApiError }
