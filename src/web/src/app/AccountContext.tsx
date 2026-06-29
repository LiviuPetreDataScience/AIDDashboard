import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';

const SELECTED_ACCOUNT_KEY = 'aid.selectedAccountId';

interface AccountContextValue {
  selectedAccountId: number | null;
  setSelectedAccountId: (accountId: number | null) => void;
}

const AccountContext = createContext<AccountContextValue | undefined>(undefined);

/** Tracks which account is currently open, persisted across reloads. */
export function AccountProvider({ children }: { children: ReactNode }) {
  const [selectedAccountId, setSelectedAccountIdState] = useState<number | null>(() => {
    const stored = localStorage.getItem(SELECTED_ACCOUNT_KEY);
    return stored ? Number(stored) : null;
  });

  const setSelectedAccountId = useCallback((accountId: number | null) => {
    setSelectedAccountIdState(accountId);
  }, []);

  useEffect(() => {
    if (selectedAccountId === null) {
      localStorage.removeItem(SELECTED_ACCOUNT_KEY);
    } else {
      localStorage.setItem(SELECTED_ACCOUNT_KEY, String(selectedAccountId));
    }
  }, [selectedAccountId]);

  return (
    <AccountContext.Provider value={{ selectedAccountId, setSelectedAccountId }}>
      {children}
    </AccountContext.Provider>
  );
}

export function useSelectedAccount(): AccountContextValue {
  const context = useContext(AccountContext);
  if (!context) {
    throw new Error('useSelectedAccount must be used within an AccountProvider.');
  }
  return context;
}
