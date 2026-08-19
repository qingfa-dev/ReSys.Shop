import { ref } from 'vue'

export function useNewsletter() {
  const email = ref('')
  const isLoading = ref(false)
  const isSuccess = ref(false)
  const error = ref('')

  async function subscribe() {
    if (!email.value) {
      error.value = 'Please enter your email address'
      return
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(email.value)) {
      error.value = 'Please enter a valid email address'
      return
    }

    isLoading.value = true
    error.value = ''

    try {
      await new Promise((resolve) => setTimeout(resolve, 800))
      isSuccess.value = true
      email.value = ''
      
      localStorage.setItem('newsletter-subscribed', 'true')
    } catch (e) {
      error.value = 'Something went wrong. Please try again.'
    } finally {
      isLoading.value = false
    }
  }

  function reset() {
    email.value = ''
    isSuccess.value = false
    error.value = ''
  }

  return {
    email,
    isLoading,
    isSuccess,
    error,
    subscribe,
    reset,
  }
}
