export interface PageBounds {
  defaultPage: number
  defaultPageSize: number
  maxPageSize: number
}

export const defaultPageBounds: PageBounds = {
  defaultPage: 1,
  defaultPageSize: 10,
  maxPageSize: 100,
}

export interface PageModel {
  page: number
  pageSize: number
  bounds: PageBounds
}

export function normalizePage(page: number | undefined, bounds: PageBounds): number {
  const p = page ?? bounds.defaultPage
  return Math.max(1, Math.min(p, Number.MAX_SAFE_INTEGER))
}

export function normalizePageSize(pageSize: number | undefined, bounds: PageBounds): number {
  const ps = pageSize ?? bounds.defaultPageSize
  return Math.max(1, Math.min(ps, bounds.maxPageSize))
}

export function skip(page: number, pageSize: number): number {
  return (page - 1) * pageSize
}

export function totalPages(totalCount: number, pageSize: number): number {
  if (pageSize <= 0) return 0
  return Math.ceil(totalCount / pageSize)
}

export function createPageModel(
  page?: number,
  pageSize?: number,
  bounds?: PageBounds,
): PageModel {
  const b = bounds ?? defaultPageBounds
  return {
    page: normalizePage(page, b),
    pageSize: normalizePageSize(pageSize, b),
    bounds: b,
  }
}

export const emptyPageModel: PageModel = Object.freeze({
  page: defaultPageBounds.defaultPage,
  pageSize: defaultPageBounds.defaultPageSize,
  bounds: defaultPageBounds,
})
