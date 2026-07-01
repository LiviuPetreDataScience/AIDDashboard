import { useMemo, useState } from 'react';
import type { ColDef } from 'ag-grid-community';
import type { ReferenceItem } from '../api/types';
import { EditableTable } from './EditableTable';
import { EditModeBar } from './EditModeBar';
import { ImportExportBar } from './ImportExportBar';
import { PageHeader } from './PageHeader';
import { numberColumn } from './gridColumns';

/** A normalized numeric cell: a value at the intersection of a row reference and a column reference. */
export interface MatrixCell {
  rowRefId: number;
  colRefId: number;
  value: number;
}

interface MatrixTabProps {
  title: string;
  /** Selected account name, shown under the title. */
  accountName?: string;
  /** Header for the first (row-label) column, e.g. "Location". */
  rowHeader: string;
  /** Reference items that form the fixed rows (e.g. Locations). */
  rowItems: ReferenceItem[];
  /** Reference items that form the numeric columns (e.g. Roles or Languages). */
  colItems: ReferenceItem[];
  cells: MatrixCell[];
  isAdmin: boolean;
  /** Open account id and backend tab key, used by the import/export controls. */
  accountId: number;
  tabKey: string;
  /** Reloads the data after an import. */
  onImported: () => void;
  onSave: (cells: MatrixCell[]) => Promise<unknown>;
  /**
   * When true, rows with no values at all are hidden in read-only mode (but every row is shown
   * while editing, so empty rows can still be filled in).
   */
  hideEmptyRowsWhenReadOnly?: boolean;
}

/** Internal grid row: a row-label plus one numeric field per column id. */
type GridRow = Record<string, unknown> & { _rowRefId: number; _label: string };

const ROW_LABEL_FIELD = '_label';

/**
 * Reusable matrix editor for tabs shaped as "row reference x column reference = number"
 * (the staffing models and contractual languages). Rows are fixed by the reference list;
 * editing toggles the numeric cells.
 */
export function MatrixTab({
  title,
  accountName,
  rowHeader,
  rowItems,
  colItems,
  cells,
  isAdmin,
  accountId,
  tabKey,
  onImported,
  onSave,
  hideEmptyRowsWhenReadOnly = false,
}: MatrixTabProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<GridRow[]>([]);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');

  // Build one grid row per row-reference, filling in the numeric cells.
  const computedRows = useMemo<GridRow[]>(() => {
    const cellLookup = new Map<string, number>();
    for (const cell of cells) {
      cellLookup.set(`${cell.rowRefId}:${cell.colRefId}`, cell.value);
    }

    return rowItems.map((rowItem) => {
      const row: GridRow = { _rowRefId: rowItem.id, _label: rowItem.name };
      for (const colItem of colItems) {
        const value = cellLookup.get(`${rowItem.id}:${colItem.id}`);
        row[String(colItem.id)] = value ?? null;
      }
      return row;
    });
  }, [cells, rowItems, colItems]);

  const columns = useMemo<ColDef<GridRow>[]>(() => {
    const labelColumn: ColDef<GridRow> = {
      field: ROW_LABEL_FIELD,
      headerName: rowHeader,
      editable: false,
      pinned: 'left',
      width: 240,
      tooltipField: ROW_LABEL_FIELD,
    };
    const valueColumns = colItems.map((colItem) => numberColumn(String(colItem.id), colItem.name));
    return [labelColumn, ...valueColumns];
  }, [colItems, rowHeader]);

  // While editing we always show every row (so empty ones can be filled in). In read-only mode,
  // optionally hide rows that have no value in any column.
  const readOnlyRows = useMemo(() => {
    if (!hideEmptyRowsWhenReadOnly) {
      return computedRows;
    }
    return computedRows.filter((row) => colItems.some((colItem) => typeof row[String(colItem.id)] === 'number'));
  }, [computedRows, colItems, hideEmptyRowsWhenReadOnly]);

  const rows = editing ? draft : readOnlyRows;

  function startEditing() {
    setDraft(structuredClone(computedRows));
    setEditing(true);
  }

  async function saveChanges() {
    setSaving(true);
    try {
      await onSave(toCells(draft, colItems));
      setEditing(false);
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <PageHeader
        title={title}
        accountName={accountName}
        search={{ value: search, onChange: setSearch }}
        importExport={
          <ImportExportBar accountId={accountId} tabKey={tabKey} canImport={isAdmin} onImported={onImported} />
        }
        editControls={
          <EditModeBar
            editing={editing}
            canEdit={isAdmin}
            saving={saving}
            onEdit={startEditing}
            onSave={saveChanges}
            onCancel={() => setEditing(false)}
          />
        }
      />
      <EditableTable<GridRow>
        columns={columns}
        rows={rows}
        editing={editing}
        getRowKey={(row) => String(row._rowRefId)}
        quickFilter={search}
      />
    </>
  );
}

/** Converts edited grid rows back to the list of populated numeric cells. */
function toCells(rows: GridRow[], colItems: ReferenceItem[]): MatrixCell[] {
  const cells: MatrixCell[] = [];
  for (const row of rows) {
    for (const colItem of colItems) {
      const value = row[String(colItem.id)];
      if (typeof value === 'number' && !Number.isNaN(value)) {
        cells.push({ rowRefId: row._rowRefId, colRefId: colItem.id, value });
      }
    }
  }
  return cells;
}
