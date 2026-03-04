<template>
  <div class="content-grid">

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

    <!-- Tableau -->
    <DataTable
      :headers="athleteHeaders"
      :is-loading="athletesAreLoading"
      :items="tableAthletes"
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
import {Tables} from "@/types/enums"

const {t} = useI18n()
const router = useRouter()
const athleteService = useAthleteService()

const athletesAreLoading = ref(false)
const pageAthletes = ref<Athlete[]>([])
const paginatedResponse = ref<PaginatedResponse<Athlete>>({totalItems: 0})

const tableAthletes = computed(() =>
  pageAthletes.value.map((x: Athlete) => ({
    id: x.id,
    firstName: x.firstName,
    lastName: x.lastName,
    email: x.email,
    team: x.teamName ?? t('global.undefined'),
    actions: {
      view: router.resolve({ name: 'admin.children.athletes.detail', params: { id: x.id } }).href,
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

const athleteHeaders = computed(() => [
  {text: t("global.firstName"), value: 'firstName', width: 150},
  {text: t("global.lastName"), value: 'lastName', width: 150},
  {text: t("global.email"), value: 'email', width: 200},
  {text: t("global.team"), value: 'team', width: 150},
  {text: t("global.table.actions"), value: 'actions', width: 80},
])
</script>
