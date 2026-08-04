<template>
  <!-- Section: Footer -->
  <footer class="bg-white border-t border-stone-200 mt-auto">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <!-- Section: Link Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        <!-- Section: Brand Column -->
        <div class="lg:col-span-1">
          <router-link to="/" class="text-xl font-bold text-stone-900" style="font-family: 'Playfair Display', serif">
            ReSys.Shop
          </router-link>
          <p class="mt-3 text-sm text-stone-500">Discover your style with visual search. Upload an image, find your look.</p>
          <div class="flex items-center gap-4 mt-4">
            <a href="#" class="text-stone-400 hover:text-teal-600 transition-colors" aria-label="Instagram">
              <i class="pi pi-instagram text-lg" />
            </a>
            <a href="#" class="text-stone-400 hover:text-teal-600 transition-colors" aria-label="Twitter">
              <i class="pi pi-twitter text-lg" />
            </a>
            <a href="#" class="text-stone-400 hover:text-teal-600 transition-colors" aria-label="Facebook">
              <i class="pi pi-facebook text-lg" />
            </a>
          </div>
        </div>

        <!-- Section: Shop Links -->
        <div>
          <h3 class="text-sm font-semibold text-stone-900 uppercase tracking-wider mb-4">Shop</h3>
          <ul class="space-y-2">
            <li><router-link to="/shop" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">All Products</router-link></li>
            <li><router-link to="/shop?sort=newest" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">New Arrivals</router-link></li>
            <li><router-link to="/collections" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Collections</router-link></li>
            <li><a href="/shop?sort=price-asc" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Sale</a></li>
          </ul>
        </div>

        <!-- Section: Support Links -->
        <div>
          <h3 class="text-sm font-semibold text-stone-900 uppercase tracking-wider mb-4">Support</h3>
          <ul class="space-y-2">
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Help Center</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Shipping Info</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Returns</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Size Guide</a></li>
          </ul>
        </div>

        <!-- Section: Company Links -->
        <div>
          <h3 class="text-sm font-semibold text-stone-900 uppercase tracking-wider mb-4">Company</h3>
          <ul class="space-y-2">
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">About Us</a></li>
            <li><a href="#" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Careers</a></li>
            <li><router-link to="/privacy" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Privacy</router-link></li>
            <li><router-link to="/terms" class="text-sm text-stone-500 hover:text-teal-600 transition-colors">Terms</router-link></li>
          </ul>
        </div>
      </div>

      <!-- Section: Bottom Bar -->
      <div class="mt-12 pt-8 border-t border-stone-200 flex flex-col sm:flex-row justify-between items-center gap-4">
        <p class="text-sm text-stone-400">&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
        <div class="flex items-center gap-3 text-stone-400">
          <i class="pi pi-credit-card text-lg" title="Visa" />
          <i class="pi pi-credit-card text-lg" title="Mastercard" />
          <i class="pi pi-credit-card text-lg" title="Stripe" />
        </div>
      </div>
    </div>
  </footer>
</template>
