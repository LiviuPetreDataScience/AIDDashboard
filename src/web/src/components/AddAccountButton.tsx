import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { accountApi } from '../api/endpoints';
import { useSelectedAccount } from '../app/AccountContext';
import { Modal } from './Modal';

/**
 * "+ Add account" button with its creation modal. On success it selects the new account and
 * invalidates the accounts list; an optional callback lets the caller navigate afterwards.
 */
export function AddAccountButton({ onCreated }: { onCreated?: (accountId: number) => void }) {
  const queryClient = useQueryClient();
  const { setSelectedAccountId } = useSelectedAccount();

  const [isOpen, setIsOpen] = useState(false);
  const [name, setName] = useState('');
  const [isCreating, setIsCreating] = useState(false);

  async function handleCreate() {
    const trimmed = name.trim();
    if (!trimmed) {
      return;
    }
    setIsCreating(true);
    try {
      const created = await accountApi.create(trimmed);
      await queryClient.invalidateQueries({ queryKey: ['accounts'] });
      setSelectedAccountId(created.id);
      setName('');
      setIsOpen(false);
      onCreated?.(created.id);
    } finally {
      setIsCreating(false);
    }
  }

  return (
    <>
      <button type="button" className="btn-secondary" onClick={() => setIsOpen(true)}>
        + Add account
      </button>

      {isOpen && (
        <Modal title="Add account" onClose={() => setIsOpen(false)}>
          <label className="form-label" htmlFor="new-account-name">Account name</label>
          <input
            id="new-account-name"
            autoFocus
            value={name}
            placeholder="e.g. Account B"
            onChange={(event) => setName(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                handleCreate();
              }
            }}
          />
          <div className="modal-actions">
            <button type="button" className="btn-secondary" onClick={() => setIsOpen(false)} disabled={isCreating}>
              Cancel
            </button>
            <button type="button" className="btn-primary" onClick={handleCreate} disabled={isCreating || !name.trim()}>
              {isCreating ? 'Creating…' : 'Create'}
            </button>
          </div>
        </Modal>
      )}
    </>
  );
}
