import { useQuery } from '@tanstack/react-query';
import { accountApi } from '../api/endpoints';
import { useSelectedAccount } from '../app/AccountContext';

/** Returns the name of the currently selected account (from the cached accounts list), if any. */
export function useSelectedAccountName(): string | undefined {
  const { selectedAccountId } = useSelectedAccount();
  const { data: accounts } = useQuery({ queryKey: ['accounts'], queryFn: accountApi.getAll });
  return accounts?.find((account) => account.id === selectedAccountId)?.name;
}
