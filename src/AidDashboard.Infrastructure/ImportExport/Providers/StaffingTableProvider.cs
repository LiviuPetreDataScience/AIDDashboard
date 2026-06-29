using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>
/// Import/export mapping for a staffing model. Two instances are registered, one for the
/// Initial model and one for the Latest Approved model, distinguished by tab key.
/// </summary>
public class StaffingTableProvider : ITabTableProvider
{
    private readonly IStaffingService _staffingService;
    private readonly IReferenceService _referenceService;
    private readonly StaffingModelType _modelType;

    public StaffingTableProvider(IStaffingService staffingService, IReferenceService referenceService, StaffingModelType modelType)
    {
        _staffingService = staffingService;
        _referenceService = referenceService;
        _modelType = modelType;
    }

    public string TabKey => _modelType == StaffingModelType.Initial ? "initial-staffing" : "latest-approved";
    public string DisplayTitle => _modelType == StaffingModelType.Initial ? "Initial Staffing Model" : "Latest Approved Model";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var locations = await _referenceService.GetByTypeAsync(ReferenceType.Location, false, cancellationToken);
        var roles = await _referenceService.GetByTypeAsync(ReferenceType.StaffingRole, false, cancellationToken);
        var cells = await _staffingService.GetAsync(accountId, _modelType, cancellationToken);

        var matrixCells = cells
            .Select(cell => new MatrixCellValue(cell.LocationRefId, cell.RoleRefId, cell.Value))
            .ToList();

        return MatrixTableHelper.Export("Location", locations, roles, matrixCells);
    }

    public async Task<TableData> TemplateAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _referenceService.GetByTypeAsync(ReferenceType.StaffingRole, false, cancellationToken);
        return MatrixTableHelper.Template("Location", roles);
    }

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var locationLookup = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Location, true, cancellationToken));
        var roleLookup = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.StaffingRole, true, cancellationToken));

        var cells = MatrixTableHelper.Import(data, locationLookup, roleLookup)
            .Select(cell => new StaffingCellDto(cell.RowRefId, cell.ColRefId, cell.Value))
            .ToList();

        await _staffingService.SaveAsync(accountId, _modelType, cells, cancellationToken);
    }
}
