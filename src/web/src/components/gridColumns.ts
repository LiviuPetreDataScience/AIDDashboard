import type { ColDef } from 'ag-grid-community';
import type { ReferenceItem } from '../api/types';

/**
 * Column factories that keep the tab grids consistent. Each returns an AG Grid column
 * definition; the page composes them and toggles edit mode via EditableTable.
 */

/** Plain text column. */
export function textColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return { field, headerName, ...extra };
}

/** Numeric column (parses edits to numbers). */
export function numberColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return { field, headerName, cellDataType: 'number', ...extra };
}

/** Date column storing an ISO yyyy-MM-dd string, matching the backend DateOnly fields. */
export function dateColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return {
    field,
    headerName,
    cellDataType: 'dateString',
    cellEditor: 'agDateStringCellEditor',
    ...extra,
  };
}

/** Yes/No column over a nullable boolean. */
export function yesNoColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return {
    field,
    headerName,
    cellEditor: 'agSelectCellEditor',
    cellEditorParams: { values: [null, true, false] },
    valueFormatter: (params) => (params.value == null ? '' : params.value ? 'Yes' : 'No'),
    filterValueGetter: (params) => {
      const value = (params.data as Record<string, unknown>)?.[field];
      return value == null ? '' : value ? 'Yes' : 'No';
    },
    ...extra,
  };
}

/**
 * Single-select reference column: stores a reference-item id but displays (and filters by)
 * its name. The dropdown lists the active items of the given reference list.
 */
export function referenceColumn(
  field: string,
  headerName: string,
  items: ReferenceItem[],
  extra?: Partial<ColDef>,
): ColDef {
  const idToName = new Map(items.map((item) => [item.id, item.name]));
  return {
    field,
    headerName,
    cellEditor: 'agSelectCellEditor',
    cellEditorParams: { values: [null, ...items.map((item) => item.id)] },
    valueFormatter: (params) => (params.value == null ? '' : idToName.get(params.value as number) ?? ''),
    filterValueGetter: (params) => {
      const value = (params.data as Record<string, unknown>)?.[field] as number | null | undefined;
      return value == null ? '' : idToName.get(value) ?? '';
    },
    ...extra,
  };
}
