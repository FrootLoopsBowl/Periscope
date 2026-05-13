import type { AthleteEffort } from "@/types/entities";

export type TranslateFn = (key: string) => string;

export interface WeeklyBucket {
  weekStartMs: number;
  label: string;
  trainingLoad: number;
  avgPleasure: number | null;
}

function mondayStart(d: Date): Date {
  const local = new Date(d);
  local.setHours(0, 0, 0, 0);
  const mondayOffset = (local.getDay() + 6) % 7;
  local.setDate(local.getDate() - mondayOffset);
  return local;
}

/** Regroupe les séances par semaine (lundi → dimanche), semaines les plus récentes en dernier. */
export function aggregateEffortsByWeek(efforts: AthleteEffort[], locale = "fr-CA"): WeeklyBucket[] {
  const groups = new Map<string, AthleteEffort[]>();

  for (const e of efforts) {
    const dt = new Date(e.createdAt ?? "");
    if (Number.isNaN(dt.getTime())) continue;
    const ws = mondayStart(dt);
    const key = `${ws.getFullYear()}-${ws.getMonth()}-${ws.getDate()}`;
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key)!.push(e);
  }

  const formatter = new Intl.DateTimeFormat(locale, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });

  const buckets: WeeklyBucket[] = [];

  for (const arr of groups.values()) {
    const first = arr[0];
    const weekStart = mondayStart(new Date(first.createdAt ?? ""));
    const trainingLoad = arr.reduce((s, x) => s + (x.effort ?? 0) * (x.durationMinutes ?? 0), 0);
    const pleasures = arr.map((x) => x.pleasure).filter((p): p is number => typeof p === "number");
    const avgPleasure = pleasures.length ? pleasures.reduce((a, b) => a + b, 0) / pleasures.length : null;

    buckets.push({
      weekStartMs: weekStart.getTime(),
      label: formatter.format(weekStart),
      trainingLoad,
      avgPleasure,
    });
  }

  buckets.sort((a, b) => a.weekStartMs - b.weekStartMs);
  return buckets;
}

const CHARGE_MAX_MULTIPLIER = 1.1;
const PRIOR_WEEKS_FOR_MAX = 5;

/** Moyenne des `PRIOR_WEEKS_FOR_MAX` semaines précédentes × `CHARGE_MAX_MULTIPLIER` par semaine (null si moins de 5 semaines d'historique). */
export function chargeMaximalePerWeek(buckets: WeeklyBucket[]): (number | null)[] {
  return buckets.map((_, i) => {
    if (i < PRIOR_WEEKS_FOR_MAX) return null;
    const slice = buckets.slice(i - PRIOR_WEEKS_FOR_MAX, i);
    const avg = slice.reduce((s, b) => s + b.trainingLoad, 0) / PRIOR_WEEKS_FOR_MAX;
    return avg * CHARGE_MAX_MULTIPLIER;
  });
}

export function buildWeeklyLoadChart(buckets: WeeklyBucket[], t: TranslateFn) {
  const maxSeries = chargeMaximalePerWeek(buckets);
  return {
    labels: buckets.map((b) => b.label),
    datasets: [
      {
        type: "line" as const,
        order: 2,
        label: t("pages.admin.dashboard.athletePage.efforts.chartTrainingLoadSeries"),
        data: buckets.map((b) => b.trainingLoad),
        borderColor: "#42b983",
        backgroundColor: "rgba(66, 185, 131, 0.1)",
        tension: 0.1,
        yAxisID: "y",
      },
      {
        type: "bar" as const,
        order: 1,
        label: t("pages.admin.dashboard.athletePage.efforts.chartMaxLoadSeries"),
        data: maxSeries,
        backgroundColor: "rgba(220, 38, 38, 0.55)",
        borderColor: "rgb(185, 28, 28)",
        borderWidth: 1,
        yAxisID: "y",
      },
    ],
  };
}

export function buildWeeklyPleasureChart(buckets: WeeklyBucket[], t: TranslateFn) {
  return {
    labels: buckets.map((b) => b.label),
    datasets: [
      {
        label: t("pages.admin.dashboard.athletePage.efforts.pleasure"),
        data: buckets.map((b) => (b.avgPleasure === null ? null : b.avgPleasure)),
        borderColor: "#4dabf7",
        backgroundColor: "rgba(77, 171, 247, 0.1)",
        tension: 0.1,
        spanGaps: false,
      },
    ],
  };
}

export function weeklyLoadChartOptions(t: TranslateFn) {
  return {
    responsive: true,
    maintainAspectRatio: false,
    interaction: { mode: "index" as const, intersect: false },
    datasets: {
      bar: {
        categoryPercentage: 0.55,
        barPercentage: 0.85,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: t("pages.admin.dashboard.athletePage.efforts.chartAxisTrainingLoad"),
        },
      },
      x: {
        title: {
          display: true,
          text: t("pages.admin.dashboard.athletePage.efforts.chartAxisWeek"),
        },
      },
    },
  };
}

export function weeklyPleasureChartOptions(t: TranslateFn) {
  return {
    responsive: true,
    maintainAspectRatio: false,
    interaction: { mode: "index" as const, intersect: false },
    scales: {
      y: {
        beginAtZero: true,
        suggestedMin: 0,
        suggestedMax: 10,
        title: {
          display: true,
          text: t("pages.admin.dashboard.athletePage.efforts.chartAxisPleasure"),
        },
      },
      x: {
        title: {
          display: true,
          text: t("pages.admin.dashboard.athletePage.efforts.chartAxisWeek"),
        },
      },
    },
  };
}
