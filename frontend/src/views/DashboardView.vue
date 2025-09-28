<template>
  <div class="dashboard-container">
    <!-- Header Section -->
    <section class="surface-panel">
      <div class="dashboard-header">
        <div class="header-content">
          <div class="stat-chip">Panel principal</div>
          <h1 class="hero-title">Dashboard Ganadero</h1>
          <p class="hero-subtitle">
            Monitorea tu operación ganadera en tiempo real. Estadísticas, tendencias y métricas clave para una gestión eficiente.
          </p>
        </div>
        <div class="header-actions">
          <button class="btn btn-outline" @click="refreshData" :disabled="loading">
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            {{ loading ? 'Actualizando...' : 'Actualizar' }}
          </button>
        </div>
      </div>
    </section>

    <!-- Statistics Cards -->
    <section class="stats-grid">
      <div class="stat-card" v-for="stat in statistics" :key="stat.id">
        <div class="stat-icon" :class="stat.colorClass">
          <component :is="stat.icon" />
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stat.value }}</h3>
          <p class="stat-label">{{ stat.label }}</p>
          <div class="stat-trend" :class="stat.trendClass" v-if="stat.trend">
            <svg class="trend-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path v-if="stat.trend > 0" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
              <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6" />
            </svg>
            <span>{{ Math.abs(stat.trend) }}%</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Charts and Tables Row -->
    <div class="dashboard-row">
      <!-- Production Chart -->
      <section class="surface-panel chart-panel">
        <header class="panel-header">
          <h2 class="panel-title">Producción por Mes</h2>
          <div class="panel-actions">
            <select v-model="selectedYear" class="select-input" @change="updateProductionChart">
              <option v-for="year in availableYears" :key="year" :value="year">{{ year }}</option>
            </select>
          </div>
        </header>
        <div class="chart-container">
          <div v-if="!productionData.length" class="chart-empty">
            <div class="empty-icon">📊</div>
            <p>No hay datos de producción para mostrar</p>
          </div>
          <div v-else class="simple-chart">
            <div class="chart-bars">
              <div
                v-for="(item, index) in productionData"
                :key="index"
                class="chart-bar"
                :style="{ height: `${(item.value / maxProduction) * 100}%` }"
                :title="`${item.month}: ${item.value}L`"
              >
                <span class="bar-value">{{ item.value }}L</span>
              </div>
            </div>
            <div class="chart-labels">
              <span v-for="item in productionData" :key="item.month" class="chart-label">
                {{ item.month }}
              </span>
            </div>
          </div>
        </div>
      </section>

      <!-- Recent Activity -->
      <section class="surface-panel activity-panel">
        <header class="panel-header">
          <h2 class="panel-title">Actividad Reciente</h2>
          <router-link to="/ganado" class="btn btn-outline btn-sm">Ver todo</router-link>
        </header>
        <div class="activity-list">
          <div v-if="!recentActivity.length" class="activity-empty">
            <p>No hay actividad reciente</p>
          </div>
          <div v-else class="activity-items">
            <div v-for="activity in recentActivity" :key="activity.id" class="activity-item">
              <div class="activity-icon" :class="activity.type">
                <component :is="activity.icon" />
              </div>
              <div class="activity-content">
                <p class="activity-title">{{ activity.title }}</p>
                <p class="activity-description">{{ activity.description }}</p>
                <span class="activity-time">{{ formatTimeAgo(activity.date) }}</span>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Livestock Distribution -->
    <section class="surface-panel distribution-panel">
      <header class="panel-header">
        <h2 class="panel-title">Distribución del Ganado</h2>
        <div class="panel-actions">
          <button class="btn btn-outline btn-sm" @click="toggleDistributionView">
            {{ distributionView === 'farm' ? 'Ver por Tipo' : 'Ver por Finca' }}
          </button>
        </div>
      </header>

      <div class="distribution-content">
        <div class="distribution-chart">
          <div v-if="!distributionData.length" class="chart-empty">
            <p>No hay datos de distribución</p>
          </div>
          <div v-else class="pie-chart-container">
            <div class="pie-chart">
              <div
                v-for="(segment, index) in distributionData"
                :key="segment.name"
                class="pie-segment"
                :style="getPieSegmentStyle(segment, index)"
                :title="`${segment.name}: ${segment.value} (${segment.percentage}%)`"
              />
            </div>
            <div class="pie-legend">
              <div v-for="(segment, index) in distributionData" :key="segment.name" class="legend-item">
                <div class="legend-color" :style="{ backgroundColor: getSegmentColor(index) }"></div>
                <span class="legend-label">{{ segment.name }}</span>
                <span class="legend-value">{{ segment.value }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Quick Actions -->
    <section class="surface-panel quick-actions-panel">
      <header class="panel-header">
        <h2 class="panel-title">Acciones Rápidas</h2>
      </header>
      <div class="quick-actions-grid">
        <router-link to="/ganado" class="quick-action-card">
          <div class="action-icon livestock">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Gestionar Ganado</h3>
            <p>Inventario y registro de animales</p>
          </div>
        </router-link>

        <router-link to="/produccion" class="quick-action-card">
          <div class="action-icon production">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Control Productivo</h3>
            <p>Registrar producción diaria</p>
          </div>
        </router-link>

        <router-link to="/sanidad" class="quick-action-card">
          <div class="action-icon health">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Control Sanitario</h3>
            <p>Vacunas y tratamientos médicos</p>
          </div>
        </router-link>

        <router-link to="/reproduccion" class="quick-action-card">
          <div class="action-icon reproduction">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Gestión Reproductiva</h3>
            <p>Servicios, gestaciones y partos</p>
          </div>
        </router-link>

        <router-link to="/fincas" class="quick-action-card">
          <div class="action-icon farms">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Gestionar Fincas</h3>
            <p>Ubicaciones y propiedades</p>
          </div>
        </router-link>

        <button class="quick-action-card" @click="generateReport">
          <div class="action-icon reports">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Generar Reportes</h3>
            <p>Informes y estadísticas</p>
          </div>
        </button>

        <router-link to="/usuarios" class="quick-action-card">
          <div class="action-icon users">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
            </svg>
          </div>
          <div class="action-content">
            <h3>Gestión de Usuarios</h3>
            <p>Accesos y permisos</p>
          </div>
        </router-link>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';
import { ganadoApi } from '@/services/api.js';

const { token } = useAuth();
const { pushToast } = useToasts();

// State
const loading = ref(true);
const selectedYear = ref(new Date().getFullYear());
const distributionView = ref('farm'); // 'farm' or 'type'

// Data
const statistics = ref([]);
const productionData = ref([]);
const recentActivity = ref([]);
const distributionData = ref([]);

// Computed
const availableYears = computed(() => {
  const currentYear = new Date().getFullYear();
  return [currentYear - 2, currentYear - 1, currentYear];
});

const maxProduction = computed(() => {
  return Math.max(...productionData.value.map(item => item.value), 1);
});

// Methods
async function loadDashboardData() {
  loading.value = true;
  try {
    const authToken = token.value;

    // Load all necessary data
    const [ganado, fincas, tipos] = await Promise.all([
      ganadoApi.list(authToken),
      ganadoApi.fincas(authToken),
      ganadoApi.tipos(authToken)
    ]);

    // Calculate statistics
    calculateStatistics(ganado, fincas, tipos);

    // Generate mock production data (replace with real API call)
    generateProductionData();

    // Generate recent activity (replace with real API call)
    generateRecentActivity(ganado);

    // Calculate distribution
    calculateDistribution(ganado, fincas, tipos);

  } catch (error) {
    pushToast(error.message || 'Error al cargar el dashboard', 'error');
  } finally {
    loading.value = false;
  }
}

function calculateStatistics(ganado, fincas, tipos) {
  const totalGanado = ganado.length;
  const totalFincas = fincas.length;
  const totalTipos = tipos.length;

  // Calculate by gender
  const machos = ganado.filter(animal => animal.sexo === 'M').length;
  const hembras = ganado.filter(animal => animal.sexo === 'H').length;

  statistics.value = [
    {
      id: 'total-ganado',
      label: 'Total Ganado',
      value: totalGanado.toLocaleString(),
      icon: 'LivestockIcon',
      colorClass: 'stat-primary',
      trend: 5.2,
      trendClass: 'trend-up'
    },
    {
      id: 'total-fincas',
      label: 'Fincas Activas',
      value: totalFincas.toLocaleString(),
      icon: 'FarmIcon',
      colorClass: 'stat-success',
      trend: 2.1,
      trendClass: 'trend-up'
    },
    {
      id: 'machos',
      label: 'Machos',
      value: machos.toLocaleString(),
      icon: 'MaleIcon',
      colorClass: 'stat-info',
      trend: null
    },
    {
      id: 'hembras',
      label: 'Hembras',
      value: hembras.toLocaleString(),
      icon: 'FemaleIcon',
      colorClass: 'stat-warning',
      trend: null
    }
  ];
}

function generateProductionData() {
  const months = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
                  'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

  productionData.value = months.map(month => ({
    month,
    value: Math.floor(Math.random() * 500) + 100 // Mock data
  }));
}

function generateRecentActivity(ganado) {
  const activities = [
    { type: 'birth', icon: 'PlusIcon', title: 'Nuevo nacimiento', description: 'Ternero registrado en Finca La Esperanza' },
    { type: 'health', icon: 'HeartIcon', title: 'Vacunación completa', description: 'Lote #12 - Aplicación de vacunas' },
    { type: 'production', icon: 'DropIcon', title: 'Registro de producción', description: '250L de leche recolectados' },
    { type: 'movement', icon: 'ArrowIcon', title: 'Traslado de ganado', description: '5 animales movidos a Finca San José' }
  ];

  recentActivity.value = activities.map((activity, index) => ({
    ...activity,
    id: index + 1,
    date: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000)
  }));
}

function calculateDistribution(ganado, fincas, tipos) {
  let groupData = [];

  if (distributionView.value === 'farm') {
    // Group by farm
    const farmCounts = {};
    ganado.forEach(animal => {
      const farmId = animal.idfinca || animal.Idfinca;
      const farm = fincas.find(f => f.id === farmId || f.idfinca === farmId);
      const farmName = farm ? (farm.nombreFinca || farm.nombre || `Finca ${farmId}`) : `Finca ${farmId}`;
      farmCounts[farmName] = (farmCounts[farmName] || 0) + 1;
    });
    groupData = Object.entries(farmCounts);
  } else {
    // Group by type
    const typeCounts = {};
    ganado.forEach(animal => {
      const typeId = animal.idtipoGanado || animal.IdtipoGanado || animal.idtipo;
      const type = tipos.find(t => t.id === typeId || t.idtipo === typeId);
      const typeName = type ? (type.nombre || type.descripcion || `Tipo ${typeId}`) : `Tipo ${typeId}`;
      typeCounts[typeName] = (typeCounts[typeName] || 0) + 1;
    });
    groupData = Object.entries(typeCounts);
  }

  const total = groupData.reduce((sum, [, count]) => sum + count, 0);

  distributionData.value = groupData.map(([name, value]) => ({
    name,
    value,
    percentage: total > 0 ? Math.round((value / total) * 100) : 0
  }));
}

function toggleDistributionView() {
  distributionView.value = distributionView.value === 'farm' ? 'type' : 'farm';
  // Recalculate with current data - in real app, you'd reload data
  // For now, just trigger a refresh
  loadDashboardData();
}

function updateProductionChart() {
  generateProductionData(); // In real app, fetch data for selected year
}

function refreshData() {
  loadDashboardData();
}

function generateReport() {
  pushToast('Generando reporte...', 'info');
  // In real app, this would generate and download a report
  setTimeout(() => {
    pushToast('Reporte generado exitosamente', 'success');
  }, 2000);
}

function formatTimeAgo(date) {
  const now = new Date();
  const diffMs = now - date;
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffHours / 24);

  if (diffDays > 0) return `Hace ${diffDays} día${diffDays > 1 ? 's' : ''}`;
  if (diffHours > 0) return `Hace ${diffHours} hora${diffHours > 1 ? 's' : ''}`;
  return 'Hace un momento';
}

function getPieSegmentStyle(segment, index) {
  // Simple pie chart simulation with CSS
  const colors = ['#4a7c59', '#8fbc8f', '#d2b48c', '#cd853f', '#daa520', '#6b8e23'];
  return {
    backgroundColor: colors[index % colors.length],
    flex: segment.percentage
  };
}

function getSegmentColor(index) {
  const colors = ['#4a7c59', '#8fbc8f', '#d2b48c', '#cd853f', '#daa520', '#6b8e23'];
  return colors[index % colors.length];
}

// Icon components (simplified)
const LivestockIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4" /></svg>' };
const FarmIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /></svg>' };
const MaleIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><circle cx="12" cy="12" r="3" stroke-width="2"/><path d="M12 1v6m0 0V1m0 6l4-4M12 7L8 3" stroke-width="2"/></svg>' };
const FemaleIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><circle cx="12" cy="12" r="3" stroke-width="2"/><path d="M12 19v4m-2-2h4" stroke-width="2"/></svg>' };
const PlusIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" /></svg>' };
const HeartIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" /></svg>' };
const DropIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zM7 5H3m4 6H3m4 6H3m4-12a4 4 0 014-4h4a2 2 0 012 2v16a2 2 0 01-2 2h-4a4 4 0 01-4-4V5z" /></svg>' };
const ArrowIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7l5 5m0 0l-5 5m5-5H6" /></svg>' };

onMounted(() => {
  loadDashboardData();
});
</script>

<style scoped>
.dashboard-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 2rem;
}

.header-content {
  flex: 1;
}

.header-actions {
  display: flex;
  gap: 1rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.stat-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: var(--transition-fast);
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

.stat-icon {
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stat-icon svg {
  width: 1.5rem;
  height: 1.5rem;
}

.stat-primary { background: var(--brand-primary); }
.stat-success { background: var(--brand-success); }
.stat-info { background: var(--brand-secondary); }
.stat-warning { background: var(--brand-warning); }

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--color-text);
  margin: 0 0 0.25rem 0;
}

.stat-label {
  color: var(--color-text-muted);
  font-size: 0.875rem;
  margin: 0;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: 0.5rem;
  font-size: 0.75rem;
  font-weight: 600;
}

.trend-icon {
  width: 1rem;
  height: 1rem;
}

.trend-up { color: var(--brand-success); }
.trend-down { color: var(--brand-error); }

.dashboard-row {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 2rem;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
}

.panel-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0;
}

.panel-actions {
  display: flex;
  gap: 1rem;
}

.select-input {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: 0.875rem;
}

.chart-container {
  padding: 1.5rem;
  height: 300px;
}

.chart-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--color-text-muted);
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.simple-chart {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.chart-bars {
  flex: 1;
  display: flex;
  align-items: flex-end;
  gap: 0.5rem;
  padding-bottom: 1rem;
}

.chart-bar {
  flex: 1;
  background: linear-gradient(to top, var(--brand-primary), var(--brand-secondary));
  border-radius: 4px 4px 0 0;
  min-height: 20px;
  position: relative;
  transition: var(--transition-fast);
  cursor: pointer;
}

.chart-bar:hover {
  opacity: 0.8;
}

.bar-value {
  position: absolute;
  top: -1.5rem;
  left: 50%;
  transform: translateX(-50%);
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-text);
  white-space: nowrap;
}

.chart-labels {
  display: flex;
  gap: 0.5rem;
}

.chart-label {
  flex: 1;
  text-align: center;
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.activity-panel {
  min-height: 400px;
}

.activity-list {
  padding: 1.5rem;
}

.activity-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: var(--color-text-muted);
}

.activity-items {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.activity-item {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
}

.activity-icon {
  width: 2rem;
  height: 2rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.activity-icon svg {
  width: 1rem;
  height: 1rem;
}

.activity-icon.birth { background: var(--brand-success); }
.activity-icon.health { background: var(--brand-error); }
.activity-icon.production { background: var(--brand-primary); }
.activity-icon.movement { background: var(--brand-warning); }

.activity-content {
  flex: 1;
}

.activity-title {
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 0.25rem 0;
}

.activity-description {
  color: var(--color-text-muted);
  font-size: 0.875rem;
  margin: 0 0 0.25rem 0;
}

.activity-time {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.distribution-content {
  padding: 1.5rem;
}

.pie-chart-container {
  display: flex;
  gap: 2rem;
  align-items: center;
}

.pie-chart {
  width: 200px;
  height: 200px;
  border-radius: 50%;
  display: flex;
  overflow: hidden;
}

.pie-segment {
  height: 100%;
  transition: var(--transition-fast);
}

.pie-legend {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.legend-color {
  width: 1rem;
  height: 1rem;
  border-radius: 3px;
  flex-shrink: 0;
}

.legend-label {
  flex: 1;
  font-size: 0.875rem;
  color: var(--color-text);
}

.legend-value {
  font-weight: 600;
  color: var(--color-text);
  font-size: 0.875rem;
}

.quick-actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  padding: 1.5rem;
}

.quick-action-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
  background: var(--color-bg-alt);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  text-decoration: none;
  color: inherit;
  transition: var(--transition-fast);
  cursor: pointer;
}

.quick-action-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
  background: var(--color-surface);
}

.action-icon {
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.action-icon svg {
  width: 1.5rem;
  height: 1.5rem;
}

.action-icon.livestock { background: var(--brand-primary); }
.action-icon.production { background: var(--brand-success); }
.action-icon.health { background: var(--brand-error); }
.action-icon.reproduction { background: var(--brand-accent); }
.action-icon.farms { background: var(--brand-tertiary); }
.action-icon.reports { background: var(--brand-warning); }
.action-icon.users { background: var(--brand-secondary); }

.action-content h3 {
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 0.25rem 0;
}

.action-content p {
  font-size: 0.875rem;
  color: var(--color-text-muted);
  margin: 0;
}

.w-4 { width: 1rem; }
.h-4 { height: 1rem; }
.mr-2 { margin-right: 0.5rem; }

@media (max-width: 1024px) {
  .dashboard-row {
    grid-template-columns: 1fr;
  }

  .pie-chart-container {
    flex-direction: column;
    text-align: center;
  }
}

@media (max-width: 640px) {
  .dashboard-header {
    flex-direction: column;
    align-items: stretch;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .quick-actions-grid {
    grid-template-columns: 1fr;
  }
}
</style>