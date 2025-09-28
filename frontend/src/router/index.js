import { createRouter, createWebHistory } from 'vue-router';
import { useAuth } from '@/composables/useAuth.js';

const HomeView = () => import('@/views/HomeView.vue');
const DashboardView = () => import('@/views/DashboardView.vue');
const LoginView = () => import('@/views/LoginView.vue');
const FincasView = () => import('@/views/FincasView.vue');
const GanadoView = () => import('@/views/GanadoView.vue');
const UsuariosView = () => import('@/views/UsuariosView.vue');
const ProduccionView = () => import('@/views/ProduccionView.vue');
const SanidadView = () => import('@/views/SanidadView.vue');
const ReproduccionView = () => import('@/views/ReproduccionView.vue');
const AnalisisSaludView = () => import('@/views/AnalisisSaludView.vue');
const AnalisisLecheroView = () => import('@/views/AnalisisLecheroView.vue');

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/dashboard' },
    { path: '/login', name: 'login', component: LoginView, meta: { layout: 'auth' } },
    { path: '/home', name: 'home', component: HomeView, meta: { requiresAuth: true } },
    { path: '/dashboard', name: 'dashboard', component: DashboardView, meta: { requiresAuth: true } },
    { path: '/fincas', name: 'fincas', component: FincasView, meta: { requiresAuth: true } },
    { path: '/ganado', name: 'ganado', component: GanadoView, meta: { requiresAuth: true } },
    { path: '/produccion', name: 'produccion', component: ProduccionView, meta: { requiresAuth: true } },
    { path: '/sanidad', name: 'sanidad', component: SanidadView, meta: { requiresAuth: true } },
    { path: '/reproduccion', name: 'reproduccion', component: ReproduccionView, meta: { requiresAuth: true } },
    { path: '/analisis-salud', name: 'analisis-salud', component: AnalisisSaludView, meta: { requiresAuth: true } },
    { path: '/analisis-lechero', name: 'analisis-lechero', component: AnalisisLecheroView, meta: { requiresAuth: true } },
    { path: '/usuarios', name: 'usuarios', component: UsuariosView, meta: { requiresAuth: true } }
  ]
});

router.beforeEach((to, from, next) => {
  const { isAuthenticated } = useAuth();
  if (to.meta.requiresAuth && !isAuthenticated.value) {
    return next({ name: 'login', query: { redirect: to.fullPath } });
  }
  if (to.name === 'login' && isAuthenticated.value) {
    return next({ name: 'dashboard' });
  }
  return next();
});

export default router;
