import { useMemo } from 'react';
import { AgGridReact } from 'ag-grid-react';
import {
  AllCommunityModule,
  ModuleRegistry,
  type CellValueChangedEvent,
  type ColDef,
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
  /** Global "contains" search text applied across all columns. */
  quickFilter?: string;
  /** Called when rows are structurally changed (a row added or deleted). */
  onChange?: (rows: TRow[]) => void;
  /** Enables the X-to-delete column and the trailing blank "add" row (list-style tables). */
  enableAddDelete?: boolean;
  /** Produces a blank row for the trailing add row. Required when enableAddDelete is set. */
  makeEmptyRow?: () => TRow;
}

/**
 * Reusable data grid used by every table tab. Provides sorting, per-column "contains" filters
 * (a plain text box under each header) and a global quick-search. In edit mode cells become
 * editable; for list-style tables an X appears on each row to delete it and a blank row at the
 * bottom adds a new one. Columns fill the grid width (give a column `flex` to absorb spare space),
 * and the grid scrolls horizontally when the columns are wider than the page.
 */
export function EditableTable<TRow extends object>({
  columns,
  rows,
  editing,
  getRowKey,
  quickFilter,
  onChange,
  enableAddDelete = false,
  makeEmptyRow,
}: EditableTableProps<TRow>) {
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

    // Delete / add-affordance column (only meaningful while editing a list-style table).
    if (enableAddDelete && editing) {
      defs.push({
        headerName: '',
        colId: 'delete',
        width: 40,
        minWidth: 40,
        maxWidth: 40,
        pinned: 'left',
        sortable: false,
        filter: false,
        editable: false,
        resizable: false,
        cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 0 },
        cellRenderer: (params: { data: TRow }) => {
          if ((params.data as Record<string, unknown>)[NEW_ROW_FLAG]) {
            return <span className="grid-add-indicator" title="Type in this row to add a new entry">＋</span>;
          }
          return (
            <button
              type="button"
              className="grid-delete-button"
              title="Delete row"
              onClick={() => {
                const remaining = rows.filter((row) => getRowKey(row) !== getRowKey(params.data));
                onChange?.(remaining);
              }}
            >
              ✕
            </button>
          );
        },
      });
    }

    defs.push(...columns);
    return defs;
  }, [columns, enableAddDelete, editing, rows, getRowKey, onChange]);

  const defaultColDef = useMemo<ColDef<TRow>>(
    () => ({
      sortable: true,
      resizable: true,
      editable: editing,
      // A plain text "contains" filter box under every header — no funnel button, no popup,
      // and it works on numeric columns too (matches the displayed text).
      filter: 'agTextColumnFilter',
      floatingFilter: true,
      suppressHeaderMenuButton: true,
      filterParams: { filterOptions: ['contains'], maxNumConditions: 1, buttons: [] },
      floatingFilterComponentParams: { suppressFilterButton: true },
    }),
    [editing],
  );

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

  return (
    <div className={editing ? 'editable-table editing' : 'editable-table'}>
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
          onCellValueChanged={handleCellValueChanged}
          stopEditingWhenCellsLoseFocus
          animateRows
        />
      </div>
    </div>
  );
}
