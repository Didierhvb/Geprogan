<template>
  <div class="generador-reportes">
    <!-- Controles de Reporte -->
    <div class="card mb-6">
      <div class="card-header">
        <h3 class="card-title">Generar Reporte de Salud</h3>
        <p class="text-sm text-muted">Configure y exporte reportes de análisis de salud</p>
      </div>
      <div class="card-body">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <!-- Tipo de Reporte -->
          <div>
            <label class="form-label">Tipo de Reporte</label>
            <select v-model="configuracion.tipo" class="form-input">
              <option value="completo">Reporte Completo</option>
              <option value="anomalias">Solo Anomalías</option>
              <option value="resumen">Resumen Ejecutivo</option>
              <option value="tendencias">Análisis de Tendencias</option>
            </select>
          </div>

          <!-- Formato de Exportación -->
          <div>
            <label class="form-label">Formato</label>
            <select v-model="configuracion.formato" class="form-input">
              <option value="pdf">PDF</option>
              <option value="excel">Excel</option>
              <option value="csv">CSV</option>
            </select>
          </div>

          <!-- Período -->
          <div>
            <label class="form-label">Período</label>
            <select v-model="configuracion.periodo" class="form-input">
              <option value="actual">Estado Actual</option>
              <option value="semana">Última Semana</option>
              <option value="mes">Último Mes</option>
              <option value="trimestre">Último Trimestre</option>
            </select>
          </div>
        </div>

        <!-- Filtros Adicionales -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <!-- Filtro por Finca -->
          <div>
            <label class="form-label">Filtrar por Finca</label>
            <select v-model="configuracion.finca" class="form-input">
              <option value="">Todas las Fincas</option>
              <option v-for="finca in fincas" :key="finca.id" :value="finca.id">
                {{ finca.nombre }}
              </option>
            </select>
          </div>

          <!-- Nivel de Riesgo -->
          <div>
            <label class="form-label">Nivel de Riesgo Mínimo</label>
            <select v-model="configuracion.riesgoMinimo" class="form-input">
              <option value="">Todos los Niveles</option>
              <option value="Medio">Medio y Alto</option>
              <option value="Alto">Solo Alto</option>
            </select>
          </div>
        </div>

        <!-- Opciones Adicionales -->
        <div class="mb-6">
          <label class="form-label">Incluir en el Reporte</label>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-2">
            <label class="flex items-center">
              <input v-model="configuracion.incluirGraficos" type="checkbox" class="form-checkbox mr-2">
              Gráficos
            </label>
            <label class="flex items-center">
              <input v-model="configuracion.incluirRecomendaciones" type="checkbox" class="form-checkbox mr-2">
              Recomendaciones
            </label>
            <label class="flex items-center">
              <input v-model="configuracion.incluirDetalles" type="checkbox" class="form-checkbox mr-2">
              Detalles por Animal
            </label>
            <label class="flex items-center">
              <input v-model="configuracion.incluirTendencias" type="checkbox" class="form-checkbox mr-2">
              Análisis de Tendencias
            </label>
          </div>
        </div>

        <!-- Botones de Acción -->
        <div class="flex gap-4">
          <button
            @click="previsualizarReporte"
            class="btn btn-outline"
            :disabled="generandoReporte"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
            </svg>
            Previsualizar
          </button>
          <button
            @click="generarReporte"
            class="btn btn-primary"
            :disabled="generandoReporte"
          >
            <span v-if="generandoReporte" class="flex items-center">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              Generando...
            </span>
            <span v-else class="flex items-center">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
              </svg>
              Generar Reporte
            </span>
          </button>
        </div>
      </div>
    </div>

    <!-- Previsualización del Reporte -->
    <div v-if="previsualizacion" class="card">
      <div class="card-header">
        <h3 class="card-title">Previsualización del Reporte</h3>
        <div class="flex gap-2">
          <button @click="exportarReporte('pdf')" class="btn btn-sm btn-outline">
            Exportar PDF
          </button>
          <button @click="exportarReporte('excel')" class="btn btn-sm btn-outline">
            Exportar Excel
          </button>
        </div>
      </div>
      <div class="card-body">
        <div class="reporte-preview">
          <!-- Encabezado del Reporte -->
          <div class="reporte-header mb-6">
            <h1 class="text-2xl font-bold text-center mb-2">
              {{ obtenerTituloReporte() }}
            </h1>
            <div class="text-center text-muted mb-4">
              <p>Geprogan - Sistema de Gestión Ganadera</p>
              <p>Generado el: {{ formatearFecha(new Date()) }}</p>
              <p v-if="configuracion.periodo !== 'actual'">
                Período: {{ obtenerDescripcionPeriodo() }}
              </p>
            </div>
          </div>

          <!-- Resumen Ejecutivo -->
          <div v-if="previsualizacion.resumen" class="mb-6">
            <h2 class="text-xl font-semibold mb-4">Resumen Ejecutivo</h2>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
              <div class="stat-card">
                <div class="stat-number">{{ previsualizacion.resumen.totalAnimales }}</div>
                <div class="stat-label">Total Animales</div>
              </div>
              <div class="stat-card">
                <div class="stat-number text-red-600">{{ previsualizacion.resumen.animalesConAnomalias }}</div>
                <div class="stat-label">Anomalías Detectadas</div>
              </div>
              <div class="stat-card">
                <div class="stat-number">{{ previsualizacion.resumen.porcentajeAnomalias.toFixed(1) }}%</div>
                <div class="stat-label">% Anomalías</div>
              </div>
              <div class="stat-card">
                <div class="stat-number" :class="getEstadoColor(previsualizacion.resumen.estadoGeneral)">
                  {{ previsualizacion.resumen.estadoGeneral }}
                </div>
                <div class="stat-label">Estado General</div>
              </div>
            </div>
          </div>

          <!-- Distribución por Riesgo -->
          <div v-if="previsualizacion.distribucionRiesgo" class="mb-6">
            <h2 class="text-xl font-semibold mb-4">Distribución por Nivel de Riesgo</h2>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="risk-card risk-high">
                <div class="risk-number">{{ previsualizacion.distribucionRiesgo.alto }}</div>
                <div class="risk-label">Riesgo Alto</div>
              </div>
              <div class="risk-card risk-medium">
                <div class="risk-number">{{ previsualizacion.distribucionRiesgo.medio }}</div>
                <div class="risk-label">Riesgo Medio</div>
              </div>
              <div class="risk-card risk-low">
                <div class="risk-number">{{ previsualizacion.distribucionRiesgo.bajo }}</div>
                <div class="risk-label">Riesgo Bajo</div>
              </div>
            </div>
          </div>

          <!-- Anomalías Detectadas -->
          <div v-if="previsualizacion.anomalias && previsualizacion.anomalias.length > 0" class="mb-6">
            <h2 class="text-xl font-semibold mb-4">Anomalías Detectadas</h2>
            <div class="overflow-x-auto">
              <table class="min-w-full table">
                <thead>
                  <tr>
                    <th>Animal</th>
                    <th>Nivel de Riesgo</th>
                    <th>Puntuación</th>
                    <th>Alertas</th>
                    <th>Fecha Detección</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="anomalia in previsualizacion.anomalias.slice(0, 10)" :key="anomalia.idGanado">
                    <td>{{ anomalia.nombreGanado || `Animal #${anomalia.idGanado}` }}</td>
                    <td>
                      <span :class="getRiesgoColor(anomalia.nivelRiesgo)">
                        {{ anomalia.nivelRiesgo }}
                      </span>
                    </td>
                    <td>{{ (anomalia.puntuacionAnomalia * 100).toFixed(1) }}%</td>
                    <td>
                      <div class="text-sm">
                        <div v-for="alerta in anomalia.alertasSalud.slice(0, 2)" :key="alerta">
                          • {{ alerta }}
                        </div>
                        <div v-if="anomalia.alertasSalud.length > 2" class="text-muted">
                          +{{ anomalia.alertasSalud.length - 2 }} más...
                        </div>
                      </div>
                    </td>
                    <td>{{ formatearFecha(anomalia.fechaAnalisis) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Recomendaciones -->
          <div v-if="configuracion.incluirRecomendaciones && previsualizacion.recomendaciones" class="mb-6">
            <h2 class="text-xl font-semibold mb-4">Recomendaciones</h2>
            <ul class="list-disc list-inside space-y-2">
              <li v-for="recomendacion in previsualizacion.recomendaciones" :key="recomendacion">
                {{ recomendacion }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';

const props = defineProps({
  metricas: {
    type: Object,
    default: () => ({})
  },
  anomalias: {
    type: Array,
    default: () => []
  }
});

// Estado reactivo
const generandoReporte = ref(false);
const previsualizacion = ref(null);
const fincas = ref([
  { id: 1, nombre: 'Finca La Esperanza' },
  { id: 2, nombre: 'Finca El Rosal' },
  { id: 3, nombre: 'Finca San José' },
  { id: 4, nombre: 'Finca Vista Hermosa' }
]);

const configuracion = reactive({
  tipo: 'completo',
  formato: 'pdf',
  periodo: 'actual',
  finca: '',
  riesgoMinimo: '',
  incluirGraficos: true,
  incluirRecomendaciones: true,
  incluirDetalles: true,
  incluirTendencias: false
});

// Métodos
function obtenerTituloReporte() {
  const tipos = {
    completo: 'Reporte Completo de Análisis de Salud',
    anomalias: 'Reporte de Anomalías Detectadas',
    resumen: 'Resumen Ejecutivo de Salud del Hato',
    tendencias: 'Análisis de Tendencias de Salud'
  };
  return tipos[configuracion.tipo] || 'Reporte de Salud';
}

function obtenerDescripcionPeriodo() {
  const periodos = {
    semana: 'Última Semana',
    mes: 'Último Mes',
    trimestre: 'Último Trimestre'
  };
  return periodos[configuracion.periodo] || 'Período Actual';
}

function previsualizarReporte() {
  // Filtrar anomalías según configuración
  let anomaliasFiltradas = props.anomalias || [];

  if (configuracion.riesgoMinimo) {
    if (configuracion.riesgoMinimo === 'Alto') {
      anomaliasFiltradas = anomaliasFiltradas.filter(a => a.nivelRiesgo === 'Alto');
    } else if (configuracion.riesgoMinimo === 'Medio') {
      anomaliasFiltradas = anomaliasFiltradas.filter(a => ['Medio', 'Alto'].includes(a.nivelRiesgo));
    }
  }

  // Generar distribución de riesgo
  const distribucionRiesgo = {
    alto: anomaliasFiltradas.filter(a => a.nivelRiesgo === 'Alto').length,
    medio: anomaliasFiltradas.filter(a => a.nivelRiesgo === 'Medio').length,
    bajo: anomaliasFiltradas.filter(a => a.nivelRiesgo === 'Normal').length
  };

  // Generar recomendaciones
  const recomendaciones = generarRecomendaciones(props.metricas, anomaliasFiltradas);

  previsualizacion.value = {
    resumen: props.metricas,
    distribucionRiesgo,
    anomalias: anomaliasFiltradas,
    recomendaciones
  };
}

function generarRecomendaciones(metricas, anomalias) {
  const recomendaciones = [];

  if (!metricas) return recomendaciones;

  const porcentajeAnomalias = metricas.porcentajeAnomalias || 0;

  if (porcentajeAnomalias > 20) {
    recomendaciones.push('Se recomienda revisar las condiciones generales del hato inmediatamente');
    recomendaciones.push('Evaluar la alimentación y suplementación nutricional');
    recomendaciones.push('Realizar análisis veterinario completo del hato');
  } else if (porcentajeAnomalias > 10) {
    recomendaciones.push('Implementar monitoreo más frecuente de los animales');
    recomendaciones.push('Revisar protocolo de manejo sanitario');
  } else if (porcentajeAnomalias < 5) {
    recomendaciones.push('El hato presenta excelente estado de salud general');
    recomendaciones.push('Mantener las prácticas actuales de manejo');
  }

  const riesgoAlto = metricas.animalesRiesgoAlto || 0;
  if (riesgoAlto > 0) {
    recomendaciones.push(`Atención veterinaria inmediata para ${riesgoAlto} animal(es) de alto riesgo`);
  }

  const riesgoMedio = metricas.animalesRiesgoMedio || 0;
  if (riesgoMedio > 5) {
    recomendaciones.push('Implementar programa de monitoreo intensivo para animales de riesgo medio');
  }

  return recomendaciones;
}

async function generarReporte() {
  generandoReporte.value = true;
  try {
    // Simular generación de reporte
    await new Promise(resolve => setTimeout(resolve, 2000));

    if (!previsualizacion.value) {
      previsualizarReporte();
    }

    // Exportar según el formato seleccionado
    await exportarReporte(configuracion.formato);

  } catch (error) {
    console.error('Error generando reporte:', error);
    alert('Error al generar el reporte: ' + error.message);
  } finally {
    generandoReporte.value = false;
  }
}

async function exportarReporte(formato) {
  try {
    if (formato === 'pdf') {
      await exportarPDF();
    } else if (formato === 'excel') {
      await exportarExcel();
    } else if (formato === 'csv') {
      await exportarCSV();
    }
  } catch (error) {
    console.error('Error exportando reporte:', error);
    alert('Error al exportar el reporte: ' + error.message);
  }
}

async function exportarPDF() {
  // Implementación básica usando window.print
  const contenidoOriginal = document.body.innerHTML;
  const contenidoReporte = document.querySelector('.reporte-preview').innerHTML;

  document.body.innerHTML = `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      ${contenidoReporte}
    </div>
  `;

  window.print();
  document.body.innerHTML = contenidoOriginal;
  location.reload(); // Recargar para restaurar eventos
}

async function exportarExcel() {
  // Crear CSV básico para Excel
  let csv = 'Reporte de Análisis de Salud\n\n';

  if (previsualizacion.value?.resumen) {
    csv += 'Resumen Ejecutivo\n';
    csv += `Total Animales,${previsualizacion.value.resumen.totalAnimales}\n`;
    csv += `Anomalías Detectadas,${previsualizacion.value.resumen.animalesConAnomalias}\n`;
    csv += `Porcentaje Anomalías,${previsualizacion.value.resumen.porcentajeAnomalias.toFixed(1)}%\n`;
    csv += `Estado General,${previsualizacion.value.resumen.estadoGeneral}\n\n`;
  }

  if (previsualizacion.value?.anomalias) {
    csv += 'Anomalías Detectadas\n';
    csv += 'Animal,Nivel de Riesgo,Puntuación,Alertas,Fecha Detección\n';

    previsualizacion.value.anomalias.forEach(anomalia => {
      const alertas = anomalia.alertasSalud.join('; ');
      csv += `"${anomalia.nombreGanado || `Animal #${anomalia.idGanado}`}","${anomalia.nivelRiesgo}","${(anomalia.puntuacionAnomalia * 100).toFixed(1)}%","${alertas}","${formatearFecha(anomalia.fechaAnalisis)}"\n`;
    });
  }

  // Descargar archivo
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = `reporte_salud_${new Date().toISOString().split('T')[0]}.csv`;
  link.click();
}

async function exportarCSV() {
  await exportarExcel(); // Usar la misma lógica
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
</script>

<style scoped>
.stat-card {
  @apply bg-gray-50 p-4 rounded-lg text-center;
}

.stat-number {
  @apply text-2xl font-bold mb-1;
}

.stat-label {
  @apply text-sm text-muted;
}

.risk-card {
  @apply p-4 rounded-lg text-center text-white;
}

.risk-high {
  @apply bg-red-500;
}

.risk-medium {
  @apply bg-yellow-500;
}

.risk-low {
  @apply bg-green-500;
}

.risk-number {
  @apply text-2xl font-bold mb-1;
}

.risk-label {
  @apply text-sm;
}

.reporte-preview {
  @apply max-h-96 overflow-y-auto border rounded-lg p-4;
}

@media print {
  .reporte-preview {
    max-height: none;
    overflow: visible;
  }
}
</style>