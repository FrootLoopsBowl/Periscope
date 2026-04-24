<template>
  <div class="team-calendar">
    <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
      <span class="block w-1.5 h-7 rounded-full bg-green"></span>
      <h2 class="font-montserrat font-semibold text-green-dark text-base">
        {{ t('pages.teams.calendar.title') }}
      </h2>
    </div>

    <div class="p-6">
<div class="relative">
        <div v-if="isLoading" class="absolute inset-0 flex items-center justify-center bg-white bg-opacity-70 z-10">
          <Loader />
        </div>
        <vue-cal
          active-view="month"
          locale="fr"
          :disable-views="['years', 'year', 'week', 'day']"
          :events="calendarEvents"
          :time="false"
          events-on-month-view="short"
          hide-view-selector
          style="height: 600px;"
          @cell-click="onCellClick"
          @event-click="onEventClick"
          @ready="onViewChange"
          @view-change="onViewChange"
        />
      </div>
    </div>

    <!-- Modal création -->
    <div v-if="showCreateModal" class="calendar-modal">
      <span class="calendar-modal__bg" @click="showCreateModal = false"></span>
      <div class="calendar-modal__container">
        <div class="calendar-modal__header">
          <h3 class="font-montserrat font-semibold text-green-dark">{{ t('pages.teams.calendar.addEvent') }}</h3>
          <button type="button" class="calendar-modal__close" @click="showCreateModal = false">×</button>
        </div>
        <div class="calendar-modal__body">
          <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-1">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.typeLabel') }}</label>
              <select v-model="createForm.type" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green">
                <option value="Pratique">{{ t('pages.teams.calendar.typePratique') }}</option>
                <option value="Match">{{ t('pages.teams.calendar.typeMatch') }}</option>
              </select>
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.dateLabel') }}</label>
              <input type="date" v-model="createForm.date" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.startLabel') }}</label>
                <input type="time" v-model="createForm.startTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.endLabel') }}</label>
                <input type="time" v-model="createForm.endTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.descriptionLabel') }}</label>
              <textarea v-model="createForm.description" rows="3" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green resize-none" :placeholder="t('pages.teams.calendar.descriptionPlaceholder')" />
            </div>
          </div>
        </div>
        <div class="calendar-modal__actions">
          <button type="button" class="btn" @click="showCreateModal = false">{{ t('global.cancel') }}</button>
          <button type="button" class="btn btn--primary" :disabled="isSaving" @click="handleCreate">{{ t('global.save') }}</button>
        </div>
      </div>
    </div>

    <!-- Modal édition / suppression -->
    <div v-if="showEditModal && selectedEvent" class="calendar-modal">
      <span class="calendar-modal__bg" @click="showEditModal = false"></span>
      <div class="calendar-modal__container">
        <div class="calendar-modal__header">
          <h3 class="font-montserrat font-semibold text-green-dark">{{ t('pages.teams.calendar.editEvent') }}</h3>
          <button type="button" class="calendar-modal__close" @click="showEditModal = false">×</button>
        </div>
        <div class="calendar-modal__body">
          <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-1">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.typeLabel') }}</label>
              <select v-model="editForm.type" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green">
                <option value="Pratique">{{ t('pages.teams.calendar.typePratique') }}</option>
                <option value="Match">{{ t('pages.teams.calendar.typeMatch') }}</option>
              </select>
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.dateLabel') }}</label>
              <input type="date" v-model="editForm.date" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.startLabel') }}</label>
                <input type="time" v-model="editForm.startTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.endLabel') }}</label>
                <input type="time" v-model="editForm.endTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.descriptionLabel') }}</label>
              <textarea v-model="editForm.description" rows="3" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green resize-none" :placeholder="t('pages.teams.calendar.descriptionPlaceholder')" />
            </div>
          </div>
        </div>
        <div class="calendar-modal__actions justify-between">
          <button type="button" class="btn btn--danger" :disabled="isSaving" @click="handleDelete">{{ t('pages.teams.calendar.deleteEvent') }}</button>
          <div class="flex gap-2">
            <button type="button" class="btn" @click="showEditModal = false">{{ t('global.cancel') }}</button>
            <button type="button" class="btn btn--primary" :disabled="isSaving" @click="handleUpdate">{{ t('global.save') }}</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import { useI18n } from 'vue3-i18n'
import VueCal from 'vue-cal'
import 'vue-cal/dist/vuecal.css'
import { useTeamEventService } from '@/inversify.config'
import { notifySuccess, notifyError } from '@/notify'
import { TeamEvent } from '@/types/entities'
import Loader from '@/components/layouts/items/Loader.vue'

interface Props {
  teamId: string
}

const props = defineProps<Props>()
const { t } = useI18n()
const teamEventService = useTeamEventService()

const isLoading = ref(false)
const isSaving = ref(false)
const rawEvents = ref<TeamEvent[]>([])

const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedEvent = ref<TeamEvent | null>(null)

const createForm = ref({ type: 'Pratique', date: '', startTime: '08:00', endTime: '10:00', description: '' })
const editForm = ref({ type: 'Pratique', date: '', startTime: '08:00', endTime: '10:00', description: '' })

let currentFromDate: Date | null = null
let currentToDate: Date | null = null

const calendarEvents = computed(() =>
  rawEvents.value.map(e => ({
    start: e.startDateTime!.replace('T', ' ').substring(0, 16),
    end: e.endDateTime!.replace('T', ' ').substring(0, 16),
    title: e.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch'),
    class: e.type === 'Pratique' ? 'event-pratique' : 'event-match',
    _raw: e
  }))
)

function toLocalDateStr(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function toIso(date: string, time: string): string {
  return `${date}T${time}:00`
}

function onCellClick(date: Date) {
  createForm.value = {
    type: 'Pratique',
    date: toLocalDateStr(date),
    startTime: '08:00',
    endTime: '10:00',
    description: ''
  }
  showCreateModal.value = true
}

function onEventClick(event: any) {
  const raw = event._raw as TeamEvent
  selectedEvent.value = raw
  editForm.value = {
    type: raw.type ?? 'Pratique',
    date: raw.startDateTime!.substring(0, 10),
    startTime: raw.startDateTime!.substring(11, 16),
    endTime: raw.endDateTime!.substring(11, 16),
    description: raw.description ?? ''
  }
  showEditModal.value = true
}

function onViewChange({ startDate, endDate }: { startDate: Date; endDate: Date }) {
  loadEvents(startDate, endDate)
}

async function loadEvents(from: Date, to: Date) {
  if (!from || isNaN(from.getTime())) return
  isLoading.value = true
  currentFromDate = from
  currentToDate = to
  try {
    rawEvents.value = await teamEventService.getEvents(props.teamId, from.toISOString(), to.toISOString())
  } finally {
    isLoading.value = false
  }
}

async function reload() {
  if (currentFromDate && currentToDate) {
    await loadEvents(currentFromDate, currentToDate)
  }
}

async function handleCreate() {
  isSaving.value = true
  const result = await teamEventService.createEvent(props.teamId, {
    type: createForm.value.type,
    startDateTime: toIso(createForm.value.date, createForm.value.startTime),
    endDateTime: toIso(createForm.value.date, createForm.value.endTime),
    description: createForm.value.description || null
  })
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.saveSuccess'))
    showCreateModal.value = false
    await reload()
  } else {
    notifyError(t('pages.teams.calendar.saveError'))
  }
}

async function handleUpdate() {
  if (!selectedEvent.value?.id) return
  isSaving.value = true
  const result = await teamEventService.updateEvent(props.teamId, selectedEvent.value.id, {
    type: editForm.value.type,
    startDateTime: toIso(editForm.value.date, editForm.value.startTime),
    endDateTime: toIso(editForm.value.date, editForm.value.endTime),
    description: editForm.value.description || null
  })
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.saveSuccess'))
    showEditModal.value = false
    await reload()
  } else {
    notifyError(t('pages.teams.calendar.saveError'))
  }
}

async function handleDelete() {
  if (!selectedEvent.value?.id) return
  isSaving.value = true
  const result = await teamEventService.deleteEvent(props.teamId, selectedEvent.value.id)
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.deleteSuccess'))
    showEditModal.value = false
    await reload()
  } else {
    notifyError(t('pages.teams.calendar.deleteError'))
  }
}
</script>

<style scoped>
:deep(.vuecal--month-view .vuecal__cell-content) {
  justify-content: flex-start;
}

:deep(.vuecal__cell:not(.vuecal__cell--out-of-scope)) {
  cursor: pointer;
  transition: background-color 0.15s;
}

:deep(.vuecal__cell:not(.vuecal__cell--out-of-scope):hover) {
  background-color: rgba(94, 32, 40, 0.08);
}

:deep(.vuecal__cell:not(.vuecal__cell--out-of-scope)::after) {
  content: '+';
  position: absolute;
  bottom: 4px;
  right: 8px;
  font-size: 1.3rem;
  font-weight: 300;
  color: #5e2028;
  opacity: 0;
  transition: opacity 0.15s;
  pointer-events: none;
  line-height: 1;
}

:deep(.vuecal__cell:not(.vuecal__cell--out-of-scope):hover::after) {
  opacity: 0.7;
}

:deep(.vuecal__cell-date) {
  text-align: center;
  width: 100%;
}

:deep(.event-pratique) {
  background-color: #4caf50;
  color: white;
  border-radius: 4px;
  padding: 1px 4px;
}

:deep(.event-match) {
  background-color: #1565c0;
  color: white;
  border-radius: 4px;
  padding: 1px 4px;
}

.calendar-modal {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.calendar-modal__bg {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
}

.calendar-modal__container {
  position: relative;
  background: #fff;
  border-radius: 0.75rem;
  width: 95%;
  max-width: 480px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.2);
  overflow: hidden;
}

.calendar-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  background: var(--color-green-lighter, #e8f5e9);
  border-bottom: 1px solid var(--color-green-light, #c8e6c9);
}

.calendar-modal__close {
  background: none;
  border: none;
  font-size: 1.75rem;
  line-height: 1;
  cursor: pointer;
  color: var(--color-green-dark, #2c3e50);
}

.calendar-modal__body {
  padding: 1.5rem;
}

.calendar-modal__actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--color-grey-light, #e0e0e0);
  background: var(--color-grey-lighter, #fafafa);
}
</style>
