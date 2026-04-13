<template>
  <div class="content-grid">

    <!-- Modal de confirmation de suppression -->
    <ConfirmModal
      :show="showConfirmModal"
      :title="t('pages.athletes.delete.modal.title')"
      :message="t('pages.athletes.delete.modal.message')"
      @confirm="onConfirmDelete"
      @cancel="showConfirmModal = false"
    />

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
        <div class="flex items-center gap-4">
          <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
            {{ t('routes.admin.children.athletes.name') }}
          </h1>
          <span
            v-if="!athletesAreLoading"
            class="inline-flex items-center justify-center px-3 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
          >
            {{ paginatedResponse.totalItems ?? pageAthletes.length }}
          </span>
        </div>
        <div class="flex items-center gap-3">
          <button type="button" class="btn" @click="showImportModal = true">
            {{ t('pages.athletes.import.button') }}
          </button>
          <BtnLink
            :name="t('routes.admin.children.athletes.add.name')"
            :path="{ path: t('routes.admin.children.athletes.add.fullPath') }"
          />
        </div>
      </div>
    </div>

    <!-- Modale d'import CSV -->
    <Transition name="fade">
      <div v-if="showImportModal" class="import-popup">
        <span class="import-popup__bg" @click="closeImportModal"></span>
        <div class="import-popup__container">
          <div class="import-popup__header">
            <h2 class="font-montserrat font-semibold text-green-dark text-xl">
              {{ t('pages.athletes.import.modalTitle') }}
            </h2>
            <button
              type="button"
              class="import-popup__close"
              :disabled="isImporting"
              @click="closeImportModal"
              aria-label="Close"
            >×</button>
          </div>

          <div class="import-popup__content">
            <!-- Instructions -->
            <div v-if="!importResult" class="flex flex-col gap-4">
              <p class="font-montserrat text-sm text-grey-darker">
                {{ t('pages.athletes.import.modalDescription') }}
              </p>

              <div class="import-popup__columns">
                <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-2">
                  {{ t('pages.athletes.import.columnsTitle') }}
                </p>
                <ol class="font-montserrat text-sm text-grey-darker flex flex-col gap-1">
                  <li>1. {{ t('global.firstName') }}</li>
                  <li>2. {{ t('global.lastName') }}</li>
                  <li>3. {{ t('global.email') }}</li>
                  <li>4. {{ t('pages.athletes.import.dobFormat') }}</li>
                  <li>5. {{ t('global.team') }}</li>
                </ol>
              </div>

              <!-- Zone de dépôt -->
              <label
                class="import-popup__dropzone"
                :class="{ 'import-popup__dropzone--has-file': selectedFile, 'import-popup__dropzone--disabled': isImporting }"
              >
                <input
                  type="file"
                  accept=".csv"
                  class="import-popup__input"
                  @change="onFileSelected"
                  :disabled="isImporting"
                />
                <template v-if="selectedFile">
                  <span class="import-popup__dropzone-icon">📄</span>
                  <span class="font-montserrat font-semibold text-green-dark">{{ selectedFile.name }}</span>
                  <span class="font-montserrat text-xs text-grey-dark">{{ formatFileSize(selectedFile.size) }}</span>
                  <span class="font-montserrat text-xs text-green underline mt-2">
                    {{ t('pages.athletes.import.changeFile') }}
                  </span>
                </template>
                <template v-else>
                  <span class="import-popup__dropzone-icon">⬆️</span>
                  <span class="font-montserrat font-semibold text-grey-darker">
                    {{ t('pages.athletes.import.selectFile') }}
                  </span>
                  <span class="font-montserrat text-xs text-grey-dark mt-1">
                    {{ t('pages.athletes.import.fileHint') }}
                  </span>
                </template>
              </label>
            </div>

            <!-- Résultat -->
            <div v-else class="flex flex-col gap-4">
              <div class="import-popup__stats">
                <div class="import-popup__stat import-popup__stat--created">
                  <span class="import-popup__stat-value">{{ importResult.createdCount }}</span>
                  <span class="import-popup__stat-label">{{ t('pages.athletes.import.createdLabel') }}</span>
                </div>
                <div class="import-popup__stat import-popup__stat--updated">
                  <span class="import-popup__stat-value">{{ importResult.updatedCount }}</span>
                  <span class="import-popup__stat-label">{{ t('pages.athletes.import.updatedLabel') }}</span>
                </div>
                <div class="import-popup__stat import-popup__stat--errors" :class="{ 'import-popup__stat--muted': importResult.errors.length === 0 }">
                  <span class="import-popup__stat-value">{{ importResult.errors.length }}</span>
                  <span class="import-popup__stat-label">{{ t('pages.athletes.import.errorsLabel') }}</span>
                </div>
              </div>

              <div v-if="importResult.errors.length > 0" class="import-popup__errors">
                <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-2">
                  {{ t('pages.athletes.import.errorsDetailTitle') }}
                </p>
                <ul class="flex flex-col gap-2">
                  <li
                    v-for="err in importResult.errors"
                    :key="`${err.row}-${err.message}`"
                    class="import-popup__error-item"
                  >
                    <span class="import-popup__error-row">
                      {{ t('pages.athletes.import.errorRowPrefix') }} {{ err.row }}
                    </span>
                    <span v-if="err.email" class="import-popup__error-email">{{ err.email }}</span>
                    <span class="import-popup__error-message">{{ err.message }}</span>
                  </li>
                </ul>
              </div>

              <p v-else class="font-montserrat text-sm text-green-dark italic">
                {{ t('pages.athletes.import.allSuccess') }}
              </p>
            </div>
          </div>

          <div class="import-popup__actions">
            <button
              type="button"
              class="btn import-popup__btn import-popup__btn-cancel"
              @click="closeImportModal"
              :disabled="isImporting"
            >
              {{ importResult ? t('global.close') : t('global.cancel') }}
            </button>
            <button
              v-if="!importResult"
              type="button"
              class="btn btn--primary import-popup__btn"
              :disabled="!selectedFile || isImporting"
              @click="handleImport"
            >
              {{ isImporting ? t('pages.athletes.import.importing') : t('pages.athletes.import.submit') }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <Loader v-if="preventMultipleSubmit" />
    <DataTable
      :headers="athleteHeaders"
      :is-loading="athletesAreLoading"
      :items="tableAthletes"
      @delete="onDelete"
      @resend="onResendLink"
      @reload="loadAthletes"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {computed, onMounted, ref} from "vue"
import {useRouter} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import {Athlete} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import DataTable from "@/components/layouts/items/DataTable.vue"
import BtnLink from "@/components/layouts/items/BtnLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import ConfirmModal from "@/components/layouts/items/ConfirmModal.vue"
import {Tables} from "@/types/enums"
import {notifyError, notifySuccess} from "@/notify"
import {ImportAthletesResult} from "@/injection/interfaces"

const {t} = useI18n()
const router = useRouter()
const athleteService = useAthleteService()

const athletesAreLoading = ref(false)
const preventMultipleSubmit = ref(false)
const pageAthletes = ref<Athlete[]>([])
const paginatedResponse = ref<PaginatedResponse<Athlete>>({totalItems: 0})
const showConfirmModal = ref(false)
const pendingDeleteItem = ref<any>(null)
const showImportModal = ref(false)
const selectedFile = ref<File | null>(null)
const isImporting = ref(false)
const importResult = ref<ImportAthletesResult | null>(null)

const tableAthletes = computed(() =>
  pageAthletes.value.map((x: Athlete) => ({
    id: x.id,
    firstName: x.firstName,
    lastName: x.lastName,
    email: x.email,
    team: x.teamName ?? t('global.undefined'),
    actions: {
      view: router.resolve({ name: 'admin.children.athletes.detail', params: { id: x.id } }).href,
      resend: true,
      edit: router.resolve({ name: 'admin.children.athletes.edit', params: { id: x.id } }).href,
      delete: true,
    },
  }))
)

onMounted(async () => {
  await loadAthletes(1, Tables.DefaultRowsPerPage)
})

async function loadAthletes(pageIndex: number, pageSize: number) {
  athletesAreLoading.value = true
  const response = await athleteService.getAll(pageIndex, pageSize)
  if (response) {
    paginatedResponse.value = response
    if (response.items)
      pageAthletes.value = response.items
  }
  athletesAreLoading.value = false
}

function onDelete(item: any) {
  if (preventMultipleSubmit.value) return
  pendingDeleteItem.value = item
  showConfirmModal.value = true
}

async function onConfirmDelete() {
  showConfirmModal.value = false
  if (!pendingDeleteItem.value) return

  preventMultipleSubmit.value = true

  const response = await athleteService.deleteAthlete(pendingDeleteItem.value.id)
  if (response && response.succeeded) {
    const index = pageAthletes.value.findIndex(x => x.id === pendingDeleteItem.value.id)
    if (index !== -1) pageAthletes.value.splice(index, 1)
    if (paginatedResponse.value.totalItems)
      paginatedResponse.value.totalItems--
    notifySuccess(t('pages.athletes.delete.validation.successMessage'))
  } else {
    notifyError(t('pages.athletes.delete.validation.failedMessage'))
  }

  pendingDeleteItem.value = null
  preventMultipleSubmit.value = false
}

async function onResendLink(item: any) {
  if (preventMultipleSubmit.value) return

  preventMultipleSubmit.value = true

  const athletePageRelativeUrl = t("routes.athletePage.path").replace("/:token", "")
  const response = await athleteService.resendAccessLink(item.id, athletePageRelativeUrl)

  if (response && response.succeeded) {
    notifySuccess(t('pages.athletes.resend.validation.successMessage'))
    preventMultipleSubmit.value = false
    return
  }

  const errorMessages = response.getErrorMessages('pages.athletes.resend.validation')
  if (errorMessages.length === 0)
    notifyError(t('pages.athletes.resend.validation.failedMessage'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false
}

function onFileSelected(event: Event) {
  const target = event.target as HTMLInputElement
  selectedFile.value = target.files && target.files.length > 0 ? target.files[0] : null
  importResult.value = null
}

function closeImportModal() {
  if (isImporting.value) return
  const hadResult = importResult.value !== null
  showImportModal.value = false
  selectedFile.value = null
  importResult.value = null
  if (hadResult) {
    loadAthletes(1, Tables.DefaultRowsPerPage)
  }
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

async function handleImport() {
  if (!selectedFile.value || isImporting.value) return
  isImporting.value = true
  importResult.value = null

  const athletePageRelativeUrl = t("routes.athletePage.path").replace("/:token", "")
  const result = await athleteService.importAthletes(selectedFile.value, athletePageRelativeUrl)
  importResult.value = result

  if (result.succeeded) {
    notifySuccess(t('pages.athletes.import.successMessage'))
    await loadAthletes(1, Tables.DefaultRowsPerPage)
  } else {
    notifyError(t('pages.athletes.import.failedMessage'))
  }
  isImporting.value = false
}

const athleteHeaders = computed(() => [
  {text: t("global.firstName"), value: 'firstName', width: 150},
  {text: t("global.lastName"), value: 'lastName', width: 150},
  {text: t("global.email"), value: 'email', width: 200},
  {text: t("global.team"), value: 'team', width: 150},
  {text: t("global.table.actions"), value: 'actions', width: 100},
])
</script>

<style scoped>
.import-popup {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.import-popup__bg {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
}
.import-popup__container {
  position: relative;
  background: #fff;
  border-radius: 0.75rem;
  width: 95%;
  max-width: 640px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-bold, 0 20px 40px rgba(0,0,0,0.2));
  overflow: hidden;
}
.import-popup__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  background: var(--color-green-lighter, #e8f5e9);
  border-bottom: 1px solid var(--color-green-light, #c8e6c9);
}
.import-popup__close {
  background: none;
  border: none;
  font-size: 1.75rem;
  line-height: 1;
  cursor: pointer;
  color: var(--color-green-dark, #2c3e50);
  padding: 0 0.25rem;
}
.import-popup__close:disabled { opacity: 0.5; cursor: not-allowed; }
.import-popup__content {
  padding: 1.5rem;
  overflow-y: auto;
}
.import-popup__columns {
  background: var(--color-grey-lighter, #f5f5f5);
  padding: 0.875rem 1rem;
  border-radius: 0.5rem;
}

/* Dropzone */
.import-popup__dropzone {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
  padding: 2rem 1rem;
  border: 2px dashed var(--color-green-light, #c8e6c9);
  border-radius: 0.75rem;
  background: var(--color-green-lighter, #f1f8f4);
  cursor: pointer;
  text-align: center;
  transition: border-color 0.2s, background 0.2s;
}
.import-popup__dropzone:hover {
  border-color: var(--color-green, #4caf50);
}
.import-popup__dropzone--has-file {
  border-style: solid;
  border-color: var(--color-green, #4caf50);
  background: #fff;
}
.import-popup__dropzone--disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.import-popup__dropzone-icon {
  font-size: 2rem;
  margin-bottom: 0.5rem;
}
.import-popup__input {
  display: none;
}

/* Stats */
.import-popup__stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.75rem;
}
.import-popup__stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 1rem 0.5rem;
  border-radius: 0.5rem;
  border: 1px solid;
}
.import-popup__stat-value {
  font-family: var(--font-montserrat);
  font-size: 1.75rem;
  font-weight: 700;
  line-height: 1;
}
.import-popup__stat-label {
  font-family: var(--font-montserrat);
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  margin-top: 0.5rem;
  text-align: center;
}
.import-popup__stat--created {
  background: #e8f5e9;
  border-color: #a5d6a7;
  color: #1b5e20;
}
.import-popup__stat--updated {
  background: #e3f2fd;
  border-color: #90caf9;
  color: #0d47a1;
}
.import-popup__stat--errors {
  background: #ffebee;
  border-color: #ef9a9a;
  color: #b71c1c;
}
.import-popup__stat--muted {
  background: var(--color-grey-lighter, #f5f5f5);
  border-color: var(--color-grey-light, #e0e0e0);
  color: var(--color-grey-dark, #666);
}

/* Errors list */
.import-popup__errors {
  max-height: 240px;
  overflow-y: auto;
  border: 1px solid var(--color-grey-light, #e0e0e0);
  border-radius: 0.5rem;
  padding: 0.75rem;
}
.import-popup__error-item {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: baseline;
  padding: 0.5rem 0.75rem;
  border-left: 3px solid #ef5350;
  background: #fff5f5;
  border-radius: 0.25rem;
  font-family: var(--font-montserrat);
  font-size: 0.85rem;
}
.import-popup__error-row {
  font-weight: 700;
  color: #b71c1c;
}
.import-popup__error-email {
  color: var(--color-grey-dark, #666);
  font-style: italic;
}
.import-popup__error-message {
  color: var(--color-grey-darker, #333);
  flex: 1;
  min-width: 200px;
}

/* Actions */
.import-popup__actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--color-grey-light, #e0e0e0);
  background: var(--color-grey-lighter, #fafafa);
}

.fade-leave-active,
.fade-enter-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
