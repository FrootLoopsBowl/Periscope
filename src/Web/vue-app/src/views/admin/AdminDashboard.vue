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
        <p class="content-grid__text">
          {{ t("pages.admin.dashboard.athletePage.weeklyPlaceholder") }}
        </p>
      </Card>
    </div>

    <div v-else class="admin-dashboard__tiles">
      <Card :title="t('pages.admin.dashboard.injuredAthletesTitle')">
        <ul v-if="injuredAthletes.length > 0" class="admin-dashboard__notes-list">
          <li v-for="athlete in injuredAthletes" :key="athlete.id" class="admin-dashboard__note-item">
            <span class="admin-dashboard__note-contenu">{{ athlete.firstName }} {{ athlete.lastName }}</span>
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
import { FormOption } from "@/types/formOption";
import { Athlete, NoteBlessure, Team } from "@/types/entities";

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
const newNoteContenu = ref<string>("");
const isSubmittingNote = ref<boolean>(false);
const noteSubmitMessage = ref<string>("");

const teamOptions = computed<FormOption[]>(() =>
  teams.value.map((team) => ({
    name: team.id ?? "",
    label: team.name ?? "",
  })).filter((option) => option.name.length > 0 && option.label.length > 0)
);

const athleteOptions = computed<FormOption[]>(() =>
  athletes.value.map((athlete) => ({
    name: athlete.id ?? "",
    label: `${athlete.firstName ?? ""} ${athlete.lastName ?? ""}`.trim(),
  })).filter((option) => option.name.length > 0 && option.label.length > 0)
);

const displayedAthlete = computed<Athlete | undefined>(() =>
  athletes.value.find((athlete) => athlete.id === displayedAthleteId.value)
);

const displayedAthleteFullName = computed(() =>
  `${displayedAthlete.value?.firstName ?? ""} ${displayedAthlete.value?.lastName ?? ""}`.trim()
);

const isSearchDisabled = computed(() => selectedAthleteId.value.length === 0);
const isNoteButtonDisabled = computed(() => newNoteContenu.value.trim().length === 0 || isSubmittingNote.value);

onMounted(async () => {
  teams.value = await loadAllTeams();
  athletes.value = await loadAllAthletes();
  injuredAthletes.value = await athleteService.getInjured();
});

watch(displayedAthleteId, async (newId) => {
  injuryNotes.value = [];
  newNoteContenu.value = "";
  noteSubmitMessage.value = "";
  if (newId) {
    injuryNotes.value = await athleteService.getNotesBlessure(newId);
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
