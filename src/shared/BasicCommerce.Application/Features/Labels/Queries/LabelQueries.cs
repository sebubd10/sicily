using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Labels.Queries;

// ── Get template by ID ─────────────────────────────────────────
public record GetLabelTemplateQuery(Guid Id) : IRequest<LabelTemplateResponse>;

public class GetLabelTemplateQueryHandler
    : IRequestHandler<GetLabelTemplateQuery, LabelTemplateResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetLabelTemplateQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelTemplateResponse> Handle(
        GetLabelTemplateQuery request, CancellationToken ct)
    {
        var t = await _uow.LabelTemplates.GetByIdForTenantAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("LabelTemplate", request.Id);
        return LabelMapper.ToResponse(t);
    }
}

// ── List templates ─────────────────────────────────────────────
public record GetLabelTemplatesQuery(LabelType? LabelType, int Page, int PageSize)
    : IRequest<LabelTemplateListResponse>;

public class GetLabelTemplatesQueryHandler
    : IRequestHandler<GetLabelTemplatesQuery, LabelTemplateListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetLabelTemplatesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelTemplateListResponse> Handle(
        GetLabelTemplatesQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var items = await _uow.LabelTemplates.GetPagedAsync(
            tenantId, request.LabelType, request.Page, request.PageSize, ct);
        var total = await _uow.LabelTemplates.GetTotalCountAsync(
            tenantId, request.LabelType, ct);
        return new LabelTemplateListResponse(
            items.Select(LabelMapper.ToSummary).ToList().AsReadOnly(),
            total, request.Page, request.PageSize);
    }
}

// ── Get print job ──────────────────────────────────────────────
public record GetPrintJobQuery(Guid Id) : IRequest<LabelPrintJobResponse>;

public class GetPrintJobQueryHandler : IRequestHandler<GetPrintJobQuery, LabelPrintJobResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetPrintJobQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobResponse> Handle(
        GetPrintJobQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var job = await _uow.LabelPrintJobs.GetWithItemsAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("LabelPrintJob", request.Id);

        var template = await _uow.LabelTemplates.GetByIdForTenantAsync(
            tenantId, job.TemplateId, ct);

        var pairs = new List<(Domain.Entities.LabelPrintJobItem, Domain.Entities.Product)>();
        foreach (var item in job.Items)
        {
            var p = await _uow.Products.GetByIdForTenantAsync(tenantId, item.ProductId, ct);
            if (p is not null) pairs.Add((item, p));
        }

        return LabelMapper.ToResponse(job, template?.Name ?? "Unknown", pairs);
    }
}

// ── List print jobs ────────────────────────────────────────────
public record GetPrintJobsQuery(Guid? StoreId, LabelPrintJobStatus? Status,
    int Page, int PageSize) : IRequest<LabelPrintJobListResponse>;

public class GetPrintJobsQueryHandler
    : IRequestHandler<GetPrintJobsQuery, LabelPrintJobListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetPrintJobsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<LabelPrintJobListResponse> Handle(
        GetPrintJobsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var jobs = await _uow.LabelPrintJobs.GetPagedAsync(
            tenantId, request.StoreId, request.Status, request.Page, request.PageSize, ct);
        var total = await _uow.LabelPrintJobs.GetTotalCountAsync(
            tenantId, request.StoreId, request.Status, ct);

        var summaries = new List<LabelPrintJobSummaryResponse>();
        foreach (var job in jobs)
        {
            var t = await _uow.LabelTemplates.GetByIdForTenantAsync(
                tenantId, job.TemplateId, ct);
            summaries.Add(LabelMapper.ToSummary(job, t?.Name ?? "Unknown"));
        }

        return new LabelPrintJobListResponse(
            summaries.AsReadOnly(), total, request.Page, request.PageSize);
    }
}
