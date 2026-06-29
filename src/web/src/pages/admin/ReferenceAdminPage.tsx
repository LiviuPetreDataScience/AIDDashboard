import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { referenceApi } from '../../api/endpoints';
import type { ReferenceItem, ReferenceType } from '../../api/types';
import { PageHeader } from '../../components/PageHeader';
import './AdminPages.css';

/** All reference lists with human-friendly labels for the type picker. */
const REFERENCE_TYPES: { type: ReferenceType; label: string }[] = [
  { type: 'Location', label: 'Locations' },
  { type: 'StaffingRole', label: 'Staffing Roles' },
  { type: 'ContractualLanguage', label: 'Contractual Languages' },
  { type: 'SupportHoursLanguage', label: 'Support Hours Languages' },
  { type: 'Device', label: 'Devices' },
  { type: 'Country', label: 'Countries' },
  { type: 'ServiceTower', label: 'Service Towers (Delivered By / In Scope)' },
  { type: 'OpportunityType', label: 'Opportunity Types' },
  { type: 'OpportunityStatus', label: 'Opportunity Statuses' },
  { type: 'RelatedService', label: 'Related Services' },
  { type: 'SlaType', label: 'SLA / KPI Types' },
  { type: 'MeasurementType', label: 'Measurement Types' },
  { type: 'AccountType', label: 'Account Types' },
  { type: 'Connectivity', label: 'Connectivity' },
  { type: 'Telecom', label: 'Telecom' },
  { type: 'ManagedBy', label: 'Managed By' },
  { type: 'ItsmTool', label: 'ITSM Tools' },
  { type: 'ContractType', label: 'Contract Types' },
  { type: 'BillingModel', label: 'Billing (Pricing) Models' },
  { type: 'TechnologySupported', label: 'Technology Supported' },
  { type: 'Industry', label: 'Industries' },
];

/** Admin: maintain the reference lists that populate the tab dropdowns and matrix rows/columns. */
export function ReferenceAdminPage() {
  const queryClient = useQueryClient();
  const [selectedType, setSelectedType] = useState<ReferenceType>('Location');
  const [newName, setNewName] = useState('');
  const [filter, setFilter] = useState('');

  const queryKey = ['admin-reference', selectedType];
  const { data: items = [] } = useQuery({
    queryKey,
    queryFn: () => referenceApi.getByType(selectedType, true),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey });

  async function handleAdd() {
    const name = newName.trim();
    if (!name) {
      return;
    }
    await referenceApi.create(selectedType, name);
    setNewName('');
    invalidate();
  }

  async function handleSave(item: ReferenceItem, name: string, sortOrder: number, isActive: boolean) {
    await referenceApi.update(item.id, name, sortOrder, isActive);
    invalidate();
  }

  async function handleDelete(item: ReferenceItem) {
    if (!window.confirm(`Delete "${item.name}"? This cannot be undone.`)) {
      return;
    }
    await referenceApi.remove(item.id);
    invalidate();
  }

  const visibleItems = items.filter((item) => item.name.toLowerCase().includes(filter.toLowerCase()));

  return (
    <>
      <PageHeader title="Reference Tables" description="Maintain the lists used across the tabs." />

      <div className="admin-toolbar">
        <label>
          List:{' '}
          <select value={selectedType} onChange={(e) => setSelectedType(e.target.value as ReferenceType)}>
            {REFERENCE_TYPES.map((option) => (
              <option key={option.type} value={option.type}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
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
    </>
  );
}

/** One editable reference row with its own local edit state. */
function ReferenceRow({
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
