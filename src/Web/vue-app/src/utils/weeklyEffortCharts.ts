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

function addDays(d: Date, n: number): Date {
  const x = new Date(d.getTime());
  x.setDate(x.getDate() + n);
  return x;
}

/** Interprète `YYYY-MM-DD` en date locale (aligné sur les champs date du filtre). */
function parseYyyyMmDdLocal(s: string): Date | null {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s.trim());
  if (!m) return null;
  const y = Number(m[1]);
  const mo = Number(m[2]) - 1;
  const d = Number(m[3]);
  const dt = new Date(y, mo, d);
  if (dt.getFullYear() !== y || dt.getMonth() !== mo || dt.getDate() !== d) return null;
  return dt;
}

export type WeekRangeInput = { start: string; end: string };

/** Regroupe les séances par semaine (lundi → dimanche), semaines les plus récentes en dernier.
 *  Si `weekRange` est fourni, inclut chaque semaine calendaire de la plage (charge 0 si aucune séance). */
export function aggregateEffortsByWeek(
  efforts: AthleteEffort[],
  locale = "fr-CA",
  weekRange?: WeekRangeInput
): WeeklyBucket[] {
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

  const buildBucket = (weekStart: Date, arr: AthleteEffort[]): WeeklyBucket => {
    const trainingLoad = arr.reduce((s, x) => s + (x.effort ?? 0) * (x.durationMinutes ?? 0), 0);
    const pleasures = arr.map((x) => x.pleasure).filter((p): p is number => typeof p === "number");
    const avgPleasure = pleasures.length ? pleasures.reduce((a, b) => a + b, 0) / pleasures.length : null;
    return {
      weekStartMs: weekStart.getTime(),
      label: formatter.format(weekStart),
      trainingLoad,
      avgPleasure,
    };
  };

  if (weekRange) {
    const startD = parseYyyyMmDdLocal(weekRange.start);
    const endD = parseYyyyMmDdLocal(weekRange.end);
    if (startD && endD && startD.getTime() <= endD.getTime()) {
      const startM = mondayStart(startD);
      const endM = mondayStart(endD);
      const buckets: WeeklyBucket[] = [];
      for (let ws = new Date(startM.getTime()); ws.getTime() <= endM.getTime(); ws = addDays(ws, 7)) {
        const key = `${ws.getFullYear()}-${ws.getMonth()}-${ws.getDate()}`;
        const arr = groups.get(key) ?? [];
        buckets.push(buildBucket(ws, arr));
      }
      return buckets;
    }
  }

  const buckets: WeeklyBucket[] = [];
  for (const arr of groups.values()) {
    const first = arr[0];
    const weekStart = mondayStart(new Date(first.createdAt ?? ""));
    buckets.push(buildBucket(weekStart, arr));
  }

  buckets.sort((a, b) => a.weekStartMs - b.weekStartMs);
  return buckets;
}

const CHARGE_MAX_MULTIPLIER = 1.1;
const PRIOR_WEEKS_FOR_MAX = 5;

/** Moyenne des jusqu'à `PRIOR_WEEKS_FOR_MAX` semaines précédentes (toutes si moins de 5) × `CHARGE_MAX_MULTIPLIER` ; null pour la première semaine. */
export function chargeMaximalePerWeek(buckets: WeeklyBucket[]): (number | null)[] {
  return buckets.map((_, i) => {
    const n = Math.min(i, PRIOR_WEEKS_FOR_MAX);
    if (n === 0) return null;
    const slice = buckets.slice(i - n, i);
    const avg = slice.reduce((s, b) => s + b.trainingLoad, 0) / n;
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
        order: 0,
        label: t("pages.admin.dashboard.athletePage.efforts.chartTrainingLoadSeries"),
        data: buckets.map((b) => b.trainingLoad),
        borderColor: "#42b983",
        backgroundColor: "rgba(66, 185, 131, 0.08)",
        tension: 0.1,
        yAxisID: "y",
      },
      {
        type: "line" as const,
        order: 1,
        label: t("pages.admin.dashboard.athletePage.efforts.chartMaxLoadSeries"),
        data: maxSeries,
        borderColor: "rgb(220, 38, 38)",
        backgroundColor: "transparent",
        borderWidth: 2,
        borderDash: [6, 4],
        tension: 0.1,
        fill: false,
        spanGaps: true,
        pointBackgroundColor: "rgb(220, 38, 38)",
        pointBorderColor: "#fff",
        pointBorderWidth: 1,
        pointRadius: 3,
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
