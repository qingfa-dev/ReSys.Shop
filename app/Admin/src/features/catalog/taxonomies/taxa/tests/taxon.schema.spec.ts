import { describe, it, expect } from 'vitest';
import { TaxonSchema } from '../../schemas/Taxon.Schema';

describe('TaxonSchema', () => {
  it('should validate a correct taxon', () => {
    const validData = {
      taxonomyId: '00000000-0000-0000-0000-000000000000',
      name: 'electronics',
      presentation: 'Electronics',
      slug: 'electronics',
      position: 0,
      hideFromNav: false,
      automatic: false
    };
    const result = TaxonSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should require taxonomyId', () => {
    const result = TaxonSchema.safeParse({ name: 'N', presentation: 'P', slug: 's' });
    expect(result.success).toBe(false);
  });

  it('should require slug', () => {
    const result = TaxonSchema.safeParse({ taxonomyId: '00000000-0000-0000-0000-000000000000', name: 'N', presentation: 'P' });
    expect(result.success).toBe(false);
  });
});
