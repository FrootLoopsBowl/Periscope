<template>
  <div class="content-grid">

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
        <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
          {{ athlete ? `${athlete.firstName} ${athlete.lastName}` : t('pages.athletes.detail.title') }}
        </h1>
        <BackLink />
      </div>
    </div>

    <Loader v-if="isLoading" />

    <template v-if="!isLoading && athlete">

      <!-- Informations de l'athlète -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.athletes.detail.title') }}
          </h2>
        </div>
        <div class="p-6 grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-1">{{ t('global.firstName') }}</p>
            <p class="font-montserrat text-grey-darker font-medium">{{ athlete.firstName }}</p>
          </div>
          <div>
            <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-1">{{ t('global.lastName') }}</p>
            <p class="font-montserrat text-grey-darker font-medium">{{ athlete.lastName }}</p>
          </div>
          <div>
            <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-1">{{ t('global.email') }}</p>
            <p class="font-montserrat text-grey-darker font-medium">{{ athlete.email }}</p>
          </div>
          <div>
            <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-1">{{ t('global.dateOfBirth') }}</p>
            <p class="font-montserrat text-grey-darker font-medium">{{ formatDate(athlete.dateOfBirth) }}</p>
          </div>
        </div>
      </div>

      <!-- Section équipe -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.athletes.detail.teamSection') }}
          </h2>
        </div>
        <div class="p-6 flex flex-col gap-4">

          <!-- Équipe actuelle -->
          <div>
            <p class="text-xs font-montserrat uppercase tracking-widest text-grey-dark mb-2">{{ t('global.team') }}</p>
            <span
              v-if="athlete.teamName"
              class="inline-flex items-center px-3 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
            >
              {{ athlete.teamName }}
            </span>
            <span v-else class="font-montserrat text-grey-dark italic">{{ t('pages.athletes.detail.noTeam') }}</span>
          </div>

          <!-- Sélection de l'équipe -->
          <div class="flex flex-col gap-3">
            <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
              {{ t('pages.athletes.detail.assignTeam') }}
            </label>
            <select
              v-model="selectedTeamId"
              class="border border-grey rounded-lg px-4 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green max-w-md"
            >
              <option :value="null">{{ t('pages.athletes.detail.noTeam') }}</option>
              <option v-for="team in allTeams" :key="team.id" :value="team.id">
                {{ team.name }}
              </option>
            </select>
            <div class="flex gap-3">
              <button
                type="button"
                :disabled="preventMultipleSubmit"
                class="btn btn--primary"
                @click="handleAssignTeam"
              >
                {{ t('global.save') }}
              </button>
            </div>
          </div>

        </div>
      </div>

    </template>

  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useAthleteService, useTeamService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {Athlete, Team} from "@/types/entities"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()

const props = defineProps<{ id: string }>()

const athleteService = useAthleteService()
const teamService = useTeamService()

const isLoading = ref(true)
const preventMultipleSubmit = ref(false)
const athlete = ref<Athlete | null>(null)
const allTeams = ref<Team[]>([])
const selectedTeamId = ref<string | null>(null)

onMounted(async () => {
  await Promise.all([loadAthlete(), loadTeams()])
  isLoading.value = false
})

async function loadAthlete() {
  athlete.value = await athleteService.getById(props.id)
  if (athlete.value) {
    selectedTeamId.value = athlete.value.teamId ?? null
  }
}

async function loadTeams() {
  allTeams.value = await teamService.getAllNonPaginated()
}

async function handleAssignTeam() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  const response = await athleteService.assignTeam(props.id, { teamId: selectedTeamId.value })
  if (response.succeeded) {
    if (athlete.value) {
      athlete.value.teamId = selectedTeamId.value ?? undefined
      athlete.value.teamName = allTeams.value.find(t => t.id === selectedTeamId.value)?.name
    }
    notifySuccess(t('pages.athletes.detail.validation.successMessage'))
  } else {
    notifyError(t('pages.athletes.detail.validation.failedMessage'))
  }

  preventMultipleSubmit.value = false
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return t('global.undefined')
  return new Date(dateStr).toLocaleDateString()
}
</script>
