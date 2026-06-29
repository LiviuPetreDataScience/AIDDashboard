import { useQuery, useQueryClient } from '@tanstack/react-query';
import { contractualLanguageApi } from '../api/endpoints';
import { useSelectedAccount } from '../app/AccountContext';
import { useAuth } from '../auth/AuthContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { MatrixTab, type MatrixCell } from '../components/MatrixTab';
import { SelectAccountNotice } from '../components/SelectAccountNotice';

/** Contractual Languages tab (Location x Language matrix). */
export function ContractualLanguagesPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { isAdmin } = useAuth();
  const { getItems } = useReferenceData();
  const queryClient = useQueryClient();

  const queryKey = ['contractual-languages', selectedAccountId];

  const { data: cells = [] } = useQuery({
    queryKey,
    queryFn: () => contractualLanguageApi.get(selectedAccountId!),
    enabled: selectedAccountId != null,
  });

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const matrixCells: MatrixCell[] = cells.map((cell) => ({
    rowRefId: cell.locationRefId,
    colRefId: cell.languageRefId,
    value: cell.value,
  }));

  async function handleSave(updatedCells: MatrixCell[]) {
    await contractualLanguageApi.save(
      selectedAccountId!,
      updatedCells.map((cell) => ({
        locationRefId: cell.rowRefId,
        languageRefId: cell.colRefId,
        value: cell.value,
      })),
    );
    await queryClient.invalidateQueries({ queryKey });
  }

  return (
    <MatrixTab
      title="Contractual Languages"
      rowHeader="Location"
      rowItems={getItems('Location')}
      colItems={getItems('ContractualLanguage')}
      cells={matrixCells}
      isAdmin={isAdmin}
      exportFileName="contractual-languages"
      accountId={selectedAccountId}
      tabKey="contractual-languages"
      onImported={() => queryClient.invalidateQueries({ queryKey })}
      onSave={handleSave}
    />
  );
}
