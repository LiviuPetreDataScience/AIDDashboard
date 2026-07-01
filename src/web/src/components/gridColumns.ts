import type { ColDef } from 'ag-grid-community';
import type { ReferenceItem } from '../api/types';

/**
 * Column factories that keep the tab grids consistent. Columns are sized to their content/type
 * rather than stretched to fill the page: short data (dates, times, numbers, yes/no) gets narrow
 * columns sized to fit the header; text and dropdown values wrap across lines so they stay fully
 * visible. Wide tables simply scroll horizontally.
 */

/** Header always wraps onto as many lines as needed, with the full name also on hover. */
function headerBehaviour(headerName: string): Partial<ColDef> {
  return { headerTooltip: headerName, wrapHeaderText: true, autoHeaderHeight: true };
}

/** Wraps the cell value over multiple lines and grows the row height to fit. */
const wrapValue: Partial<ColDef> = { wrapText: true, autoHeight: true };

/**
 * Picks a width that lets the header wrap to roughly two lines, clamped to [min, max].
 * Used for short-value columns where the header, not the data, drives the width.
 */
function widthForHeader(headerName: string, min: number, max: number): number {
  const approxCharsPerLine = headerName.length / 2;
  const estimated = Math.ceil(approxCharsPerLine * 7) + 28;
  return Math.min(max, Math.max(min, estimated));
}

/** Wide text column (names, details); value wraps and shows in full on hover. */
export function textColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return { field, headerName, ...headerBehaviour(headerName), ...wrapValue, width: 220, tooltipField: field, ...extra };
}

/** Narrow numeric column, sized to its header (wide enough to hold a single-word header on one line). */
export function numberColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return { field, headerName, ...headerBehaviour(headerName), cellDataType: 'number', width: widthForHeader(headerName, 120, 190), ...extra };
}

/** Fixed-width date column (dates are always the same length). */
export function dateColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return {
    field,
    headerName,
    ...headerBehaviour(headerName),
    cellDataType: 'dateString',
    cellEditor: 'agDateStringCellEditor',
    width: widthForHeader(headerName, 120, 150),
    ...extra,
  };
}

/** Yes/No column over a nullable boolean, sized to its header. */
export function yesNoColumn(field: string, headerName: string, extra?: Partial<ColDef>): ColDef {
  return {
    field,
    headerName,
    ...headerBehaviour(headerName),
    width: widthForHeader(headerName, 100, 150),
    cellEditor: 'agSelectCellEditor',
    cellEditorParams: { values: [null, true, false] },
    valueFormatter: (params) => (params.value == null ? '' : params.value ? 'Yes' : 'No'),
    ...extra,
  };
}

/**
 * Single-select reference column: stores a reference-item id but displays its name.
 * The value wraps so long option names stay visible.
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
    ...headerBehaviour(headerName),
    ...wrapValue,
    width: 180,
    cellEditor: 'agSelectCellEditor',
    cellEditorParams: { values: [null, ...items.map((item) => item.id)] },
    valueFormatter: (params) => (params.value == null ? '' : idToName.get(params.value as number) ?? ''),
    ...extra,
  };
}
