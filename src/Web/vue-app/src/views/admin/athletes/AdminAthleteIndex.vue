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
        <BtnLink
          :name="t('routes.admin.children.athletes.add.name')"
          :path="{ path: t('routes.admin.children.athletes.add.fullPath') }"
        />
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
import Loader from "@/components/layouts/items/Loader.vue"
import {Tables} from "@/types/enums"
import {notifyError, notifySuccess} from "@/notify"

const {t} = useI18n()
const router = useRouter()
const athleteService = useAthleteService()

const athletesAreLoading = ref(false)
const preventMultipleSubmit = ref(false)
const pageAthletes = ref<Athlete[]>([])
const paginatedResponse = ref<PaginatedResponse<Athlete>>({totalItems: 0})
const showConfirmModal = ref(false)
const pendingDeleteItem = ref<any>(null)

const tableAthletes = computed(() =>
  pageAthletes.value.map((x: Athlete) => ({
    id: x.id,
    firstName: x.firstName,
    lastName: x.lastName,
    email: x.email,
    team: x.teamName ?? t('global.undefined'),
    actions: {
      view: router.resolve({ name: 'admin.children.athletes.detail', params: { id: x.id } }).href,
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

const athleteHeaders = computed(() => [
  {text: t("global.firstName"), value: 'firstName', width: 150},
  {text: t("global.lastName"), value: 'lastName', width: 150},
  {text: t("global.email"), value: 'email', width: 200},
  {text: t("global.team"), value: 'team', width: 150},
  {text: t("global.table.actions"), value: 'actions', width: 100},
])
</script>
