import { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import type { ColDef } from 'ag-grid-community';
import { countryDeviceApi } from '../api/endpoints';
import type { CountryRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useAuth } from '../auth/AuthContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useSelectedAccountName } from '../hooks/useSelectedAccountName';
import { EditableTable } from '../components/EditableTable';
import { EditModeBar } from '../components/EditModeBar';
import { ImportExportBar } from '../components/ImportExportBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import { numberColumn, referenceColumn } from '../components/gridColumns';
import { stableRowKey } from '../components/rowKey';

/** Grid row: a chosen country plus the two fixed counts and one numeric field per device. */
type GridRow = Record<string, unknown> & {
  countryRefId: number | null;
  noOfUsers: number | null;
  noOfLtsStaff: number | null;
};

/** Countries / Users / Devices Supported tab. Countries are added per account. */
export function CountriesDevicesPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { isAdmin } = useAuth();
  const { getItems } = useReferenceData();
  const accountName = useSelectedAccountName();
  const queryClient = useQueryClient();

  const queryKey = ['countries-devices', selectedAccountId];

  const { data: apiRows = [] } = useQuery({
    queryKey,
    queryFn: () => countryDeviceApi.get(selectedAccountId!),
    enabled: selectedAccountId != null,
  });

  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<GridRow[]>([]);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');

  const devices = getItems('Device');
  const countries = getItems('Country');

  const computedRows = useMemo<GridRow[]>(
    () =>
      apiRows.map((apiRow) => {
        const row: GridRow = {
          countryRefId: apiRow.countryRefId,
          noOfUsers: apiRow.noOfUsers ?? null,
          noOfLtsStaff: apiRow.noOfLtsStaff ?? null,
        };
        for (const device of apiRow.devices) {
          row[String(device.deviceRefId)] = device.value;
        }
        return row;
      }),
    [apiRows],
  );

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const columns: ColDef<GridRow>[] = [
    referenceColumn('countryRefId', 'Country', countries, { pinned: 'left', minWidth: 200 }),
    numberColumn('noOfUsers', 'No of Users'),
    numberColumn('noOfLtsStaff', 'No of LTS Staff'),
    ...devices.map((device) => numberColumn(String(device.id), device.name)),
  ];

  const rows = editing ? draft : computedRows;

  function startEditing() {
    setDraft(structuredClone(computedRows));
    setEditing(true);
  }

  async function saveChanges() {
    setSaving(true);
    try {
      const payload: CountryRow[] = draft
        .filter((row) => row.countryRefId != null)
        .map((row) => ({
          countryRefId: row.countryRefId as number,
          noOfUsers: typeof row.noOfUsers === 'number' ? row.noOfUsers : null,
          noOfLtsStaff: typeof row.noOfLtsStaff === 'number' ? row.noOfLtsStaff : null,
          devices: devices
            .filter((device) => typeof row[String(device.id)] === 'number')
            .map((device) => ({ deviceRefId: device.id, value: row[String(device.id)] as number })),
        }));

      await countryDeviceApi.save(selectedAccountId!, payload);
      await queryClient.invalidateQueries({ queryKey });
      setEditing(false);
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <PageHeader
        title="Countries / Users / Devices"
        accountName={accountName}
        search={{ value: search, onChange: setSearch }}
        importExport={
          <ImportExportBar
            accountId={selectedAccountId}
            tabKey="countries-devices"
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
        getRowKey={stableRowKey}
        quickFilter={search}
        onChange={setDraft}
        enableAddDelete
        makeEmptyRow={() => ({ countryRefId: null, noOfUsers: null, noOfLtsStaff: null })}
      />
    </>
  );
}
