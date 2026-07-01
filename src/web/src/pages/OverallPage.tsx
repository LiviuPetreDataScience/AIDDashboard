import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { accountApi } from '../api/endpoints';
import type { AccountOverall } from '../api/types';
import { useSelectedAccount } from '../app/AccountContext';
import { useAuth } from '../auth/AuthContext';
import { useReferenceData } from '../hooks/useReferenceData';
import { EditModeBar } from '../components/EditModeBar';
import { PageHeader } from '../components/PageHeader';
import { SelectAccountNotice } from '../components/SelectAccountNotice';
import {
  DateInput,
  Field,
  FormSection,
  NumberInput,
  ReferenceSelect,
  TextInput,
  YesNoSelect,
} from '../components/FormControls';
import { MultiSelectDropdown } from '../components/MultiSelectDropdown';

/** Overall tab: the per-account header form with grouped fields. */
export function OverallPage() {
  const { selectedAccountId } = useSelectedAccount();
  const { isAdmin } = useAuth();
  const { getItems } = useReferenceData();
  const queryClient = useQueryClient();

  const queryKey = ['overall', selectedAccountId];

  const { data } = useQuery({
    queryKey,
    queryFn: () => accountApi.getOverall(selectedAccountId!),
    enabled: selectedAccountId != null,
  });

  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState<AccountOverall | null>(null);
  const [saving, setSaving] = useState(false);

  if (selectedAccountId == null) {
    return <SelectAccountNotice />;
  }

  const model = editing ? draft : data;
  if (!model) {
    return <div>Loading…</div>;
  }

  /** Updates a single field on the editable draft. */
  function set<Key extends keyof AccountOverall>(key: Key, value: AccountOverall[Key]) {
    setDraft((current) => (current ? { ...current, [key]: value } : current));
  }

  function startEditing() {
    setDraft(structuredClone(data!));
    setEditing(true);
  }

  async function saveChanges() {
    if (!draft) {
      return;
    }
    setSaving(true);
    try {
      await accountApi.saveOverall(selectedAccountId!, draft);
      await queryClient.invalidateQueries({ queryKey });
      setEditing(false);
    } finally {
      setSaving(false);
    }
  }

  const disabled = !editing;

  return (
    <>
      <PageHeader
        title="Overall"
        accountName={model.name}
        editControls={
          <EditModeBar
            editing={editing}
            canEdit={isAdmin}
            saving={saving}
            onEdit={startEditing}
            onSave={saveChanges}
            onCancel={() => setEditing(false)}
          />
        }
      />

      <FormSection title="SG specifics">
        <Field label="Regional Ownership">
          <TextInput value={model.regionalOwnership} onChange={(v) => set('regionalOwnership', v)} disabled={disabled} />
        </Field>
        <Field label="Global SM">
          <TextInput value={model.globalSm} onChange={(v) => set('globalSm', v)} disabled={disabled} />
        </Field>
        <Field label="Global SDM">
          <TextInput value={model.globalSdm} onChange={(v) => set('globalSdm', v)} disabled={disabled} />
        </Field>
        <Field label="EMEA LTS SDM">
          <TextInput value={model.emeaLtsSdm} onChange={(v) => set('emeaLtsSdm', v)} disabled={disabled} />
        </Field>
        <Field label="NA LTS SDM">
          <TextInput value={model.naLtsSdm} onChange={(v) => set('naLtsSdm', v)} disabled={disabled} />
        </Field>
        <Field label="EMEA SM">
          <TextInput value={model.emeaSm} onChange={(v) => set('emeaSm', v)} disabled={disabled} />
        </Field>
        <Field label="EMEA SDM">
          <TextInput value={model.emeaSdm} onChange={(v) => set('emeaSdm', v)} disabled={disabled} />
        </Field>
        <Field label="NA SDM">
          <TextInput value={model.naSdm} onChange={(v) => set('naSdm', v)} disabled={disabled} />
        </Field>
        <Field label="APAC SDM">
          <TextInput value={model.apacSdm} onChange={(v) => set('apacSdm', v)} disabled={disabled} />
        </Field>
        <Field label="TSM">
          <TextInput value={model.tsm} onChange={(v) => set('tsm', v)} disabled={disabled} />
        </Field>
        <Field label="BU Leader">
          <TextInput value={model.buLeader} onChange={(v) => set('buLeader', v)} disabled={disabled} />
        </Field>
        <Field label="Transformation Manager">
          <TextInput value={model.transformationManager} onChange={(v) => set('transformationManager', v)} disabled={disabled} />
        </Field>
        <Field label="Account Manager / CRD">
          <TextInput value={model.accountManagerCrd} onChange={(v) => set('accountManagerCrd', v)} disabled={disabled} />
        </Field>
        <Field label="Launch Date">
          <DateInput value={model.launchDate} onChange={(v) => set('launchDate', v)} disabled={disabled} />
        </Field>
        <Field label="Contract End Date (MSA)">
          <DateInput value={model.contractEndDate} onChange={(v) => set('contractEndDate', v)} disabled={disabled} />
        </Field>
        <Field label="Type of account (ITO, DHS, BPO)">
          <ReferenceSelect items={getItems('AccountType')} value={model.accountTypeRefId} onChange={(v) => set('accountTypeRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Flagship account">
          <YesNoSelect value={model.flagshipAccount} onChange={(v) => set('flagshipAccount', v)} disabled={disabled} />
        </Field>
        <Field label="Contract Type">
          <MultiSelectDropdown items={getItems('ContractType')} values={model.contractType} onChange={(v) => set('contractType', v)} disabled={disabled} />
        </Field>
        <Field label="Billing (Pricing model)">
          <MultiSelectDropdown items={getItems('BillingModel')} values={model.billingModel} onChange={(v) => set('billingModel', v)} disabled={disabled} />
        </Field>
        <Field label="Billing (Pricing model) - LTS">
          <MultiSelectDropdown items={getItems('BillingModel')} values={model.billingModelLts} onChange={(v) => set('billingModelLts', v)} disabled={disabled} />
        </Field>
        <Field label="Billing (Pricing model) - Infra">
          <MultiSelectDropdown items={getItems('BillingModel')} values={model.billingModelInfra} onChange={(v) => set('billingModelInfra', v)} disabled={disabled} />
        </Field>
        <Field label="In Scope">
          <MultiSelectDropdown items={getItems('ServiceTower')} values={model.inScope} onChange={(v) => set('inScope', v)} disabled={disabled} />
        </Field>
      </FormSection>

      <FormSection title="Client specifics">
        <Field label="Industry of Company">
          <ReferenceSelect items={getItems('Industry')} value={model.industryRefId} onChange={(v) => set('industryRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Headquarter (Country)">
          <ReferenceSelect items={getItems('Country')} value={model.headquarterCountryRefId} onChange={(v) => set('headquarterCountryRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Contract supervisor (Counterparty SM)">
          <TextInput value={model.contractSupervisor} onChange={(v) => set('contractSupervisor', v)} disabled={disabled} />
        </Field>
        <Field label="No of users supported">
          <NumberInput value={model.noOfUsersSupported} onChange={(v) => set('noOfUsersSupported', v)} disabled={disabled} />
        </Field>
        <Field label="Sharing constraints (contractual point of view)">
          <YesNoSelect value={model.sharingConstraints} onChange={(v) => set('sharingConstraints', v)} disabled={disabled} />
        </Field>
      </FormSection>

      <FormSection title="Technology">
        <Field label="Connectivity (Country)">
          <ReferenceSelect items={getItems('Connectivity')} value={model.connectivityRefId} onChange={(v) => set('connectivityRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Telecom (Country)">
          <TextInput value={model.telecomCountry} onChange={(v) => set('telecomCountry', v)} disabled={disabled} />
        </Field>
        <Field label="Telecom">
          <ReferenceSelect items={getItems('Telecom')} value={model.telecomRefId} onChange={(v) => set('telecomRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Hardware: Client hardware (Y/N)">
          <YesNoSelect value={model.clientHardware} onChange={(v) => set('clientHardware', v)} disabled={disabled} />
        </Field>
        <Field label="ITSM Tool">
          <ReferenceSelect items={getItems('ItsmTool')} value={model.itsmToolRefId} onChange={(v) => set('itsmToolRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Managed by">
          <ReferenceSelect items={getItems('ManagedBy')} value={model.managedByRefId} onChange={(v) => set('managedByRefId', v)} disabled={disabled} />
        </Field>
        <Field label="Technology supported">
          <MultiSelectDropdown items={getItems('TechnologySupported')} values={model.technologySupported} onChange={(v) => set('technologySupported', v)} disabled={disabled} />
        </Field>
        <Field label="Devices supported">
          <MultiSelectDropdown items={getItems('Device')} values={model.devicesSupported} onChange={(v) => set('devicesSupported', v)} disabled={disabled} />
        </Field>
      </FormSection>

      <FormSection title="Other">
        <Field label="Project Training duration (days)">
          <NumberInput value={model.projectTrainingDurationDays} onChange={(v) => set('projectTrainingDurationDays', v)} disabled={disabled} />
        </Field>
        <Field label="Support channels">
          <TextInput value={model.supportChannels} onChange={(v) => set('supportChannels', v)} disabled={disabled} />
        </Field>
        <Field label="Work from home approved by client">
          <YesNoSelect value={model.workFromHomeApproved} onChange={(v) => set('workFromHomeApproved', v)} disabled={disabled} />
        </Field>
      </FormSection>
    </>
  );
}
