import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { accountApi } from '../api/endpoints';
import { useAuth } from '../auth/AuthContext';
import { useSelectedAccount } from '../app/AccountContext';
import { AddAccountButton } from '../components/AddAccountButton';
import { PageHeader } from '../components/PageHeader';
import './HomePage.css';

/** Formats an ISO timestamp as a readable local date/time, or a dash when missing. */
function formatDate(value?: string | null): string {
  if (!value) {
    return '—';
  }
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString();
}

/** Start page: the list of accounts with their last-updated date. Clicking one opens it in Input. */
export function HomePage() {
  const { isAdmin } = useAuth();
  const { setSelectedAccountId } = useSelectedAccount();
  const navigate = useNavigate();

  const { data: accounts = [], isLoading } = useQuery({ queryKey: ['accounts'], queryFn: accountApi.getAll });

  function openAccount(accountId: number) {
    setSelectedAccountId(accountId);
    navigate('/input/overall');
  }

  return (
    <>
      <PageHeader
        title="Accounts"
        editControls={isAdmin ? <AddAccountButton onCreated={openAccount} /> : undefined}
      />

      {isLoading ? (
        <p>Loading…</p>
      ) : accounts.length === 0 ? (
        <div className="no-account-banner">
          No accounts yet.{isAdmin ? ' Use "Add account" to create the first one.' : ''}
        </div>
      ) : (
        <table className="accounts-table">
          <thead>
            <tr>
              <th>Account</th>
              <th>Last updated</th>
            </tr>
          </thead>
          <tbody>
            {accounts.map((account) => (
              <tr key={account.id} onClick={() => openAccount(account.id)} title="Open in Input">
                <td>{account.name}</td>
                <td>{formatDate(account.lastUpdatedUtc)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  );
}
