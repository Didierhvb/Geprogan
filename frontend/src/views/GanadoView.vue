<template>
  <section class="surface-panel" style="display: flex; flex-direction: column; gap: 1.5rem;">
    <header style="display: flex; flex-wrap: wrap; gap: 1.25rem; justify-content: space-between; align-items: flex-start;">
      <div>
        <div class="stat-chip">Inventario vivo</div>
        <h1 class="hero-title" style="font-size: 2.2rem;">Control de ganado</h1>
        <p class="hero-subtitle">Sincroniza nacimientos, tipos y ubicaciones sin abandonar tu flujo de trabajo.</p>
      </div>
      <span v-if="ganado.length" class="stat-chip">{{ ganado.length }} registros</span>
    </header>

    <!-- Create form using BaseForm -->
    <BaseForm
      title="Registrar nuevo ganado"
      layout="three"
      :loading="saving"
      :is-valid="isCreateFormValid"
      @submit="handleCreate"
    >
      <BaseFormField
        v-model="createForm.idfinca"
        label="Finca"
        type="select"
        :options="fincas"
        :error="createErrors.idfinca"
        required
      />

      <BaseFormField
        v-model="createForm.idtipoGanado"
        label="Tipo"
        type="select"
        :options="tipos"
        :error="createErrors.idtipoGanado"
        required
      />

      <BaseFormField
        v-model="createForm.fechaNacimiento"
        label="Fecha nacimiento"
        type="date"
        :error="createErrors.fechaNacimiento"
        required
      />

      <BaseFormField
        v-model="createForm.sexo"
        label="Sexo"
        type="select"
        :options="sexoOptions"
        :error="createErrors.sexo"
        required
      />

      <BaseFormField
        v-model="createForm.nombreGanado"
        label="Nombre"
        help="Nombre opcional del animal"
      />

      <BaseFormField
        v-model="createForm.raza"
        label="Raza"
        help="Raza del ganado"
      />

      <BaseFormField
        v-model="createForm.marcaGanado"
        label="Marca"
        help="Marca identificativa"
      />

      <BaseFormField
        v-model="createForm.caracteristicas"
        label="Características"
        help="Características distintivas"
      />

      <BaseFormField
        v-model="createForm.numeroId"
        label="Número ID"
        type="number"
        min="0"
      />

      <BaseFormField
        v-model="createForm.numeroInventario"
        label="Inventario"
        type="number"
        min="0"
      />

      <BaseFormField
        v-model="createForm.urlImagen"
        label="URL imagen"
        type="url"
        placeholder="https://ejemplo.com/imagen.jpg"
        help="URL de imagen del animal"
      />
    </BaseForm>
  </section>

  <!-- Data table using BaseTable -->
  <BaseTable
    :data="ganado"
    :columns="ganadoColumns"
    :loading="loading"
    title="Ganado registrado"
    empty-title="Sin ganado registrado"
    empty-message="No hay animales registrados en el sistema. Agrega el primer registro usando el formulario anterior."
    :page-size="15"
  >
    <template #actions>
      <button class="btn btn-outline" @click="loadData">
        <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
        Actualizar
      </button>
    </template>

    <template #cell-nombre="{ value }">
      {{ value || '-' }}
    </template>

    <template #cell-raza="{ value }">
      {{ value || '-' }}
    </template>

    <template #cell-finca="{ item }">
      {{ lookupFinca(item.idfinca) }}
    </template>

    <template #cell-tipo="{ item }">
      {{ lookupTipo(item.idtipoGanado) }}
    </template>

    <template #cell-fechaNacimiento="{ value }">
      {{ formatDate(value) }}
    </template>

    <template #row-actions="{ item }">
      <button
        class="btn btn-outline btn-sm"
        @click="openEdit(item)"
        title="Editar registro"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
        </svg>
      </button>
      <button
        class="btn btn-ghost btn-sm"
        @click="deleteGanado(item)"
        title="Eliminar registro"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
        </svg>
      </button>
    </template>

    <template #empty-actions>
      <button class="btn btn-primary" @click="resetCreate">
        Crear primer registro
      </button>
    </template>
  </BaseTable>

  <!-- Edit modal using BaseModal -->
  <BaseModal
    v-model="editState.open"
    title="Editar ganado"
    subtitle="Modifica los datos del registro seleccionado"
    size="lg"
    show-footer
    :loading="saving"
    @confirm="handleUpdate"
    @cancel="closeEdit"
    confirm-text="Actualizar"
  >
    <BaseForm
      layout="three"
      :show-actions="false"
      :loading="saving"
    >
      <BaseFormField
        v-model="editState.form.idfinca"
        label="Finca"
        type="select"
        :options="fincas"
        required
      />

      <BaseFormField
        v-model="editState.form.idtipoGanado"
        label="Tipo"
        type="select"
        :options="tipos"
        required
      />

      <BaseFormField
        v-model="editState.form.fechaNacimiento"
        label="Fecha nacimiento"
        type="date"
        required
      />

      <BaseFormField
        v-model="editState.form.sexo"
        label="Sexo"
        type="select"
        :options="sexoOptions"
        required
      />

      <BaseFormField
        v-model="editState.form.nombreGanado"
        label="Nombre"
      />

      <BaseFormField
        v-model="editState.form.raza"
        label="Raza"
      />

      <BaseFormField
        v-model="editState.form.marcaGanado"
        label="Marca"
      />

      <BaseFormField
        v-model="editState.form.caracteristicas"
        label="Características"
      />

      <BaseFormField
        v-model="editState.form.numeroId"
        label="Número ID"
        type="number"
        min="0"
      />

      <BaseFormField
        v-model="editState.form.numeroInventario"
        label="Inventario"
        type="number"
        min="0"
      />

      <BaseFormField
        v-model="editState.form.urlImagen"
        label="URL imagen"
        type="url"
      />
    </BaseForm>
  </BaseModal>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';
import { ganadoApi } from '@/services/api.js';
import { BaseForm, BaseFormField, BaseTable, BaseModal } from '@/components/base';

const { token } = useAuth();
const { pushToast } = useToasts();

const ganado = ref([]);
const fincas = ref([]);
const tipos = ref([]);
const loading = ref(true);
const saving = ref(false);

// Options for select fields
const sexoOptions = [
  { value: 'M', label: 'Macho' },
  { value: 'H', label: 'Hembra' }
];

// Table columns configuration
const ganadoColumns = [
  { key: 'id', label: 'ID', sortable: true },
  { key: 'nombre', label: 'Nombre', sortable: true },
  { key: 'sexo', label: 'Sexo', sortable: true },
  { key: 'raza', label: 'Raza', sortable: true },
  { key: 'finca', label: 'Finca' },
  { key: 'tipo', label: 'Tipo' },
  { key: 'fechaNacimiento', label: 'Nacimiento', sortable: true }
];

const createForm = reactive({
  idfinca: '',
  idtipoGanado: '',
  fechaNacimiento: '',
  sexo: '',
  marcaGanado: '',
  raza: '',
  caracteristicas: '',
  nombreGanado: '',
  numeroId: null,
  numeroInventario: null,
  urlImagen: ''
});

const createErrors = reactive({
  idfinca: '',
  idtipoGanado: '',
  fechaNacimiento: '',
  sexo: ''
});

const editState = reactive({
  open: false,
  id: null,
  form: {
    idfinca: '',
    idtipoGanado: '',
    fechaNacimiento: '',
    sexo: '',
    marcaGanado: '',
    raza: '',
    caracteristicas: '',
    nombreGanado: '',
    numeroId: null,
    numeroInventario: null,
    urlImagen: ''
  }
});

// Computed
const isCreateFormValid = computed(() => {
  return createForm.idfinca &&
         createForm.idtipoGanado &&
         createForm.fechaNacimiento &&
         createForm.sexo;
});

// Utility functions
function normalizeGanado(raw) {
  return {
    id: raw.idganado ?? raw.id ?? null,
    idfinca: raw.idfinca ?? raw.Idfinca ?? null,
    idtipoGanado: raw.idtipoGanado ?? raw.IdtipoGanado ?? raw.idtipo ?? null,
    fechaNacimiento: raw.fechaNacimiento ?? '',
    sexo: raw.sexo ?? '',
    marcaGanado: raw.marcaGanado ?? raw.marca ?? '',
    raza: raw.raza ?? '',
    caracteristicas: raw.caracteristicas ?? '',
    nombre: raw.nombreGanado ?? raw.nombre ?? '',
    nombreGanado: raw.nombreGanado ?? raw.nombre ?? '',
    numeroId: raw.numeroId ?? null,
    numeroInventario: raw.numeroInventario ?? null,
    urlImagen: raw.urlImagen ?? ''
  };
}

function normalizeFinca(raw) {
  const id = raw.idfinca ?? raw.id ?? null;
  const name = raw.nombreFinca ?? raw.nombre ?? '';
  return { value: id, label: `${id} - ${name}`.trim() };
}

function normalizeTipo(raw) {
  const id = raw.id ?? raw.idtipo ?? raw.idTipo ?? null;
  const name = raw.nombre ?? raw.descripcion ?? '';
  return { value: id, label: `${id} - ${name}`.trim() };
}

function lookupFinca(id) {
  const data = fincas.value.find((item) => String(item.value) === String(id));
  return data ? data.label : id;
}

function lookupTipo(id) {
  const data = tipos.value.find((item) => String(item.value) === String(id));
  return data ? data.label : id;
}

function formatDate(date) {
  if (!date) return '-';
  try {
    return new Date(date).toLocaleDateString('es-ES');
  } catch {
    return date;
  }
}

function validateCreate() {
  createErrors.idfinca = createForm.idfinca ? '' : 'Selecciona la finca';
  createErrors.idtipoGanado = createForm.idtipoGanado ? '' : 'Selecciona el tipo';
  createErrors.fechaNacimiento = createForm.fechaNacimiento ? '' : 'Define la fecha';
  createErrors.sexo = createForm.sexo ? '' : 'Selecciona el sexo';
  return !createErrors.idfinca && !createErrors.idtipoGanado && !createErrors.fechaNacimiento && !createErrors.sexo;
}

async function loadData() {
  loading.value = true;
  try {
    const authToken = token.value;
    const [list, fincasList, tiposList] = await Promise.all([
      ganadoApi.list(authToken),
      ganadoApi.fincas(authToken),
      ganadoApi.tipos(authToken)
    ]);
    ganado.value = list.map(normalizeGanado).filter((item) => item.id);
    fincas.value = fincasList.map(normalizeFinca).filter((item) => item.value);
    tipos.value = tiposList.map(normalizeTipo).filter((item) => item.value);
  } catch (error) {
    pushToast(error.message || 'No se pudo cargar el ganado', 'error');
  } finally {
    loading.value = false;
  }
}

function buildDto(base) {
  return {
    idfinca: Number(base.idfinca),
    idtipoGanado: Number(base.idtipoGanado),
    fechaNacimiento: base.fechaNacimiento,
    sexo: base.sexo,
    marcaGanado: base.marcaGanado || null,
    raza: base.raza || null,
    caracteristicas: base.caracteristicas || null,
    nombreGanado: base.nombreGanado || null,
    numeroId: base.numeroId !== null && base.numeroId !== '' ? Number(base.numeroId) : null,
    numeroInventario: base.numeroInventario !== null && base.numeroInventario !== '' ? Number(base.numeroInventario) : null,
    urlImagen: base.urlImagen || null
  };
}

async function handleCreate() {
  if (!validateCreate()) {
    return;
  }
  saving.value = true;
  try {
    const dto = buildDto(createForm);
    const created = await ganadoApi.create(dto, token.value);
    pushToast(`Ganado ${created?.idganado ?? created?.id ?? ''} creado`, 'success');
    resetCreate();
    await loadData();
  } catch (error) {
    pushToast(error.message || 'No se pudo crear el registro', 'error');
  } finally {
    saving.value = false;
  }
}

function resetCreate() {
  Object.assign(createForm, {
    idfinca: '',
    idtipoGanado: '',
    fechaNacimiento: '',
    sexo: '',
    marcaGanado: '',
    raza: '',
    caracteristicas: '',
    nombreGanado: '',
    numeroId: null,
    numeroInventario: null,
    urlImagen: ''
  });

  Object.assign(createErrors, {
    idfinca: '',
    idtipoGanado: '',
    fechaNacimiento: '',
    sexo: ''
  });
}

async function openEdit(item) {
  try {
    const detail = await ganadoApi.details(item.id, token.value);
    const normalized = normalizeGanado(detail);
    editState.open = true;
    editState.id = normalized.id;
    Object.assign(editState.form, {
      idfinca: normalized.idfinca,
      idtipoGanado: normalized.idtipoGanado,
      fechaNacimiento: normalized.fechaNacimiento,
      sexo: normalized.sexo,
      marcaGanado: normalized.marcaGanado,
      raza: normalized.raza,
      caracteristicas: normalized.caracteristicas,
      nombreGanado: normalized.nombreGanado,
      numeroId: normalized.numeroId,
      numeroInventario: normalized.numeroInventario,
      urlImagen: normalized.urlImagen
    });
  } catch (error) {
    pushToast(error.message || 'No se pudo cargar el registro', 'error');
  }
}

function closeEdit() {
  editState.open = false;
  editState.id = null;
}

async function handleUpdate() {
  saving.value = true;
  try {
    const dto = buildDto(editState.form);
    await ganadoApi.update(editState.id, dto, token.value);
    pushToast('Ganado actualizado', 'success');
    closeEdit();
    await loadData();
  } catch (error) {
    pushToast(error.message || 'No se pudo actualizar', 'error');
  } finally {
    saving.value = false;
  }
}

async function deleteGanado(item) {
  if (!confirm(`¿Eliminar el registro ${item.id}? Esta acción no se puede deshacer.`)) {
    return;
  }
  try {
    await ganadoApi.remove(item.id, token.value);
    pushToast('Registro eliminado', 'success');
    await loadData();
  } catch (error) {
    pushToast(error.message || 'No se pudo eliminar', 'error');
  }
}

onMounted(() => {
  loadData();
});
</script>

<style scoped>
.w-4 {
  width: 1rem;
}

.h-4 {
  height: 1rem;
}

.mr-2 {
  margin-right: 0.5rem;
}
</style>