import { describe, it, expect } from 'vitest'
import { parseAll } from '../querying'
import {
  toRoleQueryParams,
  ROLE_SORT_FIELDS,
} from '../../../../features/identity/types/role'
import {
  toUserQueryParams,
  USER_SORT_FIELDS,
} from '../../../../features/identity/types/user'
import {
  toStockLocationQueryParams,
  STOCK_LOCATION_SORT_FIELDS,
} from '../../../../features/inventory/types/stockLocation'
import {
  toStockTransferQueryParams,
  STOCK_TRANSFER_SORT_FIELDS,
} from '../../../../features/inventory/types/stockTransfer'
import {
  toOrderQueryParams,
  ORDER_SORT_FIELDS,
} from '../../../../features/ordering/types/order'
import {
  toPaymentMethodQueryParams,
  PAYMENT_METHOD_SORT_FIELDS,
} from '../../../../features/payment/types/paymentMethod'
import {
  toProfileQueryParams,
  CUSTOMER_SORT_FIELDS,
} from '../../../../features/profile/types/profile'
import {
  toAddressQueryParams,
  ADDRESS_SORT_FIELDS,
} from '../../../../features/profile/types/address'
import type { QueryingParameters } from '../querying'

describe('querying casing regression', () => {
  const cases: {
    name: string
    params: () => QueryingParameters
    sortFields: string[]
  }[] = [
    {
      name: 'role',
      params: () => toRoleQueryParams({ sortBy: 'name', sortDirection: 'asc' }),
      sortFields: ROLE_SORT_FIELDS,
    },
    {
      name: 'user',
      params: () => toUserQueryParams({ sortBy: 'userName', sortDirection: 'asc' }),
      sortFields: USER_SORT_FIELDS,
    },
    {
      name: 'stockLocation',
      params: () => toStockLocationQueryParams({ sortBy: 'name', sortDirection: 'asc' }),
      sortFields: STOCK_LOCATION_SORT_FIELDS,
    },
    {
      name: 'stockTransfer',
      params: () => toStockTransferQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' }),
      sortFields: STOCK_TRANSFER_SORT_FIELDS,
    },
    {
      name: 'order',
      params: () => toOrderQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' }),
      sortFields: ORDER_SORT_FIELDS,
    },
    {
      name: 'paymentMethod',
      params: () => toPaymentMethodQueryParams({ sortBy: 'name', sortDirection: 'asc' }),
      sortFields: PAYMENT_METHOD_SORT_FIELDS,
    },
    {
      name: 'profile',
      params: () => toProfileQueryParams({ sortBy: 'firstName', sortDirection: 'asc' }),
      sortFields: CUSTOMER_SORT_FIELDS,
    },
    {
      name: 'address',
      params: () => toAddressQueryParams({ userId: 'u-1', sortBy: 'firstName', sortDirection: 'asc' }),
      sortFields: ADDRESS_SORT_FIELDS,
    },
  ]

  it.each(cases)('parseAll accepts $name fetchActive sortBy against its SORT_FIELDS', ({ params, sortFields }) => {
    const result = parseAll(params(), null, sortFields)
    expect(result.isSuccess).toBe(true)
  })
})
