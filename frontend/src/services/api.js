const ENV_BASE = (import.meta.env.VITE_API_BASE_URL || '').trim();
const API_BASE_KEY = 'GEPROGAN_API_BASE';
const FALLBACK_BASE = 'http://localhost:5142';

function resolveBase() {
  if (ENV_BASE) {
    return ENV_BASE;
  }
  try {
    const stored = localStorage.getItem(API_BASE_KEY);
    if (stored) {
      return stored;
    }
    localStorage.setItem(API_BASE_KEY, FALLBACK_BASE);
  } catch (error) {
    // Storage may be disabled; ignore and fall back to default.
  }
  return FALLBACK_BASE;
}

export function getApiBase() {
  return resolveBase();
}

export function setApiBase(url) {
  if (ENV_BASE) {
    return ENV_BASE;
  }
  try {
    localStorage.setItem(API_BASE_KEY, url);
  } catch (error) {
    // Ignore storage errors to keep the app usable.
  }
  return url;
}

function readToken() {
  try {
    return localStorage.getItem('GEPROGAN_TOKEN');
  } catch (error) {
    return null;
  }
}

async function apiFetch(path, { method = 'GET', body, headers = {}, auth = false } = {}) {
  const baseUrl = getApiBase();
  const url = `${baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  const finalHeaders = { ...headers };

  if (body && !(body instanceof FormData)) {
    finalHeaders['Content-Type'] = 'application/json';
  }

  if (auth) {
    const providedToken = headers.Authorization;
    const token = providedToken || readToken();
    if (!token) {
      throw new Error('No hay sesion activa');
    }
    if (!providedToken) {
      finalHeaders.Authorization = `Bearer ${token}`;
    }
  }

  const response = await fetch(url, {
    method,
    headers: finalHeaders,
    body: body && !(body instanceof FormData) ? JSON.stringify(body) : body
  });

  if (response.status === 204) {
    return null;
  }

  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const message = data?.message || data?.error || 'Error en la solicitud';
    throw new Error(message);
  }
  return data;
}

export const authApi = {
  async login(credentials) {
    return apiFetch('/api/auth/login', { method: 'POST', body: credentials });
  }
};

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
  },
  propietarios(token) {
    return apiFetch('/api/usuarios/select', { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  }
};

export const ganadoApi = {
  list(token) {
    return apiFetch('/api/ganado', { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  details(id, token) {
    return apiFetch(`/api/ganado/${id}`, { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  create(dto, token) {
    return apiFetch('/api/ganado', { method: 'POST', auth: true, body: dto, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  update(id, dto, token) {
    return apiFetch(`/api/ganado/${id}`, { method: 'PUT', auth: true, body: dto, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  remove(id, token) {
    return apiFetch(`/api/ganado/${id}`, { method: 'DELETE', auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  fincas(token) {
    return apiFetch('/api/fincas', { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  tipos(token) {
    return apiFetch('/api/tiposganado', { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  }
};

export const usuarioApi = {
  list(token) {
    return apiFetch('/api/usuarios', { auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  create(dto, token) {
    return apiFetch('/api/usuarios', { method: 'POST', auth: true, body: dto, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  },
  remove(id, token) {
    return apiFetch(`/api/usuarios/${id}`, { method: 'DELETE', auth: true, headers: token ? { Authorization: `Bearer ${token}` } : undefined });
  }
};

export const analisisSaludApi = {
  entrenarModelo(token) {
    return apiFetch('/api/AnalisisSalud/entrenar-modelo', {
      method: 'POST',
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  analizarAnimal(idGanado, token) {
    return apiFetch(`/api/AnalisisSalud/analizar-animal/${idGanado}`, {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  detectarAnomalias(token) {
    return apiFetch('/api/AnalisisSalud/detectar-anomalias', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerMetricasGenerales(token) {
    return apiFetch('/api/AnalisisSalud/metricas-generales', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerResumenSalud(token) {
    return apiFetch('/api/AnalisisSalud/resumen-salud', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerAlertasUrgentes(token) {
    return apiFetch('/api/AnalisisSalud/alertas-urgentes', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  }
};

export const analisisLecheroApi = {
  entrenarModelos(token) {
    return apiFetch('/api/AnalisisLechero/entrenar-modelos', {
      method: 'POST',
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  analizarVaca(idGanado, token) {
    return apiFetch(`/api/AnalisisLechero/analizar-vaca/${idGanado}`, {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerMetricasGenerales(token) {
    return apiFetch('/api/AnalisisLechero/metricas-generales', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerOptimizaciones(token) {
    return apiFetch('/api/AnalisisLechero/optimizaciones', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerTendencias(dias, token) {
    return apiFetch(`/api/AnalisisLechero/tendencias?dias=${dias}`, {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerDashboard(token) {
    return apiFetch('/api/AnalisisLechero/dashboard', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerPrediccionSemanal(token) {
    return apiFetch('/api/AnalisisLechero/prediccion-semanal', {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  obtenerRankingProductoras(limite, token) {
    return apiFetch(`/api/AnalisisLechero/ranking-productoras?limite=${limite}`, {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  },
  generarReporteProduccion(fechaInicio, fechaFin, token) {
    let url = '/api/AnalisisLechero/reporte-produccion';
    const params = new URLSearchParams();
    if (fechaInicio) params.append('fechaInicio', fechaInicio);
    if (fechaFin) params.append('fechaFin', fechaFin);
    if (params.toString()) url += `?${params.toString()}`;

    return apiFetch(url, {
      auth: true,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined
    });
  }
};
