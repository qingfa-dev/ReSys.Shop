import { ref, watch } from 'vue'
import { defineStore } from 'pinia'

const STORAGE_KEY = 'admin:sidebar:collapsed'

export const useSidebarStore = defineStore('sidebar', () => {
  const collapsed = ref(localStorage.getItem(STORAGE_KEY) === '1')

  function toggle() {
    collapsed.value = !collapsed.value
  }

  watch(collapsed, (v) => {
    localStorage.setItem(STORAGE_KEY, v ? '1' : '0')
  })

  return { collapsed, toggle }
})
