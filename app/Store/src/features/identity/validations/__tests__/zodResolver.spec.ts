import { describe, it, expect } from 'vitest'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { LoginFormSchema, RegisterFormSchema } from '../auth'

/**
 * Regression guard for Risk R2: `@primevue/forms`' `zodResolver` must accept
 * Zod 4 schemas. The resolver is exercised standalone (no component render) by
 * calling the resolver function it returns with a `{ values }` payload — the
 * same shape @primevue/forms passes when validating the whole form on submit.
 */
describe('zodResolver with Zod 4 schemas (@primevue/forms/resolvers/zod)', () => {
  it('returns a resolver function from zodResolver', () => {
    expect(typeof zodResolver(LoginFormSchema)).toBe('function')
  })

  it('resolves a valid login payload with no errors', async () => {
    const result = await zodResolver(LoginFormSchema)({
      values: { credential: 'user@example.com', password: 'secret123' },
    })

    expect(result.errors).toEqual({})
    expect(result.values).toEqual({ credential: 'user@example.com', password: 'secret123' })
  })

  it('reports an empty credential as error', async () => {
    const result = await zodResolver(LoginFormSchema)({
      values: { credential: '', password: 'secret123' },
    })

    expect(result.errors.credential).toBeDefined()
  })

  it('reports a missing password on the password field', async () => {
    const result = await zodResolver(LoginFormSchema)({
      values: { credential: 'user@example.com', password: '' },
    })

    expect(result.errors.password).toBeDefined()
  })

  it('surfaces a cross-field refine error on confirmPassword', async () => {
    const result = await zodResolver(RegisterFormSchema)({
      values: {
        firstName: 'Jane',
        lastName: 'Doe',
        email: 'jane@example.com',
        password: 'password1234',
        confirmPassword: 'different',
      },
    })

    const fieldErrors = result.errors.confirmPassword as Array<{ message: string }> | undefined
    expect(fieldErrors).toBeDefined()
    expect(fieldErrors?.[0]?.message).toBe('Passwords do not match')
  })

  it('passes a valid register payload with no errors', async () => {
    const result = await zodResolver(RegisterFormSchema)({
      values: {
        firstName: 'Jane',
        lastName: 'Doe',
        email: 'jane@example.com',
        password: 'password1234',
        confirmPassword: 'password1234',
      },
    })

    expect(result.errors).toEqual({})
  })
})
