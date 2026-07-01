import { useState } from 'react';
import type { ColDef } from 'ag-grid-community';
import { automationApi } from '../api/endpoints';
import type { AutomationRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useSelectedAccountName } from '../hooks/useSelectedAccountName';
import { useTabEditor } from '../hooks/useTabEditor';
import { EditableTable } from '../components/EditableTable';
import { EditModeBar } from '../components/EditModeBar';
import { ImportExportBar } from '../components/ImportExportBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import { dateColumn, numberColumn, referenceColumn, textColumn } from '../components/gridColumns';
import { stableRowKey } from '../components/rowKey';

/** Automations tab. */
export function AutomationsPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { getItems } = useReferenceData();
  const accountName = useSelectedAccountName();
  const [search, setSearch] = useState('');

  const editor = useTabEditor<AutomationRow>({
    queryKey: ['automations', selectedAccountId],
    enabled: selectedAccountId != null,
    load: () => automationApi.get(selectedAccountId!),
    save: (rows) => automationApi.save(selectedAccountId!, rows),
  });

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const serviceTowers = getItems('ServiceTower');

  const columns: ColDef<AutomationRow>[] = [
    referenceColumn('automationRefId', 'Automation Name', getItems('Automation'), { flex: 2, minWidth: 200 }),
    dateColumn('deploymentDate', 'Deployment Date'),
    numberColumn('costOfImplementationOneTime', 'Cost of implementation (one time - $)'),
    numberColumn('runningCostMonthly', 'Running cost (monthly - $)'),
    numberColumn('efficiencyImpactFtePerMonth', 'Efficiency impact (FTE/month)'),
    referenceColumn('deliveredByRefId', 'Delivered By', serviceTowers, { flex: 0, width: 120, minWidth: 110 }),
    // Details fills the remaining width but can shrink so the table avoids a horizontal scroll.
    textColumn('details', 'Details', { flex: 1, minWidth: 160 }),
  ];

  return (
    <>
      <PageHeader
        title="Automations"
        accountName={accountName}
        search={{ value: search, onChange: setSearch }}
        importExport={
          <ImportExportBar accountId={selectedAccountId} tabKey="automations" canImport={editor.isAdmin} onImported={editor.reload} />
        }
        editControls={
          <EditModeBar
            editing={editor.editing}
            canEdit={editor.isAdmin}
            saving={editor.saving}
            onEdit={editor.startEditing}
            onSave={editor.saveChanges}
            onCancel={editor.cancelEditing}
          />
        }
      />
      <EditableTable<AutomationRow>
        columns={columns}
        rows={editor.rows}
        editing={editor.editing}
        getRowKey={stableRowKey}
        quickFilter={search}
        onChange={editor.setDraft}
        enableAddDelete
        makeEmptyRow={() => ({ id: 0 } as AutomationRow)}
      />
    </>
  );
}
