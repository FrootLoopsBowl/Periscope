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
        <div class="flex items-center gap-3">
          <template v-if="!isEditingName">
            <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
              {{ team ? team.name : t('pages.teams.detail.title') }}
            </h1>
            <button v-if="team" type="button" class="text-grey-dark hover:text-green transition-colors" @click="startEditName">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536M9 13l6.586-6.586a2 2 0 012.828 2.828L11.828 15.828a2 2 0 01-1.414.586H9v-1.414a2 2 0 01.586-1.414z" />
              </svg>
            </button>
          </template>
          <template v-else>
            <input v-model="editingName" type="text" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker text-2xl focus:outline-none focus:border-green" />
            <button type="button" class="btn btn--primary" :disabled="preventMultipleSubmit" @click="handleSaveName">{{ t('global.save') }}</button>
            <button type="button" class="btn" @click="isEditingName = false">✕</button>
          </template>
        </div>
        <BackLink />
      </div>
    </div>

    <Loader v-if="isLoading" />

    <template v-if="!isLoading && team">

      <!-- Section calendrier -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <TeamCalendar :team-id="team.id!" />
      </div>

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
              <RouterLink
                :to="{ name: 'admin.children.athletes.detail', params: { id: athlete.id } }"
                class="hover:underline"
              >{{ athlete.firstName }} {{ athlete.lastName }}</RouterLink>
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

      <!-- Section athlètes blessés -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.admin.dashboard.injuredAthletesTitle') }}
          </h2>
          <span
            v-if="injuredTeamAthletes.length > 0"
            class="inline-flex items-center justify-center px-3 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
          >
            {{ injuredTeamAthletes.length }}
          </span>
        </div>
        <div class="p-6">
          <p v-if="injuredTeamAthletes.length === 0" class="font-montserrat text-grey-dark italic">
            {{ t('pages.admin.dashboard.injuredAthletesEmpty') }}
          </p>
          <ul v-else class="flex flex-col divide-y divide-grey">
            <li
              v-for="athlete in injuredTeamAthletes"
              :key="athlete.id"
              class="flex items-center justify-between gap-3 py-2 px-1"
            >
              <RouterLink
                :to="{ name: 'admin.children.athletes.detail', params: { id: athlete.id } }"
                class="font-montserrat text-sm font-semibold text-green-dark hover:underline"
              >{{ athlete.firstName }} {{ athlete.lastName }}</RouterLink>
              <button
                type="button"
                class="btn btn--square"
                :title="t('pages.admin.dashboard.markAsRecovered')"
                @click="handleMarkAsRecovered(athlete.id!)"
              >
                <IconBandage :size="16" />
              </button>
            </li>
          </ul>
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

      <!-- Zone dangereuse -->
      <div class="bg-white rounded-xl border border-red-300 overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-red-50 border-b border-red-300">
          <span class="block w-1.5 h-7 rounded-full bg-red-400"></span>
          <h2 class="font-montserrat font-semibold text-red-600 text-base">
            {{ t('pages.teams.delete.modal.title') }}
          </h2>
        </div>
        <div class="p-6 flex items-center justify-between gap-4">
          <p class="font-montserrat text-sm text-grey-dark">
            {{ t('pages.teams.delete.modal.message') }}
          </p>
          <button
            type="button"
            :disabled="preventMultipleSubmit"
            class="btn btn--red flex-shrink-0"
            @click="showConfirmModal = true"
          >
            {{ t('pages.teams.delete.modal.title') }}
          </button>
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
import IconBandage from 'vue-material-design-icons/Bandage.vue'
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import ConfirmModal from "@/components/layouts/items/ConfirmModal.vue"
import {useTeamStore} from "@/stores/teamStore"
import TeamCalendar from '@/components/calendar/TeamCalendar.vue'

const {t} = useI18n()
const router = useRouter()
const isEditingName = ref(false)
const editingName = ref('')

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

const injuredTeamAthletes = computed(() =>
  (team.value?.athletes ?? []).filter(a => a.isInjured)
)

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
  isEditingName.value = false
  editingName.value = ''
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

async function handleMarkAsRecovered(athleteId: string) {
  const result = await athleteService.toggleInjured(athleteId, false)
  if (result.succeeded && team.value?.athletes) {
    const athlete = team.value.athletes.find(a => a.id === athleteId)
    if (athlete) athlete.isInjured = false
    notifySuccess(t('pages.admin.dashboard.markAsRecoveredSuccess'))
  } else {
    notifyError(t('pages.admin.dashboard.markAsRecoveredError'))
  }
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
function startEditName() {
    editingName.value = team.value?.name ?? ''
    isEditingName.value = true
}
async function handleSaveName() {
    if (!editingName.value.trim() || preventMultipleSubmit.value) return
        preventMultipleSubmit.value = true
        const response = await teamService.updateTeam(props.id, { name: editingName.value.trim() })
        if (response.succeeded) {
            if (team.value) team.value.name = editingName.value.trim()
            if (team.value) {
                team.value.name = editingName.value.trim()
                const updatedTeams = teamStore.teams.map(t =>
                    t.id === props.id ? { ...t, name: editingName.value.trim() } : t
                )
                teamStore.setTeams(updatedTeams)
                isEditingName.value = false
                notifySuccess(t('pages.teams.edit.validation.successMessage'))
            }
        } else {
            const errorMessages = response.getErrorMessages('pages.teams.edit.validation')
            notifyError(errorMessages.length > 0 ? errorMessages[0] : t('pages.teams.edit.validation.failedMessage'))
        }
        preventMultipleSubmit.value = false
    }
</script>
