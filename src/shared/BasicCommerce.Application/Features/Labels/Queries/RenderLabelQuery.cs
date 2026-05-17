using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Labels.Queries;

/// <summary>Renders a single product on a given template immediately (no job created).</summary>
public record RenderSingleLabelQuery(
    Guid TemplateId,
    Guid ProductId,
    Guid? StoreId,
    LabelOutputFormat OutputFormat,
    decimal? OverridePrice,
    string? CustomText,
    string? LotNumber,
    DateTime? ExpiryDate,
    decimal? WeightKg,
    int Copies) : IRequest<RenderLabelResponse>;

public class RenderSingleLabelQueryHandler
    : IRequestHandler<RenderSingleLabelQuery, RenderLabelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILabelRenderService _renderer;

    public RenderSingleLabelQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser,
        ILabelRenderService renderer)
    { _uow = uow; _currentUser = currentUser; _renderer = renderer; }

    public async Task<RenderLabelResponse> Handle(
        RenderSingleLabelQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, request.TemplateId, ct)
            ?? throw new NotFoundException("LabelTemplate", request.TemplateId);

        var product = await _uow.Products.GetByIdForTenantAsync(
            tenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        Domain.Entities.Store? store = null;
        if (request.StoreId.HasValue)
            store = await _uow.Stores.GetByIdForTenantAsync(tenantId, request.StoreId.Value, ct);

        var ctx = new LabelRenderContext(
            Product: product,
            Price: request.OverridePrice ?? product.Price.Amount,
            OriginalPrice: request.OverridePrice.HasValue ? product.Price.Amount : null,
            Store: store,
            LotNumber: request.LotNumber,
            ExpiryDate: request.ExpiryDate,
            CustomText: request.CustomText,
            WeightKg: request.WeightKg);

        var result = _renderer.RenderJob(template,
            [(ctx, Math.Max(1, request.Copies))], request.OutputFormat);

        return new RenderLabelResponse(result.Content, result.Format.ToString(),
            result.MimeType, result.LabelCount);
    }
}

/// <summary>Renders all labels for an existing print job and marks it as Printing.</summary>
public record RenderPrintJobQuery(Guid JobId) : IRequest<RenderLabelResponse>;

public class RenderPrintJobQueryHandler
    : IRequestHandler<RenderPrintJobQuery, RenderLabelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILabelRenderService _renderer;

    public RenderPrintJobQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser,
        ILabelRenderService renderer)
    { _uow = uow; _currentUser = currentUser; _renderer = renderer; }

    public async Task<RenderLabelResponse> Handle(
        RenderPrintJobQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var job = await _uow.LabelPrintJobs.GetWithItemsAsync(tenantId, request.JobId, ct)
            ?? throw new NotFoundException("LabelPrintJob", request.JobId);

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, job.TemplateId, ct)
            ?? throw new NotFoundException("LabelTemplate", job.TemplateId);

        Domain.Entities.Store? store = await _uow.Stores.GetByIdForTenantAsync(
            tenantId, job.StoreId, ct);

        var renderItems = new List<(LabelRenderContext, int)>();

        foreach (var item in job.Items)
        {
            var product = await _uow.Products.GetByIdForTenantAsync(
                tenantId, item.ProductId, ct);
            if (product is null) continue;

            var ctx = new LabelRenderContext(
                Product: product,
                Price: item.OverridePrice ?? product.Price.Amount,
                OriginalPrice: item.OverridePrice.HasValue ? product.Price.Amount : null,
                Store: store,
                LotNumber: item.LotNumber,
                ExpiryDate: item.ExpiryDate,
                CustomText: item.CustomText);

            renderItems.Add((ctx, item.Quantity));
        }

        // Mark job as printing
        job.MarkPrinting(_currentUser.UserId);
        _uow.LabelPrintJobs.Update(job);
        await _uow.SaveChangesAsync(ct);

        var result = _renderer.RenderJob(template, renderItems, job.OutputFormat);

        return new RenderLabelResponse(result.Content, result.Format.ToString(),
            result.MimeType, result.LabelCount);
    }
}

/// <summary>Marks a job as completed after the client has confirmed printing.</summary>
public record CompletePrintJobCommand(Guid JobId) : IRequest<LabelPrintJobResponse>;

public class CompletePrintJobCommandHandler
    : IRequestHandler<CompletePrintJobCommand, LabelPrintJobResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CompletePrintJobCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobResponse> Handle(
        CompletePrintJobCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var job = await _uow.LabelPrintJobs.GetWithItemsAsync(tenantId, request.JobId, ct)
            ?? throw new NotFoundException("LabelPrintJob", request.JobId);

        job.MarkCompleted();
        _uow.LabelPrintJobs.Update(job);
        await _uow.SaveChangesAsync(ct);

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, job.TemplateId, ct);
        return LabelMapper.ToResponse(job, template?.Name ?? "Unknown", []);
    }
}
