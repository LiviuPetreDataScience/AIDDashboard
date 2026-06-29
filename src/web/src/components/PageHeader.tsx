import type { ReactNode } from 'react';
import './PageHeader.css';

interface PageHeaderProps {
  title: string;
  description?: string;
  /** Right-aligned actions, typically the EditModeBar and import/export controls. */
  actions?: ReactNode;
}

/** Consistent header for each tab/page: title on the left, actions on the right. */
export function PageHeader({ title, description, actions }: PageHeaderProps) {
  return (
    <div className="page-header">
      <div>
        <h2 className="page-title">{title}</h2>
        {description && <p className="page-description">{description}</p>}
      </div>
      <div className="page-actions">{actions}</div>
    </div>
  );
}
