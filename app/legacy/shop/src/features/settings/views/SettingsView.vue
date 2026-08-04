<script setup lang="ts">
import { usePreferencesStore } from '@/app/stores/preferences'
import Dropdown from 'primevue/dropdown'
import InputSwitch from 'primevue/inputswitch'

const preferencesStore = usePreferencesStore()

const themeOptions = [
  { label: 'Light', value: 'light' },
  { label: 'Dark', value: 'dark' },
  { label: 'System', value: 'system' },
]

const currencyOptions = [
  { label: 'USD ($)', value: 'USD' },
  { label: 'EUR (€)', value: 'EUR' },
  { label: 'GBP (£)', value: 'GBP' },
]

const languageOptions = [
  { label: 'English', value: 'en' },
  { label: 'Spanish', value: 'es' },
  { label: 'French', value: 'fr' },
]
</script>

<template>
  <div class="settings-view">
    <h2>Settings</h2>
    
    <div class="settings-section">
      <h3>Appearance</h3>
      
      <div class="setting-item">
        <div class="setting-label">
          <span>Theme</span>
          <small>Choose your preferred color theme</small>
        </div>
        <Dropdown
          v-model="preferencesStore.preferences.theme"
          :options="themeOptions"
          optionLabel="label"
          optionValue="value"
        />
      </div>
    </div>
    
    <div class="settings-section">
      <h3>Regional</h3>
      
      <div class="setting-item">
        <div class="setting-label">
          <span>Currency</span>
          <small>Display prices in your currency</small>
        </div>
        <Dropdown
          v-model="preferencesStore.preferences.currency"
          :options="currencyOptions"
          optionLabel="label"
          optionValue="value"
        />
      </div>
      
      <div class="setting-item">
        <div class="setting-label">
          <span>Language</span>
          <small>Choose your preferred language</small>
        </div>
        <Dropdown
          v-model="preferencesStore.preferences.language"
          :options="languageOptions"
          optionLabel="label"
          optionValue="value"
        />
      </div>
    </div>
    
    <div class="settings-section">
      <h3>Notifications</h3>
      
      <div class="setting-item">
        <div class="setting-label">
          <span>Email Notifications</span>
          <small>Receive order updates via email</small>
        </div>
        <InputSwitch v-model="preferencesStore.preferences.notifications.email" />
      </div>
      
      <div class="setting-item">
        <div class="setting-label">
          <span>SMS Notifications</span>
          <small>Receive order updates via SMS</small>
        </div>
        <InputSwitch v-model="preferencesStore.preferences.notifications.sms" />
      </div>
      
      <div class="setting-item">
        <div class="setting-label">
          <span>Newsletter</span>
          <small>Receive promotional emails</small>
        </div>
        <InputSwitch v-model="preferencesStore.preferences.newsletter" />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.settings-view {
  h2 {
    font-size: var(--font-size-xl);
    margin-bottom: 1.5rem;
  }
  
  h3 {
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    margin-bottom: 1rem;
    color: var(--color-text-secondary);
  }
}

.settings-section {
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid var(--color-border-light);
  
  &:last-child {
    border-bottom: none;
  }
}

.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
}

.setting-label {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  
  span {
    font-weight: var(--font-weight-medium);
  }
  
  small {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
  }
}
</style>
