import { ref } from 'vue';

const STORAGE_KEY = 'geprogan-theme';
const theme = ref('light');
let initialized = false;

function applyTheme(target) {
  const doc = document.documentElement;
  doc.setAttribute('data-theme', target);
  doc.setAttribute('data-bs-theme', target);
  theme.value = target;
}

function readStored() {
  try {
    return localStorage.getItem(STORAGE_KEY);
  } catch (error) {
    return null;
  }
}

function writeStored(value) {
  try {
    localStorage.setItem(STORAGE_KEY, value);
  } catch (error) {
    /* storage disabled */
  }
}

function removeStored() {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch (error) {
    /* ignore */
  }
}

export function useTheme() {
  if (!initialized) {
    const stored = readStored();
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    const detected = stored || (prefersDark ? 'dark' : 'light');
    applyTheme(detected);
    if (window.matchMedia) {
      const query = window.matchMedia('(prefers-color-scheme: dark)');
      if (query.addEventListener) {
        query.addEventListener('change', (event) => {
          if (!readStored()) {
            applyTheme(event.matches ? 'dark' : 'light');
          }
        });
      }
    }
    initialized = true;
  }

  function setTheme(value) {
    const normalized = value === 'dark' ? 'dark' : 'light';
    applyTheme(normalized);
    writeStored(normalized);
  }

  function toggleTheme() {
    setTheme(theme.value === 'dark' ? 'light' : 'dark');
  }

  function resetTheme() {
    removeStored();
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    applyTheme(prefersDark ? 'dark' : 'light');
  }

  return {
    theme,
    setTheme,
    toggleTheme,
    resetTheme
  };
}
