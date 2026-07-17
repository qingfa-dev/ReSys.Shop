import type { StockAdjustmentParameters } from '../schemas/StockItem.Schema'
export type StockAdjustmentRequest = StockAdjustmentParameters
export interface StockAuditRequest { physicalCount: number; reason?: string; reference?: string }
