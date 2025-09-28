<template>
  <div class="base-table-container">
    <!-- Table header with search and actions -->
    <header v-if="showHeader" class="table-header">
      <div class="table-header-left">
        <h3 v-if="title" class="table-title">{{ title }}</h3>
        <div v-if="showSearch" class="search-container">
          <input
            v-model="searchQuery"
            type="text"
            class="search-input"
            :placeholder="searchPlaceholder"
          />
          <svg class="search-icon" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd" />
          </svg>
        </div>
      </div>

      <div class="table-header-right">
        <slot name="actions" />
      </div>
    </header>

    <!-- Loading state -->
    <div v-if="loading" class="table-loading">
      <div class="loading-spinner-large"></div>
      <p>Cargando datos...</p>
    </div>

    <!-- Empty state -->
    <div v-else-if="!filteredData.length" class="table-empty">
      <div class="empty-icon">📊</div>
      <h4>{{ emptyTitle }}</h4>
      <p>{{ emptyMessage }}</p>
      <slot name="empty-actions" />
    </div>

    <!-- Data table -->
    <div v-else class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th
              v-for="column in columns"
              :key="column.key"
              :class="[
                'table-header-cell',
                { 'sortable': column.sortable },
                { 'sorted': sortKey === column.key }
              ]"
              @click="column.sortable && handleSort(column.key)"
            >
              {{ column.label }}
              <span v-if="column.sortable" class="sort-indicator">
                <svg v-if="sortKey === column.key && sortOrder === 'asc'" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M14.707 12.707a1 1 0 01-1.414 0L10 9.414l-3.293 3.293a1 1 0 01-1.414-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 010 1.414z" clip-rule="evenodd" />
                </svg>
                <svg v-else-if="sortKey === column.key && sortOrder === 'desc'" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
                </svg>
                <svg v-else class="sort-icon-neutral" viewBox="0 0 20 20" fill="currentColor">
                  <path d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" />
                </svg>
              </span>
            </th>
            <th v-if="showActions" class="table-header-cell actions-column">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(item, index) in paginatedData"
            :key="getRowKey(item, index)"
            class="table-row"
            :class="{ 'row-clickable': rowClickable }"
            @click="rowClickable && $emit('row-click', item)"
          >
            <td
              v-for="column in columns"
              :key="column.key"
              class="table-cell"
            >
              <slot
                :name="`cell-${column.key}`"
                :item="item"
                :value="getNestedValue(item, column.key)"
                :column="column"
              >
                {{ formatCellValue(item, column) }}
              </slot>
            </td>
            <td v-if="showActions" class="table-cell actions-column">
              <div class="action-buttons">
                <slot name="row-actions" :item="item" />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <footer v-if="showPagination && totalPages > 1" class="table-pagination">
      <div class="pagination-info">
        Mostrando {{ startItem }} - {{ endItem }} de {{ filteredData.length }} registros
      </div>
      <div class="pagination-controls">
        <button
          class="btn btn-outline btn-sm"
          :disabled="currentPage === 1"
          @click="changePage(currentPage - 1)"
        >
          Anterior
        </button>

        <div class="page-numbers">
          <button
            v-for="page in visiblePages"
            :key="page"
            :class="['btn', 'btn-sm', currentPage === page ? 'btn-primary' : 'btn-outline']"
            @click="changePage(page)"
          >
            {{ page }}
          </button>
        </div>

        <button
          class="btn btn-outline btn-sm"
          :disabled="currentPage === totalPages"
          @click="changePage(currentPage + 1)"
        >
          Siguiente
        </button>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'

const props = defineProps({
  data: {
    type: Array,
    default: () => []
  },
  columns: {
    type: Array,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  },
  title: String,
  showHeader: {
    type: Boolean,
    default: true
  },
  showSearch: {
    type: Boolean,
    default: true
  },
  showActions: {
    type: Boolean,
    default: true
  },
  showPagination: {
    type: Boolean,
    default: true
  },
  pageSize: {
    type: Number,
    default: 10
  },
  searchPlaceholder: {
    type: String,
    default: 'Buscar...'
  },
  emptyTitle: {
    type: String,
    default: 'No hay datos'
  },
  emptyMessage: {
    type: String,
    default: 'No se encontraron registros para mostrar.'
  },
  rowKey: {
    type: [String, Function],
    default: 'id'
  },
  rowClickable: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['row-click'])

// State
const searchQuery = ref('')
const sortKey = ref('')
const sortOrder = ref('asc')
const currentPage = ref(1)

// Computed
const filteredData = computed(() => {
  let result = [...props.data]

  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(item =>
      props.columns.some(column => {
        const value = getNestedValue(item, column.key)
        return String(value).toLowerCase().includes(query)
      })
    )
  }

  // Apply sorting
  if (sortKey.value) {
    result.sort((a, b) => {
      const aVal = getNestedValue(a, sortKey.value)
      const bVal = getNestedValue(b, sortKey.value)

      let comparison = 0
      if (aVal > bVal) comparison = 1
      if (aVal < bVal) comparison = -1

      return sortOrder.value === 'desc' ? -comparison : comparison
    })
  }

  return result
})

const totalPages = computed(() => Math.ceil(filteredData.value.length / props.pageSize))

const paginatedData = computed(() => {
  const start = (currentPage.value - 1) * props.pageSize
  const end = start + props.pageSize
  return filteredData.value.slice(start, end)
})

const startItem = computed(() => {
  return filteredData.value.length === 0 ? 0 : (currentPage.value - 1) * props.pageSize + 1
})

const endItem = computed(() => {
  return Math.min(currentPage.value * props.pageSize, filteredData.value.length)
})

const visiblePages = computed(() => {
  const delta = 2
  const range = []
  const rangeWithDots = []

  for (let i = Math.max(2, currentPage.value - delta);
       i <= Math.min(totalPages.value - 1, currentPage.value + delta);
       i++) {
    range.push(i)
  }

  if (currentPage.value - delta > 2) {
    rangeWithDots.push(1, '...')
  } else {
    rangeWithDots.push(1)
  }

  rangeWithDots.push(...range)

  if (currentPage.value + delta < totalPages.value - 1) {
    rangeWithDots.push('...', totalPages.value)
  } else if (totalPages.value > 1) {
    rangeWithDots.push(totalPages.value)
  }

  return rangeWithDots.filter((v, i, arr) => arr.indexOf(v) === i)
})

// Methods
const handleSort = (key) => {
  if (sortKey.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortOrder.value = 'asc'
  }
}

const changePage = (page) => {
  currentPage.value = page
}

const getNestedValue = (obj, path) => {
  return path.split('.').reduce((o, p) => o?.[p], obj)
}

const getRowKey = (item, index) => {
  if (typeof props.rowKey === 'function') {
    return props.rowKey(item, index)
  }
  return getNestedValue(item, props.rowKey) || index
}

const formatCellValue = (item, column) => {
  const value = getNestedValue(item, column.key)

  if (column.format && typeof column.format === 'function') {
    return column.format(value, item)
  }

  return value
}

// Reset page when search changes
watch(searchQuery, () => {
  currentPage.value = 1
})
</script>

<style scoped>
.base-table-container {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border);
  overflow: hidden;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
  gap: 1rem;
}

.table-header-left {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  flex: 1;
}

.table-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0;
}

.search-container {
  position: relative;
  max-width: 300px;
  flex: 1;
}

.search-input {
  width: 100%;
  padding: 0.5rem 0.75rem 0.5rem 2.5rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-bg);
  font-size: 0.875rem;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  width: 1rem;
  height: 1rem;
  color: var(--color-text-muted);
}

.table-loading,
.table-empty {
  padding: 3rem;
  text-align: center;
  color: var(--color-text-muted);
}

.loading-spinner-large {
  width: 2rem;
  height: 2rem;
  border: 3px solid var(--color-border);
  border-top: 3px solid var(--brand-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.table-wrapper {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.table-header-cell {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
  background: var(--color-bg-alt);
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--color-text);
}

.table-header-cell.sortable {
  cursor: pointer;
  user-select: none;
  transition: var(--transition-fast);
}

.table-header-cell.sortable:hover {
  background: var(--color-surface-muted);
}

.sort-indicator {
  display: inline-flex;
  align-items: center;
  margin-left: 0.5rem;
}

.sort-indicator svg {
  width: 1rem;
  height: 1rem;
}

.sort-icon-neutral {
  opacity: 0.4;
}

.table-row {
  transition: var(--transition-fast);
}

.table-row:hover {
  background: var(--color-surface-muted);
}

.table-row.row-clickable {
  cursor: pointer;
}

.table-cell {
  padding: 1rem;
  border-bottom: 1px solid var(--color-border);
  font-size: 0.875rem;
}

.actions-column {
  width: 1%;
  white-space: nowrap;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.table-pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--color-border);
  background: var(--color-bg-alt);
}

.pagination-info {
  font-size: 0.875rem;
  color: var(--color-text-muted);
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.page-numbers {
  display: flex;
  gap: 0.25rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .table-header {
    flex-direction: column;
    align-items: stretch;
  }

  .table-header-left {
    flex-direction: column;
    gap: 1rem;
  }

  .pagination-info {
    display: none;
  }
}
</style>