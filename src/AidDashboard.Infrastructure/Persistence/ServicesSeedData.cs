namespace AidDashboard.Infrastructure.Persistence;

/// <summary>A node in the seed services hierarchy (a leaf with no children is a service).</summary>
public sealed record ServiceSeedNode(string Name, IReadOnlyList<ServiceSeedNode> Children);

/// <summary>
/// The canonical Services hierarchy used to seed the catalog (9 categories, 113 services).
/// A node with children is a (sub)category; a node without children is a leaf service.
/// Spellings are preserved exactly as in the source; admins can correct them later.
/// </summary>
public static class ServicesSeedData
{
    // Short helpers: C = category (has children), S = service (leaf).
    private static ServiceSeedNode C(string name, params ServiceSeedNode[] children) => new(name, children);
    private static ServiceSeedNode S(string name) => new(name, Array.Empty<ServiceSeedNode>());

    public static IReadOnlyList<ServiceSeedNode> Categories { get; } = new[]
    {
        C("End-User Proximity Services",
            S("Service Desk"), S("VIP@Home Service"), S("Remote Technical Support"), S("Near-Touch Support"),
            S("Digital Café"), S("Virtual Digital Café"), S("Depot HUB"), S("Lite Service Desk"),
            S("Local Technical Support")),

        C("Enterprise Solutions & Service",
            S("Device(Hardware) as a Service"), S("Preventive Device Management"), S("Preventive Application Management"),
            S("Identity and Access Management"), S("Monitoring & Event management"),
            C("Modern Endpoint Management",
                S("Application Packaging and Deployment"), S("Configuration Policies Management"),
                S("Operating System Deployment / Provisioning"), S("Updates and Upgrades Management"),
                S("Compliance and Conditional Access Management"), S("Infrastructure / Client Health"),
                S("MAC OS / JAMF - Device Management"), S("Mobile Device Management and Support"),
                S("Azure Virtual Desktop - Platform Management and Support"),
                S("UniversalPrintManagementandSupport"), S("Professional Services")),
            C("Digital Workplace Platforms",
                S("M365 Platform Management & Support"), S("On-prem deployment lifecycle"),
                S("M365 Integrations & Automation"), S("Messaging Solutions Management and Support"),
                S("Business Application Support"), S("Advanced Automations"), S("Professional Services")),
            C("Unified Communications",
                S("Enterprise Voice Services"), S("Videoconferencing Services"), S("Edge Voice Services")),
            C("ITSM Platform & Consultancy",
                S("Platform / Architecture"), S("License Resell"), S("ITIL Consultancy / Training"),
                C("Support",
                    S("Platform Maintenance"), S("Minor Enhancements"), S("Report Design"),
                    S("Patching and License Management")),
                C("Adapt",
                    S("Enchancement Development"), S("Code Review and testing"), S("Platform Upgrade"),
                    S("SCRUM Methodology")),
                C("Evolve",
                    S("Programme or Project Mgmt"), S("Product / Application Implementation"), S("Integrations"),
                    S("Custom Application Development")),
                C("Consultancy",
                    S("Best Practices Methdology"), S("Architectural Guidance"), S("Presales and License Consultancy"),
                    S("ITIL Consultancy / Training")))),

        C("ITSM & SIAM",
            S("Service Delivery Management as a Service"), S("SIAM as a Service"), S("ITSM & SIAM Advisory Services"),
            S("Process Design & Optimization"), S("Major Incident Management"), S("E2E Incident Management"),
            S("Proactive Problem Management"), S("Knowledge Management"), S("Change Management"),
            S("IT Asset Management"), S("Supplier Management"), S("Continous Service Improvement")),

        C("Enablers & Service Accelerators",
            S("Voice of IT"), S("EUX"), S("Cognitive Analysis"), S("End-User Training"), S("Data Analytics"),
            S("Transformation Management"), S("Engagement Experience"), S("ITQX"), S("Chameneleon"), S("eContact"),
            S("iForgot"), S("Ticket Power-up"), S("DeskGuru"), S("IQ Mangement"), S("Sophie")),

        C("Digital Healthcare Services",
            S("Clinical Trials"), S("Patient Retention"), S("Medical Helpdesk"), S("CRO Field Force IT Support"),
            S("Medical Device as a Service")),

        C("CSS",
            S("Security Monitoring (NSOC)"), S("Detection and Response (MDR)"), S("Vulnerability Scanning"),
            S("Threat Inteligence"), S("Identity and Priviledged Access management"),
            S("Security Platform Support & Management"),
            S("Proactive Services (Ethical Hacking, Pen Test, Threat hunting)"), S("Reactive Service - CSIRT"),
            S("Professional Services")),

        C("Network Services",
            S("Remote Network Monitoring & Reporting (NSOC)"), S("Network Security"), S("Enterprise WAN/LAN"),
            S("SD-WAN Services"), S("Data Center Network Services"), S("Cloud Network Services"),
            S("Hardware as a Service"), S("Professional Services")),

        C("Hybrid Cloud Infrastructure",
            S("Digital Cloud Services"), S("Data Fabric Services"), S("Computer Services"),
            S("Operating Systems & Databases"), S("Enterprise Operating Center"), S("SAP Migration & Transformation"),
            S("SAP Cloud Platforms"), S("S/4 Hana Conversion & Operations"), S("Professional Services")),

        C("Operations",
            S("Business Process Outsourcing")),
    };
}
