<template>
  <button class="theme-toggle" :data-mode="mode" type="button" @click="toggleTheme" aria-label="Cambiar modo">
    <span v-if="mode === 'light'">Light</span>
    <span v-else>Dark</span>
  </button>
</template>

<script setup>
import { computed } from 'vue';
import { useTheme } from '@/composables/useTheme.js';

const { theme, toggleTheme } = useTheme();
const mode = computed(() => theme.value);
</script>
