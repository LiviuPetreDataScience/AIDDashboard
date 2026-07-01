import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { serviceCatalogApi } from '../../api/endpoints';
import type { ServiceCatalogItem, ServiceNodeKind } from '../../api/types';
import './AdminPages.css';

interface Crumb {
  id: number;
  name: string;
}

/**
 * Drill-down editor for the hierarchical Services catalog. Starts at the top-level categories;
 * click a category to edit its children (sub-categories or services), and keep drilling down.
 * Each level can be filtered, items edited/deleted, and new categories/services added.
 */
export function ServicesTreeEditor() {
  const queryClient = useQueryClient();
  const [parentId, setParentId] = useState<number | null>(null);
  const [breadcrumb, setBreadcrumb] = useState<Crumb[]>([]);
  const [filter, setFilter] = useState('');
  const [newName, setNewName] = useState('');
  const [newKind, setNewKind] = useState<ServiceNodeKind>('Category');

  const queryKey = ['service-catalog', parentId];
  const { data: items = [] } = useQuery({
    queryKey,
    queryFn: () => serviceCatalogApi.children(parentId, true),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey });

  function drillInto(item: ServiceCatalogItem) {
    setBreadcrumb((current) => [...current, { id: item.id, name: item.name }]);
    setParentId(item.id);
    setFilter('');
  }

  function jumpToCrumb(index: number) {
    // index -1 = root (top-level categories)
    if (index < 0) {
      setBreadcrumb([]);
      setParentId(null);
    } else {
      setBreadcrumb((current) => current.slice(0, index + 1));
      setParentId(breadcrumb[index].id);
    }
    setFilter('');
  }

  async function handleAdd() {
    const name = newName.trim();
    if (!name) {
      return;
    }
    // The top level only holds categories; deeper levels may hold categories or services.
    const kind: ServiceNodeKind = parentId === null ? 'Category' : newKind;
    await serviceCatalogApi.create({ parentId, name, kind });
    setNewName('');
    invalidate();
  }

  async function handleSave(item: ServiceCatalogItem, name: string, sortOrder: number, isActive: boolean) {
    await serviceCatalogApi.update(item.id, { name: name.trim(), sortOrder, isActive });
    invalidate();
  }

  async function handleDelete(item: ServiceCatalogItem) {
    const message =
      item.kind === 'Category'
        ? `Delete category "${item.name}" and everything under it? This cannot be undone.`
        : `Delete service "${item.name}"? This cannot be undone.`;
    if (!window.confirm(message)) {
      return;
    }
    await serviceCatalogApi.remove(item.id);
    invalidate();
  }

  const visibleItems = items.filter((item) => item.name.toLowerCase().includes(filter.toLowerCase()));
  const atRoot = parentId === null;

  return (
    <div>
      {/* Breadcrumb */}
      <div className="services-breadcrumb">
        <button type="button" className="crumb" onClick={() => jumpToCrumb(-1)}>
          Categories
        </button>
        {breadcrumb.map((crumb, index) => (
          <span key={crumb.id}>
            <span className="crumb-sep">›</span>
            <button type="button" className="crumb" onClick={() => jumpToCrumb(index)}>
              {crumb.name}
            </button>
          </span>
        ))}
      </div>

      <div className="admin-toolbar">
        <input
          type="search"
          placeholder="Filter this level…"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          style={{ maxWidth: 220 }}
        />
        <div className="admin-add-inline">
          {!atRoot && (
            <select value={newKind} onChange={(e) => setNewKind(e.target.value as ServiceNodeKind)} style={{ width: 'auto' }}>
              <option value="Category">Category</option>
              <option value="Service">Service</option>
            </select>
          )}
          <input
            placeholder={atRoot ? 'New category name' : `New ${newKind.toLowerCase()} name`}
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
            <th style={{ width: '45%' }}>Name</th>
            <th>Type</th>
            <th>Sort order</th>
            <th>Active</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {visibleItems.map((item) => (
            <ServiceRow
              key={item.id}
              item={item}
              onOpen={drillInto}
              onSave={handleSave}
              onDelete={handleDelete}
            />
          ))}
          {visibleItems.length === 0 && (
            <tr>
              <td colSpan={5} style={{ color: 'var(--color-text-muted)' }}>
                Nothing here yet — use “Add” above.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

/** One editable catalog row with its own local edit state. */
function ServiceRow({
  item,
  onOpen,
  onSave,
  onDelete,
}: {
  item: ServiceCatalogItem;
  onOpen: (item: ServiceCatalogItem) => void;
  onSave: (item: ServiceCatalogItem, name: string, sortOrder: number, isActive: boolean) => void;
  onDelete: (item: ServiceCatalogItem) => void;
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
      <td>{item.kind}</td>
      <td>
        <input type="number" value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} style={{ width: 80 }} />
      </td>
      <td>
        <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
      </td>
      <td className="admin-actions">
        {item.kind === 'Category' && (
          <button type="button" className="btn-secondary" onClick={() => onOpen(item)}>
            Open ›
          </button>
        )}
        <button type="button" className="btn-primary" disabled={!isDirty} onClick={() => onSave(item, name.trim(), sortOrder, isActive)}>
          Save
        </button>
        <button type="button" className="btn-secondary" onClick={() => onDelete(item)}>
          Delete
        </button>
      </td>
    </tr>
  );
}
