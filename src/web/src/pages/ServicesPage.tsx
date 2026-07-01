import { useState } from 'react';
import type { ColDef } from 'ag-grid-community';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { accountServicesApi } from '../api/endpoints';
import type { AccountServiceRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useAuth } from '../auth/AuthContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useSelectedAccountName } from '../hooks/useSelectedAccountName';
import { EditableTable } from '../components/EditableTable';
import { EditModeBar } from '../components/EditModeBar';
import { ImportExportBar } from '../components/ImportExportBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import { dateColumn, referenceColumn, textColumn, yesNoColumn } from '../components/gridColumns';

/**
 * Services tab. Rows are the full service catalog (path + service shown read-only), and the
 * account fills in the data columns. The row set is fixed by the catalog, like Support Hours.
 */
export function ServicesPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { isAdmin } = useAuth();
  const { getItems } = useReferenceData();
  const accountName = useSelectedAccountName();
  const queryClient = useQueryClient();

  const queryKey = ['services', selectedAccountId];

  const { data: apiRows = [] } = useQuery({
    queryKey,
    queryFn: () => accountServicesApi.get(selectedAccountId!),
    enabled: selectedAccountId != null,
  });

  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<AccountServiceRow[]>([]);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const columns: ColDef<AccountServiceRow>[] = [
    { field: 'path', headerName: 'Path', editable: false, pinned: 'left', width: 300, wrapText: true, autoHeight: true, tooltipField: 'path' },
    { field: 'serviceName', headerName: 'Service', editable: false, pinned: 'left', width: 240, wrapText: true, autoHeight: true, tooltipField: 'serviceName' },
    dateColumn('contractEndDate', 'Contract end date (SOW)'),
    yesNoColumn('existsInCompany', 'Does this service exist in the company?'),
    yesNoColumn('providedByStefaniniGroup', 'Is it provided by Stefanini Group?'),
    referenceColumn('providedByRefId', 'If not provided by SG then by who (by client or by a competitor provider)?', getItems('Competitor')),
    yesNoColumn('opportunityToProvide', 'If service is not existent, is there an opportunity to provide it?'),
    textColumn('details', 'Details', { flex: 1, minWidth: 240 }),
  ];

  const rows = editing ? draft : apiRows;

  function startEditing() {
    setDraft(structuredClone(apiRows));
    setEditing(true);
  }

  async function saveChanges() {
    setSaving(true);
    try {
      await accountServicesApi.save(selectedAccountId!, draft);
      await queryClient.invalidateQueries({ queryKey });
      setEditing(false);
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <PageHeader
        title="Services"
        accountName={accountName}
        search={{ value: search, onChange: setSearch }}
        importExport={
          <ImportExportBar
            accountId={selectedAccountId}
            tabKey="services"
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
      <EditableTable<AccountServiceRow>
        columns={columns}
        rows={rows}
        editing={editing}
        getRowKey={(row) => String(row.serviceItemId)}
        quickFilter={search}
      />
    </>
  );
}
