import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { referenceApi } from '../../api/endpoints';
import type { ReferenceItem, ReferenceType } from '../../api/types';
import { PageHeader } from '../../components/PageHeader';
import { ServicesTreeEditor } from './ServicesTreeEditor';
import { ReferenceRow } from './ReferenceListSection';
import { AutomationsReferenceGroup } from './AutomationsReferenceGroup';
import './AdminPages.css';

/** Sentinel selector values for editors that are not a single flat reference list. */
const SERVICES_TREE = 'ServicesTree';
const AUTOMATIONS = 'Automations';
type ReferenceSelection = ReferenceType | typeof SERVICES_TREE | typeof AUTOMATIONS;

/**
 * Flat reference lists with human-friendly labels for the type picker. The Automation-related
 * lists are not here — they live together under the grouped "Automations" selection below.
 */
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
  { type: 'Competitor', label: 'Competitors' },
];

/** Admin: maintain the reference lists that populate the tab dropdowns and matrix rows/columns. */
export function ReferenceAdminPage() {
  const queryClient = useQueryClient();
  const [selectedType, setSelectedType] = useState<ReferenceSelection>('Location');
  const [newName, setNewName] = useState('');
  const [filter, setFilter] = useState('');

  const isFlatList = selectedType !== SERVICES_TREE && selectedType !== AUTOMATIONS;
  const queryKey = ['admin-reference', selectedType];
  const { data: items = [] } = useQuery({
    queryKey,
    queryFn: () => referenceApi.getByType(selectedType as ReferenceType, true),
    enabled: isFlatList,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey });

  async function handleAdd() {
    const name = newName.trim();
    if (!name) {
      return;
    }
    await referenceApi.create(selectedType as ReferenceType, name);
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
          <select value={selectedType} onChange={(e) => setSelectedType(e.target.value as ReferenceSelection)}>
            {REFERENCE_TYPES.map((option) => (
              <option key={option.type} value={option.type}>
                {option.label}
              </option>
            ))}
            <option value={AUTOMATIONS}>Automations (names, categories, goals, environments, attributes)</option>
            <option value={SERVICES_TREE}>Services (hierarchy)</option>
          </select>
        </label>
        {isFlatList && (
          <>
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
          </>
        )}
      </div>

      {selectedType === SERVICES_TREE ? (
        <ServicesTreeEditor />
      ) : selectedType === AUTOMATIONS ? (
        <AutomationsReferenceGroup />
      ) : (
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
      )}
    </>
  );
}
