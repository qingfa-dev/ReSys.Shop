import { describe, it, expect } from 'vitest';
import { OptionValueSchema } from '../schemas/OptionValue.Schema';

describe('OptionValueSchema', () => {
  it('should validate a correct option value', () => {
    const validData = {
      name: 'small',
      presentation: 'Small',
      position: 1
    };

    const result = OptionValueSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should require name and presentation', () => {
    const invalidData = {
      position: 1
    };

    const result = OptionValueSchema.safeParse(invalidData);
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

    const result = OptionValueSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.name).toBeDefined();
    }
  });

  it('should set default value for position', () => {
    const minimalData = {
      name: 'name',
      presentation: 'presentation'
    };

    const result = OptionValueSchema.parse(minimalData);
    expect(result.position).toBe(0);
  });
});
