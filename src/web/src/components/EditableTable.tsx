import { useMemo, useRef, useState } from 'react';
import { AgGridReact } from 'ag-grid-react';
import {
  AllCommunityModule,
  ModuleRegistry,
  type CellValueChangedEvent,
  type ColDef,
  type GridApi,
  type GridReadyEvent,
} from 'ag-grid-community';
import { aidGridTheme } from '../theme/agGridTheme';
import './EditableTable.css';

// Register all AG Grid Community features once, when the first grid-using tab is loaded.
// Living here (rather than in main.tsx) keeps AG Grid out of the initial app bundle.
ModuleRegistry.registerModules([AllCommunityModule]);

/** Marks the trailing blank row that lets the user append a new record while editing. */
const NEW_ROW_FLAG = '__isNewRow';

export interface EditableTableProps<TRow> {
  /** Column definitions (without the delete column, which is added automatically). */
  columns: ColDef<TRow>[];
  /** The rows to display. In edit mode this should be the editable draft. */
  rows: TRow[];
  /** Whether the grid is currently editable. */
  editing: boolean;
  /** Stable unique key for a row (used by AG Grid to track rows). */
  getRowKey: (row: TRow) => string;
  /** Called when rows are structurally changed (a row added or deleted). */
  onChange?: (rows: TRow[]) => void;
  /** Enables the X-to-delete column and the trailing blank "add" row (list-style tables). */
  enableAddDelete?: boolean;
  /** Produces a blank row for the trailing add row. Required when enableAddDelete is set. */
  makeEmptyRow?: () => TRow;
  /** File name (without extension) used for the CSV export. */
  exportFileName?: string;
}

/**
 * Reusable data grid used by every table tab. Provides sorting, per-column filtering,
 * a global search box and CSV export. In edit mode cells become editable; for list-style
 * tables an X appears on each row to delete it and a blank row at the bottom adds a new one.
 */
export function EditableTable<TRow extends object>({
  columns,
  rows,
  editing,
  getRowKey,
  onChange,
  enableAddDelete = false,
  makeEmptyRow,
  exportFileName = 'export',
}: EditableTableProps<TRow>) {
  const gridApiRef = useRef<GridApi<TRow> | null>(null);
  const [quickFilter, setQuickFilter] = useState('');

  // Append a blank row at the end while editing a list-style table.
  const displayRows = useMemo(() => {
    if (editing && enableAddDelete && makeEmptyRow) {
      const blank = { ...makeEmptyRow(), [NEW_ROW_FLAG]: true } as TRow;
      return [...rows, blank];
    }
    return rows;
  }, [rows, editing, enableAddDelete, makeEmptyRow]);

  const columnDefs = useMemo<ColDef<TRow>[]>(() => {
    const defs: ColDef<TRow>[] = [];

    // Delete column (only meaningful while editing a list-style table).
    if (enableAddDelete && editing) {
      defs.push({
        headerName: '',
        colId: 'delete',
        width: 48,
        pinned: 'left',
        sortable: false,
        filter: false,
        editable: false,
        cellRenderer: (params: { data: TRow }) => {
          if ((params.data as Record<string, unknown>)[NEW_ROW_FLAG]) {
            return '';
          }
          const button = document.createElement('button');
          button.textContent = '✕';
          button.title = 'Delete row';
          button.className = 'grid-delete-button';
          button.onclick = () => {
            const remaining = rows.filter((row) => getRowKey(row) !== getRowKey(params.data));
            onChange?.(remaining);
          };
          return button;
        },
      });
    }

    defs.push(...columns);
    return defs;
  }, [columns, enableAddDelete, editing, rows, getRowKey, onChange]);

  const defaultColDef = useMemo<ColDef<TRow>>(
    () => ({
      sortable: true,
      filter: true,
      floatingFilter: true,
      resizable: true,
      editable: editing,
      flex: 1,
      minWidth: 120,
    }),
    [editing],
  );

  function handleGridReady(event: GridReadyEvent<TRow>) {
    gridApiRef.current = event.api;
  }

  function handleCellValueChanged(event: CellValueChangedEvent<TRow>) {
    const data = event.data as Record<string, unknown>;
    if (data[NEW_ROW_FLAG]) {
      // The blank row was edited: promote it to a real row and let a fresh blank row appear.
      const promoted = { ...(event.data as TRow) };
      delete (promoted as Record<string, unknown>)[NEW_ROW_FLAG];
      onChange?.([...rows, promoted]);
    }
    // Edits to existing rows mutate the draft objects in place; the parent reads them on save.
  }

  function handleExportCsv() {
    gridApiRef.current?.exportDataAsCsv({ fileName: `${exportFileName}.csv` });
  }

  return (
    <div className="editable-table">
      <div className="editable-table-toolbar">
        <input
          type="search"
          className="grid-search"
          placeholder="Search this table…"
          value={quickFilter}
          onChange={(event) => setQuickFilter(event.target.value)}
        />
        <button type="button" className="btn-secondary" onClick={handleExportCsv}>
          Export CSV
        </button>
      </div>

      <div className="editable-table-grid">
        <AgGridReact<TRow>
          theme={aidGridTheme}
          columnDefs={columnDefs}
          rowData={displayRows}
          defaultColDef={defaultColDef}
          quickFilterText={quickFilter}
          getRowId={(params) =>
            (params.data as Record<string, unknown>)[NEW_ROW_FLAG]
              ? '__new_row__'
              : getRowKey(params.data)
          }
          onGridReady={handleGridReady}
          onCellValueChanged={handleCellValueChanged}
          stopEditingWhenCellsLoseFocus
          animateRows
        />
      </div>
    </div>
  );
}
