<template>
  <div class="graficos-salud">
    <!-- Gráfico de Distribución de Riesgo -->
    <div class="card mb-6">
      <div class="card-header">
        <h3 class="card-title">Distribución de Riesgo</h3>
        <p class="text-sm text-muted">Clasificación de animales por nivel de riesgo</p>
      </div>
      <div class="card-body">
        <GraficoBase
          type="doughnut"
          :data="datosDoughnut"
          :options="opcionesDoughnut"
          :height="300"
        />
      </div>
    </div>

    <!-- Gráfico de Tendencia de Anomalías -->
    <div class="card mb-6">
      <div class="card-header">
        <h3 class="card-title">Tendencia de Detección</h3>
        <p class="text-sm text-muted">Evolución de anomalías detectadas en el tiempo</p>
      </div>
      <div class="card-body">
        <GraficoBase
          type="line"
          :data="datosLinea"
          :options="opcionesLinea"
          :height="350"
        />
      </div>
    </div>

    <!-- Gráfico de Comparación por Finca -->
    <div class="card mb-6">
      <div class="card-header">
        <h3 class="card-title">Salud por Finca</h3>
        <p class="text-sm text-muted">Comparación de anomalías entre fincas</p>
      </div>
      <div class="card-body">
        <GraficoBase
          type="bar"
          :data="datosBarra"
          :options="opcionesBarra"
          :height="350"
        />
      </div>
    </div>

    <!-- Gráfico de Puntuaciones de Anomalía -->
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">Distribución de Puntuaciones</h3>
        <p class="text-sm text-muted">Histograma de puntuaciones de anomalía</p>
      </div>
      <div class="card-body">
        <GraficoBase
          type="bar"
          :data="datosHistograma"
          :options="opcionesHistograma"
          :height="300"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import GraficoBase from './GraficoBase.vue';

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

// Datos para gráfico doughnut de distribución de riesgo
const datosDoughnut = computed(() => {
  const metricas = props.metricas;
  if (!metricas) return { labels: [], datasets: [] };

  const riesgoAlto = metricas.animalesRiesgoAlto || 0;
  const riesgoMedio = metricas.animalesRiesgoMedio || 0;
  const animalesNormales = (metricas.totalAnimales || 0) - (metricas.animalesConAnomalias || 0);
  const riesgoBajo = (metricas.animalesConAnomalias || 0) - riesgoAlto - riesgoMedio;

  return {
    labels: ['Normal', 'Riesgo Bajo', 'Riesgo Medio', 'Riesgo Alto'],
    datasets: [{
      data: [animalesNormales, Math.max(0, riesgoBajo), riesgoMedio, riesgoAlto],
      backgroundColor: [
        '#10B981', // Verde para normal
        '#F59E0B', // Amarillo para riesgo bajo
        '#F97316', // Naranja para riesgo medio
        '#EF4444'  // Rojo para riesgo alto
      ],
      borderWidth: 2,
      borderColor: '#ffffff'
    }]
  };
});

const opcionesDoughnut = {
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

// Datos simulados para tendencia (esto se podría obtener del backend)
const datosLinea = computed(() => {
  // Simular datos de tendencia de los últimos 30 días
  const fechas = [];
  const anomalias = [];
  const total = [];

  for (let i = 29; i >= 0; i--) {
    const fecha = new Date();
    fecha.setDate(fecha.getDate() - i);
    fechas.push(fecha.toLocaleDateString('es-ES', { month: 'short', day: 'numeric' }));

    // Simular datos
    anomalias.push(Math.floor(Math.random() * 5) + 1);
    total.push(Math.floor(Math.random() * 20) + 80);
  }

  return {
    labels: fechas,
    datasets: [
      {
        label: 'Anomalías Detectadas',
        data: anomalias,
        borderColor: '#EF4444',
        backgroundColor: 'rgba(239, 68, 68, 0.1)',
        fill: true,
        tension: 0.4
      },
      {
        label: 'Total Analizado',
        data: total,
        borderColor: '#3B82F6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        fill: false,
        tension: 0.4
      }
    ]
  };
});

const opcionesLinea = {
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
        text: 'Cantidad de Animales'
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

// Datos para gráfico de barras por finca (simulado)
const datosBarra = computed(() => {
  // Esto se podría obtener agrupando los datos reales por finca
  const fincas = ['Finca La Esperanza', 'Finca El Rosal', 'Finca San José', 'Finca Vista Hermosa'];
  const normales = [45, 32, 28, 35];
  const anomalias = [5, 8, 3, 7];

  return {
    labels: fincas,
    datasets: [
      {
        label: 'Animales Normales',
        data: normales,
        backgroundColor: '#10B981',
        borderRadius: 4
      },
      {
        label: 'Anomalías Detectadas',
        data: anomalias,
        backgroundColor: '#EF4444',
        borderRadius: 4
      }
    ]
  };
});

const opcionesBarra = {
  scales: {
    x: {
      title: {
        display: true,
        text: 'Fincas'
      }
    },
    y: {
      title: {
        display: true,
        text: 'Número de Animales'
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

// Histograma de puntuaciones de anomalía
const datosHistograma = computed(() => {
  if (!props.anomalias || props.anomalias.length === 0) {
    return {
      labels: ['0.0-0.2', '0.2-0.4', '0.4-0.6', '0.6-0.8', '0.8-1.0'],
      datasets: [{
        label: 'Número de Animales',
        data: [0, 0, 0, 0, 0],
        backgroundColor: '#6366F1',
        borderRadius: 4
      }]
    };
  }

  // Agrupar puntuaciones en rangos
  const rangos = [0, 0, 0, 0, 0]; // 0.0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1.0

  props.anomalias.forEach(anomalia => {
    const puntuacion = anomalia.puntuacionAnomalia || 0;
    const indice = Math.min(Math.floor(puntuacion * 5), 4);
    rangos[indice]++;
  });

  return {
    labels: ['0.0-0.2', '0.2-0.4', '0.4-0.6', '0.6-0.8', '0.8-1.0'],
    datasets: [{
      label: 'Número de Animales',
      data: rangos,
      backgroundColor: [
        '#10B981', // Verde para puntuaciones bajas
        '#F59E0B', // Amarillo
        '#F97316', // Naranja
        '#EF4444', // Rojo
        '#7C2D12'  // Rojo oscuro para puntuaciones altas
      ],
      borderRadius: 4
    }]
  };
});

const opcionesHistograma = {
  scales: {
    x: {
      title: {
        display: true,
        text: 'Rango de Puntuación de Anomalía'
      }
    },
    y: {
      title: {
        display: true,
        text: 'Número de Animales'
      },
      beginAtZero: true
    }
  },
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      callbacks: {
        title: function(context) {
          return `Rango: ${context[0].label}`;
        },
        label: function(context) {
          return `Animales: ${context.parsed.y}`;
        }
      }
    }
  }
};
</script>

<style scoped>
.graficos-salud {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
</style>