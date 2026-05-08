using BasicCommerce.Application.Features.Labels;
using BasicCommerce.Application.Features.Labels.Commands;
using BasicCommerce.Application.Features.Labels.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/labels")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator) => _mediator = mediator;

    // ──────────────────────────────────────────────────────────
    // Templates
    // ──────────────────────────────────────────────────────────

    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponse<LabelTemplateListResponse>>> GetTemplates(
        [FromQuery] string? labelType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var type = Enum.TryParse<LabelType>(labelType, true, out var t) ? t : (LabelType?)null;
        var result = await _mediator.Send(
            new GetLabelTemplatesQuery(type, page, pageSize), ct);
        return Ok(ApiResponse<LabelTemplateListResponse>.Ok(result));
    }

    [HttpGet("templates/{id:guid}")]
    public async Task<ActionResult<ApiResponse<LabelTemplateResponse>>> GetTemplate(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLabelTemplateQuery(id), ct);
        return Ok(ApiResponse<LabelTemplateResponse>.Ok(result));
    }

    /// <summary>Returns the 6 built-in layout JSON samples for reference.</summary>
    [HttpGet("templates/builtins")]
    public ActionResult<ApiResponse<IReadOnlyList<BuiltinTemplateInfo>>> GetBuiltins()
    {
        var list = BuiltinLabelTemplates.All.Select(b =>
            new BuiltinTemplateInfo(b.Id, b.Name, b.LabelType,
                b.WidthMm, b.HeightMm,
                $"Pre-configured {b.Name} template ({b.WidthMm}×{b.HeightMm} mm)",
                b.LayoutJson))
            .ToList().AsReadOnly();
        return Ok(ApiResponse<IReadOnlyList<BuiltinTemplateInfo>>.Ok(list));
    }

    /// <summary>Seeds all built-in templates for this tenant (skips existing).</summary>
    [HttpPost("templates/import-builtins")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LabelTemplateResponse>>>> ImportBuiltins(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ImportBuiltinTemplatesCommand(), ct);
        return Ok(ApiResponse<IReadOnlyList<LabelTemplateResponse>>.Ok(result));
    }

    [HttpPost("templates")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<LabelTemplateResponse>>> CreateTemplate(
        [FromBody] CreateLabelTemplateRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<LabelType>(request.LabelType, true, out var labelType))
            return BadRequest(ApiResponse<LabelTemplateResponse>.Fail(
                $"Unknown label type '{request.LabelType}'."));

        if (!Enum.TryParse<BarcodeSymbology>(request.DefaultBarcodeSymbology, true, out var symb))
            return BadRequest(ApiResponse<LabelTemplateResponse>.Fail(
                $"Unknown barcode symbology '{request.DefaultBarcodeSymbology}'."));

        var result = await _mediator.Send(new CreateLabelTemplateCommand(
            request.Name, request.Description, labelType,
            request.WidthMm, request.HeightMm, request.LayoutJson, symb,
            request.PrinterDpi, request.IsDefault, request.SortOrder), ct);

        return CreatedAtAction(nameof(GetTemplate), new { id = result.Id },
            ApiResponse<LabelTemplateResponse>.Ok(result));
    }

    [HttpPut("templates/{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<LabelTemplateResponse>>> UpdateTemplate(
        Guid id, [FromBody] UpdateLabelTemplateRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<LabelType>(request.LabelType, true, out var labelType))
            return BadRequest(ApiResponse<LabelTemplateResponse>.Fail(
                $"Unknown label type '{request.LabelType}'."));

        if (!Enum.TryParse<BarcodeSymbology>(request.DefaultBarcodeSymbology, true, out var symb))
            return BadRequest(ApiResponse<LabelTemplateResponse>.Fail(
                $"Unknown barcode symbology '{request.DefaultBarcodeSymbology}'."));

        var result = await _mediator.Send(new UpdateLabelTemplateCommand(
            id, request.Name, request.Description, labelType,
            request.WidthMm, request.HeightMm, request.LayoutJson, symb,
            request.PrinterDpi, request.SortOrder), ct);

        return Ok(ApiResponse<LabelTemplateResponse>.Ok(result));
    }

    [HttpPost("templates/{id:guid}/set-default")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<LabelTemplateResponse>>> SetDefault(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetDefaultLabelTemplateCommand(id), ct);
        return Ok(ApiResponse<LabelTemplateResponse>.Ok(result));
    }

    [HttpDelete("templates/{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLabelTemplateCommand(id), ct);
        return NoContent();
    }

    // ──────────────────────────────────────────────────────────
    // Quick Render (no print job created)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Renders a single product on a template and returns the output directly.
    /// For ZPL, Content-Type is application/x-zebra-zpl.
    /// For HTML, returns text/html for browser printing.
    /// For JSON layout, returns application/json for client-side rendering.
    /// </summary>
    [HttpPost("templates/{templateId:guid}/render")]
    public async Task<IActionResult> RenderQuick(
        Guid templateId, [FromBody] RenderLabelRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<LabelOutputFormat>(request.OutputFormat, true, out var fmt))
            return BadRequest(ApiResponse<RenderLabelResponse>.Fail(
                $"Unknown output format '{request.OutputFormat}'."));

        var result = await _mediator.Send(new RenderSingleLabelQuery(
            templateId, request.ProductId,
            storeId: null,
            fmt, request.OverridePrice, request.CustomText,
            request.LotNumber, request.ExpiryDate, request.WeightKg,
            Math.Max(1, request.Copies)), ct);

        if (fmt == LabelOutputFormat.Html)
            return Content(result.Content, "text/html");
        if (fmt == LabelOutputFormat.Zpl)
            return Content(result.Content, "application/x-zebra-zpl");

        return Ok(ApiResponse<RenderLabelResponse>.Ok(result));
    }

    // ──────────────────────────────────────────────────────────
    // Print Jobs
    // ──────────────────────────────────────────────────────────

    [HttpGet("jobs")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobListResponse>>> GetJobs(
        [FromQuery] Guid? storeId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var statusEnum = Enum.TryParse<LabelPrintJobStatus>(status, true, out var s)
            ? s : (LabelPrintJobStatus?)null;
        var result = await _mediator.Send(
            new GetPrintJobsQuery(storeId, statusEnum, page, pageSize), ct);
        return Ok(ApiResponse<LabelPrintJobListResponse>.Ok(result));
    }

    [HttpGet("jobs/{id:guid}")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobResponse>>> GetJob(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPrintJobQuery(id), ct);
        return Ok(ApiResponse<LabelPrintJobResponse>.Ok(result));
    }

    [HttpPost("jobs")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobResponse>>> CreateJob(
        [FromBody] CreatePrintJobRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<LabelOutputFormat>(request.OutputFormat, true, out var fmt))
            return BadRequest(ApiResponse<LabelPrintJobResponse>.Fail(
                $"Unknown output format '{request.OutputFormat}'."));

        var result = await _mediator.Send(new CreatePrintJobCommand(
            request.StoreId, request.TemplateId, fmt,
            request.PrinterName, request.Notes), ct);

        return CreatedAtAction(nameof(GetJob), new { id = result.Id },
            ApiResponse<LabelPrintJobResponse>.Ok(result));
    }

    [HttpPost("jobs/{id:guid}/items")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobResponse>>> AddItem(
        Guid id, [FromBody] AddPrintJobItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddPrintJobItemCommand(
            id, request.ProductId, request.Quantity, request.OverridePrice,
            request.CustomText, request.LotNumber, request.ExpiryDate), ct);
        return Ok(ApiResponse<LabelPrintJobResponse>.Ok(result));
    }

    [HttpDelete("jobs/{id:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobResponse>>> RemoveItem(
        Guid id, Guid itemId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RemovePrintJobItemCommand(id, itemId), ct);
        return Ok(ApiResponse<LabelPrintJobResponse>.Ok(result));
    }

    [HttpPost("jobs/{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobResponse>>> CancelJob(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelPrintJobCommand(id), ct);
        return Ok(ApiResponse<LabelPrintJobResponse>.Ok(result));
    }

    /// <summary>
    /// Renders the full print job and marks it as Printing.
    /// Returns ZPL string, HTML, or JSON layout depending on job's output format.
    /// </summary>
    [HttpPost("jobs/{id:guid}/render")]
    public async Task<IActionResult> RenderJob(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RenderPrintJobQuery(id), ct);

        if (result.Format == "Html")
            return Content(result.Content, "text/html");
        if (result.Format == "Zpl")
            return Content(result.Content, "application/x-zebra-zpl");

        return Ok(ApiResponse<RenderLabelResponse>.Ok(result));
    }

    /// <summary>Marks a job as completed after the client confirms print success.</summary>
    [HttpPost("jobs/{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<LabelPrintJobResponse>>> CompleteJob(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CompletePrintJobCommand(id), ct);
        return Ok(ApiResponse<LabelPrintJobResponse>.Ok(result));
    }
}
