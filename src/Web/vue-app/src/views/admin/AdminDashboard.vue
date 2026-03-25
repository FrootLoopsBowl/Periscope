<template>
  <div class="content-grid">
    <div class="content-grid__header">
      <div class="admin-dashboard__hero">
        <h1 class="back-link-title">{{ t("routes.admin.children.dashboard.name") }}</h1>
      </div>
    </div>

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

      <div class="admin-dashboard__actions">
        <button type="button" class="btn" :disabled="isSearchDisabled" @click="handleSearch">
          {{ t("pages.admin.dashboard.filters.search") }}
        </button>
        <button type="button" class="btn admin-dashboard__btn-reset" @click="handleReset">
          {{ t("pages.admin.dashboard.filters.reset") }}
        </button>
      </div>
    </Card>

    <div v-if="displayedAthlete" class="admin-dashboard__athlete-page">
      <Card :title="t('pages.admin.dashboard.athletePage.infosTitle')">
        <div class="admin-dashboard__athlete-info">
          <div class="admin-dashboard__athlete-row">
            <span class="admin-dashboard__athlete-label">{{ t("global.fullName") }}</span>
            <span class="admin-dashboard__athlete-value">
              {{ displayValue(displayedAthleteFullName) }}
            </span>
          </div>
          <div class="admin-dashboard__athlete-row">
            <span class="admin-dashboard__athlete-label">{{ t("global.email") }}</span>
            <span class="admin-dashboard__athlete-value">
              {{ displayValue(displayedAthlete.email) }}
            </span>
          </div>
          <div class="admin-dashboard__athlete-row">
            <span class="admin-dashboard__athlete-label">{{ t("global.dateOfBirth") }}</span>
            <span class="admin-dashboard__athlete-value">
              {{ displayValue(formatDateOnly(displayedAthlete.dateOfBirth)) }}
            </span>
          </div>
          <div class="admin-dashboard__athlete-row">
            <span class="admin-dashboard__athlete-label">{{ t("pages.admin.dashboard.athletePage.createdAtLabel") }}</span>
            <span class="admin-dashboard__athlete-value">
              {{ displayValue(formatDateOnly(displayedAthlete.createdAt)) }}
            </span>
          </div>
        </div>
      </Card>

      <Card :title="t('pages.admin.dashboard.athletePage.injuryNotesTitle')">
        <div class="admin-dashboard__injury-notes">
          <div class="admin-dashboard__injury-form">
            <textarea
              class="admin-dashboard__injury-textarea"
              v-model="newNoteContenu"
              :placeholder="t('pages.admin.dashboard.athletePage.injuryNotesPlaceholder')"
              rows="3"
            ></textarea>
            <button
              type="button"
              class="btn"
              :disabled="isNoteButtonDisabled"
              @click="handleAddNote"
            >
              {{ t("pages.admin.dashboard.athletePage.injuryNotesSubmit") }}
            </button>
            <p v-if="noteSubmitMessage" class="admin-dashboard__note-message">{{ noteSubmitMessage }}</p>
          </div>
          <ul v-if="injuryNotes.length > 0" class="admin-dashboard__notes-list">
            <li v-for="note in injuryNotes" :key="note.id" class="admin-dashboard__note-item">
              <span class="admin-dashboard__note-date">{{ formatDateOnly(note.createdAt) }}</span>
              <span class="admin-dashboard__note-contenu">{{ note.contenu }}</span>
            </li>
          </ul>
          <p v-else class="content-grid__text">
            {{ t("pages.admin.dashboard.athletePage.injuryNotesEmpty") }}
          </p>
        </div>
      </Card>

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
        
        <!-- Chart -->
        <div v-if="effortChartData" class="admin-dashboard__chart-container">
          <LineChart 
            :chart-data="effortChartData" 
            :options="chartOptions"
            class="admin-dashboard__chart"
          />
        </div>
        
        <!-- Table -->
        <div v-if="athleteEfforts.length > 0" class="admin-dashboard__efforts-table">
          <table class="admin-dashboard__table">
            <thead>
              <tr>
                <th>{{ t('pages.admin.dashboard.athletePage.efforts.date') }}</th>
                <th>{{ t('pages.admin.dashboard.athletePage.efforts.effort') }}</th>
                <th>{{ t('pages.admin.dashboard.athletePage.efforts.pleasure') }}</th>
                <th>{{ t('pages.admin.dashboard.athletePage.efforts.duration') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="effort in athleteEfforts" :key="effort.id">
                <td>{{ formatDateOnly(effort.createdAt) }}</td>
                <td>{{ effort.effort }}</td>
                <td>
                  <span v-if="effort.pleasure !== undefined && effort.pleasure !== null">
                    {{ effort.pleasure }}
                  </span>
                  <span v-else class="text-grey-dark italic">
                    {{ t('pages.admin.dashboard.athletePage.efforts.noPleasureData') }}
                  </span>
                </td>
                <td>{{ effort.durationMinutes }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-else class="content-grid__text">
          {{ t("pages.admin.dashboard.athletePage.efforts.empty") }}
        </p>
      </Card>
    </div>

    <div v-else class="admin-dashboard__tiles">
      <Card :title="t('pages.admin.dashboard.injuredAthletesTitle')">
        <ul v-if="injuredAthletes.length > 0" class="admin-dashboard__notes-list">
          <li v-for="athlete in injuredAthletes" :key="athlete.id" class="admin-dashboard__note-item">
            <RouterLink
              class="admin-dashboard__note-contenu"
              :to="{ name: 'admin.children.athletes.detail', params: { id: athlete.id } }"
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
        <p v-else class="content-grid__text">
          {{ t("pages.admin.dashboard.injuredAthletesEmpty") }}
        </p>
      </Card>

      <Card :title="t('pages.admin.dashboard.weeklyGraphsTitle')">
        <p class="content-grid__text">
          {{ t("pages.admin.dashboard.weeklyGraphsPlaceholder") }}
        </p>
      </Card>

      <Card :title="t('pages.admin.dashboard.prioritizedAthletesTitle')">
        <p class="content-grid__text">
          {{ t("pages.admin.dashboard.prioritizedAthletesPlaceholder") }}
        </p>
      </Card>

      <Card :title="t('pages.admin.dashboard.watchlistTitle')">
        <p class="content-grid__text">
          {{ t("pages.admin.dashboard.watchlistPlaceholder") }}
        </p>
      </Card>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue3-i18n";
import { useAthleteService, useTeamService } from "@/inversify.config";
import Card from "@/components/layouts/items/Card.vue";
import LineChart from "@/components/charts/LineChart.vue";
import { FormOption } from "@/types/formOption";
import { Athlete, AthleteEffort, NoteBlessure, Team } from "@/types/entities";
import IconBandage from 'vue-material-design-icons/Bandage.vue';

const { t } = useI18n();

const teamService = useTeamService();
const athleteService = useAthleteService();

const selectedTeamId = ref<string>("");
const selectedAthleteId = ref<string>("");
const displayedAthleteId = ref<string>("");

const teams = ref<Team[]>([]);
const athletes = ref<Athlete[]>([]);
const injuredAthletes = ref<Athlete[]>([]);
const injuryNotes = ref<NoteBlessure[]>([]);
const athleteEfforts = ref<AthleteEffort[]>([]);
const effortChartData = ref<any>(null);
const today = new Date();
const tomorrow = new Date(today);
tomorrow.setDate(today.getDate() + 1);
const fiveWeeksAgo = new Date(today);
fiveWeeksAgo.setDate(today.getDate() - 35);

const toDateInputValue = (date: Date) => date.toISOString().split("T")[0];

const startDateFilter = ref<string>(toDateInputValue(fiveWeeksAgo));
const endDateFilter = ref<string>(toDateInputValue(tomorrow));
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
  injuredAthletes.value = await athleteService.getInjured();
});

watch([displayedAthleteId, startDateFilter, endDateFilter], async ([newId, newStartDate, newEndDate]) => {
  injuryNotes.value = [];
  athleteEfforts.value = [];
  effortChartData.value = null;
  newNoteContenu.value = "";
  noteSubmitMessage.value = "";
  if (newId) {
    injuryNotes.value = await athleteService.getNotesBlessure(newId);
    const effortsResponse = await athleteService.getAthleteEfforts(newId, 1, 10, newStartDate, newEndDate);
    athleteEfforts.value = effortsResponse.items;
    
    // Debug: Log the efforts data to check if pleasure field is present
    console.log('Efforts loaded:', effortsResponse.items);
    
    // Generate chart data
    if (effortsResponse.items && effortsResponse.items.length > 0) {
      effortChartData.value = {
        labels: effortsResponse.items.map(e => formatDateOnly(e.createdAt)),
        datasets: [
          {
            label: t('pages.admin.dashboard.athletePage.efforts.effort'),
            data: effortsResponse.items.map(e => e.effort),
            borderColor: '#42b983',
            backgroundColor: 'rgba(66, 185, 131, 0.1)',
            tension: 0.1,
            yAxisID: 'y'
          },
          {
            label: t('pages.admin.dashboard.athletePage.efforts.pleasure'),
            data: effortsResponse.items.map(e => e.pleasure !== undefined && e.pleasure !== null ? e.pleasure : 0),
            borderColor: '#4dabf7',
            backgroundColor: 'rgba(77, 171, 247, 0.1)',
            tension: 0.1,
            yAxisID: 'y'
          },
          {
            label: t('pages.admin.dashboard.athletePage.efforts.duration'),
            data: effortsResponse.items.map(e => e.durationMinutes),
            borderColor: '#ff6b6b',
            backgroundColor: 'rgba(255, 107, 107, 0.1)',
            tension: 0.1,
            yAxisID: 'y1'
          }
        ]
      };
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

async function handleMarkAsRecovered(athleteId: string) {
  const result = await athleteService.toggleInjured(athleteId, false);
  if (result.succeeded) {
    injuredAthletes.value = injuredAthletes.value.filter((a) => a.id !== athleteId);
  }
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

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        beginAtZero: true,
        max: 10,
        title: {
          display: true,
          text: t('pages.admin.dashboard.athletePage.efforts.effortPleasure')
        }
      },
      y1: {
        beginAtZero: true,
        position: 'right',
        title: {
          display: true,
          text: t('pages.admin.dashboard.athletePage.efforts.duration')
        },
        grid: {
          drawOnChartArea: false
        }
      },
      x: {
        title: {
          display: true,
          text: t('pages.admin.dashboard.athletePage.efforts.date')
        }
      }
    }
  };
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
</style>
