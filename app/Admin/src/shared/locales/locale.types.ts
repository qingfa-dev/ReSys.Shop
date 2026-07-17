export interface FeatureLocales {
  titles: Record<string, string>
  descriptions?: Record<string, string>
  labels?: Record<string, string>
  placeholders?: Record<string, string>
  tooltips?: Record<string, string>
  messages: Record<string, string>
  actions: Record<string, string>
  table?: Record<string, string>
  filters?: Record<string, string>
  confirm?: Record<string, string | ((...args: string[]) => string)>
  common?: Record<string, string>
  [key: string]: unknown
}

export interface GeneralLocales {
  common: {
    confirm: string
    cancel: string
    save: string
    delete: string
    edit: string
    back: string
    next: string
    prev: string
    yes: string
    no: string
    loading: string
    success: string
    error: string
    warning: string
    info: string
  }
  navigation: {
    dashboard: string
    home: string
    about: string
    testing: string
    catalog: string
    profile: string
    settings: string
    logout: string
  }
  layout: {
    search: string
    noResults: string
  }
}
