import type { RouteRecordRaw } from 'vue-router'

const ProductsList = () => import('../views/ProductsList.vue')
const ProductDetail = () => import('../views/ProductDetail.vue')
const TaxonomiesList = () => import('../views/TaxonomiesList.vue')
const TaxonomyDetail = () => import('../views/TaxonomyDetail.vue')
const TaxonsList = () => import('../views/TaxonsList.vue')
const OptionTypesList = () => import('../views/OptionTypesList.vue')
const OptionTypeDetail = () => import('../views/OptionTypeDetail.vue')

export const catalogRoutes: RouteRecordRaw[] = [
  {
    path: 'catalog',
    redirect: { name: 'catalog-products' },
  },
  {
    path: 'catalog/products',
    name: 'catalog-products',
    component: ProductsList,
    meta: { title: 'Products' },
  },
  {
    path: 'catalog/products/:id',
    name: 'catalog-product-detail',
    component: ProductDetail,
    meta: { title: 'Product Detail' },
  },
  {
    path: 'catalog/taxonomies',
    name: 'catalog-taxonomies',
    component: TaxonomiesList,
    meta: { title: 'Taxonomies' },
  },
  {
    path: 'catalog/taxonomies/:id',
    name: 'catalog-taxonomy-detail',
    component: TaxonomyDetail,
    meta: { title: 'Taxonomy Detail' },
  },
  {
    path: 'catalog/taxons',
    name: 'catalog-taxons',
    component: TaxonsList,
    meta: { title: 'Taxons' },
  },
  {
    path: 'catalog/option-types',
    name: 'catalog-option-types',
    component: OptionTypesList,
    meta: { title: 'Option Types' },
  },
  {
    path: 'catalog/option-types/:id',
    name: 'catalog-option-type-detail',
    component: OptionTypeDetail,
    meta: { title: 'Option Type Detail' },
  },
]

export const catalogMenuItems = [
  {
    label: 'Catalog',
    icon: 'pi pi-fw pi-box',
    items: [
      { label: 'Products', icon: 'pi pi-fw pi-tag', to: '/catalog/products' },
      { label: 'Taxonomies', icon: 'pi pi-fw pi-sitemap', to: '/catalog/taxonomies' },
      { label: 'Option Types', icon: 'pi pi-fw pi-sliders-h', to: '/catalog/option-types' },
    ],
  },
]
