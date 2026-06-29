import type { ReactNode } from 'react';
import type { ReferenceItem } from '../api/types';
import './FormControls.css';

/** A labelled form field wrapper used throughout the Overall form. */
export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="form-field">
      <label className="form-label">{label}</label>
      {children}
    </div>
  );
}

/** A titled group of fields laid out in a responsive grid. */
export function FormSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="form-section">
      <h3 className="form-section-title">{title}</h3>
      <div className="form-grid">{children}</div>
    </section>
  );
}

export function TextInput({
  value,
  onChange,
  disabled,
}: {
  value: string | null | undefined;
  onChange: (value: string | null) => void;
  disabled: boolean;
}) {
  return (
    <input
      type="text"
      value={value ?? ''}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value === '' ? null : event.target.value)}
    />
  );
}

export function NumberInput({
  value,
  onChange,
  disabled,
}: {
  value: number | null | undefined;
  onChange: (value: number | null) => void;
  disabled: boolean;
}) {
  return (
    <input
      type="number"
      value={value ?? ''}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value === '' ? null : Number(event.target.value))}
    />
  );
}

export function DateInput({
  value,
  onChange,
  disabled,
}: {
  value: string | null | undefined;
  onChange: (value: string | null) => void;
  disabled: boolean;
}) {
  return (
    <input
      type="date"
      value={value ?? ''}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value === '' ? null : event.target.value)}
    />
  );
}

/** Yes / No / (blank) selector over a nullable boolean. */
export function YesNoSelect({
  value,
  onChange,
  disabled,
}: {
  value: boolean | null | undefined;
  onChange: (value: boolean | null) => void;
  disabled: boolean;
}) {
  const stringValue = value == null ? '' : value ? 'yes' : 'no';
  return (
    <select
      value={stringValue}
      disabled={disabled}
      onChange={(event) => {
        const next = event.target.value;
        onChange(next === '' ? null : next === 'yes');
      }}
    >
      <option value="">—</option>
      <option value="yes">Yes</option>
      <option value="no">No</option>
    </select>
  );
}

/** Single-select dropdown over a reference list, storing the item id. */
export function ReferenceSelect({
  items,
  value,
  onChange,
  disabled,
}: {
  items: ReferenceItem[];
  value: number | null | undefined;
  onChange: (value: number | null) => void;
  disabled: boolean;
}) {
  return (
    <select
      value={value ?? ''}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value === '' ? null : Number(event.target.value))}
    >
      <option value="">—</option>
      {items.map((item) => (
        <option key={item.id} value={item.id}>
          {item.name}
        </option>
      ))}
    </select>
  );
}

/** Multi-select checkbox group over a reference list, storing an array of item ids. */
export function MultiCheckboxGroup({
  items,
  values,
  onChange,
  disabled,
}: {
  items: ReferenceItem[];
  values: number[];
  onChange: (values: number[]) => void;
  disabled: boolean;
}) {
  function toggle(id: number, checked: boolean) {
    if (checked) {
      onChange([...values, id]);
    } else {
      onChange(values.filter((value) => value !== id));
    }
  }

  return (
    <div className="checkbox-group">
      {items.map((item) => (
        <label key={item.id} className="checkbox-option">
          <input
            type="checkbox"
            checked={values.includes(item.id)}
            disabled={disabled}
            onChange={(event) => toggle(item.id, event.target.checked)}
          />
          {item.name}
        </label>
      ))}
    </div>
  );
}
