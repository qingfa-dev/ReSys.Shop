import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCartStore } from '../stores/cart'

describe('cart store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('starts empty', () => {
    const cart = useCartStore()
    expect(cart.items).toHaveLength(0)
    expect(cart.itemCount).toBe(0)
    expect(cart.subtotal).toBe(0)
  })

  it('adds an item', () => {
    const cart = useCartStore()
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 1 })
    expect(cart.items).toHaveLength(1)
    expect(cart.itemCount).toBe(1)
    expect(cart.subtotal).toBe(10)
  })

  it('increments quantity when adding duplicate item', () => {
    const cart = useCartStore()
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 1 })
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 2 })
    expect(cart.items).toHaveLength(1)
    expect(cart.itemCount).toBe(3)
    expect(cart.subtotal).toBe(30)
  })

  it('removes an item', () => {
    const cart = useCartStore()
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 1 })
    cart.addItem({ id: '2', name: 'Another Product', price: 20, image: '', quantity: 1 })
    cart.removeItem('1')
    expect(cart.items).toHaveLength(1)
    expect(cart.itemCount).toBe(1)
  })

  it('updates item quantity', () => {
    const cart = useCartStore()
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 1 })
    cart.updateQuantity('1', 5)
    expect(cart.itemCount).toBe(5)
    expect(cart.subtotal).toBe(50)
  })

  it('removes item when quantity reaches zero', () => {
    const cart = useCartStore()
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 1 })
    cart.updateQuantity('1', 0)
    expect(cart.items).toHaveLength(0)
  })

  it('clears the cart', () => {
    const cart = useCartStore()
    cart.addItem({ id: '1', name: 'Test Product', price: 10, image: '', quantity: 1 })
    cart.addItem({ id: '2', name: 'Another Product', price: 20, image: '', quantity: 2 })
    cart.clearCart()
    expect(cart.items).toHaveLength(0)
    expect(cart.itemCount).toBe(0)
    expect(cart.subtotal).toBe(0)
  })

  it('toggles isOpen', () => {
    const cart = useCartStore()
    expect(cart.isOpen).toBe(false)
    cart.toggleCart()
    expect(cart.isOpen).toBe(true)
    cart.toggleCart()
    expect(cart.isOpen).toBe(false)
  })
})
