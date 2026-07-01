import { useMemo, useState } from 'react';
import { Modal } from './Modal';
import './ConnectModal.css';

export type ConnectMode = 'database' | 'api';

/**
 * The columns of each tab's table, used as the mapping targets. This is a demonstration surface:
 * it shows how an external source's columns/fields would map onto our table, ready to be wired to
 * a real connector later.
 */
const TARGET_FIELDS: Record<string, string[]> = {
  automations: ['Automation Name', 'Deployment Date', 'Cost of Implementation', 'Running Cost (monthly)', 'Efficiency Impact (FTE/mo)', 'Delivered By', 'Details'],
  opportunities: ['Name', 'Opportunity Type', 'Related Service', 'Status', 'Estimated Monthly Value', 'Estimated Duration (months)', 'Delivered By', 'Details'],
  'sla-kpis': ['Name', 'Type', 'Description', 'Formula', 'Target %', 'Measurement Type', 'Can be Reported', 'Financial Penalties', 'Bonus'],
  'support-hours': ['Language', 'From (Mon–Fri)', 'To (Mon–Fri)', 'Coverage', 'On-call Interpret DHS', 'Sophie'],
  'contractual-languages': ['Location', 'Language', 'Value'],
  'countries-devices': ['Country', 'No. of Users', 'LTS Staff', 'Device', 'Value'],
  services: ['Category Path', 'Service', 'Contract End Date', 'Exists in Company', 'Provided by Stefanini', 'Provided By', 'Opportunity to Provide', 'Details'],
  'initial-staffing': ['Location', 'Role', 'Value'],
  'latest-approved': ['Location', 'Role', 'Value'],
};

const DB_PROVIDERS = ['SQL Server', 'PostgreSQL', 'MySQL', 'Oracle', 'SQLite'];
type TestStatus = 'idle' | 'testing' | 'ok';

interface ConnectModalProps {
  mode: ConnectMode;
  tabKey: string;
  onClose: () => void;
}

/**
 * Configuration dialog for wiring a tab to an external data source instead of manual entry/import.
 * Two flavours: a database connection and a REST API connection. Both offer a "Test connection"
 * check and a mapping of the source columns/JSON fields onto our table's fields. The actions here
 * are illustrative (not yet wired to a live connector).
 */
export function ConnectModal({ mode, tabKey, onClose }: ConnectModalProps) {
  const targetFields = useMemo(() => TARGET_FIELDS[tabKey] ?? ['Column 1', 'Column 2', 'Column 3'], [tabKey]);

  const [testStatus, setTestStatus] = useState<TestStatus>('idle');
  const [saved, setSaved] = useState(false);
  const [mapping, setMapping] = useState<Record<string, string>>({});

  // Database fields
  const [provider, setProvider] = useState(DB_PROVIDERS[0]);
  const [server, setServer] = useState('');
  const [port, setPort] = useState('');
  const [database, setDatabase] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [sourceTable, setSourceTable] = useState('');

  // API fields
  const [baseUrl, setBaseUrl] = useState('');
  const [method, setMethod] = useState('GET');
  const [path, setPath] = useState('');
  const [authType, setAuthType] = useState('None');
  const [authValue, setAuthValue] = useState('');
  const [recordsPath, setRecordsPath] = useState('');

  function runTest() {
    setTestStatus('testing');
    // Simulated check — a real connector would open the connection / call the endpoint here.
    window.setTimeout(() => setTestStatus('ok'), 800);
  }

  function save() {
    setSaved(true);
    window.setTimeout(onClose, 1200);
  }

  const title = mode === 'database' ? 'Connect a database' : 'Connect an API';

  return (
    <Modal title={title} onClose={onClose}>
      <div className="connect">
        <p className="connect-intro">
          Wire this table to an external source instead of manual entry or file import. Configure the
          connection, test it, then map the source {mode === 'database' ? 'columns' : 'JSON fields'} onto
          our table’s fields.
        </p>

        {mode === 'database' ? (
          <div className="connect-fields">
            <label>
              <span>Provider</span>
              <select value={provider} onChange={(e) => setProvider(e.target.value)}>
                {DB_PROVIDERS.map((p) => (
                  <option key={p}>{p}</option>
                ))}
              </select>
            </label>
            <label>
              <span>Server / Host</span>
              <input value={server} onChange={(e) => setServer(e.target.value)} placeholder="db.example.com" />
            </label>
            <label>
              <span>Port</span>
              <input value={port} onChange={(e) => setPort(e.target.value)} placeholder="1433" />
            </label>
            <label>
              <span>Database</span>
              <input value={database} onChange={(e) => setDatabase(e.target.value)} placeholder="reporting" />
            </label>
            <label>
              <span>Username</span>
              <input value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="off" />
            </label>
            <label>
              <span>Password</span>
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="new-password" />
            </label>
            <label className="connect-wide">
              <span>Source table / view</span>
              <input value={sourceTable} onChange={(e) => setSourceTable(e.target.value)} placeholder="dbo.Automations" />
            </label>
          </div>
        ) : (
          <div className="connect-fields">
            <label className="connect-wide">
              <span>Base URL</span>
              <input value={baseUrl} onChange={(e) => setBaseUrl(e.target.value)} placeholder="https://api.example.com" />
            </label>
            <label>
              <span>Method</span>
              <select value={method} onChange={(e) => setMethod(e.target.value)}>
                <option>GET</option>
                <option>POST</option>
              </select>
            </label>
            <label>
              <span>Endpoint path</span>
              <input value={path} onChange={(e) => setPath(e.target.value)} placeholder="/v1/automations" />
            </label>
            <label>
              <span>Authentication</span>
              <select value={authType} onChange={(e) => setAuthType(e.target.value)}>
                <option>None</option>
                <option>API Key</option>
                <option>Bearer token</option>
              </select>
            </label>
            <label>
              <span>{authType === 'API Key' ? 'API key' : authType === 'Bearer token' ? 'Token' : 'Key / token'}</span>
              <input
                value={authValue}
                onChange={(e) => setAuthValue(e.target.value)}
                disabled={authType === 'None'}
                autoComplete="off"
              />
            </label>
            <label className="connect-wide">
              <span>Records path (JSON)</span>
              <input value={recordsPath} onChange={(e) => setRecordsPath(e.target.value)} placeholder="data.items" />
            </label>
          </div>
        )}

        <div className="connect-test">
          <button type="button" className="btn-secondary" onClick={runTest} disabled={testStatus === 'testing'}>
            {testStatus === 'testing' ? 'Testing…' : 'Test connection'}
          </button>
          {testStatus === 'ok' && <span className="connect-ok">✓ Connection successful (demo)</span>}
        </div>

        <h4 className="connect-map-title">Field mapping</h4>
        <table className="connect-map">
          <thead>
            <tr>
              <th>Our field</th>
              <th>{mode === 'database' ? 'Source column' : 'Source JSON field'}</th>
            </tr>
          </thead>
          <tbody>
            {targetFields.map((field) => (
              <tr key={field}>
                <td>{field}</td>
                <td>
                  <input
                    value={mapping[field] ?? ''}
                    placeholder={mode === 'database' ? 'column_name' : 'json.field'}
                    onChange={(e) => setMapping((m) => ({ ...m, [field]: e.target.value }))}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="connect-actions">
          {saved && <span className="connect-ok">✓ Connection saved (demo)</span>}
          <button type="button" className="btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button type="button" className="btn-primary" onClick={save}>
            Save connection
          </button>
        </div>
      </div>
    </Modal>
  );
}
