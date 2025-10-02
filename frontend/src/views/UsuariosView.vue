<template>
  <section class="surface-panel" style="display: flex; flex-direction: column; gap: 1.5rem;">
    <header style="display: flex; justify-content: space-between; flex-wrap: wrap; gap: 1rem;">
      <div>
        <div class="stat-chip">Equipo</div>
        <h1 class="hero-title" style="font-size: 2.1rem;">Usuarios del sistema</h1>
        <p class="hero-subtitle">Controla acceso por roles y comunica estados con feedback inmediato.</p>
      </div>
      <span class="stat-chip" v-if="usuarios.length">{{ usuarios.length }} activos</span>
    </header>

    <form class="form-grid two" @submit.prevent="handleCreate" novalidate>
      <label>
        <span class="form-label">Tipo identificacion</span>
        <select v-model="createForm.tipoIdentificacion" class="select-control" required>
          <option value="">Selecciona</option>
          <option value="CC">Cedula de ciudadania</option>
          <option value="TI">Tarjeta de identidad</option>
          <option value="CE">Cedula de extranjeria</option>
          <option value="NIT">NIT</option>
          <option value="PAS">Pasaporte</option>
        </select>
        <span v-if="errors.tipoIdentificacion" class="form-error">{{ errors.tipoIdentificacion }}</span>
      </label>
      <label>
        <span class="form-label">Rol</span>
        <select v-model.number="createForm.rol" class="select-control" required>
          <option value="">Selecciona</option>
          <option v-for="rol in roles" :key="rol.value" :value="rol.value">
            {{ rol.label }}
          </option>
        </select>
        <span v-if="errors.rol" class="form-error">{{ errors.rol }}</span>
      </label>
      <label>
        <span class="form-label">Nombre</span>
        <input v-model.trim="createForm.nombre" class="form-control" required />
        <span v-if="errors.nombre" class="form-error">{{ errors.nombre }}</span>
      </label>
      <label>
        <span class="form-label">Apellido</span>
        <input v-model.trim="createForm.apellido" class="form-control" required />
        <span v-if="errors.apellido" class="form-error">{{ errors.apellido }}</span>
      </label>
      <label>
        <span class="form-label">Correo</span>
        <input v-model.trim="createForm.email" type="email" class="form-control" required />
        <span v-if="errors.email" class="form-error">{{ errors.email }}</span>
      </label>
      <label>
        <span class="form-label">Telefono</span>
        <input v-model.trim="createForm.telefono" maxlength="10" class="form-control" inputmode="numeric" required />
        <span v-if="errors.telefono" class="form-error">{{ errors.telefono }}</span>
      </label>
      <label>
        <span class="form-label">Contrasena</span>
        <input :type="showPassword ? 'text' : 'password'" v-model="createForm.contrasena" class="form-control" required minlength="8" />
        <span v-if="errors.contrasena" class="form-error">{{ errors.contrasena }}</span>
      </label>
      <label>
        <span class="form-label">Confirmar</span>
        <input :type="showPassword ? 'text' : 'password'" v-model="createForm.confirmar" class="form-control" required minlength="8" />
        <span v-if="errors.confirmar" class="form-error">{{ errors.confirmar }}</span>
      </label>
      <label style="display: flex; align-items: center; gap: 0.5rem;">
        <input type="checkbox" v-model="showPassword" />
        <span>Mostrar contrasena</span>
      </label>
      <div style="display: flex; gap: 1rem; align-items: center;">
        <button class="btn btn-primary" type="submit" :disabled="saving">{{ saving ? 'Guardando...' : 'Crear usuario' }}</button>
        <button class="btn btn-outline" type="button" @click="resetForm" :disabled="saving">Limpiar</button>
      </div>
    </form>
  </section>

  <section class="table-panel">
    <header style="display: flex; justify-content: space-between; align-items: center; padding: 1.5rem 1.5rem 0;">
      <h2 style="font-size: 1.2rem;">Usuarios</h2>
    </header>
    <div style="overflow-x: auto;">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Email</th>
            <th>Telefono</th>
            <th>Rol</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="6">Cargando usuarios...</td>
          </tr>
          <tr v-for="user in usuarios" :key="user.id">
            <td>{{ user.id }}</td>
            <td>{{ user.nombre }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.telefono }}</td>
            <td>{{ user.rolNombre }}</td>
            <td style="text-align: right;">
              <button class="btn btn-ghost" type="button" @click="deleteUsuario(user)">Eliminar</button>
            </td>
          </tr>
          <tr v-if="!loading && !usuarios.length">
            <td colspan="6">Sin usuarios registrados.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';
import { usuarioApi } from '@/services/api.js';

const { token } = useAuth();
const { pushToast } = useToasts();

const usuarios = ref([]);
const roles = ref([]);
const loading = ref(true);
const saving = ref(false);
const showPassword = ref(false);

const createForm = reactive({
  tipoIdentificacion: '',
  rol: '',
  nombre: '',
  apellido: '',
  email: '',
  telefono: '',
  contrasena: '',
  confirmar: ''
});

const errors = reactive({
  tipoIdentificacion: '',
  rol: '',
  nombre: '',
  apellido: '',
  email: '',
  telefono: '',
  contrasena: '',
  confirmar: ''
});

function normalizeUsuario(raw) {
  const id = raw.idusuario ?? raw.id ?? null;
  const nombre = `${raw.nombreUr ?? raw.nombre ?? ''} ${raw.apellidoUr ?? raw.apellido ?? ''}`.trim();
  return {
    id,
    nombre,
    email: raw.emailUr ?? raw.email ?? '',
    telefono: raw.telefonoUr ?? raw.telefono ?? '',
    rolNombre: raw.rolNombre ?? raw.rol ?? raw.nombreRol ?? ''
  };
}

async function loadUsuarios() {
  loading.value = true;
  try {
    const list = await usuarioApi.list(token.value);
    usuarios.value = (list || []).map(normalizeUsuario).filter((item) => item.id);
  } catch (error) {
    pushToast(error.message || 'No se pudo cargar usuarios', 'error');
  } finally {
    loading.value = false;
  }
}

async function loadRoles() {
  try {
    const list = await usuarioApi.roles(token.value);
    roles.value = (list || []).map(r => ({
      value: r.id ?? r.idrol ?? r.Idrol,
      label: r.nombre ?? r.nombreRol ?? r.NombreRol ?? ''
    }));
  } catch (error) {
    pushToast(error.message || 'No se pudo cargar roles', 'error');
  }
}

function validate() {
  errors.tipoIdentificacion = createForm.tipoIdentificacion ? '' : 'Selecciona un tipo';
  errors.rol = createForm.rol ? '' : 'Selecciona rol';
  errors.nombre = createForm.nombre ? '' : 'Ingresa nombre';
  errors.apellido = createForm.apellido ? '' : 'Ingresa apellido';
  errors.email = /.+@.+/.test(createForm.email) ? '' : 'Correo invalido';
  errors.telefono = /^\d{10}$/.test(createForm.telefono) ? '' : 'Telefono 10 digitos';
  errors.contrasena = createForm.contrasena && createForm.contrasena.length >= 8 ? '' : 'Minimo 8 caracteres';
  errors.confirmar = createForm.contrasena === createForm.confirmar ? '' : 'Las contrasenas no coinciden';
  return Object.values(errors).every((value) => !value);
}

async function handleCreate() {
  if (!validate()) {
    return;
  }
  saving.value = true;
  try {
    const dto = {
      IdrolUr: Number(createForm.rol),
      TipoIdentificacion: createForm.tipoIdentificacion,
      NombreUr: createForm.nombre,
      ApellidoUr: createForm.apellido,
      EmailUr: createForm.email,
      TelefonoUr: createForm.telefono,
      Contrasena: createForm.contrasena,
      UrlImageUr: ''
    };
    // NO establecer Idusuario - es auto-increment en la DB
    await usuarioApi.create(dto, token.value);
    pushToast('Usuario creado', 'success');
    resetForm();
    await loadUsuarios();
  } catch (error) {
    pushToast(error.message || 'No se pudo crear usuario', 'error');
  } finally {
    saving.value = false;
  }
}

function resetForm() {
  createForm.tipoIdentificacion = '';
  createForm.rol = '';
  createForm.nombre = '';
  createForm.apellido = '';
  createForm.email = '';
  createForm.telefono = '';
  createForm.contrasena = '';
  createForm.confirmar = '';
}

async function deleteUsuario(user) {
  if (!confirm(`Eliminar usuario ${user.nombre}?`)) {
    return;
  }
  try {
    await usuarioApi.remove(user.id, token.value);
    pushToast('Usuario eliminado', 'success');
    await loadUsuarios();
  } catch (error) {
    pushToast(error.message || 'No se pudo eliminar', 'error');
  }
}

onMounted(async () => {
  await Promise.all([
    loadUsuarios(),
    loadRoles()
  ]);
});
</script>

