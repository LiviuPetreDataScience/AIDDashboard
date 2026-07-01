import { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { dashboardApi, referenceApi } from '../../api/endpoints';
import type { AutomationProfile, ReferenceItem } from '../../api/types';

/**
 * Admin editor for the shared attributes of each automation (Category, Goal, Environment and
 * whether it uses AI). These attributes live on the automation itself — not per account — and
 * feed the Automations dashboard's groupings. One row per automation in the reference list.
 */
export function AutomationProfilesEditor() {
  const queryClient = useQueryClient();
  const [filter, setFilter] = useState('');

  const profilesQuery = useQuery({
    queryKey: ['automation-profiles'],
    queryFn: dashboardApi.automationProfiles,
  });
  // includeInactive=true and the same query keys as the list sections, so editing those lists
  // refreshes these dropdowns automatically (shared React Query cache entries).
  const categories = useQuery({
    queryKey: ['admin-reference', 'AutomationCategory'],
    queryFn: () => referenceApi.getByType('AutomationCategory', true),
  });
  const goals = useQuery({
    queryKey: ['admin-reference', 'AutomationGoal'],
    queryFn: () => referenceApi.getByType('AutomationGoal', true),
  });
  const environments = useQuery({
    queryKey: ['admin-reference', 'Environment'],
    queryFn: () => referenceApi.getByType('Environment', true),
  });

  const profiles = profilesQuery.data ?? [];
  const visibleProfiles = useMemo(
    () => profiles.filter((profile) => profile.automationName.toLowerCase().includes(filter.toLowerCase())),
    [profiles, filter],
  );

  async function handleSave(
    profile: AutomationProfile,
    attributes: {
      categoryRefId: number | null;
      goalRefId: number | null;
      environmentRefId: number | null;
      aiUsed: boolean | null;
    },
  ) {
    await dashboardApi.saveAutomationProfile(profile.automationRefId, attributes);
    // Refresh this editor and the dashboard so the new attributes show everywhere.
    queryClient.invalidateQueries({ queryKey: ['automation-profiles'] });
    queryClient.invalidateQueries({ queryKey: ['dashboard', 'automations'] });
  }

  if (profilesQuery.isLoading) {
    return <p>Loading automation attributes…</p>;
  }
  if (profilesQuery.isError) {
    return <p className="admin-error">Could not load the automation attributes.</p>;
  }

  return (
    <>
      <div className="admin-toolbar">
        <input
          type="search"
          placeholder="Filter automations…"
          value={filter}
          onChange={(event) => setFilter(event.target.value)}
          style={{ maxWidth: 260 }}
        />
      </div>

      <table className="admin-table">
        <thead>
          <tr>
            <th style={{ width: '28%' }}>Automation</th>
            <th>Category</th>
            <th>Goal</th>
            <th>Environment affected</th>
            <th>AI used</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {visibleProfiles.map((profile) => (
            <ProfileRow
              key={profile.automationRefId}
              profile={profile}
              categories={categories.data ?? []}
              goals={goals.data ?? []}
              environments={environments.data ?? []}
              onSave={handleSave}
            />
          ))}
        </tbody>
      </table>
    </>
  );
}

/** Tri-state AI value <-> the <select> string value. */
const AI_OPTIONS: { value: string; label: string; flag: boolean | null }[] = [
  { value: '', label: '(blank)', flag: null },
  { value: 'yes', label: 'Yes', flag: true },
  { value: 'no', label: 'No', flag: false },
];

function aiToSelectValue(flag: boolean | null | undefined): string {
  return AI_OPTIONS.find((option) => option.flag === (flag ?? null))?.value ?? '';
}

/** One editable automation row with its own local draft state. */
function ProfileRow({
  profile,
  categories,
  goals,
  environments,
  onSave,
}: {
  profile: AutomationProfile;
  categories: ReferenceItem[];
  goals: ReferenceItem[];
  environments: ReferenceItem[];
  onSave: (
    profile: AutomationProfile,
    attributes: {
      categoryRefId: number | null;
      goalRefId: number | null;
      environmentRefId: number | null;
      aiUsed: boolean | null;
    },
  ) => void;
}) {
  const [categoryRefId, setCategoryRefId] = useState<number | null>(profile.categoryRefId ?? null);
  const [goalRefId, setGoalRefId] = useState<number | null>(profile.goalRefId ?? null);
  const [environmentRefId, setEnvironmentRefId] = useState<number | null>(profile.environmentRefId ?? null);
  const [aiUsed, setAiUsed] = useState<boolean | null>(profile.aiUsed ?? null);
  const [saving, setSaving] = useState(false);

  const isDirty =
    categoryRefId !== (profile.categoryRefId ?? null) ||
    goalRefId !== (profile.goalRefId ?? null) ||
    environmentRefId !== (profile.environmentRefId ?? null) ||
    aiUsed !== (profile.aiUsed ?? null);

  async function save() {
    setSaving(true);
    try {
      await onSave(profile, { categoryRefId, goalRefId, environmentRefId, aiUsed });
    } finally {
      setSaving(false);
    }
  }

  return (
    <tr>
      <td>{profile.automationName}</td>
      <td>
        <RefSelect items={categories} value={categoryRefId} onChange={setCategoryRefId} />
      </td>
      <td>
        <RefSelect items={goals} value={goalRefId} onChange={setGoalRefId} />
      </td>
      <td>
        <RefSelect items={environments} value={environmentRefId} onChange={setEnvironmentRefId} />
      </td>
      <td>
        <select
          value={aiToSelectValue(aiUsed)}
          onChange={(event) => setAiUsed(AI_OPTIONS.find((option) => option.value === event.target.value)?.flag ?? null)}
        >
          {AI_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </td>
      <td className="admin-actions">
        <button type="button" className="btn-primary" disabled={!isDirty || saving} onClick={save}>
          {saving ? 'Saving…' : 'Save'}
        </button>
      </td>
    </tr>
  );
}

/** A reference-item dropdown with a "(blank)" option that maps to null. */
function RefSelect({
  items,
  value,
  onChange,
}: {
  items: ReferenceItem[];
  value: number | null;
  onChange: (value: number | null) => void;
}) {
  return (
    <select
      value={value ?? ''}
      onChange={(event) => onChange(event.target.value === '' ? null : Number(event.target.value))}
    >
      <option value="">(blank)</option>
      {items.map((item) => (
        <option key={item.id} value={item.id}>
          {item.name}
        </option>
      ))}
    </select>
  );
}
