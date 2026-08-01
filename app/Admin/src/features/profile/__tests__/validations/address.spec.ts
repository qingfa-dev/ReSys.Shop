import { describe, it, expect } from 'vitest'
import {
  addressType,
  addressFirstName,
  addressAddress1,
  addressCity,
  addressCountryName,
  addressLastName,
  addressAddress2,
  addressZipCode,
  addressPhone,
  addressLabel,
  addressSchema,
} from '../../validations/address'

describe('addressType', () => {
  it('accepts Shipping', () => {
    expect(addressType.safeParse('Shipping').success).toBe(true)
  })

  it('rejects an invalid value', () => {
    expect(addressType.safeParse('Bogus').success).toBe(false)
  })
})

describe('addressFirstName', () => {
  it('accepts a valid first name', () => {
    expect(addressFirstName.safeParse('A').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(addressFirstName.safeParse('').success).toBe(false)
  })
})

describe('addressAddress1', () => {
  it('accepts a valid address line', () => {
    expect(addressAddress1.safeParse('1 Main St').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(addressAddress1.safeParse('').success).toBe(false)
  })
})

describe('addressCity', () => {
  it('accepts a valid city', () => {
    expect(addressCity.safeParse('Hanoi').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(addressCity.safeParse('').success).toBe(false)
  })
})

describe('addressCountryName', () => {
  it('accepts a valid country', () => {
    expect(addressCountryName.safeParse('Vietnam').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(addressCountryName.safeParse('').success).toBe(false)
  })
})

describe('addressLastName', () => {
  it('accepts a value', () => {
    expect(addressLastName.safeParse('Smith').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(addressLastName.safeParse(undefined).success).toBe(true)
  })
})

describe('addressAddress2', () => {
  it('accepts a value', () => {
    expect(addressAddress2.safeParse('Apt 2').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(addressAddress2.safeParse(undefined).success).toBe(true)
  })
})

describe('addressZipCode', () => {
  it('accepts a value', () => {
    expect(addressZipCode.safeParse('10000').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(addressZipCode.safeParse(undefined).success).toBe(true)
  })
})

describe('addressPhone', () => {
  it('accepts a value', () => {
    expect(addressPhone.safeParse('123').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(addressPhone.safeParse(undefined).success).toBe(true)
  })
})

describe('addressLabel', () => {
  it('accepts a value', () => {
    expect(addressLabel.safeParse('Home').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(addressLabel.safeParse(undefined).success).toBe(true)
  })
})

describe('addressSchema', () => {
  it('accepts a valid address form', () => {
    const result = addressSchema.safeParse({
      userId: 'u-1',
      addressType: 'Shipping',
      firstName: 'A',
      address1: '1 Main St',
      city: 'Hanoi',
      isDefault: true,
      countryName: 'Vietnam',
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing address1', () => {
    const result = addressSchema.safeParse({
      userId: 'u-1',
      addressType: 'Shipping',
      firstName: 'A',
      city: 'Hanoi',
      isDefault: true,
      countryName: 'Vietnam',
    })
    expect(result.success).toBe(false)
  })

  it('rejects an invalid addressType', () => {
    const result = addressSchema.safeParse({
      userId: 'u-1',
      addressType: 'Bogus',
      firstName: 'A',
      address1: '1 Main St',
      city: 'Hanoi',
      isDefault: true,
      countryName: 'Vietnam',
    })
    expect(result.success).toBe(false)
  })
})
