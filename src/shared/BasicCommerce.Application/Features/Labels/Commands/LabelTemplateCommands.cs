using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Labels.Commands;

// ── Create template ────────────────────────────────────────────
public record CreateLabelTemplateCommand(
    string Name,
    string? Description,
    LabelType LabelType,
    decimal WidthMm,
    decimal HeightMm,
    string LayoutJson,
    BarcodeSymbology DefaultBarcodeSymbology,
    int PrinterDpi,
    bool IsDefault,
    int SortOrder) : IRequest<LabelTemplateResponse>;

public class CreateLabelTemplateCommandValidator
    : AbstractValidator<CreateLabelTemplateCommand>
{
    public CreateLabelTemplateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WidthMm).InclusiveBetween(1, 300);
        RuleFor(x => x.HeightMm).InclusiveBetween(1, 600);
        RuleFor(x => x.LayoutJson).NotEmpty();
        RuleFor(x => x.PrinterDpi).Must(d => d is 203 or 300)
            .WithMessage("PrinterDpi must be 203 or 300.");
    }
}

public class CreateLabelTemplateCommandHandler
    : IRequestHandler<CreateLabelTemplateCommand, LabelTemplateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateLabelTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelTemplateResponse> Handle(
        CreateLabelTemplateCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (request.IsDefault)
            await _uow.LabelTemplates.ClearDefaultForTypeAsync(
                tenantId, request.LabelType, ct);

        var template = LabelTemplate.Create(tenantId, request.Name, request.Description,
            request.LabelType, request.WidthMm, request.HeightMm,
            request.LayoutJson, request.DefaultBarcodeSymbology,
            request.PrinterDpi, request.IsDefault, request.SortOrder);

        await _uow.LabelTemplates.AddAsync(template, ct);
        await _uow.SaveChangesAsync(ct);
        return LabelMapper.ToResponse(template);
    }
}

// ── Update template ────────────────────────────────────────────
public record UpdateLabelTemplateCommand(
    Guid Id,
    string Name,
    string? Description,
    LabelType LabelType,
    decimal WidthMm,
    decimal HeightMm,
    string LayoutJson,
    BarcodeSymbology DefaultBarcodeSymbology,
    int PrinterDpi,
    int SortOrder) : IRequest<LabelTemplateResponse>;

public class UpdateLabelTemplateCommandHandler
    : IRequestHandler<UpdateLabelTemplateCommand, LabelTemplateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateLabelTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelTemplateResponse> Handle(
        UpdateLabelTemplateCommand request, CancellationToken ct)
    {
        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("LabelTemplate", request.Id);

        template.Update(request.Name, request.Description, request.LabelType,
            request.WidthMm, request.HeightMm, request.LayoutJson,
            request.DefaultBarcodeSymbology, request.PrinterDpi, request.SortOrder);

        _uow.LabelTemplates.Update(template);
        await _uow.SaveChangesAsync(ct);
        return LabelMapper.ToResponse(template);
    }
}

// ── Set default template ───────────────────────────────────────
public record SetDefaultLabelTemplateCommand(Guid Id) : IRequest<LabelTemplateResponse>;

public class SetDefaultLabelTemplateCommandHandler
    : IRequestHandler<SetDefaultLabelTemplateCommand, LabelTemplateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetDefaultLabelTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelTemplateResponse> Handle(
        SetDefaultLabelTemplateCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("LabelTemplate", request.Id);

        await _uow.LabelTemplates.ClearDefaultForTypeAsync(tenantId, template.LabelType, ct);
        template.SetAsDefault();
        _uow.LabelTemplates.Update(template);
        await _uow.SaveChangesAsync(ct);
        return LabelMapper.ToResponse(template);
    }
}

// ── Delete template ────────────────────────────────────────────
public record DeleteLabelTemplateCommand(Guid Id) : IRequest;

public class DeleteLabelTemplateCommandHandler
    : IRequestHandler<DeleteLabelTemplateCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteLabelTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteLabelTemplateCommand request, CancellationToken ct)
    {
        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("LabelTemplate", request.Id);

        template.SoftDelete(_currentUser.UserId);
        _uow.LabelTemplates.Update(template);
        await _uow.SaveChangesAsync(ct);
    }
}
