import { describe, it, expect } from 'vitest'
import { TaxonomySchema } from '../schemas/Taxonomy.Schema'

describe('TaxonomySchema', () => {
  it('should validate a correct taxonomy', () => {
    const validData = {
      name: 'categories',
      presentation: 'Categories',
      position: 1,
    }
    const result = TaxonomySchema.safeParse(validData)
    expect(result.success).toBe(true)
  })

  it('should require name', () => {
    const result = TaxonomySchema.safeParse({ presentation: 'P' })
    expect(result.success).toBe(false)
  })
})
