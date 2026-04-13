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
    <div v-if="showImportModal" class="import-modal-overlay" @click.self="closeImportModal">
      <div class="import-modal">
        <h2 class="text-2xl font-montserrat font-semibold text-grey-darker mb-4">
          {{ t('pages.athletes.import.modalTitle') }}
        </h2>
        <p class="font-montserrat text-sm text-grey-dark mb-4">
          {{ t('pages.athletes.import.modalDescription') }}
        </p>

        <input
          type="file"
          accept=".csv"
          @change="onFileSelected"
          class="mb-4 font-montserrat"
          :disabled="isImporting"
        />

        <div v-if="importResult" class="mb-4 flex flex-col gap-2">
          <p class="font-montserrat text-grey-darker">
            <strong>{{ t('pages.athletes.import.createdLabel') }}</strong> {{ importResult.createdCount }}
          </p>
          <p class="font-montserrat text-grey-darker">
            <strong>{{ t('pages.athletes.import.updatedLabel') }}</strong> {{ importResult.updatedCount }}
          </p>
          <div v-if="importResult.errors.length > 0">
            <p class="font-montserrat text-grey-darker mb-2">
              <strong>{{ t('pages.athletes.import.errorsLabel') }}</strong> {{ importResult.errors.length }}
            </p>
            <ul class="flex flex-col gap-1 max-h-60 overflow-y-auto">
              <li v-for="err in importResult.errors" :key="`${err.row}-${err.message}`" class="font-montserrat text-sm text-red">
                {{ t('pages.athletes.import.errorRowPrefix') }} {{ err.row }}<span v-if="err.email"> ({{ err.email }})</span>: {{ err.message }}
              </li>
            </ul>
          </div>
        </div>

        <div class="flex gap-2 justify-end">
          <button type="button" class="btn" @click="closeImportModal" :disabled="isImporting">
            {{ t('global.close') }}
          </button>
          <button type="button" class="btn btn--primary" :disabled="!selectedFile || isImporting" @click="handleImport">
            {{ isImporting ? t('pages.athletes.import.importing') : t('pages.athletes.import.submit') }}
          </button>
        </div>
      </div>
    </div>

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
  showImportModal.value = false
  selectedFile.value = null
  importResult.value = null
  if (importResult.value === null) {
    loadAthletes(1, Tables.DefaultRowsPerPage)
  }
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
.import-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.import-modal {
  background: white;
  border-radius: 0.75rem;
  padding: 2rem;
  max-width: 600px;
  width: 90%;
  max-height: 85vh;
  overflow-y: auto;
}
</style>
