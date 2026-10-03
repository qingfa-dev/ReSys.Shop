<script setup lang="ts">
import Button from 'primevue/button'
</script>

<template>
  <div class="about-view">
    <div class="page-header">
      <h1>About ReSys.Shop</h1>
      <p>Crafting fashion for the modern lifestyle</p>
    </div>
    
    <section class="about-content">
      <div class="about-text">
        <h2>Our Story</h2>
        <p>
          ReSys.Shop was born from a simple idea: fashion should be personal, accessible, and sustainable. 
          We believe that great style shouldn't come at the cost of our planet or your wallet.
        </p>
        <p>
          Our curated collection brings together emerging designers and established brands, 
          all chosen for their commitment to quality, ethics, and innovation.
        </p>
      </div>
      <div class="about-image"></div>
    </section>
    
    <section class="values">
      <h2>Our Values</h2>
      <div class="values-grid">
        <div class="value-card">
          <i class="pi pi-heart"></i>
          <h3>Sustainability</h3>
          <p>We're committed to reducing our environmental footprint through responsible sourcing and packaging.</p>
        </div>
        <div class="value-card">
          <i class="pi pi-star"></i>
          <h3>Quality</h3>
          <p>Every piece in our collection is chosen for its exceptional craftsmanship and durability.</p>
        </div>
        <div class="value-card">
          <i class="pi pi-users"></i>
          <h3>Community</h3>
          <p>We believe in building a community of conscious consumers who inspire each other.</p>
        </div>
      </div>
    </section>
    
    <section class="cta">
      <h2>Join Our Journey</h2>
      <p>Be the first to know about new arrivals, exclusive offers, and behind-the-scenes stories.</p>
      <div class="cta-actions">
        <Button label="Shop Now" size="large" />
        <Button label="Contact Us" size="large" outlined severity="secondary" />
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss">
.about-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  text-align: center;
  padding: 4rem 0;
  
  h1 {
    font-size: var(--font-size-4xl);
    margin-bottom: 0.5rem;
  }
  
  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-lg);
  }
}

.about-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  align-items: center;
  padding: 4rem 0;
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    gap: 2rem;
  }
}

.about-text {
  h2 {
    font-size: var(--font-size-3xl);
    margin-bottom: 1.5rem;
  }
  
  p {
    color: var(--color-text-secondary);
    margin-bottom: 1rem;
    line-height: var(--line-height-relaxed);
  }
}

.about-image {
  aspect-ratio: 4/3;
  background: linear-gradient(135deg, var(--color-surface-ground) 0%, var(--color-border-light) 100%);
  border-radius: var(--radius-xl);
}

.values {
  padding: 4rem 0;
  text-align: center;
  
  h2 {
    font-size: var(--font-size-3xl);
    margin-bottom: 3rem;
  }
}

.values-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.value-card {
  padding: 2rem;
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-sm);
  
  i {
    font-size: 2.5rem;
    color: var(--color-primary);
    margin-bottom: 1rem;
  }
  
  h3 {
    font-family: var(--font-body);
    font-size: var(--font-size-xl);
    margin-bottom: 0.75rem;
  }
  
  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }
}

.cta {
  text-align: center;
  padding: 6rem 2rem;
  background: var(--color-surface);
  border-radius: var(--radius-2xl);
  margin-top: 4rem;
  
  h2 {
    font-size: var(--font-size-3xl);
    margin-bottom: 1rem;
  }
  
  p {
    color: var(--color-text-secondary);
    margin-bottom: 2rem;
    max-width: 500px;
    margin-left: auto;
    margin-right: auto;
  }
}

.cta-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  
  @media (max-width: 480px) {
    flex-direction: column;
  }
}
</style>
