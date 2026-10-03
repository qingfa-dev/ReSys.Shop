<script setup lang="ts">
import { RouterLink } from 'vue-router'
import NewsletterForm from '@/app/components/ui/NewsletterForm.vue'

interface FooterLink {
  label: string
  path: string
  queryString?: string
}

interface FooterLinkGroup {
  title: string
  links: FooterLink[]
}

const defaultFooterLinks: FooterLinkGroup[] = [
  {
    title: 'Shop',
    links: [
      { label: 'All Products', path: '/shop' },
      { label: 'Collections', path: '/collections' },
      { label: 'New Arrivals', path: '/shop', queryString: 'tag=new' },
      { label: 'Sale', path: '/shop', queryString: 'sale=true' },
    ],
  },
  {
    title: 'Support',
    links: [
      { label: 'Contact', path: '/contact' },
      { label: 'FAQ', path: '/faq' },
      { label: 'Shipping', path: '/shipping' },
      { label: 'Returns', path: '/returns' },
    ],
  },
  {
    title: 'Company',
    links: [
      { label: 'About Us', path: '/about' },
      { label: 'Careers', path: '/careers' },
      { label: 'Blog', path: '/blog' },
    ],
  },
]

defineProps<{
  linkGroups?: FooterLinkGroup[]
}>()

const footerLinkGroups = defaultFooterLinks

function getQueryString(path: string): string | undefined {
  const link = footerLinkGroups.flatMap(g => g.links).find(l => l.path === path)
  return link?.queryString
}
</script>

<template>
  <footer class="app-footer">
    <div class="footer-container">
      <div class="footer-grid">
        <div class="footer-brand">
          <RouterLink to="/" class="logo">
            <span class="logo-text">ReSys</span>
            <span class="logo-dot">.</span>
            <span class="logo-text">Shop</span>
          </RouterLink>
          <p class="brand-tagline">Curated fashion for the modern lifestyle.</p>
        </div>
        
        <div v-for="group in footerLinkGroups" :key="group.title" class="footer-links">
          <h4>{{ group.title }}</h4>
          <RouterLink 
            v-for="link in group.links" 
            :key="link.path + link.queryString"
            :to="link.path + (link.queryString ? '?' + link.queryString : '')"
          >
            {{ link.label }}
          </RouterLink>
        </div>
        
        <div class="footer-newsletter">
          <NewsletterForm />
        </div>
      </div>
      
      <div class="footer-bottom">
        <p>&copy; 2026 ReSys.Shop. All rights reserved.</p>
        <div class="footer-social">
          <a href="#" aria-label="Instagram"><i class="pi pi-instagram"></i></a>
          <a href="#" aria-label="Twitter"><i class="pi pi-twitter"></i></a>
          <a href="#" aria-label="Facebook"><i class="pi pi-facebook"></i></a>
        </div>
      </div>
    </div>
  </footer>
</template>

<style scoped lang="scss">
.app-footer {
  background: var(--color-surface);
  border-top: 1px solid var(--color-border-light);
  margin-top: 4rem;
}

.footer-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 4rem 2rem 2rem;
}

.footer-grid {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr 1.5fr;
  gap: 3rem;
  
  @media (max-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);
  }
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr 1fr;
    gap: 2rem;
  }
  
  @media (max-width: 480px) {
    grid-template-columns: 1fr;
  }
}

.footer-brand {
  .logo {
    margin-bottom: 1rem;
    font-family: var(--font-display);
    font-size: var(--font-size-xl);
    font-weight: var(--font-weight-bold);
    text-decoration: none;
    display: flex;
    align-items: center;
    
    .logo-text {
      color: var(--color-text);
    }
    
    .logo-dot {
      color: var(--color-primary);
      margin: 0 2px;
    }
  }
  
  .brand-tagline {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }
}

.footer-links {
  h4 {
    font-family: var(--font-body);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 1rem;
    color: var(--color-text);
  }
  
  a {
    display: block;
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
    text-decoration: none;
    margin-bottom: 0.5rem;
    transition: color var(--transition-fast);
    
    &:hover {
      color: var(--color-text);
    }
  }
}

.footer-newsletter {
  h4 {
    font-family: var(--font-body);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 1rem;
    color: var(--color-text);
  }
}

.footer-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 2rem;
  margin-top: 3rem;
  border-top: 1px solid var(--color-border-light);
  
  @media (max-width: 480px) {
    flex-direction: column;
    gap: 1rem;
  }
  
  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }
}

.footer-social {
  display: flex;
  gap: 1rem;
  
  a {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-full);
    background: var(--color-surface-ground);
    color: var(--color-text-secondary);
    transition: all var(--transition-fast);
    
    &:hover {
      background: var(--color-primary);
      color: white;
    }
  }
}
</style>
