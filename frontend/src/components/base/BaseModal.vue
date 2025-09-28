<template>
  <Teleport to="body">
    <Transition name="modal" appear>
      <div
        v-if="modelValue"
        class="modal-overlay"
        :class="{ 'modal-persistent': persistent }"
        @click="handleOverlayClick"
        @keydown.esc="handleEscape"
        tabindex="-1"
      >
        <div
          :class="['modal-container', sizeClass]"
          role="dialog"
          :aria-labelledby="titleId"
          :aria-describedby="descriptionId"
          @click.stop
        >
          <!-- Header -->
          <header v-if="showHeader" class="modal-header">
            <div class="modal-title-section">
              <h3 :id="titleId" class="modal-title">{{ title }}</h3>
              <p v-if="subtitle" :id="descriptionId" class="modal-subtitle">{{ subtitle }}</p>
            </div>
            <button
              v-if="showClose"
              type="button"
              class="modal-close-btn"
              @click="handleClose"
              aria-label="Cerrar modal"
            >
              <svg viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
              </svg>
            </button>
          </header>

          <!-- Content -->
          <main class="modal-body">
            <slot />
          </main>

          <!-- Footer -->
          <footer v-if="showFooter" class="modal-footer">
            <slot name="footer">
              <div class="modal-actions">
                <button
                  v-if="showCancel"
                  type="button"
                  class="btn btn-outline"
                  :disabled="loading"
                  @click="handleCancel"
                >
                  {{ cancelText }}
                </button>
                <button
                  v-if="showConfirm"
                  type="button"
                  :class="['btn', confirmClass]"
                  :disabled="loading || !confirmEnabled"
                  @click="handleConfirm"
                >
                  <span v-if="loading" class="loading-spinner"></span>
                  {{ loading ? loadingText : confirmText }}
                </button>
              </div>
            </slot>
          </footer>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed, ref, nextTick, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  title: String,
  subtitle: String,
  size: {
    type: String,
    default: 'md',
    validator: (value) => ['xs', 'sm', 'md', 'lg', 'xl', 'full'].includes(value)
  },
  persistent: {
    type: Boolean,
    default: false
  },
  showHeader: {
    type: Boolean,
    default: true
  },
  showFooter: {
    type: Boolean,
    default: false
  },
  showClose: {
    type: Boolean,
    default: true
  },
  showCancel: {
    type: Boolean,
    default: true
  },
  showConfirm: {
    type: Boolean,
    default: true
  },
  cancelText: {
    type: String,
    default: 'Cancelar'
  },
  confirmText: {
    type: String,
    default: 'Confirmar'
  },
  loadingText: {
    type: String,
    default: 'Procesando...'
  },
  confirmClass: {
    type: String,
    default: 'btn-primary'
  },
  loading: {
    type: Boolean,
    default: false
  },
  confirmEnabled: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['update:modelValue', 'close', 'cancel', 'confirm'])

// Computed
const titleId = computed(() => `modal-title-${Math.random().toString(36).substr(2, 9)}`)
const descriptionId = computed(() => `modal-desc-${Math.random().toString(36).substr(2, 9)}`)

const sizeClass = computed(() => {
  const sizes = {
    xs: 'modal-xs',
    sm: 'modal-sm',
    md: 'modal-md',
    lg: 'modal-lg',
    xl: 'modal-xl',
    full: 'modal-full'
  }
  return sizes[props.size] || 'modal-md'
})

// Methods
const handleClose = () => {
  if (!props.loading) {
    emit('update:modelValue', false)
    emit('close')
  }
}

const handleCancel = () => {
  emit('cancel')
  handleClose()
}

const handleConfirm = () => {
  emit('confirm')
}

const handleOverlayClick = () => {
  if (!props.persistent) {
    handleClose()
  }
}

const handleEscape = () => {
  if (!props.persistent) {
    handleClose()
  }
}

// Focus management
watch(() => props.modelValue, async (isOpen) => {
  if (isOpen) {
    await nextTick()
    // Focus the modal container for keyboard navigation
    const modalContainer = document.querySelector('.modal-container')
    if (modalContainer) {
      modalContainer.focus()
    }
  }
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 1000;
  overflow-y: auto;
}

.modal-container {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-xl);
  display: flex;
  flex-direction: column;
  max-height: calc(100vh - 2rem);
  width: 100%;
  position: relative;
  outline: none;
}

/* Size variants */
.modal-xs { max-width: 320px; }
.modal-sm { max-width: 480px; }
.modal-md { max-width: 640px; }
.modal-lg { max-width: 800px; }
.modal-xl { max-width: 1024px; }
.modal-full {
  max-width: calc(100vw - 2rem);
  max-height: calc(100vh - 2rem);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
  gap: 1rem;
}

.modal-title-section {
  flex: 1;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 0.25rem 0;
}

.modal-subtitle {
  font-size: 0.875rem;
  color: var(--color-text-muted);
  margin: 0;
}

.modal-close-btn {
  background: none;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 0.25rem;
  border-radius: var(--radius-md);
  transition: var(--transition-fast);
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-close-btn svg {
  width: 1.25rem;
  height: 1.25rem;
}

.modal-close-btn:hover {
  background: var(--color-surface-muted);
  color: var(--color-text);
}

.modal-body {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
}

.modal-footer {
  padding: 1.5rem;
  border-top: 1px solid var(--color-border);
  background: var(--color-bg-alt);
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
}

.loading-spinner {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-right: 0.5rem;
}

/* Transitions */
.modal-enter-active {
  transition: opacity 0.3s ease;
}

.modal-leave-active {
  transition: opacity 0.2s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .modal-container {
  transition: transform 0.3s ease;
}

.modal-leave-active .modal-container {
  transition: transform 0.2s ease;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.95);
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 640px) {
  .modal-overlay {
    padding: 0.5rem;
  }

  .modal-xs,
  .modal-sm,
  .modal-md,
  .modal-lg,
  .modal-xl {
    max-width: 100%;
  }

  .modal-header,
  .modal-body,
  .modal-footer {
    padding: 1rem;
  }

  .modal-actions {
    flex-direction: column-reverse;
  }
}
</style>