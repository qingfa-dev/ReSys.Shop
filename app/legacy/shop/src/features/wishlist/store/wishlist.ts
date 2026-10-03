import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { WishlistItem, Wishlist } from '../types'

export const useWishlistStore = defineStore('wishlist', () => {
    const items = ref<WishlistItem[]>([])
    const wishlists = ref<Wishlist[]>([])
    const isLoading = ref(false)

    const count = computed(() => items.value.length)
    const isEmpty = computed(() => items.value.length === 0)

    function isWishlisted(productId: string): boolean {
        return items.value.some(item => item.productId === productId)
    }

    function addToWishlist(product: any) {
        if (!isWishlisted(product.id)) {
            items.value.push({
                id: crypto.randomUUID(),
                userId: '',
                productId: product.id,
                dateAdded: new Date().toISOString(),
                productName: product.name,
                productImage: product.images?.[0]?.url || product.image || '',
                productPrice: product.price,
            })
        }
    }

    function removeFromWishlist(productId: string) {
        items.value = items.value.filter(item => item.productId !== productId)
    }

    function moveToCart(productId: string) {
        removeFromWishlist(productId)
    }

    return {
        items,
        wishlists,
        isLoading,
        count,
        isEmpty,
        isWishlisted,
        addToWishlist,
        removeFromWishlist,
        moveToCart,
    }
})
