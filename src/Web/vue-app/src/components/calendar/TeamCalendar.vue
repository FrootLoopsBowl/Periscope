<template>
  <div class="team-calendar">
    <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
      <span class="block w-1.5 h-7 rounded-full bg-green"></span>
      <h2 class="font-montserrat font-semibold text-green-dark text-base">
        {{ t('pages.teams.calendar.title') }}
      </h2>
    </div>

    <div class="p-6">
      <Loader v-if="isLoading" />

      <vue-cal
        v-else
        active-view="month"
        :disable-views="['years', 'year', 'week', 'day']"
        :events="calendarEvents"
        :locale="currentLocale"
        :time="false"
        hide-view-selector
        @cell-click="onCellClick"
        @event-click="onEventClick"
        @view-change="onViewChange"
        class="team-calendar__cal"
      />
    </div>

    <Transition name="fade">
      <div v-if="showCreateModal" class="calendar-modal">
        <span class="calendar-modal__bg" @click="showCreateModal = false"></span>
        <div class="calendar-modal__container">
          <div class="calendar-modal__header">
            <h3 class="font-montserrat font-semibold text-green-dark">
              {{ t('pages.teams.calendar.addEvent') }}
            </h3>
            <button type="button" class="calendar-modal__close" @click="showCreateModal = false">×</button>
          </div>
          <div class="calendar-modal__body">
            <div class="flex flex-col gap-4">
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.typeLabel') }}
                </label>
                <select v-model="createForm.type" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green">
                  <option value="Pratique">{{ t('pages.teams.calendar.typePratique') }}</option>
                  <option value="Match">{{ t('pages.teams.calendar.typeMatch') }}</option>
                </select>
              </div>
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.dateLabel') }}
                </label>
                <input type="date" v-model="createForm.date" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.startLabel') }}
                  </label>
                  <input type="time" v-model="createForm.startTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.endLabel') }}
                  </label>
                  <input type="time" v-model="createForm.endTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
              </div>
            </div>
          </div>
          <div class="calendar-modal__actions">
            <button type="button" class="btn" @click="showCreateModal = false">{{ t('global.cancel') }}</button>
            <button type="button" class="btn btn--primary" :disabled="isSaving" @click="handleCreate">
              {{ t('global.save') }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <Transition name="fade">
      <div v-if="showEditModal && selectedEvent" class="calendar-modal">
        <span class="calendar-modal__bg" @click="showEditModal = false"></span>
        <div class="calendar-modal__container">
          <div class="calendar-modal__header">
            <h3 class="font-montserrat font-semibold text-green-dark">
              {{ t('pages.teams.calendar.editEvent') }}
            </h3>
            <button type="button" class="calendar-modal__close" @click="showEditModal = false">×</button>
          </div>
          <div class="calendar-modal__body">
            <div class="flex flex-col gap-4">
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.typeLabel') }}
                </label>
                <select v-model="editForm.type" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green">
                  <option value="Pratique">{{ t('pages.teams.calendar.typePratique') }}</option>
                  <option value="Match">{{ t('pages.teams.calendar.typeMatch') }}</option>
                </select>
              </div>
              <div class="flex flex-col gap-1">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                  {{ t('pages.teams.calendar.dateLabel') }}
                </label>
                <input type="date" v-model="editForm.date" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.startLabel') }}
                  </label>
                  <input type="time" v-model="editForm.startTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
                <div class="flex flex-col gap-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">
                    {{ t('pages.teams.calendar.endLabel') }}
                  </label>
                  <input type="time" v-model="editForm.endTime" class="border border-grey rounded-lg px-3 py-2 font-montserrat text-grey-darker focus:outline-none focus:border-green" />
                </div>
              </div>
            </div>
          </div>
          <div class="calendar-modal__actions">
            <button type="button" class="btn btn--danger" :disabled="isSaving" @click="handleDelete">
              {{ t('pages.teams.calendar.deleteEvent') }}
            </button>
            <div class="flex gap-2">
              <button type="button" class="btn" @click="showEditModal = false">{{ t('global.cancel') }}</button>
              <button type="button" class="btn btn--primary" :disabled="isSaving" @click="handleUpdate">
                {{ t('global.save') }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue'
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
const { t, locale } = useI18n()
const teamEventService = useTeamEventService()

const currentLocale = computed(() => locale.value === 'fr' ? 'fr' : 'en')

const isLoading = ref(false)
const isSaving = ref(false)
const rawEvents = ref<TeamEvent[]>([])

const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedEvent = ref<TeamEvent | null>(null)

const createForm = ref({ type: 'Pratique', date: '', startTime: '08:00', endTime: '10:00' })
const editForm = ref({ type: 'Pratique', date: '', startTime: '08:00', endTime: '10:00' })

let currentFrom = ''
let currentTo = ''

const calendarEvents = computed(() =>
  rawEvents.value.map(e => ({
    start: toVueCalDate(e.startDateTime!),
    end: toVueCalDate(e.endDateTime!),
    title: e.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch'),
    class: e.type === 'Pratique' ? 'event-pratique' : 'event-match',
    _raw: e
  }))
)

function toVueCalDate(iso: string): string {
  return iso.replace('T', ' ').substring(0, 16)
}

function toDateInputValue(iso: string): string {
  return iso.substring(0, 10)
}

function toTimeInputValue(iso: string): string {
  return iso.substring(11, 16)
}

function toIso(date: string, time: string): string {
  return `${date}T${time}:00`
}

async function loadEvents(from: Date, to: Date) {
  isLoading.value = true
  currentFrom = from.toISOString()
  currentTo = to.toISOString()
  rawEvents.value = await teamEventService.getEvents(props.teamId, currentFrom, currentTo)
  isLoading.value = false
}

function onViewChange({ startDate, endDate }: { startDate: Date; endDate: Date }) {
  loadEvents(startDate, endDate)
}

function onCellClick(date: Date) {
  const dateStr = date.toISOString().substring(0, 10)
  createForm.value = { type: 'Pratique', date: dateStr, startTime: '08:00', endTime: '10:00' }
  showCreateModal.value = true
}

function onEventClick({ event }: { event: any }) {
  const raw = event._raw as TeamEvent
  selectedEvent.value = raw
  editForm.value = {
    type: raw.type ?? 'Pratique',
    date: toDateInputValue(raw.startDateTime!),
    startTime: toTimeInputValue(raw.startDateTime!),
    endTime: toTimeInputValue(raw.endDateTime!)
  }
  showEditModal.value = true
}

async function handleCreate() {
  isSaving.value = true
  const result = await teamEventService.createEvent(props.teamId, {
    type: createForm.value.type,
    startDateTime: toIso(createForm.value.date, createForm.value.startTime),
    endDateTime: toIso(createForm.value.date, createForm.value.endTime)
  })
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.saveSuccess'))
    showCreateModal.value = false
    await loadEvents(new Date(currentFrom), new Date(currentTo))
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
    endDateTime: toIso(editForm.value.date, editForm.value.endTime)
  })
  isSaving.value = false
  if (result.succeeded) {
    notifySuccess(t('pages.teams.calendar.saveSuccess'))
    showEditModal.value = false
    await loadEvents(new Date(currentFrom), new Date(currentTo))
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
    await loadEvents(new Date(currentFrom), new Date(currentTo))
  } else {
    notifyError(t('pages.teams.calendar.deleteError'))
  }
}

onMounted(() => {
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  loadEvents(from, to)
})
</script>

<style scoped>
.team-calendar__cal {
  height: 600px;
}

:deep(.event-pratique) {
  background-color: #4caf50;
  color: white;
  border-radius: 4px;
}

:deep(.event-match) {
  background-color: #1565c0;
  color: white;
  border-radius: 4px;
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
  box-shadow: var(--shadow-bold, 0 20px 40px rgba(0,0,0,0.2));
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
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--color-grey-light, #e0e0e0);
  background: var(--color-grey-lighter, #fafafa);
}

.fade-leave-active,
.fade-enter-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
