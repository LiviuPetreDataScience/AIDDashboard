import { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import type { ColDef } from 'ag-grid-community';
import { supportHourApi } from '../api/endpoints';
import type { SupportCoverage, SupportHourRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useAuth } from '../auth/AuthContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useSelectedAccountName } from '../hooks/useSelectedAccountName';
import { EditableTable } from '../components/EditableTable';
import { EditModeBar } from '../components/EditModeBar';
import { ImportExportBar } from '../components/ImportExportBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import { yesNoColumn } from '../components/gridColumns';

/** Friendly labels for the three mutually-exclusive coverage options. */
const COVERAGE_LABELS: Record<SupportCoverage, string> = {
  None: '—',
  EightByFive: '8x5',
  TwentyFourFive: '24/5',
  TwentyFourSeven: '24/7',
};

/** Grid row: one support-hours language with its hours, coverage and flags. */
type GridRow = Record<string, unknown> & {
  languageRefId: number;
  _label: string;
  fromMondayFriday: string | null;
  toMondayFriday: string | null;
  coverage: SupportCoverage;
  onCallInterpretDhs: boolean;
  sophie: boolean;
};

/** Support Hours tab. Rows are fixed by the Support Hours Languages reference list. */
export function SupportHoursPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { isAdmin } = useAuth();
  const { getItems } = useReferenceData();
  const accountName = useSelectedAccountName();
  const queryClient = useQueryClient();

  const queryKey = ['support-hours', selectedAccountId];

  const { data: apiRows = [] } = useQuery({
    queryKey,
    queryFn: () => supportHourApi.get(selectedAccountId!),
    enabled: selectedAccountId != null,
  });

  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<GridRow[]>([]);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');

  const languages = getItems('SupportHoursLanguage');

  const computedRows = useMemo<GridRow[]>(() => {
    const byLanguage = new Map<number, SupportHourRow>();
    for (const apiRow of apiRows) {
      byLanguage.set(apiRow.languageRefId, apiRow);
    }

    return languages.map((language) => {
      const existing = byLanguage.get(language.id);
      return {
        languageRefId: language.id,
        _label: language.name,
        fromMondayFriday: existing?.fromMondayFriday ?? null,
        toMondayFriday: existing?.toMondayFriday ?? null,
        coverage: existing?.coverage ?? 'None',
        onCallInterpretDhs: existing?.onCallInterpretDhs ?? false,
        sophie: existing?.sophie ?? false,
      };
    });
  }, [apiRows, languages]);

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  // Flex widths so the table fills the page like the others (Language gets the largest share).
  const columns: ColDef<GridRow>[] = [
    { field: '_label', headerName: 'Language', editable: false, flex: 3, minWidth: 200 },
    { field: 'fromMondayFriday', headerName: 'From (M-F)', flex: 1, minWidth: 110 },
    { field: 'toMondayFriday', headerName: 'To (M-F)', flex: 1, minWidth: 110 },
    {
      field: 'coverage',
      headerName: 'Coverage (8x5 / 24x5 / 24x7)',
      headerTooltip: 'Coverage (8x5 / 24x5 / 24x7)',
      wrapHeaderText: true,
      autoHeaderHeight: true,
      flex: 2,
      minWidth: 160,
      cellEditor: 'agSelectCellEditor',
      cellEditorParams: { values: ['None', 'EightByFive', 'TwentyFourFive', 'TwentyFourSeven'] },
      valueFormatter: (params) => COVERAGE_LABELS[params.value as SupportCoverage] ?? '',
    },
    yesNoColumn('onCallInterpretDhs', 'On call / Interpret (DHS)', { flex: 2, minWidth: 150 }),
    yesNoColumn('sophie', 'Sophie', { flex: 1, minWidth: 110 }),
  ];

  const rows = editing ? draft : computedRows;

  function startEditing() {
    setDraft(structuredClone(computedRows));
    setEditing(true);
  }

  async function saveChanges() {
    setSaving(true);
    try {
      // Persist only rows that carry information; the rest stay implicitly "None".
      const payload: SupportHourRow[] = draft
        .filter(
          (row) =>
            row.coverage !== 'None' ||
            row.onCallInterpretDhs ||
            row.sophie ||
            row.fromMondayFriday ||
            row.toMondayFriday,
        )
        .map((row) => ({
          languageRefId: row.languageRefId,
          fromMondayFriday: row.fromMondayFriday || null,
          toMondayFriday: row.toMondayFriday || null,
          coverage: row.coverage,
          onCallInterpretDhs: Boolean(row.onCallInterpretDhs),
          sophie: Boolean(row.sophie),
        }));

      await supportHourApi.save(selectedAccountId!, payload);
      await queryClient.invalidateQueries({ queryKey });
      setEditing(false);
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <PageHeader
        title="Support Hours"
        accountName={accountName}
        search={{ value: search, onChange: setSearch }}
        importExport={
          <ImportExportBar
            accountId={selectedAccountId}
            tabKey="support-hours"
            canImport={isAdmin}
            onImported={() => queryClient.invalidateQueries({ queryKey })}
          />
        }
        editControls={
          <EditModeBar
            editing={editing}
            canEdit={isAdmin}
            saving={saving}
            onEdit={startEditing}
            onSave={saveChanges}
            onCancel={() => setEditing(false)}
          />
        }
      />
      <EditableTable<GridRow>
        columns={columns}
        rows={rows}
        editing={editing}
        getRowKey={(row) => String(row.languageRefId)}
        quickFilter={search}
      />
    </>
  );
}
