<template>
  <div v-if="toasts.length" class="toast-stack" role="status" aria-live="polite">
    <article
      v-for="toast in toasts"
      :key="toast.id"
      class="toast"
      :data-type="toast.type"
    >
      <div>{{ toast.message }}</div>
      <button class="btn btn-ghost" type="button" @click="dismissToast(toast.id)">Cerrar</button>
    </article>
  </div>
</template>

<script setup>
import { useToasts } from '@/composables/useToasts.js';

const { toasts, dismissToast } = useToasts();
</script>
