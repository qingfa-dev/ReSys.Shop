export interface FormField {
  name: string
  label: string
  type: 'text' | 'email' | 'number' | 'password' | 'textarea' | 'select' | 'checkbox' | 'radio' | 'date'
  placeholder?: string
  required?: boolean
  disabled?: boolean
  options?: { label: string; value: unknown }[]
  validators?: ((value: unknown) => true | string)[]
}

export interface FormSection {
  title?: string
  description?: string
  fields: FormField[]
  columns?: 1 | 2 | 3
}

export interface FormConfig {
  sections: FormSection[]
  submitLabel?: string
  cancelLabel?: string
  cancelRoute?: string
}

export interface FormErrors {
  [field: string]: string | string[]
}
