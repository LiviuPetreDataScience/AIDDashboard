import { useRef, useState } from 'react';
import { importExportApi, type ExportFormat } from '../api/endpoints';
import './ImportExportBar.css';

interface ImportExportBarProps {
  accountId: number;
  /** Backend tab key, e.g. "automations" or "initial-staffing". */
  tabKey: string;
  /** Whether the user may import (administrators only). */
  canImport: boolean;
  /** Called after a successful import so the page can reload its data. */
  onImported: () => void;
}

/** Triggers a browser download of a blob returned by the API. */
function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

/** Export (XLSX/CSV/PDF), template download and import controls for a table tab. */
export function ImportExportBar({ accountId, tabKey, canImport, onImported }: ImportExportBarProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);

  async function handleExport(format: ExportFormat) {
    setBusy(true);
    try {
      const blob = await importExportApi.export(accountId, tabKey, format);
      downloadBlob(blob, `${tabKey}.${format}`);
    } finally {
      setBusy(false);
    }
  }

  async function handleTemplate(format: 'xlsx' | 'csv') {
    setBusy(true);
    try {
      const blob = await importExportApi.template(accountId, tabKey, format);
      downloadBlob(blob, `${tabKey}-template.${format}`);
    } finally {
      setBusy(false);
    }
  }

  async function handleImportFile(file: File) {
    setBusy(true);
    try {
      await importExportApi.import(accountId, tabKey, file);
      onImported();
    } catch {
      window.alert('Import failed. Please check the file matches the template.');
    } finally {
      setBusy(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  }

  return (
    <div className="import-export-bar">
      <div className="ie-group">
        <span className="ie-label">Export:</span>
        <button type="button" className="btn-secondary" disabled={busy} onClick={() => handleExport('xlsx')}>
          XLSX
        </button>
        <button type="button" className="btn-secondary" disabled={busy} onClick={() => handleExport('csv')}>
          CSV
        </button>
        <button type="button" className="btn-secondary" disabled={busy} onClick={() => handleExport('pdf')}>
          PDF
        </button>
      </div>

      {canImport && (
        <div className="ie-group">
          <span className="ie-label">Import:</span>
          <button type="button" className="btn-secondary" disabled={busy} onClick={() => handleTemplate('xlsx')}>
            Template
          </button>
          <button type="button" className="btn-secondary" disabled={busy} onClick={() => fileInputRef.current?.click()}>
            Upload file
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept=".xlsx,.csv"
            style={{ display: 'none' }}
            onChange={(event) => {
              const file = event.target.files?.[0];
              if (file) {
                handleImportFile(file);
              }
            }}
          />
        </div>
      )}
    </div>
  );
}
