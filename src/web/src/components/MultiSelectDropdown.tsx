import { useEffect, useRef, useState } from 'react';
import type { ReferenceItem } from '../api/types';
import './MultiSelectDropdown.css';

interface MultiSelectDropdownProps {
  items: ReferenceItem[];
  values: number[];
  onChange: (values: number[]) => void;
  disabled: boolean;
}

/**
 * A compact multi-select. When not editing it simply shows the selected values joined by ", ".
 * When editing, clicking the box opens a checklist popover; it closes on outside click.
 * This avoids permanently showing every option as a long list of checkboxes.
 */
export function MultiSelectDropdown({ items, values, onChange, disabled }: MultiSelectDropdownProps) {
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Close the popover when clicking outside of it.
  useEffect(() => {
    if (!open) {
      return;
    }
    function handleOutsideClick(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleOutsideClick);
    return () => document.removeEventListener('mousedown', handleOutsideClick);
  }, [open]);

  const selectedNames = items.filter((item) => values.includes(item.id)).map((item) => item.name);
  const label = selectedNames.length > 0 ? selectedNames.join(', ') : '—';

  // Read-only mode: show the selected values as plain text.
  if (disabled) {
    return (
      <div className="ms-display ms-readonly" title={label}>
        {label}
      </div>
    );
  }

  function toggle(id: number, checked: boolean) {
    onChange(checked ? [...values, id] : values.filter((value) => value !== id));
  }

  return (
    <div className="ms-root" ref={containerRef}>
      <button type="button" className="ms-display" onClick={() => setOpen((value) => !value)} title={label}>
        <span className="ms-value">{label}</span>
        <span className="ms-caret">▾</span>
      </button>
      {open && (
        <div className="ms-panel">
          {items.map((item) => (
            <label key={item.id} className="ms-option">
              <input
                type="checkbox"
                checked={values.includes(item.id)}
                onChange={(event) => toggle(item.id, event.target.checked)}
              />
              {item.name}
            </label>
          ))}
        </div>
      )}
    </div>
  );
}
