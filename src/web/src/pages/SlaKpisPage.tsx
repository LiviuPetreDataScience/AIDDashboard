import type { ColDef } from 'ag-grid-community';
import { slaKpiApi } from '../api/endpoints';
import type { SlaKpiRow } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useTabEditor } from '../hooks/useTabEditor';
import { EditableTable } from '../components/EditableTable';
import { EditModeBar } from '../components/EditModeBar';
import { ImportExportBar } from '../components/ImportExportBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import { referenceColumn, textColumn, yesNoColumn } from '../components/gridColumns';
import { stableRowKey } from '../components/rowKey';

/** SLAs & KPIs tab. */
export function SlaKpisPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { getItems } = useReferenceData();

  const editor = useTabEditor<SlaKpiRow>({
    queryKey: ['sla-kpis', selectedAccountId],
    enabled: selectedAccountId != null,
    load: () => slaKpiApi.get(selectedAccountId!),
    save: (rows) => slaKpiApi.save(selectedAccountId!, rows),
  });

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const columns: ColDef<SlaKpiRow>[] = [
    textColumn('name', 'Name'),
    referenceColumn('typeRefId', 'Type', getItems('SlaType')),
    textColumn('description', 'Description'),
    textColumn('formula', 'Formula'),
    textColumn('targetPercent', 'Target %'),
    referenceColumn('measurementTypeRefId', 'Measurement type', getItems('MeasurementType')),
    yesNoColumn('canBeReported', 'Can it be reported?'),
    yesNoColumn('financialPenalties', 'Financial penalties'),
    yesNoColumn('bonus', 'Bonus'),
  ];

  return (
    <>
      <PageHeader
        title="SLAs & KPIs"
        actions={
          <>
            <ImportExportBar accountId={selectedAccountId} tabKey="sla-kpis" canImport={editor.isAdmin} onImported={editor.reload} />
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
      <EditableTable<SlaKpiRow>
        columns={columns}
        rows={editor.rows}
        editing={editor.editing}
        getRowKey={stableRowKey}
        onChange={editor.setDraft}
        enableAddDelete
        makeEmptyRow={() => ({ id: 0, canBeReported: false, financialPenalties: false, bonus: false } as SlaKpiRow)}
        exportFileName="sla-kpis"
      />
    </>
  );
}
