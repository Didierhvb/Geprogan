<template>
  <div class="page">
    <div class="page-header">
      <h1>Análisis de Producción Lechera con IA</h1>
      <p class="text-muted">Optimización inteligente de la producción y calidad lechera</p>
    </div>

    <!-- Dashboard Principal -->
    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 mb-8">
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
            Modelos Entrenados
          </div>
          <div v-else class="flex items-center text-yellow-600 mb-2">
            <svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
            </svg>
            Modelos no entrenados
          </div>
          <button
            @click="entrenarModelos"
            :disabled="entrenandoModelos"
            class="btn btn-primary w-full"
          >
            <span v-if="entrenandoModelos" class="flex items-center">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              Entrenando...
            </span>
            <span v-else>{{ modeloEntrenado ? 'Reentrenar Modelos' : 'Entrenar Modelos' }}</span>
          </button>
        </div>
      </div>

      <!-- Resumen de Producción -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Producción Diaria</h3>
        </div>
        <div class="card-body" v-if="dashboard?.resumenGeneral">
          <div class="text-center mb-4">
            <div class="text-3xl font-bold mb-1 text-blue-600">
              {{ dashboard.resumenGeneral.produccionTotalDiaria }}L
            </div>
            <div class="text-sm text-muted">Total del Hato</div>
          </div>
          <div class="space-y-2">
            <div class="flex justify-between">
              <span>Promedio/Vaca:</span>
              <strong>{{ dashboard.resumenGeneral.produccionPromedioPorVaca }}L</strong>
            </div>
            <div class="flex justify-between">
              <span>Total Vacas:</span>
              <strong>{{ dashboard.resumenGeneral.totalVacasLecheras }}</strong>
            </div>
            <div class="flex justify-between">
              <span>Estado:</span>
              <strong :class="getEstadoColor(dashboard.resumenGeneral.estadoGeneral)">
                {{ dashboard.resumenGeneral.estadoGeneral }}
              </strong>
            </div>
          </div>
        </div>
      </div>

      <!-- Calidad de Leche -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Calidad de Leche</h3>
        </div>
        <div class="card-body" v-if="dashboard?.distribucionCalidad">
          <div class="text-center mb-4">
            <div class="text-3xl font-bold mb-1 text-green-600">
              {{ dashboard.resumenGeneral.porcentajeCalidadAlta }}%
            </div>
            <div class="text-sm text-muted">Calidad Alta</div>
          </div>
          <div class="space-y-2">
            <div class="flex justify-between">
              <span>Alta:</span>
              <strong class="text-green-600">{{ dashboard.distribucionCalidad.alta }}</strong>
            </div>
            <div class="flex justify-between">
              <span>Media:</span>
              <strong class="text-yellow-600">{{ dashboard.distribucionCalidad.media }}</strong>
            </div>
            <div class="flex justify-between">
              <span>Baja:</span>
              <strong class="text-red-600">{{ dashboard.distribucionCalidad.baja }}</strong>
            </div>
          </div>
        </div>
      </div>

      <!-- Tendencia Reciente -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Tendencia (7 días)</h3>
        </div>
        <div class="card-body" v-if="dashboard?.tendenciaReciente">
          <div class="text-center mb-4">
            <div class="text-3xl font-bold mb-1" :class="getTendenciaColor(dashboard.tendenciaReciente.tendencia)">
              {{ dashboard.tendenciaReciente.tendencia }}
            </div>
            <div class="text-sm text-muted">
              {{ dashboard.tendenciaReciente.variacionPorcentual > 0 ? '+' : '' }}{{ dashboard.tendenciaReciente.variacionPorcentual.toFixed(1) }}%
            </div>
          </div>
          <div class="text-center">
            <div class="text-sm text-muted mb-2">Últimos 7 días</div>
            <div class="flex justify-center space-x-1">
              <div
                v-for="(dia, index) in dashboard.tendenciaReciente.datos"
                :key="index"
                class="w-2 h-8 bg-gray-200 rounded"
                :style="{ height: getBarHeight(dia.produccionReal, dashboard.tendenciaReciente.datos) }"
                :class="index === dashboard.tendenciaReciente.datos.length - 1 ? 'bg-blue-500' : 'bg-gray-400'"
              ></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Gráficos de Producción -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
      <!-- Gráfico de Distribución de Calidad -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Distribución de Calidad</h3>
        </div>
        <div class="card-body">
          <GraficoBase
            v-if="dashboard?.distribucionCalidad"
            type="doughnut"
            :data="datosGraficoCalidad"
            :options="opcionesGraficoCalidad"
            :height="300"
          />
        </div>
      </div>

      <!-- Gráfico de Tendencias -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Tendencia de Producción</h3>
        </div>
        <div class="card-body">
          <GraficoBase
            v-if="tendencias && tendencias.length > 0"
            type="line"
            :data="datosGraficoTendencias"
            :options="opcionesGraficoTendencias"
            :height="300"
          />
        </div>
      </div>
    </div>

    <!-- Ranking de Productoras -->
    <div class="card mb-8">
      <div class="card-header">
        <h3 class="card-title">Top Productoras</h3>
        <button @click="cargarRanking" class="btn btn-outline btn-sm">Actualizar</button>
      </div>
      <div class="card-body">
        <div v-if="ranking?.ranking" class="overflow-x-auto">
          <table class="min-w-full table">
            <thead>
              <tr>
                <th>Pos.</th>
                <th>Vaca</th>
                <th>Producción</th>
                <th>Categoría</th>
                <th>Eficiencia</th>
                <th>Potencial</th>
                <th>Insignia</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="vaca in ranking.ranking.slice(0, 10)" :key="vaca.idGanado">
                <td class="font-bold">{{ vaca.posicion }}</td>
                <td>{{ vaca.nombreGanado }}</td>
                <td class="font-semibold">{{ vaca.produccionActual }}L</td>
                <td>
                  <span :class="getCategoriaColor(vaca.categoria)">{{ vaca.categoria }}</span>
                </td>
                <td>{{ vaca.eficiencia }}%</td>
                <td class="text-green-600">+{{ vaca.potencialMejora }}L</td>
                <td>{{ vaca.insignia }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Optimizaciones Prioritarias -->
    <div class="card mb-8">
      <div class="card-header">
        <h3 class="card-title">Optimizaciones Prioritarias</h3>
        <p class="text-sm text-muted">Vacas con mayor potencial de mejora</p>
      </div>
      <div class="card-body">
        <div v-if="dashboard?.optimizacionesPrioritarias" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="optimizacion in dashboard.optimizacionesPrioritarias"
            :key="optimizacion.idGanado"
            class="p-4 bg-yellow-50 border border-yellow-200 rounded-lg"
          >
            <div class="font-medium text-yellow-800 mb-2">{{ optimizacion.nombreGanado }}</div>
            <div class="text-sm space-y-1">
              <div class="flex justify-between">
                <span>Actual:</span>
                <strong>{{ optimizacion.produccionActual }}L</strong>
              </div>
              <div class="flex justify-between">
                <span>Potencial:</span>
                <strong class="text-green-600">+{{ optimizacion.potencialMejora }}L</strong>
              </div>
              <div class="flex justify-between">
                <span>Mejora:</span>
                <strong>{{ optimizacion.porcentajeMejora }}%</strong>
              </div>
              <div class="mt-2 text-xs text-yellow-700">
                <strong>Acción:</strong> {{ optimizacion.accionPrincipal }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Análisis Individual -->
    <div class="card mb-8">
      <div class="card-header">
        <h3 class="card-title">Análisis Individual de Vaca</h3>
      </div>
      <div class="card-body">
        <div class="flex gap-4 mb-4">
          <select v-model="vacaSeleccionada" class="form-input flex-1">
            <option value="">Seleccionar vaca para análisis</option>
            <option v-for="vaca in vacasLecheras" :key="vaca.idganado" :value="vaca.idganado">
              {{ vaca.nombreGanado || `Vaca #${vaca.idganado}` }} - {{ vaca.raza }}
            </option>
          </select>
          <button
            @click="analizarVacaSeleccionada"
            :disabled="!vacaSeleccionada || analizandoVaca"
            class="btn btn-primary"
          >
            <span v-if="analizandoVaca">Analizando...</span>
            <span v-else>Analizar Producción</span>
          </button>
        </div>

        <div v-if="analisisVaca" class="mt-6 p-4 border rounded-lg">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h4 class="font-semibold mb-3">Análisis de Producción</h4>
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span>Producción Actual:</span>
                  <span class="font-semibold">{{ analisisVaca.produccionActual.toFixed(1) }}L</span>
                </div>
                <div class="flex justify-between">
                  <span>Producción Predicha:</span>
                  <span class="font-semibold">{{ analisisVaca.produccionPredicha.toFixed(1) }}L</span>
                </div>
                <div class="flex justify-between">
                  <span>Diferencia:</span>
                  <span :class="analisisVaca.diferenciaPorcentual >= 0 ? 'text-green-600' : 'text-red-600'">
                    {{ analisisVaca.diferenciaPorcentual > 0 ? '+' : '' }}{{ analisisVaca.diferenciaPorcentual.toFixed(1) }}%
                  </span>
                </div>
                <div class="flex justify-between">
                  <span>Tendencia:</span>
                  <span :class="getTendenciaColor(analisisVaca.tendenciaProduccion)">
                    {{ analisisVaca.tendenciaProduccion }}
                  </span>
                </div>
                <div class="flex justify-between">
                  <span>Calidad:</span>
                  <span :class="getCalidadColor(analisisVaca.clasificacionCalidad)">
                    {{ analisisVaca.clasificacionCalidad }}
                  </span>
                </div>
              </div>
            </div>
            <div>
              <h4 class="font-semibold mb-3">Detalles del Animal</h4>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span>Promedio 7 días:</span>
                  <span>{{ analisisVaca.detalles.produccionPromedio7Dias.toFixed(1) }}L</span>
                </div>
                <div class="flex justify-between">
                  <span>Promedio 30 días:</span>
                  <span>{{ analisisVaca.detalles.produccionPromedio30Dias.toFixed(1) }}L</span>
                </div>
                <div class="flex justify-between">
                  <span>Días en lactancia:</span>
                  <span>{{ analisisVaca.detalles.diasEnLactancia }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Número de partos:</span>
                  <span>{{ analisisVaca.detalles.numeroPartos }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Condición corporal:</span>
                  <span>{{ analisisVaca.detalles.condicionCorporal }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Temporada:</span>
                  <span>{{ analisisVaca.detalles.temporadaActual }}</span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="analisisVaca.recomendaciones?.length > 0" class="mt-4">
            <h4 class="font-semibold mb-2">Recomendaciones:</h4>
            <ul class="list-disc list-inside space-y-1 text-sm">
              <li v-for="recomendacion in analisisVaca.recomendaciones" :key="recomendacion" class="text-blue-600">
                {{ recomendacion }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- Predicción Semanal -->
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">Predicción Semanal</h3>
        <button @click="cargarPrediccionSemanal" class="btn btn-outline btn-sm">Actualizar</button>
      </div>
      <div class="card-body">
        <div v-if="prediccionSemanal?.predicciones" class="grid grid-cols-1 md:grid-cols-7 gap-4">
          <div
            v-for="prediccion in prediccionSemanal.predicciones"
            :key="prediccion.fecha"
            class="p-3 bg-blue-50 border border-blue-200 rounded-lg text-center"
          >
            <div class="font-medium text-blue-800 text-sm">{{ prediccion.diaSemana }}</div>
            <div class="text-lg font-bold text-blue-600">{{ prediccion.produccionPredicha }}L</div>
            <div class="text-xs text-blue-500">{{ prediccion.confianza }}</div>
            <div class="text-xs text-muted">{{ prediccion.fecha.split('/').slice(0, 2).join('/') }}</div>
          </div>
        </div>
        <div v-if="prediccionSemanal" class="mt-4 text-center">
          <div class="text-lg font-semibold">
            Total Semanal Predicho: <span class="text-green-600">{{ prediccionSemanal.produccionTotalSemana }}L</span>
          </div>
          <div class="text-sm text-muted">
            Promedio Diario: {{ prediccionSemanal.promedioDiario }}L
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { analisisLecheroApi, ganadoApi } from '@/services/api.js';
import { useAuth } from '@/composables/useAuth.js';
import GraficoBase from '@/components/charts/GraficoBase.vue';

const { token } = useAuth();

// Estado reactivo
const modeloEntrenado = ref(false);
const entrenandoModelos = ref(false);
const dashboard = ref(null);
const tendencias = ref([]);
const ranking = ref(null);
const prediccionSemanal = ref(null);
const vacasLecheras = ref([]);
const vacaSeleccionada = ref('');
const analisisVaca = ref(null);
const analizandoVaca = ref(false);

// Datos para gráficos
const datosGraficoCalidad = computed(() => {
  if (!dashboard.value?.distribucionCalidad) return { labels: [], datasets: [] };

  const dist = dashboard.value.distribucionCalidad;
  return {
    labels: ['Calidad Alta', 'Calidad Media', 'Calidad Baja'],
    datasets: [{
      data: [dist.alta, dist.media, dist.baja],
      backgroundColor: ['#10B981', '#F59E0B', '#EF4444'],
      borderWidth: 2,
      borderColor: '#ffffff'
    }]
  };
});

const opcionesGraficoCalidad = {
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        padding: 20,
        usePointStyle: true
      }
    },
    tooltip: {
      callbacks: {
        label: function(context) {
          const total = context.dataset.data.reduce((a, b) => a + b, 0);
          const percentage = ((context.parsed * 100) / total).toFixed(1);
          return `${context.label}: ${context.parsed} (${percentage}%)`;
        }
      }
    }
  }
};

const datosGraficoTendencias = computed(() => {
  if (!tendencias.value || tendencias.value.length === 0) return { labels: [], datasets: [] };

  return {
    labels: tendencias.value.map(t => new Date(t.fecha).toLocaleDateString('es-ES', { month: 'short', day: 'numeric' })),
    datasets: [
      {
        label: 'Producción Real',
        data: tendencias.value.map(t => t.produccionReal),
        borderColor: '#3B82F6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        fill: true,
        tension: 0.4
      },
      {
        label: 'Producción Predicha',
        data: tendencias.value.map(t => t.produccionPredicha),
        borderColor: '#10B981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: false,
        tension: 0.4,
        borderDash: [5, 5]
      }
    ]
  };
});

const opcionesGraficoTendencias = {
  scales: {
    x: {
      title: {
        display: true,
        text: 'Fecha'
      }
    },
    y: {
      title: {
        display: true,
        text: 'Producción (Litros)'
      },
      beginAtZero: true
    }
  },
  plugins: {
    legend: {
      position: 'top'
    }
  }
};

// Métodos
async function cargarDatos() {
  try {
    // Cargar dashboard principal
    dashboard.value = await analisisLecheroApi.obtenerDashboard(token.value);

    // Cargar tendencias
    const tendenciasData = await analisisLecheroApi.obtenerTendencias(14, token.value);
    tendencias.value = tendenciasData.tendencias || [];

    // Cargar vacas lecheras para selector
    const todasVacas = await ganadoApi.list(token.value);
    vacasLecheras.value = todasVacas.filter(v => v.sexo === 'Hembra');

    modeloEntrenado.value = true;
  } catch (error) {
    console.warn('Modelos posiblemente no entrenados:', error.message);
    modeloEntrenado.value = false;
  }
}

async function entrenarModelos() {
  entrenandoModelos.value = true;
  try {
    await analisisLecheroApi.entrenarModelos(token.value);
    modeloEntrenado.value = true;
    await cargarDatos();
  } catch (error) {
    console.error('Error entrenando modelos:', error);
    alert('Error al entrenar los modelos: ' + error.message);
  } finally {
    entrenandoModelos.value = false;
  }
}

async function cargarRanking() {
  try {
    ranking.value = await analisisLecheroApi.obtenerRankingProductoras(20, token.value);
  } catch (error) {
    console.error('Error cargando ranking:', error);
  }
}

async function cargarPrediccionSemanal() {
  try {
    prediccionSemanal.value = await analisisLecheroApi.obtenerPrediccionSemanal(token.value);
  } catch (error) {
    console.error('Error cargando predicción semanal:', error);
  }
}

async function analizarVacaSeleccionada() {
  if (!vacaSeleccionada.value) return;

  analizandoVaca.value = true;
  try {
    analisisVaca.value = await analisisLecheroApi.analizarVaca(vacaSeleccionada.value, token.value);
  } catch (error) {
    console.error('Error analizando vaca:', error);
    alert('Error al analizar la vaca: ' + error.message);
  } finally {
    analizandoVaca.value = false;
  }
}

// Utilidades
function getEstadoColor(estado) {
  const colores = {
    'Excelente': 'text-green-600',
    'Bueno': 'text-blue-600',
    'Regular': 'text-yellow-600',
    'Bajo': 'text-red-600'
  };
  return colores[estado] || 'text-gray-600';
}

function getTendenciaColor(tendencia) {
  const colores = {
    'Creciente': 'text-green-600',
    'Estable': 'text-blue-600',
    'Decreciente': 'text-red-600'
  };
  return colores[tendencia] || 'text-gray-600';
}

function getCategoriaColor(categoria) {
  const colores = {
    'Elite': 'text-purple-600 font-bold',
    'Alta': 'text-green-600 font-semibold',
    'Media': 'text-blue-600',
    'Básica': 'text-yellow-600',
    'Baja': 'text-red-600'
  };
  return colores[categoria] || 'text-gray-600';
}

function getCalidadColor(calidad) {
  const colores = {
    'Alta': 'text-green-600 font-semibold',
    'Media': 'text-yellow-600',
    'Baja': 'text-red-600'
  };
  return colores[calidad] || 'text-gray-600';
}

function getBarHeight(valor, datos) {
  const max = Math.max(...datos.map(d => d.produccionReal));
  const min = Math.min(...datos.map(d => d.produccionReal));
  const range = max - min;
  if (range === 0) return '32px';
  const percentage = ((valor - min) / range) * 100;
  return `${Math.max(8, (percentage / 100) * 32)}px`;
}

// Inicialización
onMounted(async () => {
  await cargarDatos();
  await cargarRanking();
  await cargarPrediccionSemanal();
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
  .lg\:grid-cols-4 {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
  .lg\:grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
  .lg\:grid-cols-3 {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (min-width: 768px) {
  .md\:grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
  .md\:grid-cols-7 {
    grid-template-columns: repeat(7, minmax(0, 1fr));
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