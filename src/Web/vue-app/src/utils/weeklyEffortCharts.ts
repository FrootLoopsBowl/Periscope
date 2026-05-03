import type { AthleteEffort } from "@/types/entities";

export type TranslateFn = (key: string) => string;

export interface WeeklyBucket {
  weekStartMs: number;
  label: string;
  avgEffort: number;
  sumDurationMinutes: number;
  avgPleasure: number | null;
}

function mondayStart(d: Date): Date {
  const local = new Date(d);
  local.setHours(0, 0, 0, 0);
  const mondayOffset = (local.getDay() + 6) % 7;
  local.setDate(local.getDate() - mondayOffset);
  return local;
}

/** Regroupe les séances par semaine (lundi → dimanche), semaines les plus récentes en premier. */
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
    const sumEffort = arr.reduce((s, x) => s + (x.effort ?? 0), 0);
    const avgEffort = arr.length ? sumEffort / arr.length : 0;
    const sumDurationMinutes = arr.reduce((s, x) => s + (x.durationMinutes ?? 0), 0);
    const pleasures = arr.map((x) => x.pleasure).filter((p): p is number => typeof p === "number");
    const avgPleasure = pleasures.length ? pleasures.reduce((a, b) => a + b, 0) / pleasures.length : null;

    buckets.push({
      weekStartMs: weekStart.getTime(),
      label: formatter.format(weekStart),
      avgEffort,
      sumDurationMinutes,
      avgPleasure,
    });
  }

  buckets.sort((a, b) => b.weekStartMs - a.weekStartMs);
  return buckets;
}

export function buildWeeklyLoadChart(buckets: WeeklyBucket[], t: TranslateFn) {
  return {
    labels: buckets.map((b) => b.label),
    datasets: [
      {
        label: t("pages.admin.dashboard.athletePage.efforts.chartEffortSeries"),
        data: buckets.map((b) => b.avgEffort),
        borderColor: "#42b983",
        backgroundColor: "rgba(66, 185, 131, 0.1)",
        tension: 0.1,
        yAxisID: "y",
      },
      {
        label: t("pages.admin.dashboard.athletePage.efforts.chartDurationSeries"),
        data: buckets.map((b) => b.sumDurationMinutes),
        borderColor: "#e67700",
        backgroundColor: "rgba(230, 119, 0, 0.08)",
        tension: 0.1,
        yAxisID: "y1",
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
    scales: {
      y: {
        beginAtZero: true,
        position: "left" as const,
        title: {
          display: true,
          text: t("pages.admin.dashboard.athletePage.efforts.chartAxisEffort"),
        },
      },
      y1: {
        beginAtZero: true,
        position: "right" as const,
        grid: { drawOnChartArea: false },
        title: {
          display: true,
          text: t("pages.admin.dashboard.athletePage.efforts.chartAxisDuration"),
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
