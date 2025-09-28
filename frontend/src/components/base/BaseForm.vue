<template>
  <form :class="['base-form', formClass]" @submit.prevent="handleSubmit" novalidate>
    <header v-if="title || subtitle" class="form-header">
      <h2 v-if="title" class="form-title">{{ title }}</h2>
      <p v-if="subtitle" class="form-subtitle">{{ subtitle }}</p>
    </header>

    <div :class="['form-content', layoutClass]">
      <slot />
    </div>

    <footer v-if="showActions" class="form-actions">
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
        type="submit"
        :class="['btn', submitClass]"
        :disabled="loading || !isValid"
      >
        <span v-if="loading" class="loading-spinner"></span>
        {{ loading ? loadingText : submitText }}
      </button>
    </footer>
  </form>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  title: String,
  subtitle: String,
  layout: {
    type: String,
    default: 'single',
    validator: (value) => ['single', 'two', 'three', 'four', 'auto'].includes(value)
  },
  loading: {
    type: Boolean,
    default: false
  },
  showActions: {
    type: Boolean,
    default: true
  },
  showCancel: {
    type: Boolean,
    default: false
  },
  submitText: {
    type: String,
    default: 'Guardar'
  },
  cancelText: {
    type: String,
    default: 'Cancelar'
  },
  loadingText: {
    type: String,
    default: 'Guardando...'
  },
  submitClass: {
    type: String,
    default: 'btn-primary'
  },
  formClass: String,
  isValid: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['submit', 'cancel'])

const layoutClass = computed(() => {
  const layouts = {
    single: 'form-grid-single',
    two: 'form-grid-two',
    three: 'form-grid-three',
    four: 'form-grid-four',
    auto: 'form-grid-auto'
  }
  return layouts[props.layout] || 'form-grid-single'
})

const handleSubmit = () => {
  if (!props.loading && props.isValid) {
    emit('submit')
  }
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<style scoped>
.base-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-header {
  text-align: center;
}

.form-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: 0.5rem;
}

.form-subtitle {
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

/* Grid layouts */
.form-grid-single {
  display: grid;
  gap: 1.25rem;
}

.form-grid-two {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.25rem;
}

.form-grid-three {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.25rem;
}

.form-grid-four {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 1.25rem;
}

.form-grid-auto {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.25rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
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

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Responsive adjustments */
@media (max-width: 640px) {
  .form-grid-two,
  .form-grid-three,
  .form-grid-four,
  .form-grid-auto {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column-reverse;
  }
}
</style>