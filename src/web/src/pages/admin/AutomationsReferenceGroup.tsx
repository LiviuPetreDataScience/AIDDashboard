import { useQueryClient } from '@tanstack/react-query';
import { AutomationProfilesEditor } from './AutomationProfilesEditor';
import { ReferenceListSection } from './ReferenceListSection';

/**
 * Groups every Automation-related reference list under a single "Automations" selection: the
 * automation names, their categories, goals and affected environments, plus the attribute
 * editor that connects an automation to those values. Each list keeps its own filter and Add.
 */
export function AutomationsReferenceGroup() {
  const queryClient = useQueryClient();

  // When the automation NAME list changes, the attribute editor (one row per automation) must refresh.
  const refreshProfiles = () => queryClient.invalidateQueries({ queryKey: ['automation-profiles'] });

  return (
    <div className="ref-group">
      <ReferenceListSection type="Automation" heading="Automation Names" onChange={refreshProfiles} />
      <ReferenceListSection type="AutomationCategory" heading="Automation Categories" />
      <ReferenceListSection type="AutomationGoal" heading="Automation Goals" />
      <ReferenceListSection type="Environment" heading="Automation Environments" />

      <section className="ref-section">
        <h3 className="ref-section-title">Automation Attributes (Category / Goal / Environment / AI)</h3>
        <p className="ref-section-hint">Set the shared attributes of each automation. These feed the Automations dashboard.</p>
        <AutomationProfilesEditor />
      </section>
    </div>
  );
}
