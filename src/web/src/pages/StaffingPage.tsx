import { useQuery, useQueryClient } from '@tanstack/react-query';
import { staffingApi } from '../api/endpoints';
import type { StaffingModelType } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useAuth } from '../auth/AuthContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { useSelectedAccountName } from '../hooks/useSelectedAccountName';
import { MatrixTab, type MatrixCell } from '../components/MatrixTab';
import { SelectAccountNotice } from '../components/SelectAccountNotice';

/** Initial Staffing Model and Latest Approved Model tabs (Location x Role matrix). */
export function StaffingPage({ modelType, title }: { modelType: StaffingModelType; title: string }) {
  const { selectedAccountId } = useSelectedAccount();
  const { isAdmin } = useAuth();
  const { getItems } = useReferenceData();
  const accountName = useSelectedAccountName();
  const queryClient = useQueryClient();

  const queryKey = ['staffing', modelType, selectedAccountId];

  const { data: cells = [] } = useQuery({
    queryKey,
    queryFn: () => staffingApi.get(selectedAccountId!, modelType),
    enabled: selectedAccountId != null,
  });

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const matrixCells: MatrixCell[] = cells.map((cell) => ({
    rowRefId: cell.locationRefId,
    colRefId: cell.roleRefId,
    value: cell.value,
  }));

  async function handleSave(updatedCells: MatrixCell[]) {
    await staffingApi.save(
      selectedAccountId!,
      modelType,
      updatedCells.map((cell) => ({
        locationRefId: cell.rowRefId,
        roleRefId: cell.colRefId,
        value: cell.value,
      })),
    );
    await queryClient.invalidateQueries({ queryKey });
  }

  return (
    <MatrixTab
      title={title}
      accountName={accountName}
      rowHeader="Location"
      rowItems={getItems('Location')}
      colItems={getItems('StaffingRole')}
      cells={matrixCells}
      isAdmin={isAdmin}
      accountId={selectedAccountId}
      tabKey={modelType === 'Initial' ? 'initial-staffing' : 'latest-approved'}
      onImported={() => queryClient.invalidateQueries({ queryKey })}
      onSave={handleSave}
    />
  );
}
