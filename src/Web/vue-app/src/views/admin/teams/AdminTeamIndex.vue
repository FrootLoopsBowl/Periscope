<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.teams.name') }}</h1>
    </div>
    <div class="content-grid__actions">
      <BtnLink
          :name="t('routes.admin.children.teams.add.name')"
          :path="{ path: t('routes.admin.children.teams.add.fullPath') }"
      />
    </div>
    <DataTable
        :headers="teamHeaders"
        :is-loading="teamsAreLoading"
        :items="tableTeams"
        :total-items="paginatedResponse.totalItems"
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
import {Tables} from "@/types/enums"

const {t} = useI18n()
const teamService = useTeamService()

const teamsAreLoading = ref(false)
const pageTeams = ref<Team[]>([])
const paginatedResponse = ref<PaginatedResponse<Team>>({totalItems: 0})

const tableTeams = computed(() =>
  pageTeams.value.map((x: Team) => ({
    id: x.id,
    name: x.name,
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

const teamHeaders = computed(() => [
  {text: t("global.name"), value: 'name', width: 300},
])
</script>
