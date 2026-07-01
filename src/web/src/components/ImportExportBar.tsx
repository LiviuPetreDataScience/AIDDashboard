import { useEffect, useRef, useState } from 'react';
import { importExportApi, type ExportFormat } from '../api/endpoints';
import { ConnectModal, type ConnectMode } from './ConnectModal';
import './ImportExportBar.css';

interface ImportExportBarProps {
  accountId: number;
  /** Backend tab key, e.g. "automations" or "initial-staffing". */
  tabKey: string;
  /** Whether the user may import (administrators only). */
  canImport: boolean;
  /** Called after a successful import so the page can reload its data. */
  onImported: () => void;
  /** Called by the Connect ▸ Refresh action. Defaults to reloading like an import. */
  onRefresh?: () => void;
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

/**
 * Compact export/import controls: an "Export" button (XLSX/CSV/PDF) and an "Import" button
 * (download template / upload file), each opening a small menu. Kept narrow so the page header
 * stays on one row.
 */
export function ImportExportBar({ accountId, tabKey, canImport, onImported, onRefresh }: ImportExportBarProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [busy, setBusy] = useState(false);
  const [openMenu, setOpenMenu] = useState<'export' | 'import' | 'connect' | null>(null);
  const [connectMode, setConnectMode] = useState<ConnectMode | null>(null);

  // Close the open menu when clicking outside the bar.
  useEffect(() => {
    if (!openMenu) {
      return;
    }
    function handleOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpenMenu(null);
      }
    }
    document.addEventListener('mousedown', handleOutside);
    return () => document.removeEventListener('mousedown', handleOutside);
  }, [openMenu]);

  async function handleExport(format: ExportFormat) {
    setOpenMenu(null);
    setBusy(true);
    try {
      const blob = await importExportApi.export(accountId, tabKey, format);
      downloadBlob(blob, `${tabKey}.${format}`);
    } finally {
      setBusy(false);
    }
  }

  async function handleTemplate() {
    setOpenMenu(null);
    setBusy(true);
    try {
      const blob = await importExportApi.template(accountId, tabKey, 'xlsx');
      downloadBlob(blob, `${tabKey}-template.xlsx`);
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
    <div className="ie-bar" ref={containerRef}>
      <div className="ie-menu-wrap">
        <button
          type="button"
          className="btn-secondary"
          disabled={busy}
          onClick={() => setOpenMenu((m) => (m === 'export' ? null : 'export'))}
        >
          Export ▾
        </button>
        {openMenu === 'export' && (
          <div className="ie-menu">
            <button type="button" onClick={() => handleExport('xlsx')}>Excel (XLSX)</button>
            <button type="button" onClick={() => handleExport('csv')}>CSV</button>
            <button type="button" onClick={() => handleExport('pdf')}>PDF</button>
          </div>
        )}
      </div>

      {canImport && (
        <div className="ie-menu-wrap">
          <button
            type="button"
            className="btn-secondary"
            disabled={busy}
            onClick={() => setOpenMenu((m) => (m === 'import' ? null : 'import'))}
          >
            Import ▾
          </button>
          {openMenu === 'import' && (
            <div className="ie-menu">
              <button type="button" onClick={handleTemplate}>Download template</button>
              <button type="button" onClick={() => { setOpenMenu(null); fileInputRef.current?.click(); }}>
                Upload file…
              </button>
            </div>
          )}
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

      <div className="ie-menu-wrap">
        <button
          type="button"
          className="btn-secondary ie-connect-btn"
          disabled={busy}
          onClick={() => setOpenMenu((m) => (m === 'connect' ? null : 'connect'))}
        >
          {/* Plug icon */}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
            <path d="M9 2v6M15 2v6M7 8h10v3a5 5 0 0 1-10 0V8ZM12 16v6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
          Connect ▾
        </button>
        {openMenu === 'connect' && (
          <div className="ie-menu">
            <button type="button" onClick={() => { setOpenMenu(null); setConnectMode('database'); }}>
              Connect data source
            </button>
            <button type="button" onClick={() => { setOpenMenu(null); setConnectMode('api'); }}>
              Connect API
            </button>
            <button type="button" onClick={() => { setOpenMenu(null); (onRefresh ?? onImported)(); }}>
              Refresh
            </button>
          </div>
        )}
      </div>

      {connectMode && <ConnectModal mode={connectMode} tabKey={tabKey} onClose={() => setConnectMode(null)} />}
    </div>
  );
}
