using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Labels.Commands;

/// <summary>
/// Seeds all built-in label templates for the current tenant.
/// Skips templates that already exist (matched by name).
/// </summary>
public record ImportBuiltinTemplatesCommand : IRequest<IReadOnlyList<LabelTemplateResponse>>;

public class ImportBuiltinTemplatesCommandHandler
    : IRequestHandler<ImportBuiltinTemplatesCommand, IReadOnlyList<LabelTemplateResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ImportBuiltinTemplatesCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<IReadOnlyList<LabelTemplateResponse>> Handle(
        ImportBuiltinTemplatesCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var existing = await _uow.LabelTemplates.GetAllForTenantAsync(tenantId, ct);
        var existingNames = existing.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var results = new List<LabelTemplateResponse>();
        var sort = 0;

        foreach (var bt in BuiltinLabelTemplates.All)
        {
            if (existingNames.Contains(bt.Name)) continue;

            if (!Enum.TryParse<LabelType>(bt.LabelType, out var labelType)) continue;

            var symb = bt.LabelType switch
            {
                "WeightPriceLabel" or "ReceivingLabel" => BarcodeSymbology.Code128,
                _ => BarcodeSymbology.EAN13
            };

            var template = LabelTemplate.Create(tenantId, bt.Name, null,
                labelType, bt.WidthMm, bt.HeightMm, bt.LayoutJson, symb, 203, false, sort++);

            await _uow.LabelTemplates.AddAsync(template, ct);
            results.Add(LabelMapper.ToResponse(template));
        }

        if (results.Any())
            await _uow.SaveChangesAsync(ct);

        return results.AsReadOnly();
    }
}
