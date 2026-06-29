import { Suspense } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { accountApi } from '../api/endpoints';
import { useAuth } from '../auth/AuthContext';
import { useSelectedAccount } from '../app/AccountContext';
import { adminTabs, mainTabs } from '../app/navigation';
import './Layout.css';

/** Application shell: brand bar with account picker and user menu, tab navigation, and content. */
export function Layout() {
  const { user, isAdmin, logout } = useAuth();
  const { selectedAccountId, setSelectedAccountId } = useSelectedAccount();
  const queryClient = useQueryClient();

  const { data: accounts = [] } = useQuery({
    queryKey: ['accounts'],
    queryFn: accountApi.getAll,
  });

  async function handleAddAccount() {
    const name = window.prompt('New account name:')?.trim();
    if (!name) {
      return;
    }
    const created = await accountApi.create(name);
    await queryClient.invalidateQueries({ queryKey: ['accounts'] });
    setSelectedAccountId(created.id);
  }

  return (
    <div className="layout">
      <header className="layout-header">
        <div className="layout-brand">
          AID <span>Dashboard</span>
        </div>

        <div className="layout-account">
          <label htmlFor="account-picker">Account:</label>
          <select
            id="account-picker"
            value={selectedAccountId ?? ''}
            onChange={(event) =>
              setSelectedAccountId(event.target.value ? Number(event.target.value) : null)
            }
          >
            <option value="">— Select an account —</option>
            {accounts.map((account) => (
              <option key={account.id} value={account.id}>
                {account.name}
              </option>
            ))}
          </select>
          {isAdmin && (
            <button type="button" className="btn-secondary" onClick={handleAddAccount}>
              + Add account
            </button>
          )}
        </div>

        <div className="layout-spacer" />

        <div className="layout-user">
          <span>{user?.displayName ?? user?.username}</span>
          <span className="role-badge">{user?.role}</span>
          <button type="button" className="btn-secondary" onClick={logout}>
            Log out
          </button>
        </div>
      </header>

      <nav className="layout-nav">
        {mainTabs.map((tab) => (
          <NavLink key={tab.path} to={tab.path}>
            {tab.label}
          </NavLink>
        ))}
        {isAdmin && (
          <div className="nav-admin">
            {adminTabs.map((tab) => (
              <NavLink key={tab.path} to={tab.path}>
                {tab.label}
              </NavLink>
            ))}
          </div>
        )}
      </nav>

      <main className="layout-main">
        <Suspense fallback={<div style={{ padding: '1rem' }}>Loading…</div>}>
          <Outlet />
        </Suspense>
      </main>
    </div>
  );
}
