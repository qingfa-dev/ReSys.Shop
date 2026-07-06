import { ref, watch } from 'vue'
import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(false)

  function toggle() {
    isDark.value = !isDark.value
  }
  function setDark(value: boolean) {
    isDark.value = value
  }

  watch(
    isDark,
    (v) => {
      document.documentElement.classList.toggle('p-dark', v)
    },
    { immediate: true },
  )

  return { isDark, toggle, setDark }
})
