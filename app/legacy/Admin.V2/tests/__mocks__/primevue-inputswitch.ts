export default {
  name: 'InputSwitch',
  template: '<input type="checkbox" class="input-switch-stub" :disabled="disabled" />',
  props: ['modelValue', 'disabled'],
}
