<template>
  <header class="navbar">
    <router-link to="/" class="logo">Geprogan</router-link>
    <nav v-if="isAuthenticated" class="navbar-nav" aria-label="Modulos">
      <router-link
        v-for="item in navItems"
        :key="item.to"
        class="navpill"
        :data-active="route.path.startsWith(item.match)"
        :to="item.to"
      >
        {{ item.label }}
      </router-link>
    </nav>
    <div class="navbar-nav" style="margin-left: 0; gap: 0.75rem;">
      <ThemeToggle />
      <router-link v-if="!isAuthenticated" class="btn btn-outline" to="/login">Entrar</router-link>
      <button v-else class="btn btn-outline" type="button" @click="handleLogout">Salir</button>
    </div>
  </header>
</template>

<script setup>
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ThemeToggle from '@/components/ThemeToggle.vue';
import { useAuth } from '@/composables/useAuth.js';

const router = useRouter();
const route = useRoute();
const { isAuthenticated, logout } = useAuth();

const navItems = computed(() => [
  { to: '/dashboard', match: '/dashboard', label: 'Dashboard' },
  { to: '/fincas', match: '/fincas', label: 'Fincas' },
  { to: '/ganado', match: '/ganado', label: 'Ganado' },
  { to: '/produccion', match: '/produccion', label: 'Producción' },
  { to: '/sanidad', match: '/sanidad', label: 'Sanidad' },
  { to: '/reproduccion', match: '/reproduccion', label: 'Reproducción' },
  { to: '/analisis-salud', match: '/analisis-salud', label: 'IA Salud' },
  { to: '/analisis-lechero', match: '/analisis-lechero', label: 'IA Leche' },
  { to: '/usuarios', match: '/usuarios', label: 'Usuarios' }
]);

function handleLogout() {
  logout();
  router.push({ name: 'login' });
}
</script>
