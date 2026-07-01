import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { referenceApi } from '../../api/endpoints';
import type { ReferenceItem, ReferenceType } from '../../api/types';

/**
 * A self-contained editor for one flat reference list: its own filter, "add" control and table
 * of editable rows. Used both for a single selected list and stacked inside grouped views
 * (e.g. all the Automation-related lists under one "Automations" heading).
 */
export function ReferenceListSection({
  type,
  heading,
  onChange,
}: {
  type: ReferenceType;
  /** Optional sub-heading shown above the section (used when several sections are stacked). */
  heading?: string;
  /** Called after any successful add/save/delete, so callers can refresh dependent views. */
  onChange?: () => void;
}) {
  const queryClient = useQueryClient();
  const [newName, setNewName] = useState('');
  const [filter, setFilter] = useState('');

  const queryKey = ['admin-reference', type];
  const { data: items = [] } = useQuery({
    queryKey,
    queryFn: () => referenceApi.getByType(type, true),
  });

  function refresh() {
    queryClient.invalidateQueries({ queryKey });
    onChange?.();
  }

  async function handleAdd() {
    const name = newName.trim();
    if (!name) {
      return;
    }
    await referenceApi.create(type, name);
    setNewName('');
    refresh();
  }

  async function handleSave(item: ReferenceItem, name: string, sortOrder: number, isActive: boolean) {
    await referenceApi.update(item.id, name, sortOrder, isActive);
    refresh();
  }

  async function handleDelete(item: ReferenceItem) {
    if (!window.confirm(`Delete "${item.name}"? This cannot be undone.`)) {
      return;
    }
    await referenceApi.remove(item.id);
    refresh();
  }

  const visibleItems = items.filter((item) => item.name.toLowerCase().includes(filter.toLowerCase()));

  return (
    <section className="ref-section">
      {heading && <h3 className="ref-section-title">{heading}</h3>}

      <div className="admin-toolbar">
        <input
          type="search"
          placeholder="Filter…"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          style={{ maxWidth: 220 }}
        />
        <div className="admin-add-inline">
          <input
            placeholder="New item name"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
          />
          <button type="button" className="btn-primary" onClick={handleAdd}>
            Add
          </button>
        </div>
      </div>

      <table className="admin-table">
        <thead>
          <tr>
            <th style={{ width: '50%' }}>Name</th>
            <th>Sort order</th>
            <th>Active</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {visibleItems.map((item) => (
            <ReferenceRow key={item.id} item={item} onSave={handleSave} onDelete={handleDelete} />
          ))}
        </tbody>
      </table>
    </section>
  );
}

/** One editable reference row with its own local edit state. */
export function ReferenceRow({
  item,
  onSave,
  onDelete,
}: {
  item: ReferenceItem;
  onSave: (item: ReferenceItem, name: string, sortOrder: number, isActive: boolean) => void;
  onDelete: (item: ReferenceItem) => void;
}) {
  const [name, setName] = useState(item.name);
  const [sortOrder, setSortOrder] = useState(item.sortOrder);
  const [isActive, setIsActive] = useState(item.isActive);

  const isDirty = name !== item.name || sortOrder !== item.sortOrder || isActive !== item.isActive;

  return (
    <tr>
      <td>
        <input value={name} onChange={(e) => setName(e.target.value)} />
      </td>
      <td>
        <input
          type="number"
          value={sortOrder}
          onChange={(e) => setSortOrder(Number(e.target.value))}
          style={{ width: 80 }}
        />
      </td>
      <td>
        <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
      </td>
      <td className="admin-actions">
        <button
          type="button"
          className="btn-primary"
          disabled={!isDirty}
          onClick={() => onSave(item, name.trim(), sortOrder, isActive)}
        >
          Save
        </button>
        <button type="button" className="btn-secondary" onClick={() => onDelete(item)}>
          Delete
        </button>
      </td>
    </tr>
  );
}
