import { describe, it, expect } from 'vitest';
import { PropertyTypeSchema } from '../schemas/PropertyType.Schema';

describe('PropertyTypeSchema', () => {
  it('should validate a correct property type', () => {
    const validData = {
      name: 'material',
      presentation: 'Material',
      kind: 'String' as const,
      position: 1,
      filterable: true
    };

    const result = PropertyTypeSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should require name and presentation', () => {
    const result = PropertyTypeSchema.safeParse({});
    expect(result.success).toBe(false);
  });

  it('should default to String kind', () => {
    const minimalData = { name: 'n', presentation: 'p' };
    const result = PropertyTypeSchema.parse(minimalData);
    expect(result.kind).toBe('String');
  });
});
