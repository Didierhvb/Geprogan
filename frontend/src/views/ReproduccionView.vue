<template>
  <div class="reproduccion-container">
    <!-- Header Section -->
    <section class="surface-panel">
      <div class="header-section">
        <div class="header-content">
          <div class="stat-chip">Ciclo reproductivo</div>
          <h1 class="hero-title">Gestión Reproductiva</h1>
          <p class="hero-subtitle">
            Controla servicios, gestaciones, partos y seguimiento de crías. Optimiza la reproducción de tu ganado.
          </p>
        </div>
        <div class="header-stats">
          <div class="stat-item">
            <span class="stat-value">{{ hembrasGestantes }}</span>
            <span class="stat-label">Gestantes</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ proximosPartos }}</span>
            <span class="stat-label">Partos esperados</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ criasLactando }}</span>
            <span class="stat-label">En lactancia</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Quick Actions Tabs -->
    <section class="surface-panel">
      <div class="tabs-container">
        <div class="tabs-header">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            :class="['tab-button', { active: activeTab === tab.id }]"
            @click="activeTab = tab.id"
          >
            <component :is="tab.icon" class="tab-icon" />
            {{ tab.label }}
          </button>
        </div>

        <div class="tab-content">
          <!-- Servicio Tab -->
          <div v-if="activeTab === 'servicio'" class="tab-panel">
            <BaseForm
              title="Registro de Servicio"
              subtitle="Registra servicios naturales o inseminación artificial"
              layout="three"
              :loading="saving"
              :is-valid="isServicioFormValid"
              submit-text="Registrar Servicio"
              @submit="handleServicio"
            >
              <BaseFormField
                v-model="servicioForm.idhembra"
                label="Hembra"
                type="select"
                :options="hembrasOptions"
                :error="servicioErrors.idhembra"
                required
                help="Selecciona la hembra a servir"
              />

              <BaseFormField
                v-model="servicioForm.fecha"
                label="Fecha del Servicio"
                type="date"
                :max="today"
                :error="servicioErrors.fecha"
                required
              />

              <BaseFormField
                v-model="servicioForm.tipoServicio"
                label="Tipo de Servicio"
                type="select"
                :options="tipoServicioOptions"
                :error="servicioErrors.tipoServicio"
                required
              />

              <BaseFormField
                v-model="servicioForm.macho"
                label="Macho/Semen"
                help="Identificación del macho o código de semen"
              />

              <BaseFormField
                v-model="servicioForm.observaciones"
                label="Observaciones"
                type="textarea"
                :rows="2"
                help="Notas sobre el servicio"
              />

              <BaseFormField
                v-model="servicioForm.fechaPalpacion"
                label="Fecha Palpación Estimada"
                type="date"
                :min="getMinPalpacionDate(servicioForm.fecha)"
                help="Fecha estimada para palpación (60 días después)"
              />
            </BaseForm>
          </div>

          <!-- Palpación Tab -->
          <div v-if="activeTab === 'palpacion'" class="tab-panel">
            <BaseForm
              title="Registro de Palpación"
              subtitle="Confirma gestaciones y fechas estimadas de parto"
              layout="three"
              :loading="saving"
              :is-valid="isPalpacionFormValid"
              submit-text="Registrar Palpación"
              @submit="handlePalpacion"
            >
              <BaseFormField
                v-model="palpacionForm.idhembra"
                label="Hembra"
                type="select"
                :options="hembrasServicioOptions"
                :error="palpacionErrors.idhembra"
                required
                help="Hembra con servicio registrado"
              />

              <BaseFormField
                v-model="palpacionForm.fecha"
                label="Fecha de Palpación"
                type="date"
                :max="today"
                :error="palpacionErrors.fecha"
                required
              />

              <BaseFormField
                v-model="palpacionForm.resultado"
                label="Resultado"
                type="select"
                :options="resultadoPalpacionOptions"
                :error="palpacionErrors.resultado"
                required
              />

              <BaseFormField
                v-model="palpacionForm.fechaParto"
                label="Fecha Estimada de Parto"
                type="date"
                :min="today"
                help="Solo si resultado es positivo"
                :disabled="palpacionForm.resultado !== 'Positiva'"
              />

              <BaseFormField
                v-model="palpacionForm.observaciones"
                label="Observaciones"
                type="textarea"
                :rows="2"
                help="Notas del veterinario"
              />
            </BaseForm>
          </div>

          <!-- Parto Tab -->
          <div v-if="activeTab === 'parto'" class="tab-panel">
            <BaseForm
              title="Registro de Parto"
              subtitle="Registra nacimientos y datos de las crías"
              layout="three"
              :loading="saving"
              :is-valid="isPartoFormValid"
              submit-text="Registrar Parto"
              @submit="handleParto"
            >
              <BaseFormField
                v-model="partoForm.idhembra"
                label="Hembra"
                type="select"
                :options="hembrasGestantesOptions"
                :error="partoErrors.idhembra"
                required
                help="Hembra gestante"
              />

              <BaseFormField
                v-model="partoForm.fecha"
                label="Fecha del Parto"
                type="date"
                :max="today"
                :error="partoErrors.fecha"
                required
              />

              <BaseFormField
                v-model="partoForm.tipoParto"
                label="Tipo de Parto"
                type="select"
                :options="tipoPartoOptions"
                required
              />

              <BaseFormField
                v-model="partoForm.numeroCrias"
                label="Número de Crías"
                type="number"
                min="1"
                max="5"
                :error="partoErrors.numeroCrias"
                required
              />

              <BaseFormField
                v-model="partoForm.sexoCria"
                label="Sexo de la Cría"
                type="select"
                :options="sexoOptions"
                help="Solo para partos simples"
                :disabled="parseInt(partoForm.numeroCrias) !== 1"
              />

              <BaseFormField
                v-model="partoForm.pesoCria"
                label="Peso de la Cría (kg)"
                type="number"
                min="0"
                step="0.1"
                help="Peso al nacer"
              />

              <BaseFormField
                v-model="partoForm.observaciones"
                label="Observaciones"
                type="textarea"
                :rows="3"
                help="Detalles del parto y estado de madre/cría"
              />

              <BaseFormField
                v-model="partoForm.complicaciones"
                label="Complicaciones"
                type="select"
                :options="complicacionesOptions"
              />
            </BaseForm>
          </div>
        </div>
      </div>
    </section>

    <!-- Reproductive Records Table -->
    <BaseTable
      :data="filteredReproductiveRecords"
      :columns="reproductiveColumns"
      :loading="loading"
      title="Historial Reproductivo"
      empty-title="Sin registros reproductivos"
      empty-message="No hay servicios o partos registrados. Comienza registrando servicios para controlar la reproducción."
      :page-size="15"
      :search-placeholder="'Buscar por animal, fecha o tipo...'"
    >
      <template #actions>
        <div class="table-filters">
          <select v-model="filterType" class="filter-select">
            <option value="">Todos los registros</option>
            <option value="servicio">Solo Servicios</option>
            <option value="palpacion">Solo Palpaciones</option>
            <option value="parto">Solo Partos</option>
          </select>
          <select v-model="filterStatus" class="filter-select">
            <option value="">Todos los estados</option>
            <option value="pendiente">Pendientes</option>
            <option value="positivo">Gestantes</option>
            <option value="completado">Completados</option>
          </select>
        </div>
        <button class="btn btn-outline" @click="generateReproductiveReport">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          Reporte Reproductivo
        </button>
        <button class="btn btn-outline" @click="loadReproductiveData">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Actualizar
        </button>
      </template>

      <template #cell-animal="{ item }">
        <div class="animal-info">
          <span class="animal-name">{{ getAnimalName(item.idhembra || item.idganado) }}</span>
          <small class="animal-id">ID: {{ item.idhembra || item.idganado }}</small>
        </div>
      </template>

      <template #cell-fecha="{ value }">
        {{ formatDate(value) }}
      </template>

      <template #cell-tipo="{ item }">
        <span class="type-badge" :class="getTypeClass(item.tipo)">
          {{ item.tipo }}
        </span>
      </template>

      <template #cell-estado="{ item }">
        <span class="status-badge" :class="getStatusClass(item.estado)">
          {{ item.estado }}
        </span>
      </template>

      <template #cell-fechaEstimada="{ value }">
        <span v-if="value" class="estimated-date" :class="{ 'near-date': isNearDate(value) }">
          {{ formatDate(value) }}
        </span>
        <span v-else class="no-date">-</span>
      </template>

      <template #row-actions="{ item }">
        <button
          v-if="item.tipo === 'Servicio' && !item.palpacionRealizada"
          class="btn btn-success btn-sm"
          @click="quickPalpacion(item)"
          title="Registrar palpación"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </button>
        <button
          v-if="item.gestante && !item.partoRealizado"
          class="btn btn-primary btn-sm"
          @click="quickParto(item)"
          title="Registrar parto"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
        </button>
        <button
          class="btn btn-outline btn-sm"
          @click="viewReproductiveHistory(item)"
          title="Ver historial completo"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
          </svg>
        </button>
      </template>

      <template #empty-actions>
        <button class="btn btn-primary" @click="activeTab = 'servicio'">
          Registrar Primer Servicio
        </button>
      </template>
    </BaseTable>

    <!-- Lactation Tracking Section -->
    <section class="surface-panel">
      <header class="section-header">
        <div>
          <h2 class="section-title">Seguimiento de Lactancia</h2>
          <p class="section-subtitle">Control de crías en período de lactancia y destete</p>
        </div>
        <button class="btn btn-outline" @click="openDesteteModal">
          Programar Destete
        </button>
      </header>

      <BaseTable
        :data="lactanciaRecords"
        :columns="lactanciaColumns"
        :loading="loadingLactancia"
        title=""
        :show-header="false"
        empty-title="Sin crías en lactancia"
        empty-message="No hay crías en período de lactancia actualmente."
        :page-size="10"
      >
        <template #cell-madre="{ item }">
          {{ getAnimalName(item.idmadre) }}
        </template>

        <template #cell-cria="{ item }">
          <div class="cria-info">
            <span class="cria-name">{{ item.nombreCria || `Cría ${item.idcria}` }}</span>
            <small class="cria-details">{{ item.sexo }} - {{ formatDate(item.fechaNacimiento) }}</small>
          </div>
        </template>

        <template #cell-edad="{ item }">
          {{ calculateAge(item.fechaNacimiento) }} días
        </template>

        <template #cell-pesoActual="{ value }">
          <span class="weight-badge">{{ value || '-' }} kg</span>
        </template>

        <template #cell-estadoLactancia="{ item }">
          <span class="lactation-badge" :class="getLactationStatusClass(item)">
            {{ getLactationStatus(item) }}
          </span>
        </template>

        <template #row-actions="{ item }">
          <button
            class="btn btn-outline btn-sm"
            @click="updateCriaWeight(item)"
            title="Actualizar peso"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </button>
          <button
            v-if="!item.fechaDestete"
            class="btn btn-warning btn-sm"
            @click="realizarDestete(item)"
            title="Realizar destete"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </button>
        </template>
      </BaseTable>
    </section>

    <!-- Reproductive Calendar Modal -->
    <BaseModal
      v-model="calendarModal.open"
      title="Calendario Reproductivo"
      subtitle="Próximos eventos reproductivos programados"
      size="xl"
      show-footer
      :show-confirm="false"
      cancel-text="Cerrar"
      @cancel="closeCalendarModal"
    >
      <!-- Calendar content would go here -->
      <div class="calendar-placeholder">
        <p>Calendario reproductivo - Funcionalidad en desarrollo</p>
      </div>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';
import { ganadoApi } from '@/services/api.js';
import { BaseForm, BaseFormField, BaseTable, BaseModal } from '@/components/base';

const { token } = useAuth();
const { pushToast } = useToasts();

// State
const loading = ref(true);
const loadingLactancia = ref(true);
const saving = ref(false);
const activeTab = ref('servicio');
const filterType = ref('');
const filterStatus = ref('');

// Data
const reproductiveRecords = ref([]);
const lactanciaRecords = ref([]);
const ganadoList = ref([]);

// Computed
const today = computed(() => new Date().toISOString().split('T')[0]);

const hembrasOptions = computed(() =>
  ganadoList.value
    .filter(animal => animal.sexo === 'H')
    .map(animal => ({
      value: animal.id,
      label: `${animal.nombre || `Animal ${animal.id}`} - ${animal.raza || 'Sin raza'}`
    }))
);

const hembrasServicioOptions = computed(() =>
  reproductiveRecords.value
    .filter(record => record.tipo === 'Servicio' && !record.palpacionRealizada)
    .map(record => ({
      value: record.idhembra,
      label: getAnimalName(record.idhembra)
    }))
);

const hembrasGestantesOptions = computed(() =>
  reproductiveRecords.value
    .filter(record => record.gestante && !record.partoRealizado)
    .map(record => ({
      value: record.idhembra,
      label: getAnimalName(record.idhembra)
    }))
);

const filteredReproductiveRecords = computed(() => {
  let filtered = reproductiveRecords.value;

  if (filterType.value) {
    filtered = filtered.filter(record =>
      record.tipo.toLowerCase().includes(filterType.value.toLowerCase())
    );
  }

  if (filterStatus.value) {
    filtered = filtered.filter(record => {
      switch (filterStatus.value) {
        case 'pendiente':
          return record.estado === 'Pendiente';
        case 'positivo':
          return record.gestante;
        case 'completado':
          return record.estado === 'Completado';
        default:
          return true;
      }
    });
  }

  return filtered;
});

const hembrasGestantes = computed(() => {
  return reproductiveRecords.value.filter(record => record.gestante).length;
});

const proximosPartos = computed(() => {
  const hoy = new Date();
  const proximosMes = new Date();
  proximosMes.setMonth(proximosMes.getMonth() + 1);

  return reproductiveRecords.value.filter(record => {
    if (!record.fechaParto || !record.gestante) return false;
    const fecha = new Date(record.fechaParto);
    return fecha >= hoy && fecha <= proximosMes;
  }).length;
});

const criasLactando = computed(() => {
  return lactanciaRecords.value.filter(record => !record.fechaDestete).length;
});

const isServicioFormValid = computed(() => {
  return servicioForm.idhembra &&
         servicioForm.fecha &&
         servicioForm.tipoServicio;
});

const isPalpacionFormValid = computed(() => {
  return palpacionForm.idhembra &&
         palpacionForm.fecha &&
         palpacionForm.resultado;
});

const isPartoFormValid = computed(() => {
  return partoForm.idhembra &&
         partoForm.fecha &&
         partoForm.numeroCrias;
});

// Tabs configuration
const tabs = [
  { id: 'servicio', label: 'Servicio', icon: 'ServiceIcon' },
  { id: 'palpacion', label: 'Palpación', icon: 'PalpationIcon' },
  { id: 'parto', label: 'Parto', icon: 'BirthIcon' }
];

// Options
const tipoServicioOptions = [
  { value: 'Natural', label: 'Monta Natural' },
  { value: 'IA', label: 'Inseminación Artificial' },
  { value: 'IATF', label: 'IA a Tiempo Fijo' },
  { value: 'Embriones', label: 'Transferencia de Embriones' }
];

const resultadoPalpacionOptions = [
  { value: 'Positiva', label: 'Positiva (Gestante)' },
  { value: 'Negativa', label: 'Negativa (Vacía)' },
  { value: 'Dudosa', label: 'Dudosa (Repetir)' }
];

const tipoPartoOptions = [
  { value: 'Normal', label: 'Parto Normal' },
  { value: 'Asistido', label: 'Parto Asistido' },
  { value: 'Cesarea', label: 'Cesárea' },
  { value: 'Distocico', label: 'Parto Distócico' }
];

const sexoOptions = [
  { value: 'M', label: 'Macho' },
  { value: 'H', label: 'Hembra' }
];

const complicacionesOptions = [
  { value: 'Ninguna', label: 'Sin Complicaciones' },
  { value: 'Leve', label: 'Complicaciones Leves' },
  { value: 'Moderada', label: 'Complicaciones Moderadas' },
  { value: 'Grave', label: 'Complicaciones Graves' }
];

// Forms
const servicioForm = reactive({
  idhembra: '',
  fecha: today.value,
  tipoServicio: '',
  macho: '',
  observaciones: '',
  fechaPalpacion: ''
});

const servicioErrors = reactive({
  idhembra: '',
  fecha: '',
  tipoServicio: ''
});

const palpacionForm = reactive({
  idhembra: '',
  fecha: today.value,
  resultado: '',
  fechaParto: '',
  observaciones: ''
});

const palpacionErrors = reactive({
  idhembra: '',
  fecha: '',
  resultado: ''
});

const partoForm = reactive({
  idhembra: '',
  fecha: today.value,
  tipoParto: 'Normal',
  numeroCrias: 1,
  sexoCria: '',
  pesoCria: '',
  observaciones: '',
  complicaciones: 'Ninguna'
});

const partoErrors = reactive({
  idhembra: '',
  fecha: '',
  numeroCrias: ''
});

const calendarModal = reactive({
  open: false
});

// Table columns
const reproductiveColumns = [
  { key: 'animal', label: 'Animal' },
  { key: 'fecha', label: 'Fecha', sortable: true },
  { key: 'tipo', label: 'Tipo', sortable: true },
  { key: 'estado', label: 'Estado' },
  { key: 'fechaEstimada', label: 'Fecha Estimada' },
  { key: 'observaciones', label: 'Observaciones' }
];

const lactanciaColumns = [
  { key: 'madre', label: 'Madre' },
  { key: 'cria', label: 'Cría' },
  { key: 'edad', label: 'Edad', sortable: true },
  { key: 'pesoActual', label: 'Peso Actual' },
  { key: 'estadoLactancia', label: 'Estado' }
];

// Methods
async function loadReproductiveData() {
  loading.value = true;
  try {
    const authToken = token.value;

    // Load ganado for selects
    const ganado = await ganadoApi.list(authToken);
    ganadoList.value = ganado.map(animal => ({
      id: animal.idganado || animal.id,
      nombre: animal.nombreGanado || animal.nombre,
      raza: animal.raza,
      sexo: animal.sexo
    }));

    // Mock reproductive data - replace with real API calls
    await loadMockReproductiveData();

  } catch (error) {
    pushToast(error.message || 'Error al cargar datos reproductivos', 'error');
  } finally {
    loading.value = false;
  }
}

async function loadMockReproductiveData() {
  // Mock reproductive records
  const mockRecords = [];
  const hembras = ganadoList.value.filter(animal => animal.sexo === 'H').slice(0, 8);

  for (let i = 0; i < 12; i++) {
    const randomHembra = hembras[Math.floor(Math.random() * hembras.length)];
    const date = new Date();
    date.setDate(date.getDate() - Math.floor(Math.random() * 180));

    const tipoRandom = Math.random();
    let tipo, estado, gestante = false, fechaEstimada = null;

    if (tipoRandom < 0.4) {
      tipo = 'Servicio';
      estado = Math.random() > 0.3 ? 'Pendiente' : 'Completado';
      if (Math.random() > 0.5) {
        gestante = true;
        const fechaParto = new Date(date);
        fechaParto.setDate(fechaParto.getDate() + 280); // 9 meses aprox
        fechaEstimada = fechaParto.toISOString().split('T')[0];
      }
    } else if (tipoRandom < 0.7) {
      tipo = 'Palpación';
      estado = Math.random() > 0.2 ? 'Positiva' : 'Negativa';
      gestante = estado === 'Positiva';
    } else {
      tipo = 'Parto';
      estado = 'Completado';
    }

    mockRecords.push({
      id: i + 1,
      idhembra: randomHembra?.id || 1,
      idganado: randomHembra?.id || 1,
      fecha: date.toISOString().split('T')[0],
      tipo,
      estado,
      gestante,
      fechaEstimada,
      observaciones: `${tipo} registrado`,
      palpacionRealizada: tipo !== 'Servicio' || Math.random() > 0.5,
      partoRealizado: tipo === 'Parto'
    });
  }

  reproductiveRecords.value = mockRecords.sort((a, b) => new Date(b.fecha) - new Date(a.fecha));

  // Mock lactation records
  const mockLactancia = [];
  for (let i = 0; i < 6; i++) {
    const randomHembra = hembras[Math.floor(Math.random() * hembras.length)];
    const fechaNacimiento = new Date();
    fechaNacimiento.setDate(fechaNacimiento.getDate() - Math.floor(Math.random() * 120));

    mockLactancia.push({
      id: i + 1,
      idmadre: randomHembra?.id || 1,
      idcria: (i + 1) * 100,
      nombreCria: `Cría ${i + 1}`,
      sexo: Math.random() > 0.5 ? 'M' : 'H',
      fechaNacimiento: fechaNacimiento.toISOString().split('T')[0],
      pesoNacimiento: (25 + Math.random() * 15).toFixed(1),
      pesoActual: (35 + Math.random() * 25).toFixed(1),
      fechaDestete: Math.random() > 0.7 ? null : new Date(fechaNacimiento.getTime() + 180 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    });
  }

  lactanciaRecords.value = mockLactancia;
  loadingLactancia.value = false;
}

function validateServicioForm() {
  servicioErrors.idhembra = servicioForm.idhembra ? '' : 'Selecciona una hembra';
  servicioErrors.fecha = servicioForm.fecha ? '' : 'Ingresa la fecha';
  servicioErrors.tipoServicio = servicioForm.tipoServicio ? '' : 'Selecciona el tipo de servicio';

  return !servicioErrors.idhembra && !servicioErrors.fecha && !servicioErrors.tipoServicio;
}

function validatePalpacionForm() {
  palpacionErrors.idhembra = palpacionForm.idhembra ? '' : 'Selecciona una hembra';
  palpacionErrors.fecha = palpacionForm.fecha ? '' : 'Ingresa la fecha';
  palpacionErrors.resultado = palpacionForm.resultado ? '' : 'Selecciona el resultado';

  return !palpacionErrors.idhembra && !palpacionErrors.fecha && !palpacionErrors.resultado;
}

function validatePartoForm() {
  partoErrors.idhembra = partoForm.idhembra ? '' : 'Selecciona una hembra';
  partoErrors.fecha = partoForm.fecha ? '' : 'Ingresa la fecha';
  partoErrors.numeroCrias = partoForm.numeroCrias && partoForm.numeroCrias > 0 ? '' : 'Ingresa el número de crías';

  return !partoErrors.idhembra && !partoErrors.fecha && !partoErrors.numeroCrias;
}

async function handleServicio() {
  if (!validateServicioForm()) return;

  saving.value = true;
  try {
    const newRecord = {
      id: reproductiveRecords.value.length + 1,
      idhembra: parseInt(servicioForm.idhembra),
      idganado: parseInt(servicioForm.idhembra),
      fecha: servicioForm.fecha,
      tipo: 'Servicio',
      estado: 'Pendiente',
      gestante: false,
      fechaEstimada: servicioForm.fechaPalpacion || null,
      observaciones: servicioForm.observaciones || 'Servicio registrado',
      palpacionRealizada: false,
      partoRealizado: false
    };

    reproductiveRecords.value.unshift(newRecord);
    pushToast('Servicio registrado exitosamente', 'success');
    resetServicioForm();

  } catch (error) {
    pushToast(error.message || 'Error al registrar servicio', 'error');
  } finally {
    saving.value = false;
  }
}

async function handlePalpacion() {
  if (!validatePalpacionForm()) return;

  saving.value = true;
  try {
    const newRecord = {
      id: reproductiveRecords.value.length + 1,
      idhembra: parseInt(palpacionForm.idhembra),
      idganado: parseInt(palpacionForm.idhembra),
      fecha: palpacionForm.fecha,
      tipo: 'Palpación',
      estado: palpacionForm.resultado,
      gestante: palpacionForm.resultado === 'Positiva',
      fechaEstimada: palpacionForm.fechaParto || null,
      observaciones: palpacionForm.observaciones || 'Palpación realizada'
    };

    reproductiveRecords.value.unshift(newRecord);

    // Update related servicio record
    const servicioRecord = reproductiveRecords.value.find(r =>
      r.idhembra === parseInt(palpacionForm.idhembra) &&
      r.tipo === 'Servicio' &&
      !r.palpacionRealizada
    );
    if (servicioRecord) {
      servicioRecord.palpacionRealizada = true;
      servicioRecord.gestante = palpacionForm.resultado === 'Positiva';
    }

    pushToast('Palpación registrada exitosamente', 'success');
    resetPalpacionForm();

  } catch (error) {
    pushToast(error.message || 'Error al registrar palpación', 'error');
  } finally {
    saving.value = false;
  }
}

async function handleParto() {
  if (!validatePartoForm()) return;

  saving.value = true;
  try {
    const newRecord = {
      id: reproductiveRecords.value.length + 1,
      idhembra: parseInt(partoForm.idhembra),
      idganado: parseInt(partoForm.idhembra),
      fecha: partoForm.fecha,
      tipo: 'Parto',
      estado: 'Completado',
      gestante: false,
      observaciones: partoForm.observaciones || 'Parto registrado'
    };

    reproductiveRecords.value.unshift(newRecord);

    // Add to lactancia if successful birth
    if (partoForm.complicaciones === 'Ninguna' || partoForm.complicaciones === 'Leve') {
      const newLactancia = {
        id: lactanciaRecords.value.length + 1,
        idmadre: parseInt(partoForm.idhembra),
        idcria: newRecord.id * 100,
        nombreCria: `Cría ${newRecord.id}`,
        sexo: partoForm.sexoCria || 'M',
        fechaNacimiento: partoForm.fecha,
        pesoNacimiento: partoForm.pesoCria || null,
        pesoActual: partoForm.pesoCria || null,
        fechaDestete: null
      };
      lactanciaRecords.value.unshift(newLactancia);
    }

    pushToast('Parto registrado exitosamente', 'success');
    resetPartoForm();

  } catch (error) {
    pushToast(error.message || 'Error al registrar parto', 'error');
  } finally {
    saving.value = false;
  }
}

function resetServicioForm() {
  Object.assign(servicioForm, {
    idhembra: '',
    fecha: today.value,
    tipoServicio: '',
    macho: '',
    observaciones: '',
    fechaPalpacion: ''
  });
  Object.assign(servicioErrors, {
    idhembra: '',
    fecha: '',
    tipoServicio: ''
  });
}

function resetPalpacionForm() {
  Object.assign(palpacionForm, {
    idhembra: '',
    fecha: today.value,
    resultado: '',
    fechaParto: '',
    observaciones: ''
  });
  Object.assign(palpacionErrors, {
    idhembra: '',
    fecha: '',
    resultado: ''
  });
}

function resetPartoForm() {
  Object.assign(partoForm, {
    idhembra: '',
    fecha: today.value,
    tipoParto: 'Normal',
    numeroCrias: 1,
    sexoCria: '',
    pesoCria: '',
    observaciones: '',
    complicaciones: 'Ninguna'
  });
  Object.assign(partoErrors, {
    idhembra: '',
    fecha: '',
    numeroCrias: ''
  });
}

function quickPalpacion(item) {
  activeTab.value = 'palpacion';
  palpacionForm.idhembra = item.idhembra;
  // Calculate estimated palpation date (60 days after service)
  const fechaServicio = new Date(item.fecha);
  fechaServicio.setDate(fechaServicio.getDate() + 60);
  palpacionForm.fecha = fechaServicio.toISOString().split('T')[0];
}

function quickParto(item) {
  activeTab.value = 'parto';
  partoForm.idhembra = item.idhembra;
}

function generateReproductiveReport() {
  pushToast('Generando reporte reproductivo...', 'info');
  setTimeout(() => {
    pushToast('Reporte reproductivo generado exitosamente', 'success');
  }, 1500);
}

function viewReproductiveHistory(item) {
  pushToast(`Ver historial reproductivo completo - Animal ${item.idhembra}`, 'info');
}

function openDesteteModal() {
  pushToast('Programar destete - Funcionalidad en desarrollo', 'info');
}

function closeCalendarModal() {
  calendarModal.open = false;
}

function updateCriaWeight(item) {
  pushToast(`Actualizar peso de ${item.nombreCria}`, 'info');
}

function realizarDestete(item) {
  if (confirm(`¿Realizar destete de ${item.nombreCria}?`)) {
    item.fechaDestete = today.value;
    pushToast('Destete realizado exitosamente', 'success');
  }
}

// Utility functions
function getAnimalName(animalId) {
  const animal = ganadoList.value.find(a => a.id === animalId);
  return animal ? (animal.nombre || `Animal ${animalId}`) : `Animal ${animalId}`;
}

function formatDate(date) {
  if (!date) return '-';
  try {
    return new Date(date).toLocaleDateString('es-ES');
  } catch {
    return date;
  }
}

function getMinPalpacionDate(servicioDate) {
  if (!servicioDate) return '';
  const date = new Date(servicioDate);
  date.setDate(date.getDate() + 45); // Minimum 45 days after service
  return date.toISOString().split('T')[0];
}

function getTypeClass(tipo) {
  const classes = {
    'Servicio': 'type-service',
    'Palpación': 'type-palpation',
    'Parto': 'type-birth'
  };
  return classes[tipo] || 'type-other';
}

function getStatusClass(estado) {
  const classes = {
    'Pendiente': 'status-pending',
    'Positiva': 'status-positive',
    'Negativa': 'status-negative',
    'Completado': 'status-completed'
  };
  return classes[estado] || 'status-other';
}

function isNearDate(date) {
  const targetDate = new Date(date);
  const today = new Date();
  const diffTime = targetDate - today;
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays <= 30 && diffDays >= 0;
}

function calculateAge(birthDate) {
  const birth = new Date(birthDate);
  const today = new Date();
  const diffTime = today - birth;
  const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
  return diffDays;
}

function getLactationStatus(item) {
  if (item.fechaDestete) return 'Destetado';
  const age = calculateAge(item.fechaNacimiento);
  if (age < 60) return 'Lactancia Temprana';
  if (age < 120) return 'Lactancia Media';
  return 'Listo para Destete';
}

function getLactationStatusClass(item) {
  const status = getLactationStatus(item);
  const classes = {
    'Destetado': 'lactation-weaned',
    'Lactancia Temprana': 'lactation-early',
    'Lactancia Media': 'lactation-mid',
    'Listo para Destete': 'lactation-ready'
  };
  return classes[status] || 'lactation-other';
}

// Icon components (simplified)
const ServiceIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" /></svg>' };
const PalpationIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>' };
const BirthIcon = { template: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" /></svg>' };

onMounted(() => {
  loadReproductiveData();
});
</script>

<style scoped>
.reproduccion-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.header-section {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 2rem;
}

.header-content {
  flex: 1;
}

.header-stats {
  display: flex;
  gap: 2rem;
}

.stat-item {
  text-align: center;
}

.stat-value {
  display: block;
  font-size: 2rem;
  font-weight: 700;
  color: var(--brand-primary);
}

.stat-label {
  font-size: 0.875rem;
  color: var(--color-text-muted);
}

.tabs-container {
  display: flex;
  flex-direction: column;
}

.tabs-header {
  display: flex;
  border-bottom: 1px solid var(--color-border);
}

.tab-button {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem 1.5rem;
  background: none;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: var(--transition-fast);
  border-bottom: 2px solid transparent;
}

.tab-button:hover {
  color: var(--color-text);
  background: var(--color-surface-muted);
}

.tab-button.active {
  color: var(--brand-primary);
  border-bottom-color: var(--brand-primary);
}

.tab-icon {
  width: 1.25rem;
  height: 1.25rem;
}

.tab-content {
  padding: 1.5rem;
}

.tab-panel {
  min-height: 400px;
}

.table-filters {
  display: flex;
  gap: 1rem;
}

.filter-select {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: 0.875rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
}

.section-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 0.25rem 0;
}

.section-subtitle {
  font-size: 0.875rem;
  color: var(--color-text-muted);
  margin: 0;
}

.animal-info {
  display: flex;
  flex-direction: column;
}

.animal-name {
  font-weight: 500;
}

.animal-id {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.type-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.type-service { background: #e0e7ff; color: #3730a3; }
.type-palpation { background: #fef3c7; color: #92400e; }
.type-birth { background: #dcfce7; color: #166534; }
.type-other { background: #f3f4f6; color: #6b7280; }

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.status-pending { background: #fef3c7; color: #92400e; }
.status-positive { background: #dcfce7; color: #166534; }
.status-negative { background: #fecaca; color: #991b1b; }
.status-completed { background: #e0e7ff; color: #3730a3; }
.status-other { background: #f3f4f6; color: #6b7280; }

.estimated-date {
  font-size: 0.875rem;
}

.estimated-date.near-date {
  color: var(--brand-warning);
  font-weight: 600;
}

.no-date {
  color: var(--color-text-muted);
}

.cria-info {
  display: flex;
  flex-direction: column;
}

.cria-name {
  font-weight: 500;
}

.cria-details {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.weight-badge {
  background: var(--brand-secondary);
  color: var(--color-text);
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-weight: 600;
  font-size: 0.875rem;
}

.lactation-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.lactation-early { background: #dcfce7; color: #166534; }
.lactation-mid { background: #fef3c7; color: #92400e; }
.lactation-ready { background: #fed7aa; color: #c2410c; }
.lactation-weaned { background: #f3f4f6; color: #6b7280; }
.lactation-other { background: #e0e7ff; color: #3730a3; }

.calendar-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 300px;
  color: var(--color-text-muted);
  background: var(--color-surface-muted);
  border-radius: var(--radius-lg);
}

.w-4 { width: 1rem; }
.h-4 { height: 1rem; }
.mr-2 { margin-right: 0.5rem; }

@media (max-width: 768px) {
  .header-section {
    flex-direction: column;
    gap: 1rem;
  }

  .header-stats {
    justify-content: center;
    flex-wrap: wrap;
  }

  .tabs-header {
    flex-direction: column;
  }

  .tab-button {
    justify-content: center;
  }

  .table-filters {
    flex-direction: column;
  }

  .section-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
}
</style>