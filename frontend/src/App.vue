<template>
  <component :is="layoutComponent">
    <router-view />
  </component>
</template>

<script setup>
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import AppShell from '@/components/layouts/AppShell.vue';
import AuthShell from '@/components/layouts/AuthShell.vue';

const route = useRoute();

const layoutComponent = computed(() => {
  const metaLayout = route.meta?.layout;
  return metaLayout === 'auth' ? AuthShell : AppShell;
});
</script>
