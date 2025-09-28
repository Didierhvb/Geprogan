<template>
  <section class="surface-panel">
    <div style="text-align: center; margin-bottom: 2rem;">
      <h1 class="hero-title" style="font-size: 2rem;">Bienvenido a Geprogan</h1>
      <p class="hero-subtitle">Autentica tu sesión para acceder al tablero futurista.</p>
    </div>

    <form @submit.prevent="handleSubmit" class="form-grid" novalidate>
      <label class="form-grid">
        <span class="form-label">Correo electrónico</span>
        <input
          v-model.trim="form.email"
          type="email"
          class="form-control"
          autocomplete="email"
          placeholder="usuario@dominio.com"
          required
        />
        <span v-if="errors.email" class="form-error">{{ errors.email }}</span>
      </label>

      <label class="form-grid">
        <span class="form-label">Contraseña</span>
        <input
          v-model="form.password"
          type="password"
          class="form-control"
          autocomplete="current-password"
          placeholder="****"
          required
        />
        <span v-if="errors.password" class="form-error">{{ errors.password }}</span>
      </label>

      <div style="display: flex; gap: 1rem; align-items: center; justify-content: space-between;">
        <router-link class="btn btn-outline" to="/">Volver al inicio</router-link>
        <button class="btn btn-primary" type="submit" :disabled="loading">
          {{ loading ? 'Ingresando...' : 'Entrar' }}
        </button>
      </div>
    </form>
  </section>
</template>

<script setup>
import { reactive, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';

const router = useRouter();
const route = useRoute();
const { login } = useAuth();
const { pushToast } = useToasts();

const form = reactive({
  email: '',
  password: ''
});

const errors = reactive({
  email: '',
  password: ''
});

const loading = ref(false);

function validate() {
  errors.email = form.email ? '' : 'Ingresa tu correo.';
  errors.password = form.password ? '' : 'Ingresa tu contrasena.';
  return !errors.email && !errors.password;
}

async function handleSubmit() {
  if (!validate()) {
    return;
  }
  loading.value = true;
  try {
    await login({ email: form.email, password: form.password });
    pushToast('Sesion iniciada correctamente', 'success');
    const redirect = route.query.redirect || '/dashboard';
    router.push(redirect);
  } catch (error) {
    pushToast(error.message || 'No se pudo iniciar sesion', 'error');
  } finally {
    loading.value = false;
  }
}
</script>

