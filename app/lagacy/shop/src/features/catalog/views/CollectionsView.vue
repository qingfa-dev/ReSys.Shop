<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <div class="collections-view">
    <div class="page-header">
      <h1>Collections</h1>
      <p>Explore our curated collections</p>
    </div>
    
    <div class="collections-grid">
      <RouterLink to="/shop?collection=summer" class="collection-card">
        <div class="collection-image"></div>
        <div class="collection-overlay">
          <h2>Summer Essentials</h2>
          <span>Shop Now</span>
        </div>
      </RouterLink>
      <RouterLink to="/shop?collection=winter" class="collection-card">
        <div class="collection-image"></div>
        <div class="collection-overlay">
          <h2>Winter Collection</h2>
          <span>Shop Now</span>
        </div>
      </RouterLink>
      <RouterLink to="/shop?collection=minimalist" class="collection-card">
        <div class="collection-image"></div>
        <div class="collection-overlay">
          <h2>Minimalist</h2>
          <span>Shop Now</span>
        </div>
      </RouterLink>
    </div>
  </div>
</template>

<style scoped lang="scss">
.collections-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  text-align: center;
  padding: 3rem 0;
  
  h1 {
    font-size: var(--font-size-4xl);
    margin-bottom: 0.5rem;
  }
  
  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-lg);
  }
}

.collections-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.collection-card {
  position: relative;
  aspect-ratio: 4/5;
  border-radius: var(--radius-xl);
  overflow: hidden;
  text-decoration: none;
  
  &:hover .collection-image {
    transform: scale(1.05);
  }
  
  .collection-image {
    position: absolute;
    inset: 0;
    background: linear-gradient(135deg, #d1d5db 0%, #9ca3af 100%);
    transition: transform var(--transition-slow);
  }
  
  .collection-overlay {
    position: absolute;
    inset: 0;
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
    padding: 2rem;
    background: linear-gradient(to top, rgba(0,0,0,0.7), transparent);
    
    h2 {
      font-size: var(--font-size-2xl);
      color: white;
      margin-bottom: 0.5rem;
    }
    
    span {
      color: rgba(255,255,255,0.8);
      font-size: var(--font-size-sm);
    }
  }
}
</style>
