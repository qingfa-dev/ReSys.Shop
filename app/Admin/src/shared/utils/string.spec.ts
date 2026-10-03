import { describe, it, expect } from 'vitest'
import { capitalize, truncate, slugify, toCamelCase } from './string'

describe('capitalize', () => {
  it('capitalizes the first letter', () => {
    expect(capitalize('hello')).toBe('Hello')
  })

  it('leaves the rest of the string unchanged', () => {
    expect(capitalize('helloWorld')).toBe('HelloWorld')
  })

  it('handles an empty string', () => {
    expect(capitalize('')).toBe('')
  })

  it('handles a single character', () => {
    expect(capitalize('a')).toBe('A')
  })

  it('does not lowercase other letters', () => {
    expect(capitalize('hELLO')).toBe('HELLO')
  })
})

describe('truncate', () => {
  it('returns the string as-is when shorter than length', () => {
    expect(truncate('hello', 10)).toBe('hello')
  })

  it('truncates at word boundary', () => {
    expect(truncate('hello world foo bar', 14)).toBe('hello world...')
  })

  it('truncates at exact length when no word boundary', () => {
    expect(truncate('helloworldfoo', 8)).toBe('hellowor...')
  })

  it('supports custom suffix', () => {
    expect(truncate('hello world', 5, ' [read more]')).toBe('hello [read more]')
  })
})

describe('slugify', () => {
  it('converts to lowercase slug', () => {
    expect(slugify('Hello World')).toBe('hello-world')
  })

  it('removes special characters', () => {
    expect(slugify('Hello! World?')).toBe('hello-world')
  })

  it('collapses multiple hyphens', () => {
    expect(slugify('hello   world')).toBe('hello-world')
  })

  it('trims leading and trailing hyphens', () => {
    expect(slugify('  hello world  ')).toBe('hello-world')
  })

  it('handles underscores', () => {
    expect(slugify('hello_world')).toBe('hello-world')
  })
})

describe('toCamelCase', () => {
  it('converts snake_case to camelCase', () => {
    expect(toCamelCase('first_name')).toBe('firstName')
  })

  it('handles multiple underscores', () => {
    expect(toCamelCase('first_middle_name')).toBe('firstMiddleName')
  })

  it('lowercases the first character', () => {
    expect(toCamelCase('FirstName')).toBe('firstName')
  })

  it('handles already camelCase', () => {
    expect(toCamelCase('firstName')).toBe('firstName')
  })
})
