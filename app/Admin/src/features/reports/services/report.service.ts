import { reportsApi } from './reports.api'

export const reportService = {
  getSalesSummary: reportsApi.getSalesSummary,
  getInventorySummary: reportsApi.getInventorySummary,
  getCatalogSummary: reportsApi.getCatalogSummary,
  getRecentActivity: reportsApi.getRecentActivity,
}
