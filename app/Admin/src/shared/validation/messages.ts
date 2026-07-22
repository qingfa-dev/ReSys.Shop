export const validationMessages: Record<string, string> = {
  required: '{field} is required.',
  email: 'Please enter a valid email address.',
  minLength: '{field} must be at least {min} characters.',
  maxLength: '{field} must not exceed {max} characters.',
  min: '{field} must be at least {min}.',
  max: '{field} must not exceed {max}.',
  url: 'Please enter a valid URL.',
  pattern: '{field} format is invalid.',
  integer: '{field} must be a whole number.',
  positive: '{field} must be a positive number.',
}

export function formatMessage(template: string, replacements: Record<string, string | number>): string {
  let result = template
  for (const [key, value] of Object.entries(replacements)) {
    result = result.replace(`{${key}}`, String(value))
  }
  return result
}
