using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.Persistence;

/// <summary>
/// The canonical seed values for every reference list, taken from the source screenshots.
/// Accounts start empty; only these reference tables are pre-populated. Admins can extend
/// or edit them from the admin section afterwards.
/// </summary>
public static class ReferenceData
{
    public static readonly IReadOnlyDictionary<ReferenceType, string[]> Items = new Dictionary<ReferenceType, string[]>
    {
        [ReferenceType.Location] = new[]
        {
            "Romania", "Moldova", "Poland", "EMEA (Management and Specialists)", "Belgium", "Portugal",
            "Morroco", "South Africa", "Brasil", "Argentina", "Southfield/US", "Philippines/Manila",
            "China/Jilin", "China/Dalian", "Mexico", "Other"
        },
        [ReferenceType.StaffingRole] = new[]
        {
            "Support Engineers", "TL", "SDM", "Operation Specialists", "Service Manager", "AL", "IMS", "QS",
            "KE", "Technical Training Specialist", "ForS", "RTS", "EUX", "Problem Analyst",
            "Cognitive Data Specialist", "PPM Specialist (Proactive problem)", "Reporting Specialist",
            "Change Coordinator", "Change Manager", "TSM", "Business Analyst", "Transition Manager", "DAM / CRD"
        },
        [ReferenceType.ContractualLanguage] = new[]
        {
            "English", "French", "German", "Polish", "Spanish", "Italian", "Portughese", "Dutch", "Hungarian",
            "Greek", "Turkish", "Russian", "Romanian", "Bulgarian", "Croatian", "Lithuanian", "Macedonian",
            "Serbian", "Czech", "Slovak", "Slovenian", "Norwegian", "Danish", "Swedish", "Finish", "Hebrew",
            "Ukrainian", "Hindi", "Cantonese", "Japanese", "Mandarin", "Korean", "Kazakh", "Tagalog", "Thai"
        },
        [ReferenceType.SupportHoursLanguage] = new[]
        {
            "EMEA English", "NA English", "EMEA French", "NA French", "German", "Polish", "EMEA Spanish",
            "LATAM Spanish", "Italian", "EMEA Portughese", "LATAM Portughese", "Dutch", "Hungarian", "Greek",
            "Turkish", "Russian", "Romanian", "Bulgarian", "Croatian", "Lithuanian", "Macedonian", "Serbian",
            "Czech", "Slovak", "Slovenian", "Norwegian", "Danish", "Swedish", "Finish", "Hebrew", "Ukrainian",
            "Hindi", "Cantonese", "Japanese", "Mandarin", "Korean", "Arabic", "Kazakh", "Tagalog", "Thai", "Urdu"
        },
        [ReferenceType.Device] = new[]
        {
            "Laptops", "MAC Books", "Desktops", "Engineering Workstations", "Tablets", "Smart Phone – Android",
            "Smart Phone – Apple", "Mobile Phones", "Printers & Plotters", "Scanners & Hand Scanners",
            "Label Printers", "Wifi/Router", "DHS – Health related devices", "Servers", "Switch", "Storage device"
        },
        [ReferenceType.ServiceTower] = new[]
        {
            "SD", "RTS", "LTS", "LTS -Staffing", "DHS", "DWP &UC", "MEM", "ITAM", "HCI &NW", "CSS", "SAP",
            "TSMO", "AMS", "SMP"
        },
        [ReferenceType.OpportunityType] = new[]
        {
            "New service", "Additional scope for existing service", "Project based work"
        },
        [ReferenceType.OpportunityStatus] = new[]
        {
            "Opportunity identified", "Proposal Delivered", "Ok from Client", "Contract Signed", "Implemented",
            "On hold", "Cancelled", "Lost", "Abandoned"
        },
        [ReferenceType.RelatedService] = new[]
        {
            "Sophie", "Preventive Device Management", "SAP Cloud Platforms", "License Resell",
            "IT Asset Management", "Data Analytics", "Service Desk", "Programme or Project Mgmt",
            "Local Technical Support"
        },
        [ReferenceType.SlaType] = new[] { "SLA", "KPI", "HCM" },
        [ReferenceType.MeasurementType] = new[] { "High", "Low" },
        [ReferenceType.AccountType] = new[] { "ITO", "ITO (SOS)", "DHS", "BPO" },
        [ReferenceType.Connectivity] = new[] { "MPLS", "VPN Tunnel", "Stefanini VDI", "Customer VDI" },
        [ReferenceType.Telecom] = new[] { "EMEA", "US", "Client" },
        [ReferenceType.ManagedBy] = new[] { "Stefanini", "Client", "3rd party" },
        [ReferenceType.ItsmTool] = new[]
        {
            "SNOW", "4me", "BMC Remedy", "TopDesk", "HPSM", "Jira", "HEAT", "Salesforce", "ControlSeries",
            "Zendesk", "Ivanti", "ServiceDesk Plus", "Matrix 42", "ServiceAide", "Cherwell", "SMAX", "SysAid",
            "OTRS", "FreshService"
        },
        [ReferenceType.ContractType] = new[] { "Managed Services", "Staffing", "Managed&Staffing" },
        [ReferenceType.BillingModel] = new[]
        {
            "Price per tickets", "Price per users", "Price per workstation", "Price per tickets per users ratio (2D)",
            "Price per tickets per user per FLR ratio", "Other (details please)", "Price per FTE",
            "Monthly fixed price", "Price per intervention", "Price per study", "Price per CI (Configuration Item)"
        },
        [ReferenceType.TechnologySupported] = new[]
        {
            "IBM", "Toshiba", "HP", "Apple", "Lenovo", "Epson", "Canon", "Fujitsu", "Brother", "Dell", "Zebra",
            "Ricoh", "Xerox", "Kyocera", "Microsoft", "Konica Minolta", "Samsung"
        },
        [ReferenceType.Industry] = new[]
        {
            "Fishing industry", "Horticulture industry", "Tobacco industry", "Wood industry", "Aerospace industry",
            "Automotive industry", "Chemical industry-other", "Chemical industry-Pharmaceutical industry",
            "Construction/Engineering industry", "Defense industry", "Environmental", "Electric power industry",
            "Electronics industry-other", "Electronics industry-Computer industry",
            "Electronics industry-Semiconductor industry", "Energy industry", "Food industry",
            "Industrial robot industry", "Low technology industry", "Meat packing", "Mining", "Petroleum industry",
            "Pulp and paper industry", "Steel industry"
        },
        [ReferenceType.Country] = Countries
    };

    /// <summary>Public ISO-3166 country list (UN members + commonly used territories).</summary>
    public static string[] Countries => new[]
    {
        "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina", "Armenia",
        "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium",
        "Belize", "Benin", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei",
        "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia", "Cameroon", "Canada",
        "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros", "Congo (Brazzaville)",
        "Congo (Kinshasa)", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czechia", "Denmark", "Djibouti",
        "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea",
        "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland", "France", "Gabon", "Gambia", "Georgia", "Germany",
        "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Honduras",
        "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Israel", "Italy",
        "Ivory Coast", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kosovo", "Kuwait",
        "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania",
        "Luxembourg", "Macao", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands",
        "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco",
        "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand", "Nicaragua", "Niger",
        "Nigeria", "North Korea", "North Macedonia", "Norway", "Oman", "Pakistan", "Palau", "Palestine", "Panama",
        "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar", "Romania", "Russia",
        "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa", "San Marino",
        "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore",
        "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Korea", "South Sudan", "Spain",
        "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Taiwan", "Tajikistan", "Tanzania",
        "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan",
        "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States", "Uruguay",
        "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe"
    };
}
