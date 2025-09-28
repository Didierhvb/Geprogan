<template>
  <div style="min-height: 100vh; background: var(--body-gradient); background-attachment: fixed;">
    <div style="display: flex; flex-direction: column; min-height: 100vh; padding: clamp(1.5rem, 3vw, 2.5rem); gap: clamp(2rem, 4vw, 3rem);">
      <AppNavigation />
      <main style="flex: 1;">
        <ToastStack />
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
import AppNavigation from '@/components/AppNavigation.vue';
import ToastStack from '@/components/ToastStack.vue';
</script>
