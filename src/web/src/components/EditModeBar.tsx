import './EditModeBar.css';

interface EditModeBarProps {
  editing: boolean;
  /** Whether the current user may edit (admins only). */
  canEdit: boolean;
  saving?: boolean;
  onEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
}

/**
 * Reusable Edit / Save / Cancel control. The layout height is the same whether showing "Edit"
 * or "Save / Cancel" (the editing hint line is always reserved, just hidden when not editing),
 * so toggling edit mode does not shift the surrounding header controls.
 */
export function EditModeBar({ editing, canEdit, saving = false, onEdit, onSave, onCancel }: EditModeBarProps) {
  if (!canEdit) {
    return <span className="edit-bar-readonly">Read-only</span>;
  }

  return (
    <div className="edit-bar">
      <div className="edit-bar-buttons">
        {editing ? (
          <>
            <button type="button" className="btn-primary" onClick={onSave} disabled={saving}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" className="btn-secondary" onClick={onCancel} disabled={saving}>
              Cancel
            </button>
          </>
        ) : (
          <button type="button" className="btn-secondary" onClick={onEdit}>
            ✎ Edit
          </button>
        )}
      </div>
      <span className="edit-bar-hint" style={{ visibility: editing ? 'visible' : 'hidden' }}>
        Editing — changes are not saved until you press Save.
      </span>
    </div>
  );
}
