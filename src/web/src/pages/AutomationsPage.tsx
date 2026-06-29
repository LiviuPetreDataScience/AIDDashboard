import type { ColDef } from 'ag-grid-community';
import { automationApi } from '../api/endpoints';
import type { AutomationRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useReferenceData } from '../hooks/useReferenceData';
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
    textColumn('name', 'Automation Name'),
    dateColumn('deploymentDate', 'Deployment Date'),
    numberColumn('costOfImplementationOneTime', 'Cost of implementation (one time - $)'),
    numberColumn('runningCostMonthly', 'Running cost (monthly - $)'),
    numberColumn('efficiencyImpactFtePerMonth', 'Efficiency impact (FTE/month)'),
    referenceColumn('deliveredByRefId', 'Delivered By', serviceTowers),
    textColumn('details', 'Details'),
  ];

  return (
    <>
      <PageHeader
        title="Automations"
        actions={
          <>
            <ImportExportBar accountId={selectedAccountId} tabKey="automations" canImport={editor.isAdmin} onImported={editor.reload} />
            <EditModeBar
              editing={editor.editing}
              canEdit={editor.isAdmin}
              saving={editor.saving}
              onEdit={editor.startEditing}
              onSave={editor.saveChanges}
              onCancel={editor.cancelEditing}
            />
          </>
        }
      />
      <EditableTable<AutomationRow>
        columns={columns}
        rows={editor.rows}
        editing={editor.editing}
        getRowKey={stableRowKey}
        onChange={editor.setDraft}
        enableAddDelete
        makeEmptyRow={() => ({ id: 0 } as AutomationRow)}
        exportFileName="automations"
      />
    </>
  );
}
