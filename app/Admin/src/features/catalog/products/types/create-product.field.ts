import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.name.required')).max(100)
}

export function descriptionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(2000).optional().nullable()
}

export function slugSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.slug.required')).max(100)
}

export function metaTitleSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(100).optional().nullable()
}

export function metaDescriptionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(255).optional().nullable()
}

export function metaKeywordsSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(255).optional().nullable()
}

export function createProductSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    slug: slugSchema(t),
    description: descriptionSchema(t),
    metaTitle: metaTitleSchema(t),
    metaDescription: metaDescriptionSchema(t),
    metaKeywords: metaKeywordsSchema(t),
  })
}

export type CreateProductParameters = z.infer<ReturnType<typeof createProductSchema>>
