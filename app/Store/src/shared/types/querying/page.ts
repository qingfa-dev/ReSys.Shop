export interface PageBounds {
  defaultPage: number
  defaultPageSize: number
  maxPageSize: number
}

export const defaultPageBounds: PageBounds = {
  defaultPage: 1,
  defaultPageSize: 20,
  maxPageSize: 100,
}

export interface PageModel {
  page: number
  pageSize: number
  bounds: PageBounds
  rawInput: string | null
  isEmpty: boolean
}

export const emptyPageModel: PageModel = {
  page: 1,
  pageSize: 20,
  bounds: defaultPageBounds,
  rawInput: null,
  isEmpty: true,
}
