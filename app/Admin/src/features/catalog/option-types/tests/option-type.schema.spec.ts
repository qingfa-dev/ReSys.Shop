import { describe, it, expect } from 'vitest';
import { createOptionTypeSchema } from '../schemas/option-type.schema';

const t = (key: string) => key;
const OptionTypeSchema = createOptionTypeSchema(t);

describe('OptionTypeSchema', () => {
  it('should validate a correct option type', () => {
    const validData = {
      name: 'valid-name',
      presentation: 'Valid Presentation',
      position: 10,
      filterable: true,
      description: 'Some description'
    };

    const result = OptionTypeSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should require name and presentation', () => {
    const invalidData = {
      position: 1
    };

    const result = OptionTypeSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      const errors = result.error.flatten().fieldErrors;
      expect(errors.name).toBeDefined();
      expect(errors.presentation).toBeDefined();
    }
  });

  it('should enforce max length on name', () => {
    const invalidData = {
      name: 'a'.repeat(101),
      presentation: 'Valid'
    };

    const result = OptionTypeSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.name).toBeDefined();
    }
  });

  it('should set default values for position and filterable', () => {
    const minimalData = {
      name: 'name',
      presentation: 'presentation'
    };

    const result = OptionTypeSchema.parse(minimalData);
    expect(result.position).toBe(0);
    expect(result.filterable).toBe(false);
  });
});
