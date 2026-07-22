import { describe, it, expect } from 'vitest'
import { catalogRoutes } from '@/app/routes/catalog.routes'
import type { RouteRecordRaw } from 'vue-router'

function collectRouteNames(routes: RouteRecordRaw[]): string[] {
  const names: string[] = []
  for (const r of routes) {
    if (r.name) names.push(r.name as string)
    if (r.children) names.push(...collectRouteNames(r.children))
  }
  return names
}

describe('catalog routes', () => {
  it('has all required route names', () => {
    const childRoutes = (catalogRoutes.children || []) as RouteRecordRaw[]
    const names = collectRouteNames(childRoutes)
    expect(names).toContain('catalog.dashboard')
    expect(names).toContain('catalog.products.list')
    expect(names).toContain('catalog.products.create')
    expect(names).toContain('catalog.products.view')
    expect(names).toContain('catalog.products.edit')
    expect(names).toContain('catalog.taxonomies.list')
    expect(names).toContain('catalog.taxonomies.create')
    expect(names).toContain('catalog.taxonomies.view')
    expect(names).toContain('catalog.taxonomies.edit')
    expect(names).toContain('catalog.option-types.list')
    expect(names).toContain('catalog.option-types.create')
    expect(names).toContain('catalog.option-types.view')
    expect(names).toContain('catalog.option-types.edit')
  })

  it('has no legacy route names', () => {
    const childRoutes = (catalogRoutes.children || []) as RouteRecordRaw[]
    const names = collectRouteNames(childRoutes)
    expect(names).not.toContain('catalog.taxa.list')
    expect(names).not.toContain('catalog.option-values.list')
  })
})
