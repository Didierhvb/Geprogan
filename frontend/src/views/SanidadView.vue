<template>
  <div class="sanidad-container">
    <!-- Header Section -->
    <section class="surface-panel">
      <div class="header-section">
        <div class="header-content">
          <div class="stat-chip">Control sanitario</div>
          <h1 class="hero-title">Gestión de Sanidad</h1>
          <p class="hero-subtitle">
            Controla vacunaciones, tratamientos médicos y el historial sanitario completo de tu ganado.
          </p>
        </div>
        <div class="header-stats">
          <div class="stat-item">
            <span class="stat-value">{{ tratamientosHoy }}</span>
            <span class="stat-label">Tratamientos hoy</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ proximasVacunas }}</span>
            <span class="stat-label">Vacunas pendientes</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Quick Health Record -->
    <section class="surface-panel">
      <BaseForm
        title="Registro Sanitario Rápido"
        subtitle="Registra tratamientos, vacunas y observaciones médicas"
        layout="three"
        :loading="saving"
        :is-valid="isHealthFormValid"
        submit-text="Registrar Tratamiento"
        @submit="handleHealthRecord"
      >
        <BaseFormField
          v-model="healthForm.idganado"
          label="Animal"
          type="select"
          :options="ganadoOptions"
          :error="healthErrors.idganado"
          required
          help="Selecciona el animal a tratar"
        />

        <BaseFormField
          v-model="healthForm.fecha"
          label="Fecha del Tratamiento"
          type="date"
          :max="today"
          :error="healthErrors.fecha"
          required
        />

        <BaseFormField
          v-model="healthForm.tipoTratamiento"
          label="Tipo de Tratamiento"
          type="select"
          :options="tipoTratamientoOptions"
          :error="healthErrors.tipoTratamiento"
          required
        />

        <BaseFormField
          v-model="healthForm.producto"
          label="Producto/Medicamento"
          type="select"
          :options="productoOptions"
          help="Selecciona el producto aplicado"
        />

        <BaseFormField
          v-model="healthForm.descripcion"
          label="Descripción del Tratamiento"
          :error="healthErrors.descripcion"
          required
          help="Describe brevemente el tratamiento"
        />

        <BaseFormField
          v-model="healthForm.dosis"
          label="Dosis Aplicada"
          help="Cantidad de producto aplicado (ml, cc, etc.)"
        />

        <BaseFormField
          v-model="healthForm.observaciones"
          label="Observaciones Médicas"
          type="textarea"
          :rows="2"
          help="Notas adicionales del veterinario"
        />

        <BaseFormField
          v-model="healthForm.proximaFecha"
          label="Próxima Aplicación"
          type="date"
          :min="healthForm.fecha"
          help="Fecha de la próxima dosis (opcional)"
        />
      </BaseForm>
    </section>

    <!-- Health Records Table -->
    <BaseTable
      :data="sanidadRecords"
      :columns="sanidadColumns"
      :loading="loading"
      title="Historial Sanitario"
      empty-title="Sin registros sanitarios"
      empty-message="No hay tratamientos registrados. Comienza registrando vacunas y tratamientos médicos."
      :page-size="15"
      :search-placeholder="'Buscar por animal, tratamiento o producto...'"
    >
      <template #actions>
        <button class="btn btn-outline" @click="openCalendarModal">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          Calendario Sanitario
        </button>
        <button class="btn btn-outline" @click="generateHealthReport">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          Reporte Sanitario
        </button>
        <button class="btn btn-primary" @click="openProductModal">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          Nuevo Producto
        </button>
        <button class="btn btn-outline" @click="loadSanidadData">
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

      <template #cell-tipo="{ value }">
        <span class="treatment-badge" :class="getTreatmentClass(value)">
          {{ value }}
        </span>
      </template>

      <template #cell-producto="{ item }">
        {{ getProductName(item.producto) }}
      </template>

      <template #cell-estado="{ item }">
        <span class="status-badge" :class="getHealthStatus(item)">
          {{ getHealthStatusText(item) }}
        </span>
      </template>

      <template #cell-proximaFecha="{ value }">
        <span v-if="value" class="next-date" :class="{ 'overdue': isOverdue(value) }">
          {{ formatDate(value) }}
        </span>
        <span v-else class="no-date">-</span>
      </template>

      <template #row-actions="{ item }">
        <button
          class="btn btn-outline btn-sm"
          @click="viewHealthHistory(item)"
          title="Ver historial completo"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </button>
        <button
          class="btn btn-outline btn-sm"
          @click="editHealthRecord(item)"
          title="Editar tratamiento"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
        </button>
        <button
          v-if="item.proximaFecha"
          class="btn btn-success btn-sm"
          @click="scheduleNextTreatment(item)"
          title="Programar siguiente dosis"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </button>
      </template>

      <template #empty-actions>
        <button class="btn btn-primary" @click="resetHealthForm">
          Registrar Primer Tratamiento
        </button>
      </template>
    </BaseTable>

    <!-- Products Management Section -->
    <section class="surface-panel">
      <header class="section-header">
        <div>
          <h2 class="section-title">Productos de Sanidad</h2>
          <p class="section-subtitle">Gestiona el inventario de medicamentos y vacunas</p>
        </div>
        <button class="btn btn-outline" @click="openProductModal">
          Nuevo Producto
        </button>
      </header>

      <BaseTable
        :data="productos"
        :columns="productosColumns"
        :loading="loadingProductos"
        title=""
        :show-header="false"
        empty-title="Sin productos registrados"
        empty-message="No hay productos de sanidad registrados. Agrega medicamentos y vacunas a tu inventario."
        :page-size="10"
      >
        <template #cell-contenido="{ value }">
          <span class="content-badge">{{ value }}</span>
        </template>

        <template #cell-tipo="{ value }">
          <span class="product-type-badge" :class="getProductTypeClass(value)">
            {{ value }}
          </span>
        </template>

        <template #cell-stock="{ item }">
          <span class="stock-badge" :class="getStockClass(item.stock, item.stockMinimo)">
            {{ item.stock || 0 }} {{ item.unidad || 'unid.' }}
          </span>
        </template>

        <template #row-actions="{ item }">
          <button
            class="btn btn-outline btn-sm"
            @click="editProduct(item)"
            title="Editar producto"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
          </button>
          <button
            class="btn btn-success btn-sm"
            @click="updateStock(item)"
            title="Actualizar stock"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
          </button>
        </template>
      </BaseTable>
    </section>

    <!-- Edit Health Record Modal -->
    <BaseModal
      v-model="editModal.open"
      title="Editar Registro Sanitario"
      subtitle="Modifica los datos del tratamiento"
      size="lg"
      show-footer
      :loading="saving"
      @confirm="updateHealthRecord"
      @cancel="closeEditModal"
      confirm-text="Actualizar"
    >
      <BaseForm layout="two" :show-actions="false">
        <BaseFormField
          v-model="editModal.form.fecha"
          label="Fecha del Tratamiento"
          type="date"
          :max="today"
          required
        />

        <BaseFormField
          v-model="editModal.form.tipoTratamiento"
          label="Tipo de Tratamiento"
          type="select"
          :options="tipoTratamientoOptions"
          required
        />

        <BaseFormField
          v-model="editModal.form.producto"
          label="Producto/Medicamento"
          type="select"
          :options="productoOptions"
        />

        <BaseFormField
          v-model="editModal.form.dosis"
          label="Dosis Aplicada"
        />

        <BaseFormField
          v-model="editModal.form.descripcion"
          label="Descripción del Tratamiento"
          required
        />

        <BaseFormField
          v-model="editModal.form.proximaFecha"
          label="Próxima Aplicación"
          type="date"
          :min="editModal.form.fecha"
        />

        <BaseFormField
          v-model="editModal.form.observaciones"
          label="Observaciones Médicas"
          type="textarea"
          :rows="3"
        />
      </BaseForm>
    </BaseModal>

    <!-- New Product Modal -->
    <BaseModal
      v-model="productModal.open"
      title="Nuevo Producto de Sanidad"
      subtitle="Agregar medicamento o vacuna al inventario"
      size="md"
      show-footer
      :loading="saving"
      @confirm="createProduct"
      @cancel="closeProductModal"
      confirm-text="Crear Producto"
    >
      <BaseForm layout="two" :show-actions="false">
        <BaseFormField
          v-model="productModal.form.nombre"
          label="Nombre del Producto"
          required
          help="Nombre comercial del medicamento/vacuna"
        />

        <BaseFormField
          v-model="productModal.form.tipo"
          label="Tipo de Producto"
          type="select"
          :options="tipoProductoOptions"
          required
        />

        <BaseFormField
          v-model="productModal.form.contenido"
          label="Contenido"
          help="Ej: 50ml, 100cc, 10 dosis"
        />

        <BaseFormField
          v-model="productModal.form.unidad"
          label="Unidad de Medida"
          type="select"
          :options="unidadOptions"
        />

        <BaseFormField
          v-model="productModal.form.stock"
          label="Stock Inicial"
          type="number"
          min="0"
        />

        <BaseFormField
          v-model="productModal.form.stockMinimo"
          label="Stock Mínimo"
          type="number"
          min="0"
          help="Cantidad mínima para alerta"
        />

        <BaseFormField
          v-model="productModal.form.laboratorio"
          label="Laboratorio"
          help="Fabricante del producto"
        />

        <BaseFormField
          v-model="productModal.form.observaciones"
          label="Observaciones"
          type="textarea"
          :rows="2"
        />
      </BaseForm>
    </BaseModal>

    <!-- Health Calendar Modal -->
    <BaseModal
      v-model="calendarModal.open"
      title="Calendario Sanitario"
      subtitle="Próximos tratamientos y vacunaciones programadas"
      size="xl"
      show-footer
      :show-confirm="false"
      cancel-text="Cerrar"
      @cancel="closeCalendarModal"
    >
      <div class="calendar-content">
        <div class="calendar-filters">
          <BaseFormField
            v-model="calendarFilter.mes"
            label="Mes"
            type="select"
            :options="mesesOptions"
          />
          <BaseFormField
            v-model="calendarFilter.tipo"
            label="Tipo de Tratamiento"
            type="select"
            :options="[{ value: '', label: 'Todos' }, ...tipoTratamientoOptions]"
          />
        </div>

        <div class="calendar-events">
          <div v-if="!proximosTratamientos.length" class="no-events">
            <p>No hay tratamientos programados para este período</p>
          </div>
          <div v-else class="events-list">
            <div
              v-for="event in proximosTratamientos"
              :key="event.id"
              class="event-item"
              :class="{ 'overdue': isOverdue(event.fecha) }"
            >
              <div class="event-date">
                <span class="day">{{ new Date(event.fecha).getDate() }}</span>
                <span class="month">{{ getMonthName(new Date(event.fecha).getMonth()) }}</span>
              </div>
              <div class="event-content">
                <h4 class="event-title">{{ event.tipo }}</h4>
                <p class="event-animal">{{ getAnimalName(event.idganado) }}</p>
                <p class="event-description">{{ event.descripcion }}</p>
              </div>
              <div class="event-actions">
                <button class="btn btn-primary btn-sm" @click="completeScheduledTreatment(event)">
                  Marcar como Realizado
                </button>
              </div>
            </div>
          </div>
        </div>
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
const loadingProductos = ref(true);
const saving = ref(false);

// Data
const sanidadRecords = ref([]);
const productos = ref([]);
const ganadoList = ref([]);
const proximosTratamientos = ref([]);

// Computed
const today = computed(() => new Date().toISOString().split('T')[0]);

const ganadoOptions = computed(() =>
  ganadoList.value.map(animal => ({
    value: animal.id,
    label: `${animal.nombre || `Animal ${animal.id}`} - ${animal.raza || 'Sin raza'}`
  }))
);

const productoOptions = computed(() =>
  productos.value.map(producto => ({
    value: producto.id,
    label: `${producto.nombre} - ${producto.contenido || ''}`
  }))
);

const tratamientosHoy = computed(() => {
  const hoy = new Date().toISOString().split('T')[0];
  return sanidadRecords.value.filter(record => record.fecha === hoy).length;
});

const proximasVacunas = computed(() => {
  const hoy = new Date();
  const proximosMes = new Date();
  proximosMes.setMonth(proximosMes.getMonth() + 1);

  return sanidadRecords.value.filter(record => {
    if (!record.proximaFecha) return false;
    const fecha = new Date(record.proximaFecha);
    return fecha >= hoy && fecha <= proximosMes;
  }).length;
});

const isHealthFormValid = computed(() => {
  return healthForm.idganado &&
         healthForm.fecha &&
         healthForm.tipoTratamiento &&
         healthForm.descripcion;
});

// Options
const tipoTratamientoOptions = [
  { value: 'Vacunación', label: 'Vacunación' },
  { value: 'Desparasitación', label: 'Desparasitación' },
  { value: 'Antibiótico', label: 'Tratamiento con Antibiótico' },
  { value: 'Vitaminas', label: 'Aplicación de Vitaminas' },
  { value: 'Antiinflamatorio', label: 'Antiinflamatorio' },
  { value: 'Cicatrizante', label: 'Tratamiento Cicatrizante' },
  { value: 'Preventivo', label: 'Tratamiento Preventivo' },
  { value: 'Curativo', label: 'Tratamiento Curativo' },
  { value: 'Otro', label: 'Otro Tratamiento' }
];

const tipoProductoOptions = [
  { value: 'Vacuna', label: 'Vacuna' },
  { value: 'Antibiótico', label: 'Antibiótico' },
  { value: 'Desparasitante', label: 'Desparasitante' },
  { value: 'Vitamina', label: 'Vitamina/Suplemento' },
  { value: 'Antiinflamatorio', label: 'Antiinflamatorio' },
  { value: 'Cicatrizante', label: 'Cicatrizante' },
  { value: 'Anestésico', label: 'Anestésico' },
  { value: 'Otro', label: 'Otro' }
];

const unidadOptions = [
  { value: 'ml', label: 'Mililitros (ml)' },
  { value: 'cc', label: 'Centímetros cúbicos (cc)' },
  { value: 'dosis', label: 'Dosis' },
  { value: 'unidad', label: 'Unidad' },
  { value: 'gramos', label: 'Gramos' },
  { value: 'comprimidos', label: 'Comprimidos' }
];

const mesesOptions = [
  { value: 0, label: 'Enero' }, { value: 1, label: 'Febrero' }, { value: 2, label: 'Marzo' },
  { value: 3, label: 'Abril' }, { value: 4, label: 'Mayo' }, { value: 5, label: 'Junio' },
  { value: 6, label: 'Julio' }, { value: 7, label: 'Agosto' }, { value: 8, label: 'Septiembre' },
  { value: 9, label: 'Octubre' }, { value: 10, label: 'Noviembre' }, { value: 11, label: 'Diciembre' }
];

// Forms
const healthForm = reactive({
  idganado: '',
  fecha: today.value,
  tipoTratamiento: '',
  producto: '',
  descripcion: '',
  dosis: '',
  observaciones: '',
  proximaFecha: ''
});

const healthErrors = reactive({
  idganado: '',
  fecha: '',
  tipoTratamiento: '',
  descripcion: ''
});

const editModal = reactive({
  open: false,
  id: null,
  form: {
    fecha: '',
    tipoTratamiento: '',
    producto: '',
    descripcion: '',
    dosis: '',
    observaciones: '',
    proximaFecha: ''
  }
});

const productModal = reactive({
  open: false,
  form: {
    nombre: '',
    tipo: '',
    contenido: '',
    unidad: 'ml',
    stock: 0,
    stockMinimo: 5,
    laboratorio: '',
    observaciones: ''
  }
});

const calendarModal = reactive({
  open: false
});

const calendarFilter = reactive({
  mes: new Date().getMonth(),
  tipo: ''
});

// Table columns
const sanidadColumns = [
  { key: 'animal', label: 'Animal' },
  { key: 'fecha', label: 'Fecha', sortable: true },
  { key: 'tipo', label: 'Tratamiento', sortable: true },
  { key: 'producto', label: 'Producto' },
  { key: 'descripcion', label: 'Descripción' },
  { key: 'proximaFecha', label: 'Próxima Dosis' },
  { key: 'estado', label: 'Estado' }
];

const productosColumns = [
  { key: 'nombre', label: 'Producto', sortable: true },
  { key: 'tipo', label: 'Tipo', sortable: true },
  { key: 'contenido', label: 'Contenido' },
  { key: 'stock', label: 'Stock Actual' },
  { key: 'laboratorio', label: 'Laboratorio' }
];

// Methods
async function loadSanidadData() {
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

    // Mock health data - replace with real API calls
    await loadMockHealthData();

  } catch (error) {
    pushToast(error.message || 'Error al cargar datos sanitarios', 'error');
  } finally {
    loading.value = false;
  }
}

async function loadMockHealthData() {
  // Mock health records
  const mockHealth = [];
  const animals = ganadoList.value.slice(0, 5);

  for (let i = 0; i < 15; i++) {
    const randomAnimal = animals[Math.floor(Math.random() * animals.length)];
    const date = new Date();
    date.setDate(date.getDate() - Math.floor(Math.random() * 90));

    const tipos = tipoTratamientoOptions.map(t => t.value);
    const tipoRandom = tipos[Math.floor(Math.random() * tipos.length)];

    mockHealth.push({
      id: i + 1,
      idganado: randomAnimal?.id || 1,
      fecha: date.toISOString().split('T')[0],
      tipoTratamiento: tipoRandom,
      producto: Math.floor(Math.random() * 5) + 1,
      descripcion: `${tipoRandom} de rutina`,
      dosis: `${(Math.random() * 10 + 5).toFixed(1)} ml`,
      observaciones: Math.random() > 0.5 ? 'Aplicado sin complicaciones' : '',
      proximaFecha: Math.random() > 0.6 ? new Date(Date.now() + Math.random() * 60 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] : null
    });
  }

  sanidadRecords.value = mockHealth.sort((a, b) => new Date(b.fecha) - new Date(a.fecha));

  // Mock products
  const mockProducts = [
    { id: 1, nombre: 'Vacuna Triple', tipo: 'Vacuna', contenido: '50ml', laboratorio: 'Zoetis', stock: 10, stockMinimo: 5, unidad: 'ml' },
    { id: 2, nombre: 'Ivermectina', tipo: 'Desparasitante', contenido: '100ml', laboratorio: 'MSD', stock: 3, stockMinimo: 5, unidad: 'ml' },
    { id: 3, nombre: 'Penicilina', tipo: 'Antibiótico', contenido: '50ml', laboratorio: 'Pfizer', stock: 8, stockMinimo: 3, unidad: 'ml' },
    { id: 4, nombre: 'Complejo B', tipo: 'Vitamina', contenido: '100ml', laboratorio: 'Bayer', stock: 15, stockMinimo: 10, unidad: 'ml' },
    { id: 5, nombre: 'Antiinflamatorio', tipo: 'Antiinflamatorio', contenido: '50ml', laboratorio: 'Virbac', stock: 6, stockMinimo: 5, unidad: 'ml' }
  ];

  productos.value = mockProducts;
  loadingProductos.value = false;

  // Generate upcoming treatments
  generateUpcomingTreatments();
}

function generateUpcomingTreatments() {
  const upcoming = [];
  const baseDate = new Date();

  for (let i = 0; i < 10; i++) {
    const date = new Date(baseDate);
    date.setDate(date.getDate() + Math.floor(Math.random() * 30));

    const randomAnimal = ganadoList.value[Math.floor(Math.random() * ganadoList.value.length)];
    const tipos = tipoTratamientoOptions.map(t => t.value);

    upcoming.push({
      id: i + 1,
      idganado: randomAnimal?.id || 1,
      fecha: date.toISOString().split('T')[0],
      tipo: tipos[Math.floor(Math.random() * tipos.length)],
      descripcion: 'Tratamiento programado'
    });
  }

  proximosTratamientos.value = upcoming.sort((a, b) => new Date(a.fecha) - new Date(b.fecha));
}

function validateHealthForm() {
  healthErrors.idganado = healthForm.idganado ? '' : 'Selecciona un animal';
  healthErrors.fecha = healthForm.fecha ? '' : 'Ingresa la fecha';
  healthErrors.tipoTratamiento = healthForm.tipoTratamiento ? '' : 'Selecciona el tipo de tratamiento';
  healthErrors.descripcion = healthForm.descripcion ? '' : 'Describe el tratamiento';

  return !healthErrors.idganado && !healthErrors.fecha && !healthErrors.tipoTratamiento && !healthErrors.descripcion;
}

async function handleHealthRecord() {
  if (!validateHealthForm()) return;

  saving.value = true;
  try {
    const newRecord = {
      id: sanidadRecords.value.length + 1,
      idganado: parseInt(healthForm.idganado),
      fecha: healthForm.fecha,
      tipoTratamiento: healthForm.tipoTratamiento,
      producto: healthForm.producto ? parseInt(healthForm.producto) : null,
      descripcion: healthForm.descripcion,
      dosis: healthForm.dosis || '',
      observaciones: healthForm.observaciones || '',
      proximaFecha: healthForm.proximaFecha || null
    };

    sanidadRecords.value.unshift(newRecord);
    pushToast('Tratamiento registrado exitosamente', 'success');
    resetHealthForm();

  } catch (error) {
    pushToast(error.message || 'Error al registrar tratamiento', 'error');
  } finally {
    saving.value = false;
  }
}

function resetHealthForm() {
  Object.assign(healthForm, {
    idganado: '',
    fecha: today.value,
    tipoTratamiento: '',
    producto: '',
    descripcion: '',
    dosis: '',
    observaciones: '',
    proximaFecha: ''
  });
  Object.assign(healthErrors, {
    idganado: '',
    fecha: '',
    tipoTratamiento: '',
    descripcion: ''
  });
}

function editHealthRecord(item) {
  editModal.open = true;
  editModal.id = item.id;
  Object.assign(editModal.form, item);
}

function closeEditModal() {
  editModal.open = false;
  editModal.id = null;
}

async function updateHealthRecord() {
  saving.value = true;
  try {
    const index = sanidadRecords.value.findIndex(r => r.id === editModal.id);
    if (index !== -1) {
      Object.assign(sanidadRecords.value[index], editModal.form);
    }

    pushToast('Registro actualizado exitosamente', 'success');
    closeEditModal();

  } catch (error) {
    pushToast(error.message || 'Error al actualizar registro', 'error');
  } finally {
    saving.value = false;
  }
}

function openProductModal() {
  productModal.open = true;
}

function closeProductModal() {
  productModal.open = false;
  Object.assign(productModal.form, {
    nombre: '',
    tipo: '',
    contenido: '',
    unidad: 'ml',
    stock: 0,
    stockMinimo: 5,
    laboratorio: '',
    observaciones: ''
  });
}

async function createProduct() {
  saving.value = true;
  try {
    const newProduct = {
      id: productos.value.length + 1,
      ...productModal.form
    };

    productos.value.unshift(newProduct);
    pushToast('Producto creado exitosamente', 'success');
    closeProductModal();

  } catch (error) {
    pushToast(error.message || 'Error al crear producto', 'error');
  } finally {
    saving.value = false;
  }
}

function openCalendarModal() {
  calendarModal.open = true;
}

function closeCalendarModal() {
  calendarModal.open = false;
}

function generateHealthReport() {
  pushToast('Generando reporte sanitario...', 'info');
  setTimeout(() => {
    pushToast('Reporte sanitario generado exitosamente', 'success');
  }, 1500);
}

function viewHealthHistory(item) {
  pushToast(`Ver historial sanitario completo - Animal ${item.idganado}`, 'info');
}

function scheduleNextTreatment(item) {
  pushToast(`Programar siguiente tratamiento para ${formatDate(item.proximaFecha)}`, 'info');
}

function completeScheduledTreatment(event) {
  pushToast(`Marcando tratamiento como realizado...`, 'success');
}

function editProduct(item) {
  pushToast(`Editar producto: ${item.nombre}`, 'info');
}

function updateStock(item) {
  pushToast(`Actualizar stock de: ${item.nombre}`, 'info');
}

// Utility functions
function getAnimalName(animalId) {
  const animal = ganadoList.value.find(a => a.id === animalId);
  return animal ? (animal.nombre || `Animal ${animalId}`) : `Animal ${animalId}`;
}

function getProductName(productId) {
  if (!productId) return '-';
  const product = productos.value.find(p => p.id === productId);
  return product ? product.nombre : `Producto ${productId}`;
}

function formatDate(date) {
  if (!date) return '-';
  try {
    return new Date(date).toLocaleDateString('es-ES');
  } catch {
    return date;
  }
}

function getTreatmentClass(tipo) {
  const classes = {
    'Vacunación': 'treatment-vaccine',
    'Desparasitación': 'treatment-deworming',
    'Antibiótico': 'treatment-antibiotic',
    'Vitaminas': 'treatment-vitamin',
    'Antiinflamatorio': 'treatment-anti-inflammatory'
  };
  return classes[tipo] || 'treatment-other';
}

function getProductTypeClass(tipo) {
  const classes = {
    'Vacuna': 'product-vaccine',
    'Antibiótico': 'product-antibiotic',
    'Desparasitante': 'product-deworming',
    'Vitamina': 'product-vitamin'
  };
  return classes[tipo] || 'product-other';
}

function getStockClass(stock, minimo) {
  if (stock <= minimo) return 'stock-low';
  if (stock <= minimo * 2) return 'stock-medium';
  return 'stock-good';
}

function getHealthStatus(item) {
  if (item.proximaFecha && isOverdue(item.proximaFecha)) return 'status-overdue';
  if (item.proximaFecha) return 'status-scheduled';
  return 'status-completed';
}

function getHealthStatusText(item) {
  if (item.proximaFecha && isOverdue(item.proximaFecha)) return 'Vencido';
  if (item.proximaFecha) return 'Programado';
  return 'Completado';
}

function isOverdue(date) {
  return new Date(date) < new Date();
}

function getMonthName(monthIndex) {
  const months = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
                  'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
  return months[monthIndex];
}

onMounted(() => {
  loadSanidadData();
});
</script>

<style scoped>
.sanidad-container {
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

.treatment-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.treatment-vaccine { background: #dcfce7; color: #166534; }
.treatment-deworming { background: #fef3c7; color: #92400e; }
.treatment-antibiotic { background: #fecaca; color: #991b1b; }
.treatment-vitamin { background: #e0e7ff; color: #3730a3; }
.treatment-anti-inflammatory { background: #fed7aa; color: #c2410c; }
.treatment-other { background: #f3f4f6; color: #6b7280; }

.product-type-badge {
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.product-vaccine { background: #dcfce7; color: #166534; }
.product-antibiotic { background: #fecaca; color: #991b1b; }
.product-deworming { background: #fef3c7; color: #92400e; }
.product-vitamin { background: #e0e7ff; color: #3730a3; }
.product-other { background: #f3f4f6; color: #6b7280; }

.content-badge {
  background: var(--color-surface-muted);
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  color: var(--color-text);
}

.stock-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.stock-good { background: #dcfce7; color: #166534; }
.stock-medium { background: #fef3c7; color: #92400e; }
.stock-low { background: #fecaca; color: #991b1b; }

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.75rem;
  font-weight: 500;
}

.status-completed { background: #dcfce7; color: #166534; }
.status-scheduled { background: #e0e7ff; color: #3730a3; }
.status-overdue { background: #fecaca; color: #991b1b; }

.next-date {
  font-size: 0.875rem;
}

.next-date.overdue {
  color: var(--brand-error);
  font-weight: 600;
}

.no-date {
  color: var(--color-text-muted);
}

.calendar-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.calendar-filters {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.calendar-events {
  max-height: 400px;
  overflow-y: auto;
}

.no-events {
  text-align: center;
  padding: 2rem;
  color: var(--color-text-muted);
}

.events-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.event-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  transition: var(--transition-fast);
}

.event-item:hover {
  box-shadow: var(--shadow-soft);
}

.event-item.overdue {
  border-color: var(--brand-error);
  background: rgba(248, 113, 113, 0.1);
}

.event-date {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 60px;
  padding: 0.75rem;
  background: var(--brand-primary);
  color: white;
  border-radius: var(--radius-md);
}

.day {
  font-size: 1.5rem;
  font-weight: 700;
}

.month {
  font-size: 0.75rem;
  text-transform: uppercase;
}

.event-content {
  flex: 1;
}

.event-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 0.25rem 0;
}

.event-animal {
  font-size: 0.875rem;
  color: var(--color-text-muted);
  margin: 0 0 0.25rem 0;
}

.event-description {
  font-size: 0.875rem;
  color: var(--color-text);
  margin: 0;
}

.event-actions {
  display: flex;
  gap: 0.5rem;
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

  .calendar-filters {
    grid-template-columns: 1fr;
  }

  .event-item {
    flex-direction: column;
    align-items: stretch;
    text-align: center;
  }
}
</style>