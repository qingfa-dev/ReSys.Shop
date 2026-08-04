import type { RouteRecordRaw } from 'vue-router'
import { productRoutes } from './products/routes'
import { taxonomyRoutes } from './taxonomies/routes'
import { taxonRoutes } from './taxa/routes'
import { optionTypeRoutes } from './option-types/routes'
import { optionValueRoutes } from './option-values/routes'
import { classificationRoutes } from './classifications/routes'
import { variantRoutes } from './variants/routes'
import { variantImageRoutes } from './variant-images/routes'
import { variantPriceRoutes } from './variant-prices/routes'
import { productOptionTypeRoutes } from './product-option-types/routes'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  meta: { breadcrumb: 'navigation.catalog' },
  children: [
    {
      path: '',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/dashboard/pages/CatalogDashboardPage.vue'),
    },
    ...productRoutes,
    ...taxonomyRoutes,
    ...taxonRoutes,
    ...optionTypeRoutes,
    ...optionValueRoutes,
    ...classificationRoutes,
    ...variantRoutes,
    ...variantImageRoutes,
    ...variantPriceRoutes,
    ...productOptionTypeRoutes,
  ],
}
