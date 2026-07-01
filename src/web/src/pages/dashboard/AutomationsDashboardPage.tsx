import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { dashboardApi } from '../../api/endpoints';
import { PageHeader } from '../../components/PageHeader';
import {
  applyFilters,
  automationsByProjectCount,
  computeKpis,
  countByDimension,
  distinctValues,
  emptyFilters,
  stackByProject,
  type DashboardFilters,
} from './automationMetrics';
import './AutomationsDashboardPage.css';

/** Colour palette for the chart series (Stefanini blues + supporting accents). */
const PALETTE = [
  '#0067c5',
  '#00a3e0',
  '#111d41',
  '#5bc2e7',
  '#c77700',
  '#1c8c4a',
  '#8e44ad',
  '#c0392b',
  '#6b7280',
  '#2f6f9f',
  '#e0a800',
  '#16a085',
];

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
const number = new Intl.NumberFormat('en-US', { maximumFractionDigits: 1 });

/** The four top-of-page dropdown filters and the dimension each one drives. */
const DROPDOWN_FILTERS: { key: keyof DashboardFilters; label: string }[] = [
  { key: 'region', label: 'Regional Ownership' },
  { key: 'account', label: 'Project' },
  { key: 'deliveredBy', label: 'Service Type' },
  { key: 'automation', label: 'Automation' },
];

export function AutomationsDashboardPage() {
  const { data: facts, isLoading, isError } = useQuery({
    queryKey: ['dashboard', 'automations'],
    queryFn: dashboardApi.automationFacts,
  });

  const [filters, setFilters] = useState<DashboardFilters>(emptyFilters);

  // "Now" is captured once per render-cycle so the cost-since-deployment figures are stable.
  const now = useMemo(() => new Date(), []);

  const allFacts = useMemo(() => facts ?? [], [facts]);

  // Dropdown options come from the FULL dataset so the choices stay stable as filters change.
  const dropdownOptions = useMemo(() => {
    const options: Partial<Record<keyof DashboardFilters, string[]>> = {};
    for (const { key } of DROPDOWN_FILTERS) options[key] = distinctValues(allFacts, key);
    return options;
  }, [allFacts]);

  // Everything below the filter bar is computed from the filtered set.
  const filtered = useMemo(() => applyFilters(allFacts, filters), [allFacts, filters]);
  const kpis = useMemo(() => computeKpis(filtered, now), [filtered, now]);
  const byCategory = useMemo(() => countByDimension(filtered, 'category'), [filtered]);
  const byGoal = useMemo(() => countByDimension(filtered, 'goal'), [filtered]);
  const byAiUsed = useMemo(() => countByDimension(filtered, 'aiUsed'), [filtered]);
  const categoryPerProject = useMemo(() => stackByProject(filtered, 'category'), [filtered]);
  const goalPerProject = useMemo(() => stackByProject(filtered, 'goal'), [filtered]);
  const automationTable = useMemo(() => automationsByProjectCount(filtered), [filtered]);

  const activeFilters = (Object.keys(filters) as (keyof DashboardFilters)[]).filter((key) => filters[key] !== null);

  /** Sets a filter from a dropdown ('' clears it). */
  function setDropdown(key: keyof DashboardFilters, value: string) {
    setFilters((current) => ({ ...current, [key]: value === '' ? null : value }));
  }

  /** Click-to-cross-filter: selecting the same value again clears it (Power BI style). */
  function toggleFilter(key: keyof DashboardFilters, value: string) {
    setFilters((current) => ({ ...current, [key]: current[key] === value ? null : value }));
  }

  function clearFilter(key: keyof DashboardFilters) {
    setFilters((current) => ({ ...current, [key]: null }));
  }

  if (isLoading) return <div className="dashboard-message">Loading automations…</div>;
  if (isError) return <div className="dashboard-message">Could not load the automations data.</div>;

  return (
    <div className="dashboard">
      <PageHeader
        title="Automations"
        description="Automations delivered across all accounts. Click any chart segment or table row to cross-filter every view."
      />

      {/* ---- Filter bar: four dropdowns + reset ---- */}
      <div className="dashboard-filters">
        {DROPDOWN_FILTERS.map(({ key, label }) => (
          <label key={key} className="dashboard-filter">
            <span>{label}</span>
            <select value={filters[key] ?? ''} onChange={(event) => setDropdown(key, event.target.value)}>
              <option value="">All</option>
              {(dropdownOptions[key] ?? []).map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>
        ))}
        <button
          type="button"
          className="dashboard-reset"
          onClick={() => setFilters(emptyFilters)}
          disabled={activeFilters.length === 0}
        >
          Reset filters
        </button>
      </div>

      {/* ---- Active filter chips (also covers chart-driven filters) ---- */}
      {activeFilters.length > 0 && (
        <div className="dashboard-chips">
          {activeFilters.map((key) => (
            <button key={key} type="button" className="dashboard-chip" onClick={() => clearFilter(key)}>
              {filters[key]}
              <span aria-hidden>×</span>
            </button>
          ))}
        </div>
      )}

      {/* ---- KPI cards ---- */}
      <div className="dashboard-kpis">
        <KpiCard label="Automations" value={number.format(kpis.automationCount)} />
        <KpiCard label="Projects" value={number.format(kpis.projectCount)} />
        <KpiCard label="Cost Total" value={currency.format(kpis.costTotal)} />
        <KpiCard label="Estimated Saving / mo" value={currency.format(kpis.estimatedSaving)} />
        <KpiCard label="FTE Impact / mo" value={number.format(kpis.fteImpact)} />
        <KpiCard label="Projects with Sophie" value={number.format(kpis.projectsWithSophie)} />
      </div>

      {/* ---- Charts ---- */}
      <div className="dashboard-grid">
        <ChartCard title="Automations by Category">
          <PieChartView slices={byCategory} onSlice={(name) => toggleFilter('category', name)} />
        </ChartCard>

        <ChartCard title="Automations by Goal">
          <PieChartView slices={byGoal} onSlice={(name) => toggleFilter('goal', name)} />
        </ChartCard>

        <ChartCard title="AI Used">
          <PieChartView slices={byAiUsed} onSlice={(name) => toggleFilter('aiUsed', name)} />
        </ChartCard>

        <ChartCard title="Categories per Project" wide>
          <StackedBarView data={categoryPerProject} onSegment={(name) => toggleFilter('category', name)} />
        </ChartCard>

        <ChartCard title="Goals per Project" wide>
          <StackedBarView data={goalPerProject} onSegment={(name) => toggleFilter('goal', name)} />
        </ChartCard>

        <ChartCard title="Automations by Number of Projects" wide>
          <div className="dashboard-table-scroll">
            <table className="dashboard-table">
              <thead>
                <tr>
                  <th>Automation</th>
                  <th className="numeric"># of Projects</th>
                  <th className="numeric">Total Deployed</th>
                </tr>
              </thead>
              <tbody>
                {automationTable.map((row) => (
                  <tr
                    key={row.name}
                    className={filters.automation === row.name ? 'selected' : ''}
                    onClick={() => toggleFilter('automation', row.name)}
                  >
                    <td>{row.name}</td>
                    <td className="numeric">{row.projects}</td>
                    <td className="numeric">{row.occurrences}</td>
                  </tr>
                ))}
                {automationTable.length === 0 && (
                  <tr>
                    <td colSpan={3} className="empty">
                      No automations match the current filters.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </ChartCard>
      </div>
    </div>
  );
}

function KpiCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="kpi-card">
      <div className="kpi-value">{value}</div>
      <div className="kpi-label">{label}</div>
    </div>
  );
}

function ChartCard({ title, wide, children }: { title: string; wide?: boolean; children: React.ReactNode }) {
  return (
    <section className={`chart-card${wide ? ' chart-card-wide' : ''}`}>
      <h3 className="chart-title">{title}</h3>
      <div className="chart-body">{children}</div>
    </section>
  );
}

/** Reads a string field from a Recharts click payload, looking inside `payload` if needed. */
function readName(arg: unknown): string | undefined {
  if (arg && typeof arg === 'object') {
    const object = arg as Record<string, unknown>;
    if (typeof object.name === 'string') return object.name;
    const payload = object.payload as Record<string, unknown> | undefined;
    if (payload && typeof payload.name === 'string') return payload.name;
  }
  return undefined;
}

/** A clickable pie chart. Clicking a slice cross-filters on that value. */
function PieChartView({
  slices,
  onSlice,
}: {
  slices: { name: string; value: number }[];
  onSlice: (name: string) => void;
}) {
  if (slices.length === 0) return <div className="chart-empty">No data</div>;
  return (
    <ResponsiveContainer width="100%" height="100%">
      <PieChart>
        <Pie
          data={slices}
          dataKey="value"
          nameKey="name"
          innerRadius="45%"
          outerRadius="80%"
          paddingAngle={1}
          onClick={(slice) => {
            const name = readName(slice);
            if (name) onSlice(name);
          }}
        >
          {slices.map((slice, index) => (
            <Cell key={slice.name} fill={PALETTE[index % PALETTE.length]} cursor="pointer" />
          ))}
        </Pie>
        <Tooltip />
        <Legend />
      </PieChart>
    </ResponsiveContainer>
  );
}

/** Horizontal stacked bars: one bar per project, split by a second dimension. */
function StackedBarView({
  data,
  onSegment,
}: {
  data: { rows: Record<string, string | number>[]; keys: string[] };
  onSegment: (name: string) => void;
}) {
  if (data.rows.length === 0) return <div className="chart-empty">No data</div>;
  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={data.rows} layout="vertical" margin={{ left: 16, right: 16, top: 4, bottom: 4 }}>
        <CartesianGrid horizontal={false} strokeDasharray="3 3" />
        <XAxis type="number" allowDecimals={false} />
        <YAxis type="category" dataKey="project" width={120} tick={{ fontSize: 12 }} />
        <Tooltip />
        <Legend />
        {data.keys.map((key, index) => (
          <Bar
            key={key}
            dataKey={key}
            stackId="stack"
            fill={PALETTE[index % PALETTE.length]}
            cursor="pointer"
            onClick={() => onSegment(key)}
          />
        ))}
      </BarChart>
    </ResponsiveContainer>
  );
}
