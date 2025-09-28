<template>
  <div class="produccion-container">
    <!-- Header Section -->
    <section class="surface-panel">
      <div class="header-section">
        <div class="header-content">
          <div class="stat-chip">Gestión productiva</div>
          <h1 class="hero-title">Control de Producción</h1>
          <p class="hero-subtitle">
            Registra y monitorea la producción diaria de leche, carne y otros productos. Gestiona ciclos productivos completos.
          </p>
        </div>
        <div class="header-stats">
          <div class="stat-item">
            <span class="stat-value">{{ totalProduccionHoy }}L</span>
            <span class="stat-label">Hoy</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ totalProduccionMes }}L</span>
            <span class="stat-label">Este mes</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Quick Production Entry -->
    <section class="surface-panel">
      <BaseForm
        title="Registro Rápido de Producción"
        subtitle="Registra la producción diaria de un animal"
        layout="three"
        :loading="saving"
        :is-valid="isQuickFormValid"
        submit-text="Registrar Producción"
        @submit="handleQuickProduction"
      >
        <BaseFormField
          v-model="quickForm.idganado"
          label="Animal"
          type="select"
          :options="ganadoOptions"
          :error="quickErrors.idganado"
          required
          help="Selecciona el animal productor"
        />

        <BaseFormField
          v-model="quickForm.fecha"
          label="Fecha"
          type="date"
          :max="today"
          :error="quickErrors.fecha"
          required
        />

        <BaseFormField
          v-model="quickForm.horario"
          label="Horario"
          type="select"
          :options="horarioOptions"
          :error="quickErrors.horario"
          required
        />

        <BaseFormField
          v-model="quickForm.cantidad"
          label="Cantidad (Litros)"
          type="number"
          min="0"
          step="0.1"
          :error="quickErrors.cantidad"
          required
          help="Cantidad producida en litros"
        />

        <BaseFormField
          v-model="quickForm.observaciones"
          label="Observaciones"
          type="textarea"
          :rows="2"
          help="Notas adicionales sobre la producción"
        />
      </BaseForm>
    </section>

    <!-- Production Records Table -->
    <BaseTable
      :data="produccionRecords"
      :columns="produccionColumns"
      :loading="loading"
      title="Registros de Producción"
      empty-title="Sin registros de producción"
      empty-message="No hay registros de producción. Comienza registrando la producción diaria de tus animales."
      :page-size="15"
      :search-placeholder="'Buscar por animal, fecha o cantidad...'"
    >
      <template #actions>
        <button class="btn btn-outline" @click="exportProduction">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          Exportar
        </button>
        <button class="btn btn-primary" @click="openCycleModal">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          Nuevo Ciclo
        </button>
        <button class="btn btn-outline" @click="loadProduccionData">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Actualizar
        </button>
      </template>

      <template #cell-animal="{ item }">
        <div class="animal-info">
          <span class="animal-name">{{ getAnimalName(item.idganado) }}</span>
          <small class="animal-id">ID: {{ item.idganado }}</small>
        </div>
      </template>

      <template #cell-fecha="{ value }">
        {{ formatDate(value) }}
      </template>

      <template #cell-cantidad="{ value }">
        <span class="quantity-badge">{{ value }}L</span>
      </template>

      <template #cell-horario="{ value }">
        <span class="horario-badge" :class="getHorarioClass(value)">
          {{ value }}
        </span>
      </template>

      <template #row-actions="{ item }">
        <button
          class="btn btn-outline btn-sm"
          @click="editProduction(item)"
          title="Editar registro"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
        </button>
        <button
          class="btn btn-ghost btn-sm"
          @click="deleteProduction(item)"
          title="Eliminar registro"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </button>
      </template>

      <template #empty-actions>
        <button class="btn btn-primary" @click="resetQuickForm">
          Registrar Primera Producción
        </button>
      </template>
    </BaseTable>

    <!-- Production Cycles Section -->
    <section class="surface-panel">
      <header class="section-header">
        <div>
          <h2 class="section-title">Ciclos de Producción</h2>
          <p class="section-subtitle">Gestiona ciclos productivos completos por animal</p>
        </div>
        <button class="btn btn-outline" @click="openCycleModal">
          Nuevo Ciclo
        </button>
      </header>

      <BaseTable
        :data="ciclosProduccion"
        :columns="ciclosColumns"
        :loading="loadingCiclos"
        title=""
        :show-header="false"
        empty-title="Sin ciclos registrados"
        empty-message="No hay ciclos de producción registrados. Crea el primer ciclo para gestionar períodos productivos completos."
        :page-size="10"
      >
        <template #cell-animal="{ item }">
          {{ getAnimalName(item.idganado) }}
        </template>

        <template #cell-periodo="{ item }">
          <div class="periodo-info">
            <span class="fecha-inicio">{{ formatDate(item.fechaInicio) }}</span>
            <span class="separator">→</span>
            <span class="fecha-fin">{{ formatDate(item.fechaFin) }}</span>
          </div>
        </template>

        <template #cell-duracion="{ item }">
          {{ calculateDuration(item.fechaInicio, item.fechaFin) }} días
        </template>

        <template #cell-total="{ value }">
          <span class="total-badge">{{ value }}L</span>
        </template>

        <template #cell-estado="{ item }">
          <span class="status-badge" :class="getCycleStatus(item)">
            {{ getCycleStatusText(item) }}
          </span>
        </template>

        <template #row-actions="{ item }">
          <button
            class="btn btn-outline btn-sm"
            @click="viewCycleDetails(item)"
            title="Ver detalles"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
            </svg>
          </button>
          <button
            class="btn btn-ghost btn-sm"
            @click="closeCycle(item)"
            :disabled="item.fechaFin && new Date(item.fechaFin) <= new Date()"
            title="Cerrar ciclo"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </button>
        </template>
      </BaseTable>
    </section>

    <!-- Edit Production Modal -->
    <BaseModal
      v-model="editModal.open"
      title="Editar Producción"
      subtitle="Modifica los datos del registro de producción"
      size="md"
      show-footer
      :loading="saving"
      @confirm="updateProduction"
      @cancel="closeEditModal"
      confirm-text="Actualizar"
    >
      <BaseForm layout="two" :show-actions="false">
        <BaseFormField
          v-model="editModal.form.fecha"
          label="Fecha"
          type="date"
          :max="today"
          required
        />

        <BaseFormField
          v-model="editModal.form.horario"
          label="Horario"
          type="select"
          :options="horarioOptions"
          required
        />

        <BaseFormField
          v-model="editModal.form.cantidad"
          label="Cantidad (Litros)"
          type="number"
          min="0"
          step="0.1"
          required
        />

        <BaseFormField
          v-model="editModal.form.observaciones"
          label="Observaciones"
          type="textarea"
          :rows="3"
        />
      </BaseForm>
    </BaseModal>

    <!-- New Cycle Modal -->
    <BaseModal
      v-model="cycleModal.open"
      title="Nuevo Ciclo de Producción"
      subtitle="Crear un nuevo período productivo para un animal"
      size="md"
      show-footer
      :loading="saving"
      @confirm="createCycle"
      @cancel="closeCycleModal"
      confirm-text="Crear Ciclo"
    >
      <BaseForm layout="two" :show-actions="false">
        <BaseFormField
          v-model="cycleModal.form.idganado"
          label="Animal"
          type="select"
          :options="ganadoOptions"
          required
        />

        <BaseFormField
          v-model="cycleModal.form.fechaInicio"
          label="Fecha Inicio"
          type="date"
          :max="today"
          required
        />

        <BaseFormField
          v-model="cycleModal.form.fechaFin"
          label="Fecha Fin Estimada"
          type="date"
          :min="cycleModal.form.fechaInicio"
          help="Fecha estimada de fin del ciclo (opcional)"
        />

        <BaseFormField
          v-model="cycleModal.form.observaciones"
          label="Observaciones"
          type="textarea"
          :rows="3"
          help="Notas sobre el ciclo productivo"
        />
      </BaseForm>
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
const loadingCiclos = ref(true);
const saving = ref(false);

// Data
const produccionRecords = ref([]);
const ciclosProduccion = ref([]);
const ganadoList = ref([]);

// Computed
const today = computed(() => new Date().toISOString().split('T')[0]);

const ganadoOptions = computed(() =>
  ganadoList.value.map(animal => ({
    value: animal.id,
    label: `${animal.nombre || `Animal ${animal.id}`} - ${animal.raza || 'Sin raza'}`
  }))
);

const horarioOptions = [
  { value: 'Mañana', label: 'Mañana (6:00 - 12:00)' },
  { value: 'Tarde', label: 'Tarde (12:00 - 18:00)' },
  { value: 'Noche', label: 'Noche (18:00 - 24:00)' },
  { value: 'Madrugada', label: 'Madrugada (0:00 - 6:00)' }
];

const totalProduccionHoy = computed(() => {
  const hoy = new Date().toISOString().split('T')[0];
  return produccionRecords.value
    .filter(record => record.fecha === hoy)
    .reduce((sum, record) => sum + (parseFloat(record.cantidad) || 0), 0)
    .toFixed(1);
});

const totalProduccionMes = computed(() => {
  const inicioMes = new Date();
  inicioMes.setDate(1);
  const inicioMesStr = inicioMes.toISOString().split('T')[0];

  return produccionRecords.value
    .filter(record => record.fecha >= inicioMesStr)
    .reduce((sum, record) => sum + (parseFloat(record.cantidad) || 0), 0)
    .toFixed(1);
});

const isQuickFormValid = computed(() => {
  return quickForm.idganado &&
         quickForm.fecha &&
         quickForm.horario &&
         quickForm.cantidad &&
         quickForm.cantidad > 0;
});

// Forms
const quickForm = reactive({
  idganado: '',
  fecha: today.value,
  horario: '',
  cantidad: '',
  observaciones: ''
});

const quickErrors = reactive({
  idganado: '',
  fecha: '',
  horario: '',
  cantidad: ''
});

const editModal = reactive({
  open: false,
  id: null,
  form: {
    fecha: '',
    horario: '',
    cantidad: '',
    observaciones: ''
  }
});

const cycleModal = reactive({
  open: false,
  form: {
    idganado: '',
    fechaInicio: today.value,
    fechaFin: '',
    observaciones: ''
  }
});

// Table columns
const produccionColumns = [
  { key: 'animal', label: 'Animal' },
  { key: 'fecha', label: 'Fecha', sortable: true },
  { key: 'horario', label: 'Horario' },
  { key: 'cantidad', label: 'Cantidad', sortable: true },
  { key: 'observaciones', label: 'Observaciones' }
];

const ciclosColumns = [
  { key: 'animal', label: 'Animal' },
  { key: 'periodo', label: 'Período' },
  { key: 'duracion', label: 'Duración' },
  { key: 'total', label: 'Total Producido', sortable: true },
  { key: 'estado', label: 'Estado' }
];

// Methods
async function loadProduccionData() {
  loading.value = true;
  try {
    const authToken = token.value;

    // Load ganado for selects
    const ganado = await ganadoApi.list(authToken);
    ganadoList.value = ganado.map(animal => ({
      id: animal.idganado || animal.id,
      nombre: animal.nombreGanado || animal.nombre,
      raza: animal.raza
    }));

    // Mock production data - replace with real API calls
    await loadMockProductionData();

  } catch (error) {
    pushToast(error.message || 'Error al cargar datos de producción', 'error');
  } finally {
    loading.value = false;
  }
}

async function loadMockProductionData() {
  // Mock data - replace with real API calls
  const mockData = [];
  const animals = ganadoList.value.slice(0, 5); // Use first 5 animals

  for (let i = 0; i < 20; i++) {
    const randomAnimal = animals[Math.floor(Math.random() * animals.length)];
    const date = new Date();
    date.setDate(date.getDate() - Math.floor(Math.random() * 30));

    mockData.push({
      id: i + 1,
      idganado: randomAnimal?.id || 1,
      fecha: date.toISOString().split('T')[0],
      horario: horarioOptions[Math.floor(Math.random() * horarioOptions.length)].value,
      cantidad: (Math.random() * 20 + 5).toFixed(1),
      observaciones: Math.random() > 0.7 ? 'Producción normal' : ''
    });
  }

  produccionRecords.value = mockData.sort((a, b) => new Date(b.fecha) - new Date(a.fecha));

  // Mock cycles data
  const mockCycles = [];
  for (let i = 0; i < 5; i++) {
    const randomAnimal = animals[Math.floor(Math.random() * animals.length)];
    const fechaInicio = new Date();
    fechaInicio.setMonth(fechaInicio.getMonth() - Math.floor(Math.random() * 6));

    const fechaFin = new Date(fechaInicio);
    fechaFin.setMonth(fechaFin.getMonth() + 3 + Math.floor(Math.random() * 3));

    mockCycles.push({
      id: i + 1,
      idganado: randomAnimal?.id || 1,
      fechaInicio: fechaInicio.toISOString().split('T')[0],
      fechaFin: fechaFin.toISOString().split('T')[0],
      cantidadTotal: (Math.random() * 500 + 200).toFixed(1),
      observaciones: `Ciclo ${i + 1}`
    });
  }

  ciclosProduccion.value = mockCycles;
  loadingCiclos.value = false;
}

function validateQuickForm() {
  quickErrors.idganado = quickForm.idganado ? '' : 'Selecciona un animal';
  quickErrors.fecha = quickForm.fecha ? '' : 'Ingresa la fecha';
  quickErrors.horario = quickForm.horario ? '' : 'Selecciona el horario';
  quickErrors.cantidad = quickForm.cantidad && quickForm.cantidad > 0 ? '' : 'Ingresa una cantidad válida';

  return !quickErrors.idganado && !quickErrors.fecha && !quickErrors.horario && !quickErrors.cantidad;
}

async function handleQuickProduction() {
  if (!validateQuickForm()) return;

  saving.value = true;
  try {
    // Mock API call - replace with real API
    const newRecord = {
      id: produccionRecords.value.length + 1,
      idganado: parseInt(quickForm.idganado),
      fecha: quickForm.fecha,
      horario: quickForm.horario,
      cantidad: parseFloat(quickForm.cantidad).toFixed(1),
      observaciones: quickForm.observaciones || ''
    };

    produccionRecords.value.unshift(newRecord);
    pushToast('Producción registrada exitosamente', 'success');
    resetQuickForm();

  } catch (error) {
    pushToast(error.message || 'Error al registrar producción', 'error');
  } finally {
    saving.value = false;
  }
}

function resetQuickForm() {
  Object.assign(quickForm, {
    idganado: '',
    fecha: today.value,
    horario: '',
    cantidad: '',
    observaciones: ''
  });
  Object.assign(quickErrors, {
    idganado: '',
    fecha: '',
    horario: '',
    cantidad: ''
  });
}

function editProduction(item) {
  editModal.open = true;
  editModal.id = item.id;
  Object.assign(editModal.form, {
    fecha: item.fecha,
    horario: item.horario,
    cantidad: item.cantidad,
    observaciones: item.observaciones || ''
  });
}

function closeEditModal() {
  editModal.open = false;
  editModal.id = null;
}

async function updateProduction() {
  saving.value = true;
  try {
    // Mock API call - replace with real API
    const index = produccionRecords.value.findIndex(r => r.id === editModal.id);
    if (index !== -1) {
      Object.assign(produccionRecords.value[index], editModal.form);
    }

    pushToast('Producción actualizada exitosamente', 'success');
    closeEditModal();

  } catch (error) {
    pushToast(error.message || 'Error al actualizar producción', 'error');
  } finally {
    saving.value = false;
  }
}

async function deleteProduction(item) {
  if (!confirm(`¿Eliminar el registro de producción del ${formatDate(item.fecha)}?`)) return;

  try {
    // Mock API call - replace with real API
    const index = produccionRecords.value.findIndex(r => r.id === item.id);
    if (index !== -1) {
      produccionRecords.value.splice(index, 1);
    }

    pushToast('Registro eliminado exitosamente', 'success');

  } catch (error) {
    pushToast(error.message || 'Error al eliminar registro', 'error');
  }
}

function openCycleModal() {
  cycleModal.open = true;
}

function closeCycleModal() {
  cycleModal.open = false;
  Object.assign(cycleModal.form, {
    idganado: '',
    fechaInicio: today.value,
    fechaFin: '',
    observaciones: ''
  });
}

async function createCycle() {
  saving.value = true;
  try {
    // Mock API call - replace with real API
    const newCycle = {
      id: ciclosProduccion.value.length + 1,
      idganado: parseInt(cycleModal.form.idganado),
      fechaInicio: cycleModal.form.fechaInicio,
      fechaFin: cycleModal.form.fechaFin,
      cantidadTotal: '0.0',
      observaciones: cycleModal.form.observaciones || ''
    };

    ciclosProduccion.value.unshift(newCycle);
    pushToast('Ciclo de producción creado exitosamente', 'success');
    closeCycleModal();

  } catch (error) {
    pushToast(error.message || 'Error al crear ciclo', 'error');
  } finally {
    saving.value = false;
  }
}

function viewCycleDetails(cycle) {
  pushToast(`Ver detalles del ciclo ${cycle.id} - Funcionalidad en desarrollo`, 'info');
}

function closeCycle(cycle) {
  if (confirm(`¿Cerrar el ciclo de producción? Esta acción no se puede deshacer.`)) {
    pushToast(`Ciclo ${cycle.id} cerrado exitosamente`, 'success');
  }
}

function exportProduction() {
  pushToast('Exportando datos de producción...', 'info');
  // Mock export functionality
  setTimeout(() => {
    pushToast('Archivo exportado exitosamente', 'success');
  }, 1500);
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

function getHorarioClass(horario) {
  const classes = {
    'Mañana': 'horario-morning',
    'Tarde': 'horario-afternoon',
    'Noche': 'horario-evening',
    'Madrugada': 'horario-night'
  };
  return classes[horario] || '';
}

function calculateDuration(inicio, fin) {
  if (!inicio || !fin) return '-';
  const start = new Date(inicio);
  const end = new Date(fin);
  const diffTime = Math.abs(end - start);
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays;
}

function getCycleStatus(cycle) {
  const today = new Date();
  const endDate = new Date(cycle.fechaFin);

  if (endDate > today) return 'status-active';
  return 'status-completed';
}

function getCycleStatusText(cycle) {
  const today = new Date();
  const endDate = new Date(cycle.fechaFin);

  if (endDate > today) return 'Activo';
  return 'Completado';
}

onMounted(() => {
  loadProduccionData();
});
</script>

<style scoped>
.produccion-container {
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

.quantity-badge {
  background: var(--brand-primary);
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-weight: 600;
  font-size: 0.875rem;
}

.total-badge {
  background: var(--brand-success);
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-weight: 600;
  font-size: 0.875rem;
}

.horario-badge {
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.horario-morning { background: #fef3c7; color: #92400e; }
.horario-afternoon { background: #fed7aa; color: #c2410c; }
.horario-evening { background: #e0e7ff; color: #3730a3; }
.horario-night { background: #e5e7eb; color: #374151; }

.periodo-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.separator {
  color: var(--color-text-muted);
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.status-active {
  background: #dcfce7;
  color: #166534;
}

.status-completed {
  background: #f3f4f6;
  color: #6b7280;
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
  }

  .section-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
}
</style>