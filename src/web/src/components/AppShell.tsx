import { Suspense } from 'react';
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { accountApi } from '../api/endpoints';
import { useAuth } from '../auth/AuthContext';
import { useSelectedAccount } from '../app/AccountContext';
import { sidebarSections } from '../app/navigation';
import { AddAccountButton } from './AddAccountButton';
import { ChatWidget } from './ChatWidget';
import './AppShell.css';

/**
 * Top-level application shell: a brand/user header, a left sidebar of sections
 * (Input / Admin / FAQ / AID Dashboard), and the routed content. The account picker only
 * appears in the header while inside the Input section.
 */
export function AppShell() {
  const { user, isAdmin, logout } = useAuth();
  const { selectedAccountId, setSelectedAccountId } = useSelectedAccount();
  const location = useLocation();

  const isInputSection = location.pathname.startsWith('/input');
  const sections = sidebarSections.filter((section) => !section.adminOnly || isAdmin);

  const { data: accounts = [] } = useQuery({ queryKey: ['accounts'], queryFn: accountApi.getAll });

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link to="/" className="app-brand" title="Go to home">
          AID <span>Dashboard</span>
        </Link>

        {isInputSection && (
          <div className="app-account-picker">
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
            {isAdmin && <AddAccountButton />}
          </div>
        )}

        <div className="app-spacer" />

        <div className="app-user">
          <span>{user?.displayName ?? user?.username}</span>
          <span className="role-badge">{user?.role}</span>
          <button type="button" className="btn-secondary" onClick={logout}>
            Log out
          </button>
        </div>
      </header>

      <div className="app-body">
        <nav className="app-sidebar">
          {sections.map((section) => (
            <NavLink key={section.path} to={section.path}>
              {section.label}
            </NavLink>
          ))}
        </nav>

        <main className="app-main">
          <Suspense fallback={<div style={{ padding: '1rem' }}>Loading…</div>}>
            <Outlet />
          </Suspense>
        </main>
      </div>

      <footer className="app-footer">
        <img src="/stefanini-logo.png" alt="Stefanini Group" />
      </footer>

      <ChatWidget />
    </div>
  );
}
