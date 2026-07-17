import type { ProductSummary, ProductDetail } from '../products/types/Product.Response.Type'
import type { VariantSummary, VariantDetail } from '../products/types/Variant.Response.Type'
import type { OptionTypeDetail, OptionTypeListItem } from '../option-types/types/OptionType.Response.Type'
import type { OptionValueListItem } from '../option-types/option-values/types/OptionValue.Response.Type'
import type { PropertyTypeDetail } from '../property-types/types/PropertyType.Response.Type'
import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/Taxonomy.Response.Type'
import type { TaxonDetail, TaxonListItem, TaxonTreeItem } from '../taxonomies/taxa/types/Taxon.Response.Type'

export function mapProductSummary(data: ProductSummary): ProductSummary {
  return data
}

export function mapProductDetail(data: ProductDetail): ProductDetail {
  return data
}

export function mapVariantSummary(data: VariantSummary): VariantSummary {
  return data
}

export function mapVariantDetail(data: VariantDetail): VariantDetail {
  return data
}

export function mapOptionType(data: OptionTypeDetail | OptionTypeListItem): OptionTypeDetail | OptionTypeListItem {
  return data
}

export function mapOptionValue(data: OptionValueListItem): OptionValueListItem {
  return data
}

export function mapPropertyType(data: PropertyTypeDetail): PropertyTypeDetail {
  return data
}

export function mapTaxonomy(data: TaxonomyDetail | TaxonomyListItem): TaxonomyDetail | TaxonomyListItem {
  return data
}

export function mapTaxon(data: TaxonDetail | TaxonListItem | TaxonTreeItem): TaxonDetail | TaxonListItem | TaxonTreeItem {
  return data
}
