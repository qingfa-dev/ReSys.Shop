import { describe, it, expect } from 'vitest'
import { truncate, titleCase } from '../strings'

describe('strings', () => {
  it('truncate appends ellipsis', () => {
    expect(truncate('hello world', 5)).toBe('hell…')
  })
  it('titleCase capitalizes words', () => {
    expect(titleCase('hello world')).toBe('Hello World')
  })
})
