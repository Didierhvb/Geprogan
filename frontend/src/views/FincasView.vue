<template>
  <section class="surface-panel" style="display: flex; flex-direction: column; gap: 1.5rem;">
    <header>
      <div class="stat-chip">Registro activo</div>
      <h1 class="hero-title" style="font-size: 2.2rem;">Fincas conectadas</h1>
      <p class="hero-subtitle">Administra ubicaciones, geolocalizacion y propietarios con una interfaz lista para modo oscuro.</p>
    </header>

    <form class="form-grid two" @submit.prevent="handleCreate" novalidate>
      <label>
        <span class="form-label">Nombre</span>
        <input v-model.trim="createForm.nombre" class="form-control" required />
        <span v-if="errors.nombre" class="form-error">{{ errors.nombre }}</span>
      </label>
      <label>
        <span class="form-label">Ubicacion</span>
        <input v-model.trim="createForm.ubicacion" class="form-control" required />
        <span v-if="errors.ubicacion" class="form-error">{{ errors.ubicacion }}</span>
      </label>
      <label>
        <span class="form-label">Hectareas</span>
        <input v-model.number="createForm.hectareas" type="number" min="0" step="0.01" class="form-control" required />
        <span v-if="errors.hectareas" class="form-error">{{ errors.hectareas }}</span>
      </label>
      <label>
        <span class="form-label">Propietario</span>
        <select v-model.number="createForm.propietario" class="select-control" required>
          <option value="">Selecciona</option>
          <option v-for="user in propietarios" :key="user.id" :value="user.id">
            {{ user.id }} - {{ user.nombre }}
          </option>
        </select>
        <span v-if="errors.propietario" class="form-error">{{ errors.propietario }}</span>
      </label>
      <label>
        <span class="form-label">Latitud</span>
        <input v-model.number="createForm.latitud" type="number" step="0.000001" class="form-control" />
      </label>
      <label>
        <span class="form-label">Longitud</span>
        <input v-model.number="createForm.longitud" type="number" step="0.000001" class="form-control" />
      </label>
      <div style="grid-column: 1 / -1; display: flex; gap: 1rem;">
        <button class="btn btn-primary" type="submit" :disabled="saving">{{ saving ? 'Guardando...' : 'Guardar finca' }}</button>
        <button class="btn btn-outline" type="button" @click="resetCreate" :disabled="saving">Limpiar</button>
      </div>
    </form>
  </section>

  <section class="table-panel">
    <header style="display: flex; justify-content: space-between; align-items: center; padding: 1.5rem 1.5rem 0;">
      <h2 style="font-size: 1.2rem;">Fincas registradas</h2>
      <span class="stat-chip" v-if="fincas.length">{{ fincas.length }} activas</span>
    </header>
    <div style="overflow-x: auto;">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Ubicacion</th>
            <th>Hectareas</th>
            <th>Lat</th>
            <th>Lng</th>
            <th>Propietario</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="8">Cargando datos...</td>
          </tr>
          <tr v-for="finca in fincas" :key="finca.id">
            <td>{{ finca.id }}</td>
            <td>{{ finca.nombre }}</td>
            <td>{{ finca.ubicacion }}</td>
            <td>{{ finca.hectareas }}</td>
            <td>{{ finca.latitud ?? '-' }}</td>
            <td>{{ finca.longitud ?? '-' }}</td>
            <td>{{ finca.propietarioNombre ?? '-' }}</td>
            <td style="text-align: right;">
              <button class="btn btn-outline" type="button" @click="openEdit(finca)">Editar</button>
              <button class="btn btn-ghost" type="button" style="margin-left: 0.5rem;" @click="deleteFinca(finca)">Eliminar</button>
            </td>
          </tr>
          <tr v-if="!loading && !fincas.length">
            <td colspan="8">Sin registros por ahora.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <Teleport to="body">
    <div v-if="editState.open" class="modal-backdrop" role="dialog" aria-modal="true">
      <div class="modal-surface">
        <header class="modal-header">
          <h2>Editar finca</h2>
          <button class="btn btn-ghost" type="button" @click="closeEdit">Cerrar</button>
        </header>
        <form class="form-grid two" @submit.prevent="handleUpdate" novalidate>
          <label>
            <span class="form-label">Nombre</span>
            <input v-model.trim="editState.form.nombre" class="form-control" required />
          </label>
          <label>
            <span class="form-label">Ubicacion</span>
            <input v-model.trim="editState.form.ubicacion" class="form-control" required />
          </label>
          <label>
            <span class="form-label">Hectareas</span>
            <input v-model.number="editState.form.hectareas" type="number" step="0.01" min="0" class="form-control" required />
          </label>
          <label>
            <span class="form-label">Propietario</span>
            <select v-model.number="editState.form.propietario" class="select-control" required>
              <option value="">Selecciona</option>
              <option v-for="user in propietarios" :key="user.id" :value="user.id">
                {{ user.id }} - {{ user.nombre }}
              </option>
            </select>
          </label>
          <label>
            <span class="form-label">Latitud</span>
            <input v-model.number="editState.form.latitud" type="number" step="0.000001" class="form-control" />
          </label>
          <label>
            <span class="form-label">Longitud</span>
            <input v-model.number="editState.form.longitud" type="number" step="0.000001" class="form-control" />
          </label>
          <div class="modal-actions">
            <button class="btn btn-outline" type="button" @click="closeEdit">Cancelar</button>
            <button class="btn btn-primary" type="submit" :disabled="saving">{{ saving ? 'Guardando...' : 'Actualizar' }}</button>
          </div>
        </form>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';
import { fincaApi } from '@/services/api.js';

const { token } = useAuth();
const { pushToast } = useToasts();

const fincas = ref([]);
const propietarios = ref([]);
const loading = ref(true);
const saving = ref(false);

const createForm = reactive({
  nombre: '',
  ubicacion: '',
  hectareas: null,
  propietario: '',
  latitud: null,
  longitud: null
});

const errors = reactive({
  nombre: '',
  ubicacion: '',
  hectareas: '',
  propietario: ''
});

const editState = reactive({
  open: false,
  id: null,
  form: {
    nombre: '',
    ubicacion: '',
    hectareas: null,
    propietario: '',
    latitud: null,
    longitud: null
  }
});

function normalizeFinca(raw) {
  return {
    id: raw.idfinca ?? raw.id ?? raw.Id ?? null,
    nombre: raw.nombreFinca ?? raw.nombre ?? raw.Nombre ?? '',
    ubicacion: raw.ubicacion ?? raw.Ubicacion ?? '',
    hectareas: raw.hectareas ?? raw.Hectareas ?? 0,
    propietario: raw.propietario ?? raw.idpropietario ?? raw.Idpropietario ?? null,
    propietarioNombre: raw.propietarioNombre ?? raw.PropietarioNombre ?? raw.propietarioNombreCompleto ?? '',
    latitud: raw.latitud ?? raw.Latitud ?? null,
    longitud: raw.longitud ?? raw.Longitud ?? null
  };
}

function normalizePropietario(raw) {
  return {
    id: raw.id ?? raw.idusuario ?? raw.Idusuario ?? null,
    nombre: [raw.nombre, raw.nombreUr, raw.NombreUr, raw.apellido, raw.apellidoUr, raw.ApellidoUr]
      .filter(Boolean)
      .join(' ').trim()
  };
}

async function loadData() {
  loading.value = true;
  try {
    const authToken = token.value;
    const [users, list] = await Promise.all([
      fincaApi.propietarios(authToken),
      fincaApi.list(authToken)
    ]);
    propietarios.value = users.map(normalizePropietario).filter((u) => u.id);
    fincas.value = list.map(normalizeFinca).filter((f) => f.id);
  } catch (error) {
    pushToast(error.message || 'No se pudo cargar la informacion', 'error');
  } finally {
    loading.value = false;
  }
}

function validateCreate() {
  errors.nombre = createForm.nombre ? '' : 'Ingresa el nombre';
  errors.ubicacion = createForm.ubicacion ? '' : 'Ingresa la ubicacion';
  errors.hectareas = createForm.hectareas && createForm.hectareas > 0 ? '' : 'Ingresa hectareas validas';
  errors.propietario = createForm.propietario ? '' : 'Selecciona propietario';
  return !errors.nombre && !errors.ubicacion && !errors.hectareas && !errors.propietario;
}

async function handleCreate() {
  if (!validateCreate()) {
    return;
  }
  saving.value = true;
  try {
    const dto = {
      nombreFinca: createForm.nombre,
      ubicacion: createForm.ubicacion,
      hectareas: Number(createForm.hectareas),
      propietario: Number(createForm.propietario),
      latitud: createForm.latitud ?? null,
      longitud: createForm.longitud ?? null
    };
    const created = await fincaApi.create(dto, token.value);
    pushToast(`Finca ${created?.idfinca ?? created?.id ?? ''} creada`, 'success');
    resetCreate();
    await loadData();
  } catch (error) {
    pushToast(error.message || 'Error al crear finca', 'error');
  } finally {
    saving.value = false;
  }
}

function resetCreate() {
  createForm.nombre = '';
  createForm.ubicacion = '';
  createForm.hectareas = null;
  createForm.propietario = '';
  createForm.latitud = null;
  createForm.longitud = null;
}

function openEdit(finca) {
  editState.open = true;
  editState.id = finca.id;
  editState.form.nombre = finca.nombre;
  editState.form.ubicacion = finca.ubicacion;
  editState.form.hectareas = finca.hectareas;
  editState.form.propietario = finca.propietario;
  editState.form.latitud = finca.latitud;
  editState.form.longitud = finca.longitud;
}

function closeEdit() {
  editState.open = false;
  editState.id = null;
}

async function handleUpdate() {
  saving.value = true;
  try {
    const dto = {
      nombreFinca: editState.form.nombre,
      ubicacion: editState.form.ubicacion,
      hectareas: Number(editState.form.hectareas || 0),
      propietario: Number(editState.form.propietario),
      latitud: editState.form.latitud ?? null,
      longitud: editState.form.longitud ?? null
    };
    await fincaApi.update(editState.id, dto, token.value);
    pushToast('Finca actualizada', 'success');
    closeEdit();
    await loadData();
  } catch (error) {
    pushToast(error.message || 'No se pudo actualizar', 'error');
  } finally {
    saving.value = false;
  }
}

async function deleteFinca(finca) {
  if (!confirm(`Eliminar finca ${finca.nombre}?`)) {
    return;
  }
  try {
    await fincaApi.remove(finca.id, token.value);
    pushToast('Finca eliminada', 'success');
    await loadData();
  } catch (error) {
    pushToast(error.message || 'No se pudo eliminar', 'error');
  }
}

onMounted(() => {
  loadData();
});
</script>

