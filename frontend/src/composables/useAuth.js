import { reactive, computed } from 'vue';
import { authApi } from '@/services/api.js';

function readToken() {
  try {
    return localStorage.getItem('GEPROGAN_TOKEN');
  } catch (error) {
    return null;
  }
}

function readUser() {
  try {
    const raw = localStorage.getItem('GEPROGAN_USER');
    return raw ? JSON.parse(raw) : null;
  } catch (error) {
    return null;
  }
}

function persistSession(token, user) {
  try {
    localStorage.setItem('GEPROGAN_TOKEN', token);
    localStorage.setItem('GEPROGAN_USER', JSON.stringify(user));
  } catch (error) {
    /* storage disabled */
  }
}

function clearSession() {
  try {
    localStorage.removeItem('GEPROGAN_TOKEN');
    localStorage.removeItem('GEPROGAN_USER');
  } catch (error) {
    /* ignore */
  }
}

const state = reactive({
  token: readToken(),
  user: readUser()
});

export function useAuth() {
  const isAuthenticated = computed(() => Boolean(state.token));

  async function login(credentials) {
    const data = await authApi.login(credentials);
    if (!data?.token) {
      throw new Error('Respuesta invalida del servidor');
    }
    state.token = data.token;
    state.user = data.user ?? null;
    persistSession(state.token, state.user);
    return data;
  }

  function logout() {
    state.token = null;
    state.user = null;
    clearSession();
  }

  return {
    token: computed(() => state.token),
    user: computed(() => state.user),
    isAuthenticated,
    login,
    logout
  };
}
