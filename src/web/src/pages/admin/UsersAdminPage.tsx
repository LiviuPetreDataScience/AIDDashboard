import { useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { userAdminApi } from '../../api/endpoints';
import type { UserDto, UserRole } from '../../api/types';
import { PageHeader } from '../../components/PageHeader';
import './AdminPages.css';

const EMPTY_FORM = { username: '', displayName: '', email: '', role: 'User' as UserRole, password: '' };

/** Admin: create and manage application users. */
export function UsersAdminPage() {
  const queryClient = useQueryClient();
  const { data: users = [] } = useQuery({ queryKey: ['admin-users'], queryFn: userAdminApi.getAll });

  const [form, setForm] = useState(EMPTY_FORM);
  const [createError, setCreateError] = useState<string | null>(null);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin-users'] });

  const createMutation = useMutation({
    mutationFn: () => userAdminApi.create(form),
    onSuccess: () => {
      setForm(EMPTY_FORM);
      setCreateError(null);
      invalidate();
    },
    onError: () => setCreateError('Could not create the user. The username may already be in use.'),
  });

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    createMutation.mutate();
  }

  async function handleRoleOrActiveChange(user: UserDto, role: UserRole, isActive: boolean) {
    await userAdminApi.update(user.id, {
      displayName: user.displayName ?? undefined,
      email: user.email ?? undefined,
      role,
      isActive,
    });
    invalidate();
  }

  async function handleResetPassword(user: UserDto) {
    const newPassword = window.prompt(`New password for ${user.username} (min 6 characters):`);
    if (!newPassword) {
      return;
    }
    await userAdminApi.resetPassword(user.id, newPassword);
    window.alert('Password updated.');
  }

  async function handleDeactivate(user: UserDto) {
    if (!window.confirm(`Deactivate ${user.username}?`)) {
      return;
    }
    await userAdminApi.deactivate(user.id);
    invalidate();
  }

  return (
    <>
      <PageHeader title="User Management" description="Create users, assign roles and manage access." />

      <form className="admin-create-form" onSubmit={handleCreate}>
        <input
          placeholder="Username"
          required
          value={form.username}
          onChange={(e) => setForm({ ...form, username: e.target.value })}
        />
        <input
          placeholder="Display name"
          value={form.displayName}
          onChange={(e) => setForm({ ...form, displayName: e.target.value })}
        />
        <input
          placeholder="Email"
          type="email"
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
        />
        <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value as UserRole })}>
          <option value="User">User (read-only)</option>
          <option value="Admin">Admin</option>
        </select>
        <input
          placeholder="Temporary password"
          required
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
        />
        <button type="submit" className="btn-primary" disabled={createMutation.isPending}>
          Add user
        </button>
      </form>
      {createError && <div className="admin-error">{createError}</div>}

      <table className="admin-table">
        <thead>
          <tr>
            <th>Username</th>
            <th>Display name</th>
            <th>Email</th>
            <th>Role</th>
            <th>Active</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>{user.username}</td>
              <td>{user.displayName ?? ''}</td>
              <td>{user.email ?? ''}</td>
              <td>
                <select
                  value={user.role}
                  onChange={(e) => handleRoleOrActiveChange(user, e.target.value as UserRole, user.isActive)}
                >
                  <option value="User">User</option>
                  <option value="Admin">Admin</option>
                </select>
              </td>
              <td>
                <input
                  type="checkbox"
                  checked={user.isActive}
                  onChange={(e) => handleRoleOrActiveChange(user, user.role, e.target.checked)}
                />
              </td>
              <td className="admin-actions">
                <button type="button" className="btn-secondary" onClick={() => handleResetPassword(user)}>
                  Reset password
                </button>
                <button type="button" className="btn-secondary" onClick={() => handleDeactivate(user)}>
                  Deactivate
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
