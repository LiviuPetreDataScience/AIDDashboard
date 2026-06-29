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
 * Reusable Edit / Save / Cancel control shown at the top of every tab. When not editing it
 * shows a single Edit button (for admins); while editing it shows Save and Cancel.
 */
export function EditModeBar({ editing, canEdit, saving = false, onEdit, onSave, onCancel }: EditModeBarProps) {
  if (!canEdit) {
    return <span className="edit-bar-readonly">Read-only</span>;
  }

  if (!editing) {
    return (
      <div className="edit-bar">
        <button type="button" className="btn-secondary" onClick={onEdit}>
          ✎ Edit
        </button>
      </div>
    );
  }

  return (
    <div className="edit-bar">
      <button type="button" className="btn-primary" onClick={onSave} disabled={saving}>
        {saving ? 'Saving…' : 'Save'}
      </button>
      <button type="button" className="btn-secondary" onClick={onCancel} disabled={saving}>
        Cancel
      </button>
      <span className="edit-bar-hint">Editing — changes are not saved until you press Save.</span>
    </div>
  );
}
