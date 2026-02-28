<template>
  <div class="content-grid">

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
        <div class="flex items-center gap-4">
          <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
            {{ t('routes.admin.children.teams.name') }}
          </h1>
          <span
            v-if="!teamsAreLoading"
            class="inline-flex items-center justify-center px-3 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
          >
            {{ paginatedResponse.totalItems ?? pageTeams.length }}
          </span>
        </div>
        <BtnLink
          :name="t('routes.admin.children.teams.add.name')"
          :path="{ path: t('routes.admin.children.teams.add.fullPath') }"
        />
      </div>
    </div>

    <!-- Tableau -->
    <Loader v-if="preventMultipleSubmit" />
    <DataTable
      :headers="teamHeaders"
      :is-loading="teamsAreLoading"
      :items="tableTeams"
      @delete="onDelete"
      @reload="loadTeams"
    />

  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {computed, onMounted, ref} from "vue"
import {useTeamService} from "@/inversify.config"
import {Team} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import DataTable from "@/components/layouts/items/DataTable.vue"
import BtnLink from "@/components/layouts/items/BtnLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import {Tables} from "@/types/enums"
import {notifyError, notifySuccess} from "@/notify"

const {t} = useI18n()
const teamService = useTeamService()

const teamsAreLoading = ref(false)
const preventMultipleSubmit = ref(false)
const pageTeams = ref<Team[]>([])
const paginatedResponse = ref<PaginatedResponse<Team>>({totalItems: 0})

const tableTeams = computed(() =>
  pageTeams.value.map((x: Team) => ({
    id: x.id,
    name: x.name,
    actions: {
      delete: true,
    },
  }))
)

onMounted(async () => {
  await loadTeams(1, Tables.DefaultRowsPerPage)
})

async function loadTeams(pageIndex: number, pageSize: number) {
  teamsAreLoading.value = true
  const response = await teamService.getAll(pageIndex, pageSize)
  if (response) {
    paginatedResponse.value = response
    if (response.items)
      pageTeams.value = response.items
  }
  teamsAreLoading.value = false
}

async function onDelete(item: any) {
  if (preventMultipleSubmit.value) return

  const confirmed = confirm(t('pages.teams.delete.confirmation'))
  if (!confirmed) return

  preventMultipleSubmit.value = true

  const response = await teamService.deleteTeam(item.id)
  if (response && response.succeeded) {
    const index = pageTeams.value.findIndex(x => x.id === item.id)
    if (index !== -1) pageTeams.value.splice(index, 1)
    if (paginatedResponse.value.totalItems)
      paginatedResponse.value.totalItems--
    notifySuccess(t('pages.teams.delete.validation.successMessage'))
  } else {
    notifyError(t('pages.teams.delete.validation.failedMessage'))
  }

  preventMultipleSubmit.value = false
}

const teamHeaders = computed(() => [
  {text: t("global.name"), value: 'name', width: 300},
  {text: t("global.table.actions"), value: 'actions', width: 100},
])
</script>
