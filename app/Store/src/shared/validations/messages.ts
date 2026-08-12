// Messages: User-friendly Zod error strings shared across form schemas.
export const zodMessages = {
  required: (field: string): string => `${field} is required`,
  email: 'Enter a valid email address',
  minLength: (field: string, n: number): string => `${field} must be at least ${n} characters`,
  maxLength: (field: string, n: number): string => `${field} must be ${n} characters or fewer`,
  usernamePattern: 'Username can contain only letters, numbers, dots, underscores and dashes',
  passwordRules: 'Password must be at least 12 characters',
  passwordsMatch: 'Passwords do not match',
  acceptTerms: 'You must accept the terms and conditions',
  mustBePositive: 'Must be a positive number',
}
