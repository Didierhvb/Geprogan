# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Geprogan is a livestock management and production software (Software para la gestión y producción ganadera) built with:
- **Backend**: ASP.NET Core 8.0 with Entity Framework Core 9.0, SQL Server, JWT authentication, and ML.NET for analytics
- **Frontend**: Vue.js 3 with Vite, Vue Router, Chart.js, and Tailwind CSS v4
- **Database**: SQL Server with Entity Framework Core migrations

## Development Commands

### Frontend (from /frontend directory)
- `npm run dev` - Start Vite development server (port 5173)
- `npm run build` - Build for production (outputs to ../backend/wwwroot)
- `npm run preview` - Preview production build (port 4173)
- `npm run api` - Start backend API server (runs `dotnet run` in backend directory)
- `npm run deploy` - Build frontend and publish backend

### Backend (from /backend directory)
- `dotnet run` - Start API server in development mode (default port 5142)
- `dotnet build` - Build the project
- `dotnet publish` - Publish for deployment
- `dotnet ef migrations add <MigrationName>` - Add new database migration
- `dotnet ef database update` - Apply migrations to database

## Architecture

### Backend Structure
- **Controllers/**: API endpoints for different domains
  - AuthController: JWT authentication and user login
  - GanadoController: Livestock management (CRUD operations)
  - TiposGanadoController: Livestock types catalog
  - FincasController: Farm/property management
  - UsuariosController: User management with role-based access
  - UsuariosSelectController: User selection lists
  - AnalisisLecheroController: Dairy production analytics with ML.NET
  - AnalisisSaludController: Health anomaly detection with ML.NET
- **Models/**: Entity models (Ganado, Finca, CicloProduccion, Parto, MovimientoGanado, etc.)
- **Data/**: Entity Framework DbContext (GeproGanContext)
- **Services/**: ML.NET services
  - ServicioDeteccionAnomalias: Health anomaly detection
  - ServicioAnalisisLechero: Dairy production analysis
- **DTOs/**: Data transfer objects for API contracts

### Frontend Structure
- **src/components/**: Reusable Vue components
  - base/: BaseForm, BaseTable, BaseModal, BaseFormField - reusable UI components
  - charts/: Chart.js wrapper components for data visualization
  - layouts/: AppShell (main layout), AuthShell (authentication layout)
- **src/views/**: Page-level Vue components
  - LoginView: Authentication
  - DashboardView: Main dashboard with metrics
  - GanadoView: Livestock management
  - FincasView: Farm management
  - UsuariosView: User administration
  - ProduccionView: Production tracking
  - ReproduccionView: Reproduction management
  - SanidadView: Health management
  - AnalisisLecheroView: Dairy analytics
  - AnalisisSaludView: Health analytics
- **src/router/**: Vue Router configuration with route guards
- **src/services/**: API service layer (api.js handles HTTP requests)
- **src/composables/**: Vue 3 composition API utilities (useAuth, useTheme, useToasts)

### Key Domain Models
- **Ganado**: Individual livestock records with health and production tracking
- **Finca**: Farm/property management with production metrics
- **CicloProduccion**: Production cycles tracking
- **Parto**: Birth records and reproductive management
- **MovimientoGanado**: Livestock movement tracking
- **Usuario**: User management with role-based access control
- **TrazabilidadGanado**: Complete livestock traceability

## Development Workflow

### Local Development Setup
1. **Database**: SQL Server Express with connection string in appsettings.json
2. **Frontend Development**: Vite dev server (port 5173) with API proxy to backend (port 5142)
3. **Backend Development**: ASP.NET Core serves API and static files in production
4. **Build Process**: Frontend builds to `../backend/wwwroot` for SPA deployment

### Key Development Patterns
1. **Entity Framework**: Migrations required when modifying models
2. **CORS Configuration**: Allows all origins in development for local frontend
3. **JWT Authentication**: Configured with symmetric key, 120-minute expiration
4. **ML.NET Integration**: Scoped services for health and dairy analytics
5. **SPA Deployment**: Backend serves frontend as static files with fallback routing

## Database

- **Provider**: SQL Server Express (localhost\SQLEXPRESS)
- **Database Name**: Geprogan
- **Context**: GeproGanContext in Data/GeproGanContext.cs
- **Connection**: Trusted connection with TrustServerCertificate=True
- **Migrations**: Managed through EF Core CLI tools (`dotnet ef`)

## Code Patterns

### Backend Patterns (backend/)

#### Controller Pattern (Controllers/*.cs)

**Ejemplo real: backend/Controllers/GanadoController.cs**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GanadoController : ControllerBase
{
    private readonly GeproGanContext _context;

    public GanadoController(GeproGanContext context) {
        _context = context;
    }

    // DTOs como records dentro del controller
    public record GanadoCreateDto(
        int Idfinca,
        int IdtipoGanado,
        string FechaNacimiento,
        string Sexo,
        string? MarcaGanado
    );
}
```

**Ejemplo real: backend/Controllers/FincasController.cs**
```csharp
[HttpGet]
public async Task<IActionResult> Get()
{
    var fincas = await _context.Fincas
        .Include(f => f.PropietarioNavigation)
        .Select(f => new
        {
            idfinca = f.Idfinca,
            nombreFinca = f.NombreFinca,
            propietarioNombre = (f.PropietarioNavigation.NombreUr + " " + f.PropietarioNavigation.ApellidoUr).Trim()
        })
        .ToListAsync();
    return Ok(fincas);
}
```

#### API Response Format (usado en todos los controllers)

**Success:**
- `Ok(data)` - GET retorna entidad o lista
- `CreatedAtAction(nameof(GetById), new { id }, entity)` - POST retorna 201
- `Ok(entity)` - PUT retorna entidad actualizada
- `NoContent()` - DELETE retorna 204

**Errors:**
- `BadRequest(new { message = "descripción" })` - Errores de validación
- `NotFound(new { message = "descripción" })` - Recurso no encontrado

**Ejemplos reales:**
```csharp
// backend/Controllers/GanadoController.cs línea 46
return BadRequest(new { message = "FechaNacimiento inválida, use formato yyyy-MM-dd" });

// backend/Controllers/FincasController.cs línea 102
if (finca == null) return NotFound(new { message = "Finca no encontrada" });
```

### Frontend Patterns (frontend/src/)

#### API Service Pattern (frontend/src/services/api.js)

**Función base (línea 45-81):**
```javascript
async function apiFetch(path, { method = 'GET', body, headers = {}, auth = false } = {}) {
  const baseUrl = getApiBase();
  const url = `${baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  const finalHeaders = { ...headers };

  if (body && !(body instanceof FormData)) {
    finalHeaders['Content-Type'] = 'application/json';
  }

  if (auth) {
    const token = readToken();
    if (!token) throw new Error('No hay sesion activa');
    finalHeaders.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    method,
    headers: finalHeaders,
    body: body && !(body instanceof FormData) ? JSON.stringify(body) : body
  });

  if (response.status === 204) return null;

  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const message = data?.message || data?.error || 'Error en la solicitud';
    throw new Error(message);
  }
  return data;
}
```

**Servicios por dominio (líneas 89-245):**
```javascript
// frontend/src/services/api.js línea 89
export const fincaApi = {
  list(token) {
    return apiFetch('/api/fincas', { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  create(dto, token) {
    return apiFetch('/api/fincas', { method: 'POST', auth: true, body: dto, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  update(id, dto, token) {
    return apiFetch(`/api/fincas/${id}`, { method: 'PUT', auth: true, body: dto, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  remove(id, token) {
    return apiFetch(`/api/fincas/${id}`, { method: 'DELETE', auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  }
};
```

#### Router Pattern (frontend/src/router/index.js)

**Lazy loading (líneas 4-14):**
```javascript
const DashboardView = () => import('@/views/DashboardView.vue');
const GanadoView = () => import('@/views/GanadoView.vue');
const FincasView = () => import('@/views/FincasView.vue');
```

**Rutas con metadata (líneas 16-32):**
```javascript
routes: [
  { path: '/', redirect: '/dashboard' },
  { path: '/login', name: 'login', component: LoginView, meta: { layout: 'auth' } },
  { path: '/dashboard', name: 'dashboard', component: DashboardView, meta: { requiresAuth: true } },
  { path: '/ganado', name: 'ganado', component: GanadoView, meta: { requiresAuth: true } }
]
```

**Route guard (líneas 34-43):**
```javascript
router.beforeEach((to, from, next) => {
  const { isAuthenticated } = useAuth();
  if (to.meta.requiresAuth && !isAuthenticated.value) {
    return next({ name: 'login', query: { redirect: to.fullPath } });
  }
  if (to.name === 'login' && isAuthenticated.value) {
    return next({ name: 'dashboard' });
  }
  return next();
});
```

#### Authentication Pattern (frontend/src/composables/useAuth.js)

**Estado global reactivo (líneas 39-42):**
```javascript
const state = reactive({
  token: readToken(),
  user: readUser()
});
```

**Composable (líneas 44-71):**
```javascript
export function useAuth() {
  const isAuthenticated = computed(() => Boolean(state.token));

  async function login(credentials) {
    const data = await authApi.login(credentials);
    state.token = data.token;
    state.user = data.user ?? null;
    persistSession(state.token, state.user);
    return data;
  }

  function logout() {
    state.token = null;
    state.user = null;
    clearSession();
  }

  return {
    token: computed(() => state.token),
    user: computed(() => state.user),
    isAuthenticated,
    login,
    logout
  };
}
```

#### View Component Pattern (frontend/src/views/GanadoView.vue)

**Script setup (líneas 261-267):**
```javascript
import { ref, reactive, onMounted, computed } from 'vue';
import { useAuth } from '@/composables/useAuth.js';
import { useToasts } from '@/composables/useToasts.js';
import { ganadoApi } from '@/services/api.js';
import { BaseForm, BaseFormField, BaseTable, BaseModal } from '@/components/base';
```

**Estado reactivo (líneas 271-276):**
```javascript
const ganado = ref([]);
const fincas = ref([]);
const tipos = ref([]);
const loading = ref(false);
const saving = ref(false);
```

**Formulario reactivo (líneas 294-306):**
```javascript
const createForm = reactive({
  idfinca: '',
  idtipoGanado: '',
  fechaNacimiento: '',
  sexo: '',
  nombreGanado: ''
});
```

**Carga de datos en paralelo (líneas 399-416):**
```javascript
async function loadData() {
  loading.value = true;
  try {
    const authToken = token.value;
    const [list, fincasList, tiposList] = await Promise.all([
      ganadoApi.list(authToken),
      ganadoApi.fincas(authToken),
      ganadoApi.tipos(authToken)
    ]);
    ganado.value = list.map(normalizeGanado);
    fincas.value = fincasList.map(normalizeFinca);
    tipos.value = tiposList.map(normalizeTipo);
  } catch (error) {
    pushToast(error.message || 'No se pudo cargar', 'error');
  } finally {
    loading.value = false;
  }
}
```

**CRUD sin recargar página (líneas 434-450):**
```javascript
async function handleCreate() {
  if (!validateCreate()) return;

  saving.value = true;
  try {
    const dto = buildDto(createForm);
    const created = await ganadoApi.create(dto, token.value);
    pushToast(`Ganado ${created?.idganado} creado`, 'success');
    resetCreate();
    await loadData(); // Refresca datos reactivos, NO recarga página
  } catch (error) {
    pushToast(error.message || 'Error', 'error');
  } finally {
    saving.value = false;
  }
}
```

### Reglas SPA - NUNCA Recargar Página

1. **Navegación:** Usar `<router-link>` o `$router.push()` (frontend/src/router/index.js)
2. **Formularios:** `@submit.prevent` + llamada API + actualizar estado reactivo
3. **CRUD:** Operación API → `await loadData()` → Vue re-renderiza automáticamente
4. **NO usar:** `window.location`, `<a href>`, `location.reload()`

### Archivos Base a Seguir

**Backend:**
- `backend/Controllers/GanadoController.cs` - Patrón CRUD básico
- `backend/Controllers/FincasController.cs` - Patrón async con Include/Select
- `backend/Program.cs` - Configuración de servicios

**Frontend:**
- `frontend/src/services/api.js` - Comunicación API
- `frontend/src/router/index.js` - Enrutamiento y guards
- `frontend/src/composables/useAuth.js` - Estado global compartido
- `frontend/src/views/GanadoView.vue` - CRUD completo con componentes base