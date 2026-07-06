import { describe, it, expect } from 'vitest'
import { slugify, humanize } from '../slug'

describe('slug', () => {
  it('slugify lowercases and dashes', () => {
    expect(slugify('Hello World!')).toBe('hello-world')
  })
  it('humanize capitalizes and spaces', () => {
    expect(humanize('hello-world')).toBe('Hello World')
  })
})
