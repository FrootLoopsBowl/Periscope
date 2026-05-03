<template>
  <div class="content-grid">
    <div class="content-grid__header">
      <div class="admin-dashboard__hero">
        <h1 class="back-link-title">{{ t("routes.admin.children.dashboard.name") }}</h1>
      </div>
    </div>

    <Card :title="t('pages.admin.dashboard.overloadedAthletesTitle')">
      <div v-if="overloadedAthletes.length > 0" class="admin-dashboard__efforts-table">
        <table class="admin-dashboard__table">
          <thead>
            <tr>
              <th>{{ t("pages.admin.dashboard.overloadedTableFirstName") }}</th>
              <th>{{ t("pages.admin.dashboard.overloadedTableLastName") }}</th>
              <th>{{ t("pages.admin.dashboard.overloadedTableTeam") }}</th>
              <th>{{ t("pages.admin.dashboard.overloadedTablePercentage") }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="athlete in overloadedAthletes"
              :key="athlete.id"
              class="admin-dashboard__overload-row"
              @click="router.push({ name: 'admin.children.athletes.detail', params: { id: athlete.id } })"
            >
              <td>{{ athlete.firstName }}</td>
              <td>{{ athlete.lastName }}</td>
              <td>{{ athlete.teamName ?? '-' }}</td>
              <td>{{ athlete.overloadPercentage }}%</td>
            </tr>
          </tbody>
        </table>
      </div>
      <p v-else class="content-grid__text">
        {{ t("pages.admin.dashboard.overloadedAthletesEmpty") }}
      </p>
    </Card>

    <Card :title="t('pages.admin.dashboard.injuredAthletesTitle')">
        <div v-if="injuredAthletes.length > 0" class="admin-dashboard__efforts-table">
            <table class="admin-dashboard__table">
                <thead>
                    <tr>
                        <th>{{ t("pages.admin.dashboard.overloadedTableFirstName") }}</th>
                        <th>{{ t("pages.admin.dashboard.overloadedTableLastName") }}</th>
                        <th>{{ t("pages.admin.dashboard.overloadedTableTeam") }}</th>
                        <th>{{ t("pages.admin.dashboard.filters.injuredTableLastNote") }}</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="athlete in injuredAthletes"
                        :key="athlete.id"
                        class="admin-dashboard__overload-row"
                        @click="router.push({ name: 'admin.children.athletes.detail', params: { id: athlete.id } })">
                        <td>{{ athlete.firstName }}</td>
                        <td>{{ athlete.lastName }}</td>
                        <td>{{ athlete.teamName ?? '-' }}</td>
                        <td>{{ athlete.lastInjuryNote || '-' }}</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <p v-else class="content-grid__text">
            {{ t("pages.admin.dashboard.injuredAthletesEmpty") }}
        </p>
    </Card>

    <Card :title="t('pages.admin.dashboard.filters.title')">
      <div class="admin-dashboard__filters">
        <div class="admin-dashboard__field">
          <label class="admin-dashboard__label" for="team-select">
            {{ t("pages.admin.dashboard.filters.teamLabel") }}
          </label>
          <template v-if="teamOptions.length > 0">
            <select
              id="team-select"
              class="admin-dashboard__select"
              v-model="selectedTeamId"
            >
              <option value="">
                {{ t("pages.admin.dashboard.filters.selectPlaceholder") }}
              </option>
              <option v-for="option in teamOptions" :key="option.name" :value="option.name">
                {{ option.label }}
              </option>
            </select>
          </template>
          <p v-else class="content-grid__text">
            {{ t("pages.admin.dashboard.filters.noTeams") }}
          </p>
        </div>

        <div class="admin-dashboard__field">
          <label class="admin-dashboard__label" for="athlete-select">
            {{ t("pages.admin.dashboard.filters.athleteLabel") }}
          </label>
          <template v-if="athleteOptions.length > 0">
            <select
              id="athlete-select"
              class="admin-dashboard__select"
              v-model="selectedAthleteId"
              @change ="handleSearch"
            >
              <option value="">
                {{ t("pages.admin.dashboard.filters.selectPlaceholder") }}
              </option>
              <option v-for="option in athleteOptions" :key="option.name" :value="option.name">
                {{ option.label }}
              </option>
            </select>
          </template>
          <p v-else class="content-grid__text">
            {{ t("pages.admin.dashboard.filters.noAthletes") }}
          </p>
        </div>
      </div>
    </Card>

    <div v-if="displayedAthlete" class="admin-dashboard__athlete-page">
      <Card :title="t('pages.admin.dashboard.athletePage.weeklyTitle')">
        <!-- Date Filters -->
        <div class="admin-dashboard__date-filters">
          <div class="admin-dashboard__filter-group">
            <label class="admin-dashboard__filter-label">{{ t('pages.admin.dashboard.athletePage.efforts.startDate') }}</label>
            <input
              type="date"
              v-model="startDateFilter"
              class="admin-dashboard__filter-input"
              :placeholder="t('pages.admin.dashboard.athletePage.efforts.startDatePlaceholder')"
            />
          </div>
          <div class="admin-dashboard__filter-group">
            <label class="admin-dashboard__filter-label">{{ t('pages.admin.dashboard.athletePage.efforts.endDate') }}</label>
            <input
              type="date"
              v-model="endDateFilter"
              class="admin-dashboard__filter-input"
              :placeholder="t('pages.admin.dashboard.athletePage.efforts.endDatePlaceholder')"
            />
          </div>
        </div>

        <!-- Charts (charges + plaisir, agrégation hebdo côté client) -->
        <div v-if="loadChartData" class="admin-dashboard__chart-block">
          <h3 class="admin-dashboard__chart-title">{{ t('pages.admin.dashboard.athletePage.efforts.chartTitleLoad') }}</h3>
          <div class="admin-dashboard__chart-container">
            <LineChart
              :chart-data="loadChartData"
              :options="loadChartOptions"
              class="admin-dashboard__chart"
            />
          </div>
        </div>
        <div v-if="pleasureChartData" class="admin-dashboard__chart-block">
          <h3 class="admin-dashboard__chart-title">{{ t('pages.admin.dashboard.athletePage.efforts.chartTitlePleasure') }}</h3>
          <div class="admin-dashboard__chart-container">
            <LineChart
              :chart-data="pleasureChartData"
              :options="pleasureChartOptions"
              class="admin-dashboard__chart"
            />
          </div>
        </div>

        <p v-if="athleteEfforts.length === 0" class="content-grid__text">
          {{ t("pages.admin.dashboard.athletePage.efforts.empty") }}
        </p>
      </Card>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, watch } from "vue";
import {
  aggregateEffortsByWeek,
  buildWeeklyLoadChart,
  buildWeeklyPleasureChart,
  weeklyLoadChartOptions,
  weeklyPleasureChartOptions,
} from "@/utils/weeklyEffortCharts";
import { useRouter } from "vue-router";
import { useI18n } from "vue3-i18n";
import { useAthleteService, useTeamService } from "@/inversify.config";
import Card from "@/components/layouts/items/Card.vue";
import LineChart from "@/components/charts/LineChart.vue";
import IconBandage from 'vue-material-design-icons/Bandage.vue';
import { FormOption } from "@/types/formOption";
import { Athlete, AthleteEffort, NoteBlessure, OverloadedAthlete, Team } from "@/types/entities";

const { t } = useI18n();
const router = useRouter();

const teamService = useTeamService();
const athleteService = useAthleteService();

const selectedTeamId = ref<string>("");
const selectedAthleteId = ref<string>("");
const displayedAthleteId = ref<string>("");

const teams = ref<Team[]>([]);
const athletes = ref<Athlete[]>([]);
type InjuredAthleteWithNote = Athlete & {
    lastInjuryNote?: string;
};
const injuredAthletes = ref<InjuredAthleteWithNote[]>([]);
const overloadedAthletes = ref<OverloadedAthlete[]>([]);
const injuryNotes = ref<NoteBlessure[]>([]);
const athleteEfforts = ref<AthleteEffort[]>([]);
const loadChartData = ref<Record<string, unknown> | null>(null);
const pleasureChartData = ref<Record<string, unknown> | null>(null);

const loadChartOptions = computed(() => weeklyLoadChartOptions(t));
const pleasureChartOptions = computed(() => weeklyPleasureChartOptions(t));
const today = new Date();
const mondayBasedDay = (today.getDay() + 6) % 7;
const startOfCurrentWeek = new Date(today);
startOfCurrentWeek.setDate(today.getDate() - mondayBasedDay);
const startOfRange = new Date(startOfCurrentWeek);
startOfRange.setDate(startOfCurrentWeek.getDate() - (5 * 7));
const endOfCurrentWeek = new Date(startOfCurrentWeek);
endOfCurrentWeek.setDate(startOfCurrentWeek.getDate() + 6);

const toDateInputValue = (date: Date) => date.toISOString().split("T")[0];

const startDateFilter = ref<string>(toDateInputValue(startOfRange));
const endDateFilter = ref<string>(toDateInputValue(endOfCurrentWeek));
const newNoteContenu = ref<string>("");
const isSubmittingNote = ref<boolean>(false);
const noteSubmitMessage = ref<string>("");

const teamOptions = computed<FormOption[]>(() =>
  teams.value.map((team) => ({
    name: team.id ?? "",
    label: team.name ?? "",
  })).filter((option) => option.name.length > 0 && option.label.length > 0)
);

const athleteOptions = computed<FormOption[]>(() => {
  let filteredAthletes = athletes.value;
  
  // Filtrer par équipe si une équipe est sélectionnée
  if (selectedTeamId.value) {
    filteredAthletes = athletes.value.filter(a => a.teamId === selectedTeamId.value);
  }
  
  return filteredAthletes.map((athlete) => ({
    name: athlete.id ?? "",
    label: `${athlete.firstName ?? ""} ${athlete.lastName ?? ""}`.trim(),
  })).filter((option) => option.name.length > 0 && option.label.length > 0)
});

const displayedAthlete = computed<Athlete | undefined>(() =>
  athletes.value.find((athlete) => athlete.id === displayedAthleteId.value)
);

const displayedAthleteFullName = computed(() =>
  `${displayedAthlete.value?.firstName ?? ""} ${displayedAthlete.value?.lastName ?? ""}`.trim()
);

const isSearchDisabled = computed(() => selectedAthleteId.value.length === 0);
const isNoteButtonDisabled = computed(() => newNoteContenu.value.trim().length === 0 || isSubmittingNote.value);

// Réinitialiser la sélection d'athlète lorsque l'équipe change
watch(selectedTeamId, (newTeamId) => {
  if (newTeamId) {
    // Vérifier si l'athlète actuellement sélectionné fait partie de la nouvelle équipe
    const currentAthlete = athletes.value.find(a => a.id === selectedAthleteId.value);
    if (currentAthlete && currentAthlete.teamId !== newTeamId) {
      // Réinitialiser la sélection si l'athlète n'est pas dans la nouvelle équipe
      selectedAthleteId.value = "";
      displayedAthleteId.value = "";
    }
  }
});

onMounted(async () => {
  teams.value = await loadAllTeams();
  athletes.value = await loadAllAthletes();
    const injured = await athleteService.getInjured();

    const injuredWithNotes: InjuredAthleteWithNote[] = await Promise.all(
        injured.map(async (athlete) => {
            const notes = await athleteService.getNotesBlessure(athlete.id!);

            const sortedNotes = [...notes].sort((a, b) => {
                const timeA = new Date(a.createdAt ?? "").getTime();
                const timeB = new Date(b.createdAt ?? "").getTime();
                return timeB - timeA; // plus récente en premier
            });

            return {
                ...athlete,
                lastInjuryNote: sortedNotes[0]?.contenu ?? ""
            };
        })
    );

    injuredAthletes.value = injuredWithNotes;
  overloadedAthletes.value = await athleteService.getOverloaded();
});

watch([displayedAthleteId, startDateFilter, endDateFilter], async ([newId, newStartDate, newEndDate]) => {
  injuryNotes.value = [];
  athleteEfforts.value = [];
  loadChartData.value = null;
  pleasureChartData.value = null;
  newNoteContenu.value = "";
  noteSubmitMessage.value = "";
  if (newId) {
    injuryNotes.value = await athleteService.getNotesBlessure(newId);
    const effortsResponse = await athleteService.getAthleteEfforts(newId, 1, 500, newStartDate, newEndDate);
    const sortedEfforts = [...(effortsResponse.items ?? [])].sort((a, b) => {
      const timeA = new Date(a.createdAt ?? "").getTime();
      const timeB = new Date(b.createdAt ?? "").getTime();
      const safeTimeA = Number.isNaN(timeA) ? 0 : timeA;
      const safeTimeB = Number.isNaN(timeB) ? 0 : timeB;
      return safeTimeB - safeTimeA;
    });
    athleteEfforts.value = sortedEfforts;

    if (sortedEfforts.length > 0) {
      const buckets = aggregateEffortsByWeek(sortedEfforts);
      if (buckets.length > 0) {
        loadChartData.value = buildWeeklyLoadChart(buckets, t);
        pleasureChartData.value = buildWeeklyPleasureChart(buckets, t);
      }
    }
  }
});

function handleSearch() {
  displayedAthleteId.value = selectedAthleteId.value;
}

function handleReset() {
  selectedTeamId.value = "";
  selectedAthleteId.value = "";
  displayedAthleteId.value = "";
}

async function handleAddNote() {
  if (!displayedAthleteId.value || newNoteContenu.value.trim().length === 0) return;
  isSubmittingNote.value = true;
  noteSubmitMessage.value = "";
  const result = await athleteService.createNoteBlessure(displayedAthleteId.value, newNoteContenu.value.trim());
  if (result.succeeded) {
    newNoteContenu.value = "";
    noteSubmitMessage.value = t("pages.admin.dashboard.athletePage.injuryNotesSubmitSuccess");
    injuryNotes.value = await athleteService.getNotesBlessure(displayedAthleteId.value);
  } else {
    noteSubmitMessage.value = t("pages.admin.dashboard.athletePage.injuryNotesSubmitError");
  }
  isSubmittingNote.value = false;
}

function displayValue(value?: string | boolean) {
  if (value === undefined || value === null || value === "") return t("global.undefined");
  if (typeof value === "boolean") return value ? t("global.yes") : t("global.no");
  return value;
}

function formatDateOnly(value?: string) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(date);
}

async function loadAllTeams(): Promise<Team[]> {
  const allTeams: Team[] = [];
  const pageSize = 100;
  let pageIndex = 1;
  let totalItems = 0;

  do {
    const response = await teamService.getAll(pageIndex, pageSize);
    const items = response?.items ?? [];
    totalItems = response?.totalItems ?? 0;
    if (items.length === 0) break;
    allTeams.push(...items);
    pageIndex += 1;
  } while (allTeams.length < totalItems);

  return allTeams;
}

async function loadAllAthletes(): Promise<Athlete[]> {
  const allAthletes: Athlete[] = [];
  const pageSize = 100;
  let pageIndex = 1;
  let totalItems = 0;

  do {
    const response = await athleteService.getAll(pageIndex, pageSize);
    const items = response?.items ?? [];
    totalItems = response?.totalItems ?? 0;
    if (items.length === 0) break;
    allAthletes.push(...items);
    pageIndex += 1;
  } while (allAthletes.length < totalItems);

  return allAthletes;
}
</script>

<style scoped>
.admin-dashboard__date-filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.admin-dashboard__filter-group {
  flex: 1;
  min-width: 200px;
}

.admin-dashboard__filter-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
}

.admin-dashboard__filter-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.admin-dashboard__chart-block {
  margin-bottom: 1rem;
}

.admin-dashboard__chart-title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  color: #2c3e50;
}

.admin-dashboard__chart-container {
  height: 300px;
  margin-bottom: 1.5rem;
  background: white;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.admin-dashboard__chart {
  height: 100%;
  width: 100%;
}

.admin-dashboard__table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}

.admin-dashboard__table th,
.admin-dashboard__table td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.admin-dashboard__table th {
  background-color: #f5f5f5;
  font-weight: 600;
}

.admin-dashboard__table tr:hover {
  background-color: #f9f9f9;
}

.admin-dashboard__overload-row {
  cursor: pointer;
}

.admin-dashboard__overload-row:hover {
  background-color: var(--color-green-lighter, #e8f5e9);
}

.admin-dashboard__note-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.admin-dashboard__table th,
.admin-dashboard__table td {
    width: 25%;
}
</style>
