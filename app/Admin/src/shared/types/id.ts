declare const brand: unique symbol
export type Brand<T, B> = T & { readonly [brand]: B }

export type UserId = Brand<string, 'UserId'>
export type RoleId = Brand<string, 'RoleId'>
export type ProductId = Brand<string, 'ProductId'>
export type VariantId = Brand<string, 'VariantId'>
export type CountryId = Brand<string, 'CountryId'>
export type StateId = Brand<string, 'StateId'>

export const asId = <T extends string>(s: string): T => s as T
