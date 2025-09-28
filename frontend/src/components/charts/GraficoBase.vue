<template>
  <div class="chart-container">
    <canvas :id="chartId" :ref="chartId"></canvas>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  RadialLinearScale
} from 'chart.js';

// Registrar componentes de Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  RadialLinearScale
);

const props = defineProps({
  type: {
    type: String,
    required: true,
    validator: value => ['line', 'bar', 'pie', 'doughnut', 'radar'].includes(value)
  },
  data: {
    type: Object,
    required: true
  },
  options: {
    type: Object,
    default: () => ({})
  },
  height: {
    type: Number,
    default: 400
  }
});

const chartId = `chart-${Math.random().toString(36).substr(2, 9)}`;
const chart = ref(null);

const defaultOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
    },
    tooltip: {
      mode: 'index',
      intersect: false,
    }
  },
  scales: {
    x: {
      display: true,
      title: {
        display: false
      }
    },
    y: {
      display: true,
      title: {
        display: false
      }
    }
  },
  interaction: {
    mode: 'nearest',
    axis: 'x',
    intersect: false
  }
};

const mergedOptions = computed(() => {
  const base = { ...defaultOptions };

  // Para gráficos circulares, no necesitamos escalas
  if (props.type === 'pie' || props.type === 'doughnut') {
    delete base.scales;
  }

  // Merge con opciones personalizadas
  return mergeDeep(base, props.options);
});

function mergeDeep(target, source) {
  const output = Object.assign({}, target);
  if (isObject(target) && isObject(source)) {
    Object.keys(source).forEach(key => {
      if (isObject(source[key])) {
        if (!(key in target))
          Object.assign(output, { [key]: source[key] });
        else
          output[key] = mergeDeep(target[key], source[key]);
      } else {
        Object.assign(output, { [key]: source[key] });
      }
    });
  }
  return output;
}

function isObject(item) {
  return item && typeof item === 'object' && !Array.isArray(item);
}

function createChart() {
  const ctx = document.getElementById(chartId);
  if (!ctx) return;

  chart.value = new ChartJS(ctx, {
    type: props.type,
    data: props.data,
    options: mergedOptions.value
  });
}

function updateChart() {
  if (chart.value) {
    chart.value.data = props.data;
    chart.value.options = mergedOptions.value;
    chart.value.update();
  }
}

function destroyChart() {
  if (chart.value) {
    chart.value.destroy();
    chart.value = null;
  }
}

// Watchers
watch(() => props.data, () => {
  updateChart();
}, { deep: true });

watch(() => props.options, () => {
  updateChart();
}, { deep: true });

// Lifecycle
onMounted(async () => {
  await nextTick();
  createChart();
});

onUnmounted(() => {
  destroyChart();
});
</script>

<script>
import { computed } from 'vue';
export default {
  name: 'GraficoBase'
};
</script>

<style scoped>
.chart-container {
  position: relative;
  height: v-bind(height + 'px');
  width: 100%;
}
</style>