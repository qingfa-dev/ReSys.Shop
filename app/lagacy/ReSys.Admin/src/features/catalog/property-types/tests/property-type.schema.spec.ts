import { describe, it, expect } from 'vitest';
import { PropertyTypeSchema } from '../schemas/property-type.schema';
import { PropertyKind } from '../types/property-kind';

describe('PropertyTypeSchema', () => {
  it('should validate a correct property type', () => {
    const validData = {
      name: 'material',
      presentation: 'Material',
      kind: PropertyKind.String,
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
    expect(result.kind).toBe(PropertyKind.String);
  });
});
