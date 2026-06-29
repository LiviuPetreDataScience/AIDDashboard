import { apiClient } from './client';
import type {
  AccountOverall,
  AccountSummary,
  AutomationRow,
  ContractualLanguageCell,
  CountryRow,
  LoginResult,
  OpportunityRow,
  ReferenceItem,
  ReferenceType,
  SlaKpiRow,
  StaffingCell,
  StaffingModelType,
  SupportHourRow,
  UserDto,
  UserRole,
} from './types';

// ---- Authentication ----
export const authApi = {
  login: (username: string, password: string) =>
    apiClient.post<LoginResult>('/auth/login', { username, password }).then((r) => r.data),
  me: () => apiClient.get<UserDto>('/auth/me').then((r) => r.data),
  changePassword: (currentPassword: string, newPassword: string) =>
    apiClient.post('/auth/change-password', { currentPassword, newPassword }),
};

// ---- Reference data ----
export const referenceApi = {
  getAll: (includeInactive = false) =>
    apiClient
      .get<ReferenceItem[]>('/reference', { params: { includeInactive } })
      .then((r) => r.data),
  getByType: (type: ReferenceType, includeInactive = false) =>
    apiClient
      .get<ReferenceItem[]>(`/reference/${type}`, { params: { includeInactive } })
      .then((r) => r.data),
  create: (type: ReferenceType, name: string, sortOrder?: number) =>
    apiClient.post<ReferenceItem>('/reference', { type, name, sortOrder }).then((r) => r.data),
  update: (id: number, name: string, sortOrder: number, isActive: boolean) =>
    apiClient.put(`/reference/${id}`, { name, sortOrder, isActive }),
  remove: (id: number) => apiClient.delete(`/reference/${id}`),
};

// ---- Accounts & Overall ----
export const accountApi = {
  getAll: () => apiClient.get<AccountSummary[]>('/accounts').then((r) => r.data),
  create: (name: string) =>
    apiClient.post<AccountSummary>('/accounts', { name }).then((r) => r.data),
  getOverall: (accountId: number) =>
    apiClient.get<AccountOverall>(`/accounts/${accountId}/overall`).then((r) => r.data),
  saveOverall: (accountId: number, overall: AccountOverall) =>
    apiClient.put(`/accounts/${accountId}/overall`, overall),
  remove: (accountId: number) => apiClient.delete(`/accounts/${accountId}`),
};

// ---- Per-tab data ----
export const staffingApi = {
  get: (accountId: number, modelType: StaffingModelType) =>
    apiClient.get<StaffingCell[]>(`/accounts/${accountId}/staffing/${modelType}`).then((r) => r.data),
  save: (accountId: number, modelType: StaffingModelType, cells: StaffingCell[]) =>
    apiClient.put(`/accounts/${accountId}/staffing/${modelType}`, cells),
};

export const contractualLanguageApi = {
  get: (accountId: number) =>
    apiClient.get<ContractualLanguageCell[]>(`/accounts/${accountId}/contractual-languages`).then((r) => r.data),
  save: (accountId: number, cells: ContractualLanguageCell[]) =>
    apiClient.put(`/accounts/${accountId}/contractual-languages`, cells),
};

export const countryDeviceApi = {
  get: (accountId: number) =>
    apiClient.get<CountryRow[]>(`/accounts/${accountId}/countries-devices`).then((r) => r.data),
  save: (accountId: number, rows: CountryRow[]) =>
    apiClient.put(`/accounts/${accountId}/countries-devices`, rows),
};

export const supportHourApi = {
  get: (accountId: number) =>
    apiClient.get<SupportHourRow[]>(`/accounts/${accountId}/support-hours`).then((r) => r.data),
  save: (accountId: number, rows: SupportHourRow[]) =>
    apiClient.put(`/accounts/${accountId}/support-hours`, rows),
};

export const automationApi = {
  get: (accountId: number) =>
    apiClient.get<AutomationRow[]>(`/accounts/${accountId}/automations`).then((r) => r.data),
  save: (accountId: number, rows: AutomationRow[]) =>
    apiClient.put(`/accounts/${accountId}/automations`, rows),
};

export const opportunityApi = {
  get: (accountId: number) =>
    apiClient.get<OpportunityRow[]>(`/accounts/${accountId}/opportunities`).then((r) => r.data),
  save: (accountId: number, rows: OpportunityRow[]) =>
    apiClient.put(`/accounts/${accountId}/opportunities`, rows),
};

export const slaKpiApi = {
  get: (accountId: number) =>
    apiClient.get<SlaKpiRow[]>(`/accounts/${accountId}/sla-kpis`).then((r) => r.data),
  save: (accountId: number, rows: SlaKpiRow[]) =>
    apiClient.put(`/accounts/${accountId}/sla-kpis`, rows),
};

// ---- Import / Export (per tab, per open account) ----
export type ExportFormat = 'xlsx' | 'csv' | 'pdf';

export const importExportApi = {
  export: (accountId: number, tabKey: string, format: ExportFormat) =>
    apiClient
      .get(`/accounts/${accountId}/tabs/${tabKey}/export`, { params: { format }, responseType: 'blob' })
      .then((r) => r.data as Blob),
  template: (accountId: number, tabKey: string, format: 'xlsx' | 'csv') =>
    apiClient
      .get(`/accounts/${accountId}/tabs/${tabKey}/template`, { params: { format }, responseType: 'blob' })
      .then((r) => r.data as Blob),
  import: (accountId: number, tabKey: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post(`/accounts/${accountId}/tabs/${tabKey}/import`, formData);
  },
};

// ---- Admin: users ----
export const userAdminApi = {
  getAll: () => apiClient.get<UserDto[]>('/admin/users').then((r) => r.data),
  create: (request: { username: string; displayName?: string; email?: string; role: UserRole; password: string }) =>
    apiClient.post<UserDto>('/admin/users', request).then((r) => r.data),
  update: (id: number, request: { displayName?: string; email?: string; role: UserRole; isActive: boolean }) =>
    apiClient.put(`/admin/users/${id}`, request),
  resetPassword: (id: number, newPassword: string) =>
    apiClient.post(`/admin/users/${id}/reset-password`, { newPassword }),
  deactivate: (id: number) => apiClient.post(`/admin/users/${id}/deactivate`),
};
