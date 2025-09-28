<template>
  <label class="form-field">
    <span class="form-label">
      {{ label }}
      <span v-if="required" class="required-indicator">*</span>
    </span>

    <!-- Input field -->
    <input
      v-if="type !== 'select' && type !== 'textarea'"
      :id="fieldId"
      :type="type"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      :min="min"
      :max="max"
      :step="step"
      :class="['form-control', { 'error': !!error }]"
      :value="modelValue"
      @input="handleInput"
      @blur="handleBlur"
    />

    <!-- Textarea -->
    <textarea
      v-else-if="type === 'textarea'"
      :id="fieldId"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      :rows="rows"
      :class="['form-control', { 'error': !!error }]"
      :value="modelValue"
      @input="handleInput"
      @blur="handleBlur"
    />

    <!-- Select dropdown -->
    <select
      v-else
      :id="fieldId"
      :required="required"
      :disabled="disabled"
      :class="['form-control', { 'error': !!error }]"
      :value="modelValue"
      @change="handleInput"
      @blur="handleBlur"
    >
      <option value="">{{ placeholder || 'Selecciona una opción' }}</option>
      <option
        v-for="option in options"
        :key="option.value"
        :value="option.value"
      >
        {{ option.label }}
      </option>
    </select>

    <!-- Error message -->
    <span v-if="error" class="form-error">{{ error }}</span>

    <!-- Help text -->
    <span v-if="help && !error" class="form-help">{{ help }}</span>
  </label>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number, Boolean],
    default: ''
  },
  label: {
    type: String,
    required: true
  },
  type: {
    type: String,
    default: 'text',
    validator: (value) => [
      'text', 'email', 'password', 'number', 'tel', 'url',
      'date', 'datetime-local', 'time', 'textarea', 'select'
    ].includes(value)
  },
  placeholder: String,
  required: {
    type: Boolean,
    default: false
  },
  disabled: {
    type: Boolean,
    default: false
  },
  error: String,
  help: String,
  options: {
    type: Array,
    default: () => []
  },
  min: [String, Number],
  max: [String, Number],
  step: [String, Number],
  rows: {
    type: Number,
    default: 3
  }
})

const emit = defineEmits(['update:modelValue', 'blur'])

const fieldId = ref(`field-${Math.random().toString(36).substr(2, 9)}`)

const handleInput = (event) => {
  let value = event.target.value

  if (props.type === 'number') {
    value = value === '' ? '' : Number(value)
  }

  emit('update:modelValue', value)
}

const handleBlur = () => {
  emit('blur')
}
</script>

<style scoped>
.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text);
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.required-indicator {
  color: var(--brand-error);
  font-weight: 600;
}

.form-control {
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: 0.875rem;
  transition: var(--transition-fast);
}

.form-control:focus {
  outline: none;
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 3px var(--accent-glow);
}

.form-control.error {
  border-color: var(--brand-error);
}

.form-control.error:focus {
  box-shadow: 0 0 0 3px rgba(178, 34, 34, 0.2);
}

.form-control:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.form-error {
  font-size: 0.75rem;
  color: var(--brand-error);
  font-weight: 500;
}

.form-help {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

textarea.form-control {
  resize: vertical;
  min-height: 80px;
}
</style>