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
  /** Header for the first (row-label) column, e.g. "Location". */
  rowHeader: string;
  /** Reference items that form the fixed rows (e.g. Locations). */
  rowItems: ReferenceItem[];
  /** Reference items that form the numeric columns (e.g. Roles or Languages). */
  colItems: ReferenceItem[];
  cells: MatrixCell[];
  isAdmin: boolean;
  exportFileName: string;
  /** Open account id and backend tab key, used by the import/export controls. */
  accountId: number;
  tabKey: string;
  /** Reloads the data after an import. */
  onImported: () => void;
  onSave: (cells: MatrixCell[]) => Promise<unknown>;
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
  rowHeader,
  rowItems,
  colItems,
  cells,
  isAdmin,
  exportFileName,
  accountId,
  tabKey,
  onImported,
  onSave,
}: MatrixTabProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<GridRow[]>([]);
  const [saving, setSaving] = useState(false);

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
      minWidth: 220,
    };
    const valueColumns = colItems.map((colItem) => numberColumn(String(colItem.id), colItem.name));
    return [labelColumn, ...valueColumns];
  }, [colItems, rowHeader]);

  const rows = editing ? draft : computedRows;

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
        actions={
          <>
            <ImportExportBar accountId={accountId} tabKey={tabKey} canImport={isAdmin} onImported={onImported} />
            <EditModeBar
              editing={editing}
              canEdit={isAdmin}
              saving={saving}
              onEdit={startEditing}
              onSave={saveChanges}
              onCancel={() => setEditing(false)}
            />
          </>
        }
      />
      <EditableTable<GridRow>
        columns={columns}
        rows={rows}
        editing={editing}
        getRowKey={(row) => String(row._rowRefId)}
        exportFileName={exportFileName}
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
