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
          :disable-views="['years', 'year', 'week', 'day']"
          :events="calendarEvents"
          :locale="currentLocale"
          :time="false"
          hide-view-selector
          :editable-events="false"
          @view-change="onViewChange"
          style="height: 600px;"
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import { useI18n } from 'vue3-i18n'
import VueCal from 'vue-cal'
import 'vue-cal/dist/vuecal.css'
// @ts-ignore
import frLocale from 'vue-cal/dist/i18n/fr.es.js'
// @ts-ignore
import enLocale from 'vue-cal/dist/i18n/en.es.js'
import { useTeamEventService } from '@/inversify.config'
import { TeamEvent } from '@/types/entities'
import Loader from '@/components/layouts/items/Loader.vue'

interface Props {
  teamId?: string | null
}

const props = defineProps<Props>()
const { t, locale } = useI18n()
const teamEventService = useTeamEventService()

const currentLocale = computed(() => locale.value === 'fr' ? frLocale : enLocale)
const isLoading = ref(false)
const rawEvents = ref<TeamEvent[]>([])

const calendarEvents = computed(() =>
  rawEvents.value.map(e => ({
    start: e.startDateTime!.replace('T', ' ').substring(0, 16),
    end: e.endDateTime!.replace('T', ' ').substring(0, 16),
    title: e.type === 'Pratique' ? t('pages.teams.calendar.typePratique') : t('pages.teams.calendar.typeMatch'),
    class: e.type === 'Pratique' ? 'event-pratique' : 'event-match'
  }))
)

async function loadEvents(from: Date, to: Date) {
  if (!props.teamId) return
  isLoading.value = true
  rawEvents.value = await teamEventService.getEvents(props.teamId, from.toISOString(), to.toISOString())
  isLoading.value = false
}

function onViewChange({ startDate, endDate }: { startDate: Date; endDate: Date }) {
  loadEvents(startDate, endDate)
}

</script>

<style scoped>
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
</style>
