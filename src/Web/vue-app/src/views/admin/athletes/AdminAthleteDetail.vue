<template>
  <div class="content-grid">

    <ConfirmModal
      :show="showDeleteNoteModal"
      :title="t('pages.admin.dashboard.athletePage.injuryNotesDeleteModalTitle')"
      :message="t('pages.admin.dashboard.athletePage.injuryNotesDeleteModalMessage')"
      @confirm="onConfirmDeleteNote"
      @cancel="showDeleteNoteModal = false"
    />

    <!-- Edit effort modal moved to top so overlay stacks above content -->
    <EditEffortModal
      v-model:show="showEditEffortModal"
      :key="editingEffort?.id ?? 'edit-effort'"
      :athleteId="athleteId"
      :effortId="editingEffort?.id"
      :effort="editingEffort"
      :existingDates="athleteEfforts.map(e => formatDateOnly(e.createdAt))"
      @saved="onEffortSaved"
    />

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
          <div class="flex items-center gap-3">
              <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
                  {{ athlete ? `${athlete.firstName} ${athlete.lastName}` : t('pages.athletes.detail.title') }}
              </h1>
              <RouterLink class="btn btn--small"
                          :to="{ name: 'admin.children.athletes.edit', params: { id: athlete.id } }">
                  <IconEdit class="icon icon--green" />
              </RouterLink>
          </div>
          <BackLink />
      </div>
    </div>

    <Loader v-if="isLoading" />

    <template v-if="!isLoading && athlete">

      <!-- Section suivi hebdomadaire -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.admin.dashboard.athletePage.weeklyTitle') }}
          </h2>
        </div>
        <div class="p-6 flex flex-col gap-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                {{ t('pages.admin.dashboard.athletePage.efforts.startDate') }}
              </label>
              <input
                type="date"
                v-model="startDateFilter"
                class="border border-grey rounded-lg px-4 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green"
                :placeholder="t('pages.admin.dashboard.athletePage.efforts.startDatePlaceholder')"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                {{ t('pages.admin.dashboard.athletePage.efforts.endDate') }}
              </label>
              <input
                type="date"
                v-model="endDateFilter"
                class="border border-grey rounded-lg px-4 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green"
                :placeholder="t('pages.admin.dashboard.athletePage.efforts.endDatePlaceholder')"
              />
            </div>
          </div>

          <div v-if="loadChartData" class="flex flex-col gap-2">
            <h3 class="font-montserrat font-semibold text-grey-darker text-sm">{{ t('pages.admin.dashboard.athletePage.efforts.chartTitleLoad') }}</h3>
            <div class="h-72 bg-white rounded-lg p-4 border border-grey-light">
              <LineChart :chart-data="loadChartData" :options="loadChartOptions" chart-js-type="bar" class="h-full" />
            </div>
          </div>
          <div v-if="pleasureChartData" class="flex flex-col gap-2 mt-4">
            <h3 class="font-montserrat font-semibold text-grey-darker text-sm">{{ t('pages.admin.dashboard.athletePage.efforts.chartTitlePleasure') }}</h3>
            <div class="h-72 bg-white rounded-lg p-4 border border-grey-light">
              <LineChart :chart-data="pleasureChartData" :options="pleasureChartOptions" class="h-full" />
            </div>
          </div>

          <div>
            <div class="mb-3">
              <button type="button" class="w-full border border-grey rounded-lg bg-white text-center px-4 py-2 font-montserrat text-sm" @click="showEfforts = !showEfforts" aria-label="Voir entrainements">
                Voir entrainements
              </button>
            </div>

            <div v-if="showEfforts && athleteEfforts.length > 0" class="overflow-x-auto">
            <table class="w-full min-w-[36rem] border border-grey-light rounded-lg overflow-hidden">
              <thead class="bg-green-lighter">
                <tr>
                  <th class="text-left px-4 py-3 font-montserrat text-xs uppercase tracking-widest text-green-dark">
                    {{ t('pages.admin.dashboard.athletePage.efforts.date') }}
                  </th>
                  <th class="text-left px-4 py-3 font-montserrat text-xs uppercase tracking-widest text-green-dark">
                    {{ t('pages.admin.dashboard.athletePage.efforts.effort') }}
                  </th>
                  <th class="text-left px-4 py-3 font-montserrat text-xs uppercase tracking-widest text-green-dark">
                    {{ t('pages.admin.dashboard.athletePage.efforts.pleasure') }}
                  </th>
                  <th class="text-left px-4 py-3 font-montserrat text-xs uppercase tracking-widest text-green-dark">
                    {{ t('pages.admin.dashboard.athletePage.efforts.duration') }}
                  </th>
                  <th class="text-right px-4 py-3 font-montserrat text-xs uppercase tracking-widest text-green-dark">&nbsp;</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="effort in athleteEfforts" :key="effort.id" class="border-t border-grey-light">
                  <td class="px-4 py-3 font-montserrat text-sm text-grey-darker">{{ formatDateOnly(effort.createdAt) }}</td>
                  <td class="px-4 py-3 font-montserrat text-sm text-grey-darker">{{ effort.effort }}</td>
                  <td class="px-4 py-3 font-montserrat text-sm text-grey-darker">
                    <span v-if="effort.pleasure !== undefined && effort.pleasure !== null">
                      {{ effort.pleasure }}
                    </span>
                    <span v-else class="text-grey-dark italic">
                      {{ t('pages.admin.dashboard.athletePage.efforts.noPleasureData') }}
                    </span>
                  </td>
                  <td class="px-4 py-3 font-montserrat text-sm text-grey-darker">{{ effort.durationMinutes }}</td>
                  <td class="px-4 py-3 font-montserrat text-sm text-right">
                    <div class="flex items-center justify-end gap-2">
                      <button v-if="userStore?.hasRole && userStore.hasRole(Role.Admin)" type="button" class="btn btn--small" @click="openEditEffort(effort)">
                        <IconEdit class="icon icon--green" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <p v-else-if="showEfforts" class="font-montserrat text-grey-dark italic">
            {{ t('pages.admin.dashboard.athletePage.efforts.empty') }}
          </p>
        </div>
        </div>
      </div>

      <!-- Section calendrier de l'équipe -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <AthleteCalendar :team-id="athlete.teamId ?? null" />
      </div>

      <!-- Section notes de blessure -->
      <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
        <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
          <span class="block w-1.5 h-7 rounded-full bg-green"></span>
          <h2 class="font-montserrat font-semibold text-green-dark text-base">
            {{ t('pages.admin.dashboard.athletePage.injuryNotesTitle') }}
          </h2>
          <div class="ml-auto flex items-center gap-2">
            <span class="font-montserrat text-xs text-grey-dark">
              {{ athlete.isInjured ? t('pages.admin.dashboard.athletePage.injuredStatus') : t('pages.admin.dashboard.athletePage.fitStatus') }}
            </span>
            <button
              type="button"
              role="switch"
              :aria-checked="!athlete.isInjured"
              class="toggle-switch"
              :class="{ 'toggle-switch--active': !athlete.isInjured }"
              @click="handleToggleInjured"
            >
              <span class="toggle-switch__thumb" />
            </button>
          </div>
        </div>
        <div class="p-6 flex flex-col gap-4">
          <div class="flex flex-col gap-2">
            <textarea
              class="border border-grey rounded-lg px-4 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green"
              v-model="newNoteContenu"
              :placeholder="t('pages.admin.dashboard.athletePage.injuryNotesPlaceholder')"
              rows="3"
            ></textarea>
            <div class="flex items-center gap-3">
              <button
                type="button"
                class="btn"
                :disabled="isNoteButtonDisabled"
                @click="handleAddNote"
              >
                {{ t('pages.admin.dashboard.athletePage.injuryNotesSubmit') }}
              </button>
              <p v-if="noteSubmitMessage" class="font-montserrat text-sm text-grey-darker">{{ noteSubmitMessage }}</p>
            </div>
          </div>
          <ul v-if="injuryNotes.length > 0" class="flex flex-col gap-2">
            <li v-for="note in paginatedInjuryNotes" :key="note.id" class="border-b border-grey pb-3">
              <div class="flex flex-col gap-1 min-w-0">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-montserrat text-grey-dark">{{ formatDate(note.createdAt) }}</span>
                  <template v-if="editingNoteId !== note.id">
                    <button
                      type="button"
                      class="inline-flex items-center justify-center text-green transition-colors hover:text-green-dark"
                      @click="startEdit(note)"
                      :aria-label="t('global.actions.update')"
                    >
                      <IconEdit class="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      class="inline-flex items-center justify-center text-red-500 transition-colors hover:text-red-700"
                      @click="handleDeleteNote(note.id!)"
                      :aria-label="t('global.actions.delete')"
                    >
                      <IconDelete class="h-4 w-4" />
                    </button>
                  </template>
                </div>
                  <template v-if="editingNoteId === note.id">
                    <textarea
                      class="border border-grey rounded-lg px-4 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green"
                      v-model="editingNoteContenu"
                      rows="3"
                    ></textarea>
                    <div class="flex gap-2">
                      <button type="button" class="btn btn--primary" :disabled="editingNoteContenu.trim().length === 0" @click="handleSaveEdit(note.id!)">
                        {{ t('pages.admin.dashboard.athletePage.injuryNotesEditSave') }}
                      </button>
                      <button type="button" class="btn" @click="cancelEdit">
                        {{ t('pages.admin.dashboard.athletePage.injuryNotesEditCancel') }}
                      </button>
                    </div>
                  </template>
                  <template v-else>
                    <span class="font-montserrat text-grey-darker break-words">{{ note.contenu }}</span>
                  </template>
              </div>
            </li>
          </ul>
          <div v-if="injuryNotes.length > notesPerPage" class="flex items-center justify-center gap-2 flex-wrap">
            <div class="flex items-center justify-center gap-2 flex-wrap">
              <button
                v-for="page in notesPageNumbers"
                :key="page"
                type="button"
                class="btn"
                :class="page === currentNotesPage
                  ? 'btn--primary underline underline-offset-4 decoration-2'
                  : 'opacity-80 hover:opacity-100'"
                @click="goToNotesPage(page)"
              >
                {{ page }}
              </button>
            </div>
          </div>
          <p v-else-if="injuryNotes.length === 0" class="font-montserrat text-grey-dark italic">
            {{ t('pages.admin.dashboard.athletePage.injuryNotesEmpty') }}
          </p>
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
            <div class="flex items-center gap-3 flex-wrap">
              <span
                v-if="athlete.teamName"
                class="inline-flex items-center px-3 py-1 rounded-full bg-green-lighter text-green-dark text-sm font-montserrat font-semibold border border-green-light"
              >
                {{ athlete.teamName }}
              </span>
              <span v-else class="font-montserrat text-grey-dark italic">{{ t('pages.athletes.detail.noTeam') }}</span>
              <button
                v-if="athlete.teamId"
                type="button"
                :disabled="preventMultipleSubmit"
                class="btn btn--red"
                @click="handleRemoveTeam"
              >
                {{ t('pages.athletes.detail.removeTeam') }}
              </button>
            </div>
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
import {computed, onMounted, ref, watch} from "vue"
import {
  aggregateEffortsByWeek,
  buildWeeklyLoadChart,
  buildWeeklyPleasureChart,
  weeklyLoadChartOptions,
  weeklyPleasureChartOptions,
} from "@/utils/weeklyEffortCharts"
import {useAthleteService, useTeamService} from "@/inversify.config"
import { useUserStore } from '@/stores/userStore'
import { Role } from '@/types/enums'
import {notifyError, notifySuccess} from "@/notify"
import {Athlete, AthleteEffort, NoteBlessure, Team} from "@/types/entities"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import ConfirmModal from "@/components/layouts/items/ConfirmModal.vue"
import LineChart from "@/components/charts/LineChart.vue"
import IconEdit from "@/assets/icons/icon__edit.svg"
import IconDelete from "@/assets/icons/icon__delete.svg"
import EditEffortModal from '@/components/athletes/EditEffortModal.vue'
import AthleteCalendar from '@/components/calendar/AthleteCalendar.vue'

const {t} = useI18n()

const props = defineProps<{ id: string }>()
const athleteId = props.id

const athleteService = useAthleteService()
const teamService = useTeamService()

const isLoading = ref(true)
const preventMultipleSubmit = ref(false)
const athlete = ref<Athlete | null>(null)
const allTeams = ref<Team[]>([])
const selectedTeamId = ref<string | null>(null)
const injuryNotes = ref<NoteBlessure[]>([])
const newNoteContenu = ref<string>("")
const isSubmittingNote = ref<boolean>(false)
const noteSubmitMessage = ref<string>("")
const editingNoteId = ref<string | null>(null)
const editingNoteContenu = ref<string>("")
const showDeleteNoteModal = ref(false)
const pendingDeleteNoteId = ref<string | null>(null)
const notesPerPage = 3
const currentNotesPage = ref(1)
const athleteEfforts = ref<AthleteEffort[]>([])
const loadChartData = ref<Record<string, unknown> | null>(null)
const pleasureChartData = ref<Record<string, unknown> | null>(null)

const loadChartOptions = computed(() => weeklyLoadChartOptions(t))
const pleasureChartOptions = computed(() => weeklyPleasureChartOptions(t))
const editingEffort = ref<any | null>(null)
const showEditEffortModal = ref(false)
const userStore = useUserStore()

const today = new Date()
const mondayBasedDay = (today.getDay() + 6) % 7
const startOfCurrentWeek = new Date(today)
startOfCurrentWeek.setDate(today.getDate() - mondayBasedDay)
const startOfRange = new Date(startOfCurrentWeek)
startOfRange.setDate(startOfCurrentWeek.getDate() - (5 * 7))
const endOfCurrentWeek = new Date(startOfCurrentWeek)
endOfCurrentWeek.setDate(startOfCurrentWeek.getDate() + 6)

const toDateInputValue = (date: Date): string => date.toISOString().split("T")[0]
const startDateFilter = ref<string>(toDateInputValue(startOfRange))
const endDateFilter = ref<string>(toDateInputValue(endOfCurrentWeek))
const showEfforts = ref<boolean>(false)

const isNoteButtonDisabled = computed(() => newNoteContenu.value.trim().length === 0 || isSubmittingNote.value)
const totalNotesPages = computed(() => Math.max(1, Math.ceil(injuryNotes.value.length / notesPerPage)))
const notesPageNumbers = computed(() => Array.from({ length: totalNotesPages.value }, (_, index) => index + 1))
const paginatedInjuryNotes = computed(() => {
  const startIndex = (currentNotesPage.value - 1) * notesPerPage
  return injuryNotes.value.slice(startIndex, startIndex + notesPerPage)
})

onMounted(async () => {
  await Promise.all([loadAthlete(), loadTeams(), loadNotes(), loadEfforts()])
  isLoading.value = false
})

function openEditEffort(effort: any) {
  // copy to avoid mutating list before save
  editingEffort.value = effort ? { ...effort } : null
  showEditEffortModal.value = true
}

async function onEffortSaved() {
  await loadEfforts()
}

watch([startDateFilter, endDateFilter], async () => {
  await loadEfforts()
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

async function handleRemoveTeam() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  const response = await athleteService.assignTeam(props.id, { teamId: null })
  if (response.succeeded) {
    if (athlete.value) {
      athlete.value.teamId = undefined
      athlete.value.teamName = undefined
    }
    selectedTeamId.value = null
    notifySuccess(t('pages.athletes.detail.validation.removeSuccessMessage'))
  } else {
    notifyError(t('pages.athletes.detail.validation.failedMessage'))
  }

  preventMultipleSubmit.value = false
}

async function handleToggleInjured() {
  if (!athlete.value) return
  const newStatus = !athlete.value.isInjured
  const result = await athleteService.toggleInjured(props.id, newStatus)
  if (result.succeeded) {
    athlete.value.isInjured = newStatus
    notifySuccess(newStatus
      ? t('pages.admin.dashboard.athletePage.markedAsInjured')
      : t('pages.admin.dashboard.markAsRecoveredSuccess'))
  } else {
    notifyError(t('pages.admin.dashboard.markAsRecoveredError'))
  }
}

async function loadNotes() {
  injuryNotes.value = await athleteService.getNotesBlessure(props.id)
  clampCurrentNotesPage()
}

async function loadEfforts() {
  const effortsResponse = await athleteService.getAthleteEfforts(props.id, 1, 500, startDateFilter.value, endDateFilter.value)
  const sortedEfforts = [...(effortsResponse.items ?? [])].sort((a, b) => {
    const timeA = new Date(a.createdAt ?? "").getTime()
    const timeB = new Date(b.createdAt ?? "").getTime()
    const safeTimeA = Number.isNaN(timeA) ? 0 : timeA
    const safeTimeB = Number.isNaN(timeB) ? 0 : timeB
    return safeTimeA - safeTimeB
  })

  athleteEfforts.value = sortedEfforts
  loadChartData.value = null
  pleasureChartData.value = null

  if (sortedEfforts.length > 0) {
    const buckets = aggregateEffortsByWeek(sortedEfforts)
    if (buckets.length > 0) {
      loadChartData.value = buildWeeklyLoadChart(buckets, t)
      pleasureChartData.value = buildWeeklyPleasureChart(buckets, t)
    }
  }
}

async function handleAddNote() {
  if (newNoteContenu.value.trim().length === 0) return
  isSubmittingNote.value = true
  noteSubmitMessage.value = ""
  const result = await athleteService.createNoteBlessure(props.id, newNoteContenu.value.trim())
  if (result.succeeded) {
    newNoteContenu.value = ""
    noteSubmitMessage.value = t('pages.admin.dashboard.athletePage.injuryNotesSubmitSuccess')
    injuryNotes.value = await athleteService.getNotesBlessure(props.id)
    currentNotesPage.value = 1
  } else {
    noteSubmitMessage.value = t('pages.admin.dashboard.athletePage.injuryNotesSubmitError')
  }
  isSubmittingNote.value = false
}

function startEdit(note: NoteBlessure) {
  editingNoteId.value = note.id ?? null
  editingNoteContenu.value = note.contenu ?? ""
}

function cancelEdit() {
  editingNoteId.value = null
  editingNoteContenu.value = ""
}

async function handleSaveEdit(noteId: string) {
  if (editingNoteContenu.value.trim().length === 0) return
  const result = await athleteService.updateNoteBlessure(props.id, noteId, editingNoteContenu.value.trim())
  if (result.succeeded) {
    cancelEdit()
    injuryNotes.value = await athleteService.getNotesBlessure(props.id)
    clampCurrentNotesPage()
    notifySuccess(t('pages.admin.dashboard.athletePage.injuryNotesEditSuccess'))
  } else {
    notifyError(t('pages.admin.dashboard.athletePage.injuryNotesEditError'))
  }
}

function handleDeleteNote(noteId: string) {
  pendingDeleteNoteId.value = noteId
  showDeleteNoteModal.value = true
}

async function onConfirmDeleteNote() {
  showDeleteNoteModal.value = false
  if (!pendingDeleteNoteId.value) return
  const result = await athleteService.deleteNoteBlessure(props.id, pendingDeleteNoteId.value)
  pendingDeleteNoteId.value = null
  if (result.succeeded) {
    injuryNotes.value = await athleteService.getNotesBlessure(props.id)
    clampCurrentNotesPage()
    notifySuccess(t('pages.admin.dashboard.athletePage.injuryNotesDeleteSuccess'))
  } else {
    notifyError(t('pages.admin.dashboard.athletePage.injuryNotesDeleteError'))
  }
}

function clampCurrentNotesPage() {
  currentNotesPage.value = Math.min(currentNotesPage.value, totalNotesPages.value)
}

function goToNotesPage(page: number) {
  currentNotesPage.value = page
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return t('global.undefined')
  return new Date(dateStr).toLocaleDateString()
}

function formatDateOnly(value?: string): string {
  if (!value) return ""
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  }).format(date)
}

</script>
