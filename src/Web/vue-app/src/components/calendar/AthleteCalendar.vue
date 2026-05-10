<template>
  <div>
    <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
      <span class="block w-1.5 h-7 rounded-full bg-green"></span>
      <h2 class="font-montserrat font-semibold text-green-dark text-base">
        {{ t('pages.athletes.calendar.title') }}
      </h2>
    </div>

    <div class="p-6">
      <p v-if="!teamId" class="font-montserrat text-grey-dark italic">
        {{ t('pages.athletes.calendar.noTeam') }}
      </p>

      <div v-else class="relative">
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
          :editable-events="false"
          style="height: 600px;"
          @ready="onViewChange"
          @view-change="onViewChange"
          @event-click="onEventClick"
        />
      </div>
    </div>

    <!-- Modal détail (lecture seule) -->
    <div v-if="showDetailModal && selectedEvent" class="calendar-modal">
      <span class="calendar-modal__bg" @click="showDetailModal = false"></span>
      <div class="calendar-modal__container">
        <div class="calendar-modal__header">
          <h3 class="font-montserrat font-semibold text-green-dark">
            {{ selectedEvent.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch') }}
          </h3>
          <button type="button" class="calendar-modal__close" @click="showDetailModal = false">×</button>
        </div>
        <div class="calendar-modal__body">
          <dl class="flex flex-col gap-3">
            <div class="flex flex-col gap-0.5">
              <dt class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.dateLabel') }}</dt>
              <dd class="font-montserrat text-grey-darker">{{ formatDate(selectedEvent.startDateTime!) }}</dd>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div class="flex flex-col gap-0.5">
                <dt class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.startLabel') }}</dt>
                <dd class="font-montserrat text-grey-darker">{{ selectedEvent.startDateTime!.substring(11, 16) }}</dd>
              </div>
              <div class="flex flex-col gap-0.5">
                <dt class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.endLabel') }}</dt>
                <dd class="font-montserrat text-grey-darker">{{ selectedEvent.endDateTime!.substring(11, 16) }}</dd>
              </div>
            </div>
            <div v-if="selectedEvent.description" class="flex flex-col gap-0.5">
              <dt class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('pages.teams.calendar.descriptionLabel') }}</dt>
              <dd class="font-montserrat text-grey-darker whitespace-pre-wrap">{{ selectedEvent.description }}</dd>
            </div>
          </dl>
        </div>
        <div class="calendar-modal__actions">
          <button type="button" class="btn btn--primary" @click="showDetailModal = false">{{ t('global.close') }}</button>
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
import { TeamEvent } from '@/types/entities'
import Loader from '@/components/layouts/items/Loader.vue'

interface Props {
  teamId?: string | null
}

const props = defineProps<Props>()
const { t } = useI18n()
const teamEventService = useTeamEventService()

const isLoading = ref(false)
const rawEvents = ref<TeamEvent[]>([])
const showDetailModal = ref(false)
const selectedEvent = ref<TeamEvent | null>(null)

const calendarEvents = computed(() =>
  rawEvents.value.map(e => ({
    start: e.startDateTime!.replace('T', ' ').substring(0, 16),
    end: e.endDateTime!.replace('T', ' ').substring(0, 16),
    title: e.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch'),
    class: e.type === 'Pratique' ? 'event-pratique' : 'event-match',
    _raw: e
  }))
)

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleDateString('fr-CA', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
}

function onEventClick(event: any) {
  selectedEvent.value = event._raw as TeamEvent
  showDetailModal.value = true
}

async function loadEvents(from: Date, to: Date) {
  if (!props.teamId || !from || isNaN(from.getTime())) return
  isLoading.value = true
  try {
    rawEvents.value = await teamEventService.getEvents(props.teamId, from.toISOString(), to.toISOString())
  } finally {
    isLoading.value = false
  }
}

function onViewChange({ startDate, endDate }: { startDate: Date; endDate: Date }) {
  loadEvents(startDate, endDate)
}
</script>

<style scoped>
:deep(.vuecal--month-view .vuecal__cell-content) {
  justify-content: flex-start;
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
  cursor: pointer;
  display: block;
  width: 100%;
  text-align: center;
}

:deep(.event-match) {
  background-color: #1565c0;
  color: white;
  border-radius: 4px;
  padding: 1px 4px;
  cursor: pointer;
  display: block;
  width: 100%;
  text-align: center;
}

.calendar-modal {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

:deep(.vuecal--month-view .vuecal__cell-content .vuecal__event) {
  display: flex !important;
  justify-content: center !important;
  align-items: center !important;
  width: 100% !important;
  box-sizing: border-box;
  padding: 0 !important;
}

:deep(.vuecal--month-view .vuecal__cell-content .vuecal__event .vuecal__event-title) {
  width: 100% !important;
  text-align: center !important;
  display: block !important;
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
  max-width: 420px;
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
