<template>
  <div class="content-grid">

    <!-- Modal de confirmation de suppression -->
    <ConfirmModal
      :show="showConfirmModal"
      :title="t('pages.teams.delete.modal.title')"
      :message="t('pages.teams.delete.modal.message')"
      @confirm="onConfirmDelete"
      @cancel="showConfirmModal = false"
    />

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
        <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
          {{ team ? team.name : t('pages.teams.detail.title') }}
        </h1>
        <div class="flex items-center gap-3">
          <button
            v-if="team"
            type="button"
            :disabled="preventMultipleSubmit"
            class="btn btn--red"
            @click="showConfirmModal = true"
          >
            {{ t('pages.teams.delete.modal.title') }}
          </button>
          <BackLink />
        </div>
      </div>
    </div>

    <Loader v-if="isLoading" />

    <template v-if="!isLoading && team">

      <!-- Section athlètes actuels -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.teams.detail.athletesSection') }}
          </h2>
          <span
            class="inline-flex items-center justify-center px-3 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
          >
            {{ team.athletes?.length ?? 0 }}
          </span>
        </div>
        <div class="p-6">
          <p v-if="!team.athletes || team.athletes.length === 0" class="font-montserrat text-grey-dark italic">
            {{ t('pages.teams.detail.noAthletes') }}
          </p>
          <div v-else class="flex flex-wrap gap-2">
            <span
              v-for="athlete in team.athletes"
              :key="athlete.id"
              class="inline-flex items-center gap-2 pl-3 pr-2 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
            >
              {{ athlete.firstName }} {{ athlete.lastName }}
              <button
                type="button"
                :disabled="preventMultipleSubmit"
                class="w-4 h-4 rounded-full bg-green-light hover:bg-red-200 text-green-dark hover:text-red-600 flex items-center justify-center transition-colors text-xs leading-none"
                :title="t('pages.teams.detail.removeAthlete')"
                @click="handleRemoveAthlete(athlete.id!)"
              >
                ×
              </button>
            </span>
          </div>
        </div>
      </div>

      <!-- Section attribution d'athlètes -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.teams.detail.assignAthletes') }}
          </h2>
        </div>
        <div class="p-6 flex flex-col gap-4">

          <div class="flex flex-col gap-2">
            <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
              {{ t('pages.teams.detail.selectAthletes') }}
            </label>
            <!-- Barre de recherche -->
            <input
              v-model="athleteSearch"
              type="text"
              :placeholder="t('pages.teams.detail.searchAthletes')"
              class="border border-grey rounded-lg px-4 py-2 font-montserrat text-grey-darker text-sm focus:outline-none focus:border-green"
            />
            <div class="border border-grey rounded-lg max-h-64 overflow-y-auto">
              <label
                v-for="athlete in filteredAthletes"
                :key="athlete.id"
                class="flex items-center gap-3 px-4 py-3 cursor-pointer hover:bg-green-lighter transition-colors border-b border-grey last:border-b-0"
              >
                <input
                  type="checkbox"
                  :value="athlete.id"
                  v-model="selectedAthleteIds"
                  class="w-4 h-4 accent-green"
                />
                <div class="flex flex-col">
                  <span class="font-montserrat font-semibold text-grey-darker text-sm">
                    {{ athlete.firstName }} {{ athlete.lastName }}
                  </span>
                  <span class="font-montserrat text-grey-dark text-xs">{{ athlete.email }}</span>
                  <span v-if="athlete.teamId" class="inline-flex items-center gap-1 mt-1 px-2 py-0.5 rounded-full text-xs font-montserrat font-semibold text-white" style="background-color: var(--color-green-medium);">
                    {{ athlete.teamName }}
                  </span>
                </div>
              </label>
              <p v-if="filteredAthletes.length === 0" class="px-4 py-3 font-montserrat text-grey-dark italic text-sm">
                {{ athleteSearch ? t('pages.teams.detail.noSearchResults') : t('pages.admin.dashboard.filters.noAthletes') }}
              </p>
            </div>
            <p class="text-xs font-montserrat text-grey-dark">
              {{ selectedAthleteIds.length }} {{ t('pages.teams.detail.selected') }}
            </p>
          </div>

          <div class="flex gap-3">
            <button
              type="button"
              :disabled="preventMultipleSubmit"
              class="btn btn--primary"
              @click="handleAssignAthletes"
            >
              {{ t('global.save') }}
            </button>
          </div>

        </div>
      </div>

    </template>

  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {computed, onMounted, ref, watch} from "vue"
import {useRouter} from "vue-router"
import {useAthleteService, useTeamService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {Athlete, Team} from "@/types/entities"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import ConfirmModal from "@/components/layouts/items/ConfirmModal.vue"
import {useTeamStore} from "@/stores/teamStore"

const {t} = useI18n()
const router = useRouter()

const props = defineProps<{ id: string }>()

const athleteService = useAthleteService()
const teamService = useTeamService()
const teamStore = useTeamStore()

const isLoading = ref(true)
const preventMultipleSubmit = ref(false)
const showConfirmModal = ref(false)
const team = ref<Team | null>(null)
const allAthletes = ref<Athlete[]>([])
const selectedAthleteIds = ref<string[]>([])
const athleteSearch = ref('')

const filteredAthletes = computed(() => {
  const q = athleteSearch.value.toLowerCase().trim()
  if (!q) return allAthletes.value
  return allAthletes.value.filter(a =>
    `${a.firstName} ${a.lastName}`.toLowerCase().includes(q) ||
    (a.email ?? '').toLowerCase().includes(q)
  )
})

async function loadData() {
  isLoading.value = true
  athleteSearch.value = ''
  await Promise.all([loadTeam(), loadAthletes()])
  isLoading.value = false
}

onMounted(loadData)

watch(() => props.id, loadData)

async function loadTeam() {
  team.value = await teamService.getById(props.id)
  if (team.value?.athletes) {
    selectedAthleteIds.value = team.value.athletes
      .map(a => a.id)
      .filter((id): id is string => id !== undefined)
  }
}

async function loadAthletes() {
  allAthletes.value = await athleteService.getAllNonPaginated()
}

async function handleAssignAthletes() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  const response = await teamService.assignAthletes(props.id, { athleteIds: selectedAthleteIds.value })
  if (response.succeeded) {
    await Promise.all([loadTeam(), loadAthletes()])
    notifySuccess(t('pages.teams.detail.validation.successMessage'))
  } else {
    notifyError(t('pages.teams.detail.validation.failedMessage'))
  }

  preventMultipleSubmit.value = false
}

async function handleRemoveAthlete(athleteId: string) {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  const response = await athleteService.assignTeam(athleteId, { teamId: null })
  if (response.succeeded) {
    if (team.value?.athletes) {
      team.value.athletes = team.value.athletes.filter(a => a.id !== athleteId)
    }
    selectedAthleteIds.value = selectedAthleteIds.value.filter(id => id !== athleteId)
    notifySuccess(t('pages.teams.detail.validation.removeSuccessMessage'))
  } else {
    notifyError(t('pages.teams.detail.validation.failedMessage'))
  }

  preventMultipleSubmit.value = false
}

async function onConfirmDelete() {
  showConfirmModal.value = false
  if (!team.value?.id || preventMultipleSubmit.value) return

  preventMultipleSubmit.value = true
  const teamId = team.value.id

  const response = await teamService.deleteTeam(teamId)
  if (response.succeeded) {
    teamStore.setTeams(teamStore.teams.filter(t => t.id !== teamId))
    notifySuccess(t('pages.teams.delete.validation.successMessage'))
    router.push({ name: 'admin.children.teams.index' })
  } else {
    notifyError(t('pages.teams.delete.validation.failedMessage'))
    preventMultipleSubmit.value = false
  }
}
</script>
