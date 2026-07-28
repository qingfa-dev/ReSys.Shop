import type { ProductEntity } from '../types/entity'
import type { ProductResponse } from '../types/response'
import type { ProductSchemaType } from '../types/schemas'

export function mapResponseToEntity(response: ProductResponse): ProductEntity {
  return {
    id: response.id,
    name: response.name,
    slug: response.slug,
    description: response.description,
    price: response.price,
    compareAtPrice: response.compareAtPrice,
    images: response.images,
    category: {
      id: response.category.id,
      name: response.category.name,
      slug: response.category.slug,
      parentId: response.category.parentId,
      image: response.category.image,
    },
    tags: response.tags,
    variants: response.variants?.map((v) => ({
      id: v.id,
      productId: v.productId,
      name: v.name,
      sku: v.sku,
      price: v.price,
      options: v.options.map((o) => ({ name: o.name, value: o.value })),
      inventory: {
        quantity: v.inventory.quantity,
        trackQuantity: v.inventory.trackQuantity,
        allowBackorder: v.inventory.allowBackorder,
        lowStockThreshold: v.inventory.lowStockThreshold,
      },
    })),
    inventory: {
      quantity: response.inventory.quantity,
      trackQuantity: response.inventory.trackQuantity,
      allowBackorder: response.inventory.allowBackorder,
      lowStockThreshold: response.inventory.lowStockThreshold,
    },
    createdAt: response.createdAt,
    updatedAt: response.updatedAt,
  }
}

export function mapSchemaToEntity(schema: ProductSchemaType): ProductEntity {
  return {
    id: schema.id,
    name: schema.name,
    slug: schema.slug,
    description: schema.description,
    price: schema.price,
    compareAtPrice: schema.compareAtPrice,
    images: schema.images,
    category: schema.category,
    tags: schema.tags,
    variants: schema.variants,
    inventory: schema.inventory,
    createdAt: schema.createdAt,
    updatedAt: schema.updatedAt,
  }
}

export function mapEntityToResponse(entity: ProductEntity): ProductResponse {
  return {
    id: entity.id,
    name: entity.name,
    slug: entity.slug,
    description: entity.description,
    price: entity.price,
    compareAtPrice: entity.compareAtPrice,
    images: entity.images,
    category: entity.category,
    tags: entity.tags,
    variants: entity.variants,
    inventory: entity.inventory,
    createdAt: entity.createdAt,
    updatedAt: entity.updatedAt,
  }
}