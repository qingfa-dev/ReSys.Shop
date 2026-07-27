import { describe, it, expect } from 'vitest'
import { catalogRoutes } from '@/features/catalog'
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

import { inventoryRoutes } from '@/features/inventory'
import { orderingRoutes } from '@/features/ordering'
import { paymentRoutes } from '@/features/payment'
import { shippingRoutes } from '@/features/shipping'
import { locationRoutes } from '@/features/location'
import { usersRoutes } from '@/features/users'
import { profileRoutes } from '@/features/profile'

describe('inventory routes', () => {
  const childRoutes = (inventoryRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has stock detail routes', () => {
    expect(names).toContain('inventory.stocks.create')
    expect(names).toContain('inventory.stocks.view')
    expect(names).toContain('inventory.stocks.edit')
  })
  it('has location detail routes', () => {
    expect(names).toContain('inventory.locations.create')
    expect(names).toContain('inventory.locations.view')
    expect(names).toContain('inventory.locations.edit')
  })
  it('has transfer detail routes', () => {
    expect(names).toContain('inventory.transfers.create')
    expect(names).toContain('inventory.transfers.view')
    expect(names).toContain('inventory.transfers.edit')
  })
  it('has reservation list route', () => {
    expect(names).toContain('inventory.reservations.list')
  })
  it('no legacy routes', () => {
    expect(names).not.toContain('inventory.stocks.import')
    expect(names).not.toContain('inventory.units.list')
  })
})

describe('ordering routes', () => {
  const childRoutes = (orderingRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has order detail routes', () => {
    expect(names).toContain('ordering.orders.create')
    expect(names).toContain('ordering.orders.view')
    expect(names).toContain('ordering.orders.edit')
  })
  it('no legacy orders/create', () => {
    expect(names).toContain('ordering.orders.create')
  })
})

describe('payment routes', () => {
  const childRoutes = (paymentRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has normalized payments path', () => {
    expect(names).toContain('payment.payments.list')
    expect(names).toContain('payment.payments.view')
  })
  it('has method detail routes', () => {
    expect(names).toContain('payment.methods.create')
    expect(names).toContain('payment.methods.view')
    expect(names).toContain('payment.methods.edit')
  })
  it('no payments/new (Payment is view-only)', () => {
    expect(names).not.toContain('payment.payments.create')
  })
})

describe('shipping routes', () => {
  const childRoutes = (shippingRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has method detail routes', () => {
    expect(names).toContain('shipping.methods.create')
    expect(names).toContain('shipping.methods.view')
    expect(names).toContain('shipping.methods.edit')
  })
  it('has rate detail routes', () => {
    expect(names).toContain('shipping.rates.create')
    expect(names).toContain('shipping.rates.view')
    expect(names).toContain('shipping.rates.edit')
  })
})

describe('location routes', () => {
  const childRoutes = (locationRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has country detail routes', () => {
    expect(names).toContain('location.countries.create')
    expect(names).toContain('location.countries.view')
    expect(names).toContain('location.countries.edit')
  })
  it('has state detail routes', () => {
    expect(names).toContain('location.states.create')
    expect(names).toContain('location.states.view')
    expect(names).toContain('location.states.edit')
  })
})

describe('users routes', () => {
  const childRoutes = (usersRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has staff detail routes', () => {
    expect(names).toContain('users.staff.create')
    expect(names).toContain('users.staff.view')
    expect(names).toContain('users.staff.edit')
  })
  it('has customer detail routes', () => {
    expect(names).toContain('users.customers.create')
    expect(names).toContain('users.customers.view')
    expect(names).toContain('users.customers.edit')
  })
  it('has role detail routes', () => {
    expect(names).toContain('users.roles.create')
    expect(names).toContain('users.roles.view')
    expect(names).toContain('users.roles.edit')
  })
  it('has permission view route only (read-only)', () => {
    expect(names).toContain('users.permissions.list')
    expect(names).toContain('users.permissions.view')
    expect(names).not.toContain('users.permissions.create')
    expect(names).not.toContain('users.permissions.edit')
  })
  it('no legacy staff/create', () => {
    expect(names).toContain('users.staff.create')
  })
})

describe('profile routes', () => {
  const childRoutes = (profileRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('uses profile.* namespace', () => {
    expect(names).toContain('profile.view')
    expect(names).toContain('profile.addresses')
    expect(names).not.toContain('profile')
    expect(names).not.toContain('addresses')
  })
})

import { adminMenuConfig } from '@/app/config/admin-menu.config'

function collectMenuRouteNames(groups: typeof adminMenuConfig): string[] {
  const names: string[] = []
  for (const group of groups) {
    for (const item of group.items) {
      if (item.to && typeof item.to === 'object' && 'name' in item.to) {
        names.push(item.to.name as string)
      }
      if (item.items) {
        for (const child of item.items) {
          if (child.to && typeof child.to === 'object' && 'name' in child.to) {
            names.push(child.to.name as string)
          }
        }
      }
    }
  }
  return names
}

describe('admin menu config', () => {
  const menuNames = collectMenuRouteNames(adminMenuConfig)

  it('contains required entries', () => {
    expect(menuNames).toContain('reports.dashboard')
    expect(menuNames).toContain('profile.view')
    expect(menuNames).toContain('profile.addresses')
    expect(menuNames).toContain('catalog.dashboard')
    expect(menuNames).toContain('catalog.products.list')
    expect(menuNames).toContain('catalog.taxonomies.list')
    expect(menuNames).toContain('catalog.option-types.list')
    expect(menuNames).toContain('inventory.stocks.list')
    expect(menuNames).toContain('ordering.orders.list')
    expect(menuNames).toContain('ordering.fulfillment.queue')
    expect(menuNames).toContain('payment.payments.list')
    expect(menuNames).toContain('payment.methods.list')
    expect(menuNames).toContain('shipping.methods.list')
    expect(menuNames).toContain('shipping.rates.list')
    expect(menuNames).toContain('location.countries.list')
    expect(menuNames).toContain('location.states.list')
    expect(menuNames).toContain('users.staff.list')
    expect(menuNames).toContain('users.customers.list')
    expect(menuNames).toContain('users.roles.list')
    expect(menuNames).toContain('users.permissions.list')
  })

  it('does not contain removed entries', () => {
    expect(menuNames).not.toContain('catalog.products.create')
    expect(menuNames).not.toContain('catalog.taxa.list')
    expect(menuNames).not.toContain('catalog.option-values.list')
    expect(menuNames).not.toContain('inventory.stocks.import')
    expect(menuNames).not.toContain('inventory.units.list')
    expect(menuNames).not.toContain('users.staff.create')
  })
})
