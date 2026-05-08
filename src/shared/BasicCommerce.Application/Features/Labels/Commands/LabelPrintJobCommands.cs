using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Labels.Commands;

// ── Create print job ───────────────────────────────────────────
public record CreatePrintJobCommand(
    Guid StoreId,
    Guid TemplateId,
    LabelOutputFormat OutputFormat,
    string? PrinterName,
    string? Notes) : IRequest<LabelPrintJobResponse>;

public class CreatePrintJobCommandValidator : AbstractValidator<CreatePrintJobCommand>
{
    public CreatePrintJobCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
    }
}

public class CreatePrintJobCommandHandler
    : IRequestHandler<CreatePrintJobCommand, LabelPrintJobResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePrintJobCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobResponse> Handle(
        CreatePrintJobCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, request.TemplateId, ct)
            ?? throw new NotFoundException("LabelTemplate", request.TemplateId);

        var job = LabelPrintJob.Create(tenantId, request.StoreId, request.TemplateId,
            request.OutputFormat, _currentUser.UserId, request.PrinterName, request.Notes);

        await _uow.LabelPrintJobs.AddAsync(job, ct);
        await _uow.SaveChangesAsync(ct);

        return LabelMapper.ToResponse(job, template.Name, []);
    }
}

// ── Add item(s) to print job ───────────────────────────────────
public record AddPrintJobItemCommand(
    Guid JobId,
    Guid ProductId,
    int Quantity,
    decimal? OverridePrice,
    string? CustomText,
    string? LotNumber,
    DateTime? ExpiryDate) : IRequest<LabelPrintJobResponse>;

public class AddPrintJobItemCommandValidator : AbstractValidator<AddPrintJobItemCommand>
{
    public AddPrintJobItemCommandValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.OverridePrice).GreaterThan(0).When(x => x.OverridePrice.HasValue);
    }
}

public class AddPrintJobItemCommandHandler
    : IRequestHandler<AddPrintJobItemCommand, LabelPrintJobResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddPrintJobItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobResponse> Handle(
        AddPrintJobItemCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var job = await _uow.LabelPrintJobs.GetWithItemsAsync(tenantId, request.JobId, ct)
            ?? throw new NotFoundException("LabelPrintJob", request.JobId);

        var product = await _uow.Products.GetByIdForTenantAsync(
            tenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        job.AddItem(request.ProductId, request.Quantity, request.OverridePrice,
            request.CustomText, request.LotNumber, request.ExpiryDate);

        _uow.LabelPrintJobs.Update(job);
        await _uow.SaveChangesAsync(ct);

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, job.TemplateId, ct);

        return await BuildResponseAsync(job, template?.Name ?? "Unknown", tenantId, ct);
    }

    private async Task<LabelPrintJobResponse> BuildResponseAsync(
        LabelPrintJob job, string templateName, Guid tenantId, CancellationToken ct)
    {
        var pairs = new List<(LabelPrintJobItem, Product)>();
        foreach (var item in job.Items)
        {
            var p = await _uow.Products.GetByIdForTenantAsync(tenantId, item.ProductId, ct);
            if (p is not null) pairs.Add((item, p));
        }
        return LabelMapper.ToResponse(job, templateName, pairs);
    }
}

// ── Remove item from print job ─────────────────────────────────
public record RemovePrintJobItemCommand(Guid JobId, Guid ItemId)
    : IRequest<LabelPrintJobResponse>;

public class RemovePrintJobItemCommandHandler
    : IRequestHandler<RemovePrintJobItemCommand, LabelPrintJobResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemovePrintJobItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobResponse> Handle(
        RemovePrintJobItemCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var job = await _uow.LabelPrintJobs.GetWithItemsAsync(tenantId, request.JobId, ct)
            ?? throw new NotFoundException("LabelPrintJob", request.JobId);

        job.RemoveItem(request.ItemId);
        _uow.LabelPrintJobs.Update(job);
        await _uow.SaveChangesAsync(ct);

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, job.TemplateId, ct);

        return LabelMapper.ToResponse(job, template?.Name ?? "Unknown", []);
    }
}

// ── Cancel print job ───────────────────────────────────────────
public record CancelPrintJobCommand(Guid JobId) : IRequest<LabelPrintJobResponse>;

public class CancelPrintJobCommandHandler
    : IRequestHandler<CancelPrintJobCommand, LabelPrintJobResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelPrintJobCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobResponse> Handle(
        CancelPrintJobCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var job = await _uow.LabelPrintJobs.GetWithItemsAsync(tenantId, request.JobId, ct)
            ?? throw new NotFoundException("LabelPrintJob", request.JobId);

        job.Cancel();
        _uow.LabelPrintJobs.Update(job);
        await _uow.SaveChangesAsync(ct);

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, job.TemplateId, ct);

        return LabelMapper.ToResponse(job, template?.Name ?? "Unknown", []);
    }
}
