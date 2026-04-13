<template>
  <div class="content-grid">

    <ConfirmModal
      :show="showDeleteNoteModal"
      :title="t('pages.admin.dashboard.athletePage.injuryNotesDeleteModalTitle')"
      :message="t('pages.admin.dashboard.athletePage.injuryNotesDeleteModalMessage')"
      @confirm="onConfirmDeleteNote"
      @cancel="showDeleteNoteModal = false"
    />

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
            <li v-for="note in injuryNotes" :key="note.id" class="flex flex-col gap-1 border-b border-grey pb-2">
              <span class="text-xs font-montserrat text-grey-dark">{{ formatDate(note.createdAt) }}</span>
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
                <span class="font-montserrat text-grey-darker">{{ note.contenu }}</span>
                <div class="flex gap-2 mt-1">
                  <button type="button" class="btn" @click="startEdit(note)"><IconEdit class="icon icon--green" /></button>
                  <button type="button" class="btn btn--red" @click="handleDeleteNote(note.id!)"><IconDelete class="icon icon--green" /></button>
                </div>
              </template>
            </li>
          </ul>
          <p v-else class="font-montserrat text-grey-dark italic">
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
import {computed, onMounted, ref} from "vue"
import {useAthleteService, useTeamService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {Athlete, NoteBlessure, Team} from "@/types/entities"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import ConfirmModal from "@/components/layouts/items/ConfirmModal.vue"
import IconEdit from "@/assets/icons/icon__edit.svg"
import IconDelete from "@/assets/icons/icon__delete.svg"

const {t} = useI18n()

const props = defineProps<{ id: string }>()

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

const isNoteButtonDisabled = computed(() => newNoteContenu.value.trim().length === 0 || isSubmittingNote.value)

onMounted(async () => {
  await Promise.all([loadAthlete(), loadTeams(), loadNotes()])
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
    notifySuccess(t('pages.admin.dashboard.athletePage.injuryNotesDeleteSuccess'))
  } else {
    notifyError(t('pages.admin.dashboard.athletePage.injuryNotesDeleteError'))
  }
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return t('global.undefined')
  return new Date(dateStr).toLocaleDateString()
}
</script>
