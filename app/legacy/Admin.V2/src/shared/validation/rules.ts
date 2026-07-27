import { REGEX } from '@/shared/constants'

export type ValidationRule = (value: unknown) => true | string

export const rules = {
  required: (label = 'This field') =>
    (value: unknown): true | string =>
      (value !== null && value !== undefined && value !== '') ? true : `${label} is required`,

  minLength: (min: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'string' && value.length >= min ? true : `${label} must be at least ${min} characters`,

  maxLength: (max: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'string' && value.length <= max ? true : `${label} must not exceed ${max} characters`,

  email: (label = 'Email') =>
    (value: unknown): true | string =>
      typeof value === 'string' && REGEX.EMAIL.test(value) ? true : `${label} is not valid`,

  min: (min: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'number' && value >= min ? true : `${label} must be at least ${min}`,

  max: (max: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'number' && value <= max ? true : `${label} must not exceed ${max}`,
}
