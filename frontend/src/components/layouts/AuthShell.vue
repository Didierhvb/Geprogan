<template>
  <div style="min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 1.5rem; background: var(--body-gradient); background-attachment: fixed;">
    <div style="width: 100%; max-width: 520px;">
      <ToastStack />
      <slot />
    </div>
  </div>
</template>

<script setup>
import ToastStack from '@/components/ToastStack.vue';
</script>
