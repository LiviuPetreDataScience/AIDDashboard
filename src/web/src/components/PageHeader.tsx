import type { ReactNode } from 'react';
import './PageHeader.css';

interface PageHeaderProps {
  title: string;
  /** Selected account name, shown as a subtitle under the title (on account-scoped tabs). */
  accountName?: string;
  /** Generic helper subtitle (used by admin pages that are not account-scoped). */
  description?: string;
  /** Optional global search box, shown right after the title. */
  search?: { value: string; onChange: (value: string) => void; placeholder?: string };
  /** Import/export controls, shown right after the search. */
  importExport?: ReactNode;
  /** Edit / Save-Cancel controls, pinned to the far right. */
  editControls?: ReactNode;
}

/**
 * Consistent header for each tab. Left to right: title (with the account name underneath),
 * a fixed-width global search, the import/export controls, and — pinned to the far right —
 * the edit controls. The right block is pinned and height-reserved so toggling edit mode does
 * not shift the other controls.
 */
export function PageHeader({ title, accountName, description, search, importExport, editControls }: PageHeaderProps) {
  return (
    <div className="page-header">
      <div className="page-header-left">
        <h2 className="page-title">{title}</h2>
        {accountName && <div className="page-account-name">{accountName}</div>}
        {description && <p className="page-description">{description}</p>}
      </div>

      {search && (
        <div className="page-search">
          <input
            type="search"
            value={search.value}
            placeholder={search.placeholder ?? 'Search this table…'}
            onChange={(event) => search.onChange(event.target.value)}
          />
        </div>
      )}

      {importExport && <div className="page-tools">{importExport}</div>}

      {editControls && <div className="page-edit">{editControls}</div>}
    </div>
  );
}
