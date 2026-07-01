import { useState } from 'react';
import type { ColDef } from 'ag-grid-community';
import { opportunityApi } from '../api/endpoints';
import type { OpportunityRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useSelectedAccountName } from '../hooks/useSelectedAccountName';
import { useTabEditor } from '../hooks/useTabEditor';
import { EditableTable } from '../components/EditableTable';
import { EditModeBar } from '../components/EditModeBar';
import { ImportExportBar } from '../components/ImportExportBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import { numberColumn, referenceColumn, textColumn } from '../components/gridColumns';
import { stableRowKey } from '../components/rowKey';

/** Opportunities tab. */
export function OpportunitiesPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { getItems } = useReferenceData();
  const accountName = useSelectedAccountName();
  const [search, setSearch] = useState('');

  const editor = useTabEditor<OpportunityRow>({
    queryKey: ['opportunities', selectedAccountId],
    enabled: selectedAccountId != null,
    load: () => opportunityApi.get(selectedAccountId!),
    save: (rows) => opportunityApi.save(selectedAccountId!, rows),
  });

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const columns: ColDef<OpportunityRow>[] = [
    textColumn('name', 'Opportunity Name'),
    referenceColumn('opportunityTypeRefId', 'Opportunity Type', getItems('OpportunityType')),
    referenceColumn('relatedServiceRefId', 'Related service', getItems('RelatedService')),
    referenceColumn('statusRefId', 'Status', getItems('OpportunityStatus')),
    numberColumn('estimatedMonthlyValue', 'Estimated monthly value ($)'),
    numberColumn('estimatedContractDurationMonths', 'Estimated contract duration (Months)'),
    referenceColumn('deliveredByRefId', 'Delivered By', getItems('ServiceTower')),
    textColumn('details', 'Details', { flex: 1, minWidth: 260 }),
  ];

  return (
    <>
      <PageHeader
        title="Opportunities"
        accountName={accountName}
        search={{ value: search, onChange: setSearch }}
        importExport={
          <ImportExportBar accountId={selectedAccountId} tabKey="opportunities" canImport={editor.isAdmin} onImported={editor.reload} />
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
      <EditableTable<OpportunityRow>
        columns={columns}
        rows={editor.rows}
        editing={editor.editing}
        getRowKey={stableRowKey}
        quickFilter={search}
        onChange={editor.setDraft}
        enableAddDelete
        makeEmptyRow={() => ({ id: 0 } as OpportunityRow)}
      />
    </>
  );
}
