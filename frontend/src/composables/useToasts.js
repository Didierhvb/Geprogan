import { reactive } from 'vue';

const toasts = reactive([]);
let counter = 0;

export function useToasts() {
  function pushToast(message, type = 'info', timeout = 4200) {
    counter += 1;
    const id = counter;
    toasts.push({ id, message, type });
    if (timeout > 0) {
      setTimeout(() => dismissToast(id), timeout);
    }
    return id;
  }

  function dismissToast(id) {
    const index = toasts.findIndex((item) => item.id === id);
    if (index >= 0) {
      toasts.splice(index, 1);
    }
  }

  return { toasts, pushToast, dismissToast };
}
