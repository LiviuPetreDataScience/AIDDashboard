/**
 * TypeScript types mirroring the backend DTOs. Enums are sent/received as strings
 * (the API uses JsonStringEnumConverter).
 */

export type UserRole = 'Admin' | 'User';

export type StaffingModelType = 'Initial' | 'LatestApproved';

export type SupportCoverage = 'None' | 'EightByFive' | 'TwentyFourFive' | 'TwentyFourSeven';

/** All reference list types, matching the backend ReferenceType enum names. */
export type ReferenceType =
  | 'Location'
  | 'StaffingRole'
  | 'ContractualLanguage'
  | 'SupportHoursLanguage'
  | 'Device'
  | 'Country'
  | 'ServiceTower'
  | 'OpportunityType'
  | 'OpportunityStatus'
  | 'RelatedService'
  | 'SlaType'
  | 'MeasurementType'
  | 'AccountType'
  | 'Connectivity'
  | 'Telecom'
  | 'ManagedBy'
  | 'ItsmTool'
  | 'ContractType'
  | 'BillingModel'
  | 'TechnologySupported'
  | 'Industry';

export interface ReferenceItem {
  id: number;
  type: ReferenceType;
  name: string;
  sortOrder: number;
  isActive: boolean;
}

export interface UserDto {
  id: number;
  username: string;
  displayName?: string | null;
  email?: string | null;
  role: UserRole;
  isActive: boolean;
  mustChangePassword: boolean;
}

export interface LoginResult {
  token: string;
  expiresAtUtc: string;
  user: UserDto;
}

export interface AccountSummary {
  id: number;
  name: string;
}

/** The Overall tab payload. Single-select fields are reference ids; multi-selects are id arrays. */
export interface AccountOverall {
  id: number;
  name: string;
  regionalOwnership?: string | null;
  globalSm?: string | null;
  globalSdm?: string | null;
  emeaLtsSdm?: string | null;
  naLtsSdm?: string | null;
  emeaSm?: string | null;
  emeaSdm?: string | null;
  naSdm?: string | null;
  apacSdm?: string | null;
  tsm?: string | null;
  buLeader?: string | null;
  transformationManager?: string | null;
  accountManagerCrd?: string | null;
  launchDate?: string | null;
  contractEndDate?: string | null;
  accountTypeRefId?: number | null;
  flagshipAccount?: boolean | null;
  industryRefId?: number | null;
  headquarterCountryRefId?: number | null;
  contractSupervisor?: string | null;
  noOfUsersSupported?: number | null;
  sharingConstraints?: boolean | null;
  connectivityRefId?: number | null;
  telecomCountry?: string | null;
  telecomRefId?: number | null;
  clientHardware?: boolean | null;
  itsmToolRefId?: number | null;
  managedByRefId?: number | null;
  projectTrainingDurationDays?: number | null;
  supportChannels?: string | null;
  workFromHomeApproved?: boolean | null;
  contractType: number[];
  billingModel: number[];
  billingModelLts: number[];
  billingModelInfra: number[];
  inScope: number[];
  technologySupported: number[];
  devicesSupported: number[];
}

export interface StaffingCell {
  locationRefId: number;
  roleRefId: number;
  value: number;
}

export interface ContractualLanguageCell {
  locationRefId: number;
  languageRefId: number;
  value: number;
}

export interface CountryDeviceValue {
  deviceRefId: number;
  value: number;
}

export interface CountryRow {
  countryRefId: number;
  noOfUsers?: number | null;
  noOfLtsStaff?: number | null;
  devices: CountryDeviceValue[];
}

export interface SupportHourRow {
  languageRefId: number;
  fromMondayFriday?: string | null;
  toMondayFriday?: string | null;
  coverage: SupportCoverage;
  onCallInterpretDhs: boolean;
  sophie: boolean;
}

export interface AutomationRow {
  id: number;
  name?: string | null;
  deploymentDate?: string | null;
  costOfImplementationOneTime?: number | null;
  runningCostMonthly?: number | null;
  efficiencyImpactFtePerMonth?: number | null;
  deliveredByRefId?: number | null;
  details?: string | null;
}

export interface OpportunityRow {
  id: number;
  name?: string | null;
  opportunityTypeRefId?: number | null;
  relatedServiceRefId?: number | null;
  statusRefId?: number | null;
  estimatedMonthlyValue?: number | null;
  estimatedContractDurationMonths?: number | null;
  deliveredByRefId?: number | null;
  details?: string | null;
}

export interface SlaKpiRow {
  id: number;
  name?: string | null;
  typeRefId?: number | null;
  description?: string | null;
  formula?: string | null;
  targetPercent?: string | null;
  measurementTypeRefId?: number | null;
  canBeReported: boolean;
  financialPenalties: boolean;
  bonus: boolean;
}
