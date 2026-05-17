using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Application.Interfaces;

public record LabelRenderContext(
    Product Product,
    decimal Price,
    decimal? OriginalPrice,
    Store? Store,
    string? LotNumber,
    DateTime? ExpiryDate,
    string? CustomText,
    decimal? WeightKg = null);

public record LabelRenderResult(
    string Content,
    LabelOutputFormat Format,
    string MimeType,
    int LabelCount);

public interface ILabelRenderService
{
    /// <summary>Renders a single label for one product.</summary>
    LabelRenderResult RenderSingle(
        LabelTemplate template, LabelRenderContext context, LabelOutputFormat format);

    /// <summary>Renders all items in a print job (concatenated ZPL or JSON array).</summary>
    LabelRenderResult RenderJob(
        LabelTemplate template,
        IEnumerable<(LabelRenderContext Context, int Quantity)> items,
        LabelOutputFormat format);
}
