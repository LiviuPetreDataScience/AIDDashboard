import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '../auth/AuthContext';

interface UseTabEditorParams<TRow> {
  queryKey: unknown[];
  /** Whether the query should run (typically: an account is selected). */
  enabled: boolean;
  load: () => Promise<TRow[]>;
  save: (rows: TRow[]) => Promise<unknown>;
}

/**
 * Encapsulates the load / edit / save lifecycle shared by every table tab:
 * fetches the rows, keeps an editable draft while in edit mode, and persists on save.
 * The grid-ready row shape is decided by the caller's load/save transforms.
 */
export function useTabEditor<TRow>({ queryKey, enabled, load, save }: UseTabEditorParams<TRow>) {
  const queryClient = useQueryClient();
  const { isAdmin } = useAuth();

  const { data, isLoading } = useQuery({ queryKey, queryFn: load, enabled });

  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<TRow[]>([]);
  const [saving, setSaving] = useState(false);

  const startEditing = () => {
    setDraft(structuredClone(data ?? []));
    setEditing(true);
  };

  const cancelEditing = () => setEditing(false);

  const saveChanges = async () => {
    setSaving(true);
    try {
      await save(draft);
      await queryClient.invalidateQueries({ queryKey });
      setEditing(false);
    } finally {
      setSaving(false);
    }
  };

  const reload = () => queryClient.invalidateQueries({ queryKey });

  const rows = editing ? draft : data ?? [];

  return {
    rows,
    setDraft,
    editing,
    saving,
    isLoading,
    isAdmin,
    startEditing,
    cancelEditing,
    saveChanges,
    reload,
  };
}
