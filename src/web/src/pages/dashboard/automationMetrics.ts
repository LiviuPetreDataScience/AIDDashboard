/**
 * Pure helper functions for the Automations dashboard: the cost/saving formulas,
 * the cross-filtering logic, and the aggregations that feed the charts.
 *
 * Keeping these free of React/Recharts makes the dashboard component small and the
 * business rules easy to read in one place.
 */
import type { AutomationFact } from '../../api/types';

/** Estimated monthly saving per 1 FTE of efficiency impact (from the requirements: $1500 * efficiency impact). */
export const SAVING_PER_FTE = 1500;

/** The four top-of-page filters plus the dimensions that chart clicks drive. */
export interface DashboardFilters {
  region: string | null;
  account: string | null;
  deliveredBy: string | null;
  automation: string | null;
  category: string | null;
  goal: string | null;
  environment: string | null;
  aiUsed: string | null; // 'Yes' | 'No' | 'Unknown'
}

export const emptyFilters: DashboardFilters = {
  region: null,
  account: null,
  deliveredBy: null,
  automation: null,
  category: null,
  goal: null,
  environment: null,
  aiUsed: null,
};

/** A blank value shown in the charts/filters when a fact has no value for a dimension. */
export const BLANK_LABEL = '(blank)';

/** Normalises the AI-used flag to the label used everywhere in the dashboard. */
export function aiUsedLabel(fact: AutomationFact): string {
  if (fact.aiUsed === true) return 'Yes';
  if (fact.aiUsed === false) return 'No';
  return 'Unknown';
}

/** Reads the value of a single filter dimension from a fact, as a display string. */
export function dimensionValue(fact: AutomationFact, dimension: keyof DashboardFilters): string {
  switch (dimension) {
    case 'region':
      return fact.regionalOwnership || BLANK_LABEL;
    case 'account':
      return fact.accountName || BLANK_LABEL;
    case 'deliveredBy':
      return fact.deliveredBy || BLANK_LABEL;
    case 'automation':
      return fact.automationName || BLANK_LABEL;
    case 'category':
      return fact.category || BLANK_LABEL;
    case 'goal':
      return fact.goal || BLANK_LABEL;
    case 'environment':
      return fact.environment || BLANK_LABEL;
    case 'aiUsed':
      return aiUsedLabel(fact);
    default:
      return BLANK_LABEL;
  }
}

/** Keeps only the facts that match every active filter (a null filter matches everything). */
export function applyFilters(facts: AutomationFact[], filters: DashboardFilters): AutomationFact[] {
  const active = (Object.keys(filters) as (keyof DashboardFilters)[]).filter((key) => filters[key] !== null);
  if (active.length === 0) return facts;
  return facts.filter((fact) => active.every((key) => dimensionValue(fact, key) === filters[key]));
}

/** Whole months between a deployment date (yyyy-MM-dd) and "now", clamped to >= 0. */
export function monthsSinceDeployment(deploymentDate: string | null | undefined, now: Date): number {
  if (!deploymentDate) return 0;
  const deployed = new Date(deploymentDate);
  if (Number.isNaN(deployed.getTime())) return 0;
  const months = (now.getFullYear() - deployed.getFullYear()) * 12 + (now.getMonth() - deployed.getMonth());
  return Math.max(0, months);
}

/** Cost Total = Cost of Implementation + Months Since Deployment * Running Cost. */
export function costTotal(fact: AutomationFact, now: Date): number {
  const implementation = fact.costOfImplementation ?? 0;
  const running = fact.runningCost ?? 0;
  return implementation + monthsSinceDeployment(fact.deploymentDate, now) * running;
}

/** Estimated Saving = $1500 * Efficiency impact (per month). */
export function estimatedSaving(fact: AutomationFact): number {
  return SAVING_PER_FTE * (fact.efficiencyImpact ?? 0);
}

/** Headline numbers shown in the KPI cards. */
export interface DashboardKpis {
  automationCount: number;
  projectCount: number;
  costTotal: number;
  estimatedSaving: number;
  fteImpact: number;
  projectsWithSophie: number;
}

export function computeKpis(facts: AutomationFact[], now: Date): DashboardKpis {
  const projects = new Set(facts.map((f) => f.accountName));
  const sophieProjects = new Set(facts.filter((f) => f.category === 'Sophie').map((f) => f.accountName));
  return {
    automationCount: facts.length,
    projectCount: projects.size,
    costTotal: facts.reduce((sum, f) => sum + costTotal(f, now), 0),
    estimatedSaving: facts.reduce((sum, f) => sum + estimatedSaving(f), 0),
    fteImpact: facts.reduce((sum, f) => sum + (f.efficiencyImpact ?? 0), 0),
    projectsWithSophie: sophieProjects.size,
  };
}

export interface CountSlice {
  name: string;
  value: number;
}

/** Counts facts per value of a dimension, biggest first (for the pie charts). */
export function countByDimension(facts: AutomationFact[], dimension: keyof DashboardFilters): CountSlice[] {
  const counts = new Map<string, number>();
  for (const fact of facts) {
    const value = dimensionValue(fact, dimension);
    counts.set(value, (counts.get(value) ?? 0) + 1);
  }
  return [...counts.entries()].map(([name, value]) => ({ name, value })).sort((a, b) => b.value - a.value);
}

export interface AutomationProjectCount {
  name: string;
  projects: number;
  occurrences: number;
}

/**
 * For the "automations by number of projects" table: how many distinct accounts each
 * automation appears in (this is why some names show more than one project).
 */
export function automationsByProjectCount(facts: AutomationFact[]): AutomationProjectCount[] {
  const byName = new Map<string, { accounts: Set<string>; occurrences: number }>();
  for (const fact of facts) {
    const name = fact.automationName || BLANK_LABEL;
    const entry = byName.get(name) ?? { accounts: new Set<string>(), occurrences: 0 };
    entry.accounts.add(fact.accountName);
    entry.occurrences += 1;
    byName.set(name, entry);
  }
  return [...byName.entries()]
    .map(([name, entry]) => ({ name, projects: entry.accounts.size, occurrences: entry.occurrences }))
    .sort((a, b) => b.projects - a.projects || b.occurrences - a.occurrences || a.name.localeCompare(b.name));
}

export interface StackedBarData {
  /** One row per project, with a count under each dimension key. */
  rows: Record<string, string | number>[];
  /** The distinct dimension values, used as the stacked series. */
  keys: string[];
}

/**
 * Builds a stacked-bar dataset: one bar per project (account), split into segments by a
 * second dimension (category / goal / environment). Used for the "… per project" charts.
 */
export function stackByProject(facts: AutomationFact[], dimension: keyof DashboardFilters): StackedBarData {
  const projects = new Map<string, Record<string, number>>();
  const keySet = new Set<string>();
  for (const fact of facts) {
    const project = fact.accountName || BLANK_LABEL;
    const key = dimensionValue(fact, dimension);
    keySet.add(key);
    const row = projects.get(project) ?? {};
    row[key] = (row[key] ?? 0) + 1;
    projects.set(project, row);
  }
  const keys = [...keySet].sort();
  const rows = [...projects.entries()].map(([project, counts]) => {
    const row: Record<string, string | number> = { project };
    for (const key of keys) row[key] = counts[key] ?? 0;
    return row;
  });
  return { rows, keys };
}

/** Distinct, sorted values of a dimension across ALL facts (stable options for the dropdowns). */
export function distinctValues(facts: AutomationFact[], dimension: keyof DashboardFilters): string[] {
  const values = new Set<string>();
  for (const fact of facts) values.add(dimensionValue(fact, dimension));
  return [...values].sort((a, b) => a.localeCompare(b));
}
