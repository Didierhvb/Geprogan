<template>
  <div class="page">
    <div class="page-header">
      <h1>Análisis de Salud con IA</h1>
      <p class="text-muted">Detección inteligente de anomalías en la salud del ganado</p>
    </div>

    <!-- Alertas y Estado del Sistema -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
      <!-- Estado del Modelo -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Estado del Modelo</h3>
        </div>
        <div class="card-body">
          <div v-if="modeloEntrenado" class="flex items-center text-green-600 mb-2">
            <svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
            </svg>
            Modelo Entrenado
          </div>
          <div v-else class="flex items-center text-yellow-600 mb-2">
            <svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
            </svg>
            Modelo no entrenado
          </div>
          <button
            @click="entrenarModelo"
            :disabled="entrenandoModelo"
            class="btn btn-primary w-full"
          >
            <span v-if="entrenandoModelo" class="flex items-center">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              Entrenando...
            </span>
            <span v-else>{{ modeloEntrenado ? 'Reentrenar Modelo' : 'Entrenar Modelo' }}</span>
          </button>
        </div>
      </div>

      <!-- Resumen General -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Resumen de Salud</h3>
        </div>
        <div class="card-body" v-if="resumenSalud">
          <div class="text-center mb-4">
            <div class="text-3xl font-bold mb-1" :class="getEstadoColor(resumenSalud.estadoGeneral)">
              {{ resumenSalud.estadoGeneral }}
            </div>
            <div class="text-sm text-muted">Estado General del Hato</div>
          </div>
          <div class="space-y-2">
            <div class="flex justify-between">
              <span>Total Animales:</span>
              <strong>{{ resumenSalud.totalAnimales }}</strong>
            </div>
            <div class="flex justify-between">
              <span>Anomalías:</span>
              <strong class="text-red-600">{{ resumenSalud.animalesConAnomalias }}</strong>
            </div>
            <div class="flex justify-between">
              <span>% Anomalías:</span>
              <strong>{{ resumenSalud.porcentajeAnomalias }}%</strong>
            </div>
          </div>
        </div>
        <div v-else class="card-body text-center text-muted">
          <div class="mb-2">📊</div>
          No hay datos disponibles
        </div>
      </div>

      <!-- Alertas Urgentes -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Alertas Urgentes</h3>
        </div>
        <div class="card-body">
          <div v-if="alertasUrgentes && alertasUrgentes.alertasUrgentes?.length > 0" class="space-y-3">
            <div
              v-for="alerta in alertasUrgentes.alertasUrgentes.slice(0, 3)"
              :key="alerta.idGanado"
              class="p-3 bg-red-50 border border-red-200 rounded-lg"
            >
              <div class="font-medium text-red-800">{{ alerta.nombreGanado }}</div>
              <div class="text-sm text-red-600">Riesgo: {{ (alerta.puntuacionRiesgo * 100).toFixed(1) }}%</div>
            </div>
            <button
              v-if="alertasUrgentes.totalAlertas > 3"
              @click="verTodasAlertas = true"
              class="btn btn-outline btn-sm w-full"
            >
              Ver todas ({{ alertasUrgentes.totalAlertas }})
            </button>
          </div>
          <div v-else class="text-center text-muted">
            <div class="mb-2">✅</div>
            No hay alertas urgentes
          </div>
        </div>
      </div>
    </div>

    <!-- Gráficos y Visualizaciones -->
    <GraficosSalud
      v-if="resumenSalud && anomaliasCompletas"
      :metricas="resumenSalud"
      :anomalias="anomaliasCompletas"
      class="mb-8"
    />

    <!-- Análisis Individual -->
    <div class="card mb-8">
      <div class="card-header">
        <h3 class="card-title">Análisis Individual</h3>
      </div>
      <div class="card-body">
        <div class="flex gap-4 mb-4">
          <select v-model="animalSeleccionado" class="form-input flex-1">
            <option value="">Seleccionar animal para analizar</option>
            <option v-for="animal in ganado" :key="animal.idganado" :value="animal.idganado">
              {{ animal.nombreGanado || `Animal #${animal.idganado}` }} - {{ animal.raza }}
            </option>
          </select>
          <button
            @click="analizarAnimalSeleccionado"
            :disabled="!animalSeleccionado || analizandoAnimal"
            class="btn btn-primary"
          >
            <span v-if="analizandoAnimal">Analizando...</span>
            <span v-else>Analizar</span>
          </button>
        </div>

        <div v-if="analisisIndividual" class="mt-6 p-4 border rounded-lg">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h4 class="font-semibold mb-3">Resultado del Análisis</h4>
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span>Estado:</span>
                  <span :class="analisisIndividual.esAnomalia ? 'text-red-600 font-semibold' : 'text-green-600 font-semibold'">
                    {{ analisisIndividual.esAnomalia ? 'Anomalía Detectada' : 'Normal' }}
                  </span>
                </div>
                <div class="flex justify-between">
                  <span>Nivel de Riesgo:</span>
                  <span :class="getRiesgoColor(analisisIndividual.nivelRiesgo)">
                    {{ analisisIndividual.nivelRiesgo }}
                  </span>
                </div>
                <div class="flex justify-between">
                  <span>Puntuación:</span>
                  <span>{{ (analisisIndividual.puntuacionAnomalia * 100).toFixed(1) }}%</span>
                </div>
              </div>
            </div>
            <div>
              <h4 class="font-semibold mb-3">Datos del Animal</h4>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span>Peso Actual:</span>
                  <span>{{ analisisIndividual.pesoActual || 'N/A' }} kg</span>
                </div>
                <div class="flex justify-between">
                  <span>Condición Corporal:</span>
                  <span>{{ analisisIndividual.condicionCorporal || 'N/A' }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Producción Promedio:</span>
                  <span>{{ analisisIndividual.produccionPromedio?.toFixed(1) || 'N/A' }} L</span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="analisisIndividual.alertasSalud?.length > 0" class="mt-4">
            <h4 class="font-semibold mb-2">Alertas Específicas:</h4>
            <ul class="list-disc list-inside space-y-1 text-sm">
              <li v-for="alerta in analisisIndividual.alertasSalud" :key="alerta" class="text-orange-600">
                {{ alerta }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal de Todas las Alertas -->
    <BaseModal v-if="verTodasAlertas" @close="verTodasAlertas = false">
      <template #header>
        <h3 class="text-lg font-semibold">Todas las Alertas Urgentes</h3>
      </template>
      <template #body>
        <div v-if="alertasUrgentes?.alertasUrgentes" class="space-y-4 max-h-96 overflow-y-auto">
          <div
            v-for="alerta in alertasUrgentes.alertasUrgentes"
            :key="alerta.idGanado"
            class="p-4 bg-red-50 border border-red-200 rounded-lg"
          >
            <div class="flex justify-between items-start mb-2">
              <div class="font-medium text-red-800">{{ alerta.nombreGanado }}</div>
              <div class="text-sm text-red-600 font-semibold">{{ (alerta.puntuacionRiesgo * 100).toFixed(1) }}%</div>
            </div>
            <div v-if="alerta.alertas?.length > 0" class="text-sm space-y-1">
              <div v-for="alert in alerta.alertas" :key="alert" class="text-red-700">
                • {{ alert }}
              </div>
            </div>
            <div class="text-xs text-red-500 mt-2">
              Detectado: {{ formatearFecha(alerta.fechaDeteccion) }}
            </div>
          </div>
        </div>
      </template>
    </BaseModal>

    <!-- Sección de Reportes -->
    <GeneradorReportes
      :metricas="resumenSalud"
      :anomalias="anomaliasCompletas"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { analisisSaludApi, ganadoApi } from '@/services/api.js';
import { useAuth } from '@/composables/useAuth.js';
import BaseModal from '@/components/base/BaseModal.vue';
import GraficosSalud from '@/components/charts/GraficosSalud.vue';
import GeneradorReportes from '@/components/reportes/GeneradorReportes.vue';

const { token } = useAuth();

// Estado reactivo
const modeloEntrenado = ref(false);
const entrenandoModelo = ref(false);
const resumenSalud = ref(null);
const alertasUrgentes = ref(null);
const anomaliasCompletas = ref([]);
const ganado = ref([]);
const animalSeleccionado = ref('');
const analisisIndividual = ref(null);
const analizandoAnimal = ref(false);
const verTodasAlertas = ref(false);

// Métodos
async function cargarDatos() {
  try {
    // Cargar ganado para selector
    ganado.value = await ganadoApi.list(token.value);

    // Cargar resumen de salud
    await cargarResumenSalud();

    // Cargar alertas urgentes
    await cargarAlertasUrgentes();

    // Cargar anomalías completas para gráficos
    await cargarAnomalias();

    modeloEntrenado.value = true; // Asumir que existe si los datos cargan
  } catch (error) {
    console.warn('Modelo posiblemente no entrenado:', error.message);
    modeloEntrenado.value = false;
  }
}

async function cargarResumenSalud() {
  try {
    resumenSalud.value = await analisisSaludApi.obtenerResumenSalud(token.value);
  } catch (error) {
    console.error('Error cargando resumen:', error);
  }
}

async function cargarAlertasUrgentes() {
  try {
    alertasUrgentes.value = await analisisSaludApi.obtenerAlertasUrgentes(token.value);
  } catch (error) {
    console.error('Error cargando alertas:', error);
  }
}

async function cargarAnomalias() {
  try {
    const resultado = await analisisSaludApi.detectarAnomalias(token.value);
    anomaliasCompletas.value = resultado.anomalias || [];
  } catch (error) {
    console.error('Error cargando anomalías:', error);
  }
}

async function entrenarModelo() {
  entrenandoModelo.value = true;
  try {
    await analisisSaludApi.entrenarModelo(token.value);
    modeloEntrenado.value = true;
    // Recargar datos después del entrenamiento
    await cargarDatos();
  } catch (error) {
    console.error('Error entrenando modelo:', error);
    alert('Error al entrenar el modelo: ' + error.message);
  } finally {
    entrenandoModelo.value = false;
  }
}

async function analizarAnimalSeleccionado() {
  if (!animalSeleccionado.value) return;

  analizandoAnimal.value = true;
  try {
    analisisIndividual.value = await analisisSaludApi.analizarAnimal(animalSeleccionado.value, token.value);
  } catch (error) {
    console.error('Error analizando animal:', error);
    alert('Error al analizar el animal: ' + error.message);
  } finally {
    analizandoAnimal.value = false;
  }
}

// Utilidades
function getEstadoColor(estado) {
  const colores = {
    'Excelente': 'text-green-600',
    'Bueno': 'text-blue-600',
    'Regular': 'text-yellow-600',
    'Crítico': 'text-red-600'
  };
  return colores[estado] || 'text-gray-600';
}

function getRiesgoColor(riesgo) {
  const colores = {
    'Alto': 'text-red-600 font-semibold',
    'Medio': 'text-yellow-600 font-semibold',
    'Normal': 'text-green-600 font-semibold'
  };
  return colores[riesgo] || 'text-gray-600';
}

function formatearFecha(fecha) {
  return new Date(fecha).toLocaleDateString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

// Inicialización
onMounted(() => {
  cargarDatos();
});
</script>

<style scoped>
.grid {
  display: grid;
}

.grid-cols-1 {
  grid-template-columns: repeat(1, minmax(0, 1fr));
}

@media (min-width: 1024px) {
  .lg\:grid-cols-3 {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (min-width: 768px) {
  .md\:grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

.gap-6 {
  gap: 1.5rem;
}

.gap-4 {
  gap: 1rem;
}

.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>