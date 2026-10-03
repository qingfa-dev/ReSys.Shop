import { describe, it, expect } from 'vitest'
import type { DashboardSummary } from '../../types/dashboard'

function buildDashboardSummary(): DashboardSummary {
  return {
    sales: {
      totalRevenue: 0,
      orderCount: 0,
      averageOrderValue: 0,
      revenueTrendPercentage: 0,
      trendHistory: [],
    },
    inventory: {
      totalVariants: 0,
      outOfStockCount: 0,
      lowStockCount: 0,
      stockAccuracyPercentage: 0,
    },
    catalog: {
      totalProducts: 0,
      activeProducts: 0,
      totalVariants: 0,
      totalTaxonomies: 0,
      totalTaxons: 0,
      recentlyAdded: [],
    },
    recentActivities: [],
  }
}

describe('DashboardSummary structure', () => {
  it('has well-formed sales fields', () => {
    const dashboard = buildDashboardSummary()
    expect(dashboard.sales.totalRevenue).toBe(0)
    expect(dashboard.sales.orderCount).toBe(0)
    expect(dashboard.sales.averageOrderValue).toBe(0)
    expect(dashboard.sales.revenueTrendPercentage).toBe(0)
    expect(Array.isArray(dashboard.sales.trendHistory)).toBe(true)
  })

  it('has well-formed inventory fields', () => {
    const dashboard = buildDashboardSummary()
    expect(dashboard.inventory.totalVariants).toBe(0)
    expect(dashboard.inventory.outOfStockCount).toBe(0)
    expect(dashboard.inventory.lowStockCount).toBe(0)
    expect(dashboard.inventory.stockAccuracyPercentage).toBe(0)
  })

  it('has well-formed catalog fields', () => {
    const dashboard = buildDashboardSummary()
    expect(dashboard.catalog.totalProducts).toBe(0)
    expect(dashboard.catalog.activeProducts).toBe(0)
    expect(dashboard.catalog.totalVariants).toBe(0)
    expect(dashboard.catalog.totalTaxonomies).toBe(0)
    expect(dashboard.catalog.totalTaxons).toBe(0)
    expect(Array.isArray(dashboard.catalog.recentlyAdded)).toBe(true)
  })

  it('has well-formed recentActivities field', () => {
    const dashboard = buildDashboardSummary()
    expect(Array.isArray(dashboard.recentActivities)).toBe(true)
  })
})
