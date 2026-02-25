<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.athletes.name') }}</h1>
    </div>
    <div class="content-grid__actions">
      <BtnLink
          :name="t('routes.admin.children.athletes.add.name')"
          :path="{ path: t('routes.admin.children.athletes.add.fullPath') }"
      />
    </div>
    <DataTable
        :headers="athleteHeaders"
        :is-loading="athletesAreLoading"
        :items="tableAthletes"
        :total-items="paginatedResponse.totalItems"
        @reload="loadAthletes"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {computed, onMounted, ref} from "vue"
import {useAthleteService} from "@/inversify.config"
import {Athlete} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import DataTable from "@/components/layouts/items/DataTable.vue"
import BtnLink from "@/components/layouts/items/BtnLink.vue"
import {Tables} from "@/types/enums"

const {t} = useI18n()
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
    team: undefined,
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
])
</script>
