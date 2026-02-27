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
        <button type="button" class="btn">
          {{ t("pages.admin.dashboard.filters.search") }}
        </button>
        <button type="button" class="btn admin-dashboard__btn-reset">
          {{ t("pages.admin.dashboard.filters.reset") }}
        </button>
      </div>
    </Card>

    <div class="admin-dashboard__tiles">
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
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue3-i18n";
import { useAthleteService, useTeamService } from "@/inversify.config";
import Card from "@/components/layouts/items/Card.vue";
import { FormOption } from "@/types/formOption";
import { Athlete, Team } from "@/types/entities";

const { t } = useI18n();

const teamService = useTeamService();
const athleteService = useAthleteService();

const selectedTeamId = ref<string>("");
const selectedAthleteId = ref<string>("");

const teams = ref<Team[]>([]);
const athletes = ref<Athlete[]>([]);

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

onMounted(async () => {
  teams.value = await loadAllTeams();
  athletes.value = await loadAllAthletes();
});

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
