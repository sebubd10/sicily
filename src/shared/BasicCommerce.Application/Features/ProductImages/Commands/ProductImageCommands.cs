using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.ProductImages.Commands;

// ── Add by URL ──────────────────────────────────────────────────────────────

public record AddProductImageByUrlCommand(
    Guid ProductId, string Title, string Url,
    string? Description = null, int SortOrder = 0) : IRequest<ProductImageResponse>;

public class AddProductImageByUrlCommandValidator : AbstractValidator<AddProductImageByUrlCommand>
{
    public AddProductImageByUrlCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public class AddProductImageByUrlCommandHandler : IRequestHandler<AddProductImageByUrlCommand, ProductImageResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddProductImageByUrlCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductImageResponse> Handle(AddProductImageByUrlCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != tenantId)
            throw new NotFoundException("Product", request.ProductId);

        var image = ProductImage.CreateFromUrl(
            tenantId, request.ProductId, request.Title, request.Url,
            request.Description, request.SortOrder);

        await _uow.ProductImages.AddAsync(image, ct);
        await _uow.SaveChangesAsync(ct);

        return Mapper.ToResponse(image);
    }
}

// ── Add by Upload ────────────────────────────────────────────────────────────

public record AddProductImageByUploadCommand(
    Guid ProductId, string Title, Stream FileStream, string OriginalFileName,
    string? Description = null, int SortOrder = 0) : IRequest<ProductImageResponse>;

public class AddProductImageByUploadCommandValidator : AbstractValidator<AddProductImageByUploadCommand>
{
    private static readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public AddProductImageByUploadCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .Must(f => _allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .WithMessage("Only JPG, PNG, WEBP and GIF images are allowed.");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public class AddProductImageByUploadCommandHandler : IRequestHandler<AddProductImageByUploadCommand, ProductImageResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public AddProductImageByUploadCommandHandler(
        IUnitOfWork uow, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _uow = uow;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<ProductImageResponse> Handle(AddProductImageByUploadCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != tenantId)
            throw new NotFoundException("Product", request.ProductId);

        var (url, storedFileName) = await _fileStorage.UploadAsync(
            tenantId, request.ProductId, request.FileStream, request.OriginalFileName, ct);

        var image = ProductImage.CreateFromUpload(
            tenantId, request.ProductId, request.Title, url, storedFileName,
            request.Description, request.SortOrder);

        await _uow.ProductImages.AddAsync(image, ct);
        await _uow.SaveChangesAsync(ct);

        return Mapper.ToResponse(image);
    }
}

// ── Update ───────────────────────────────────────────────────────────────────

public record UpdateProductImageCommand(
    Guid ImageId, string Title, string? Description) : IRequest<ProductImageResponse>;

public class UpdateProductImageCommandValidator : AbstractValidator<UpdateProductImageCommand>
{
    public UpdateProductImageCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand, ProductImageResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductImageCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductImageResponse> Handle(UpdateProductImageCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var image = await _uow.ProductImages.GetByIdAsync(request.ImageId, ct)
            ?? throw new NotFoundException("ProductImage", request.ImageId);

        if (image.TenantId != tenantId)
            throw new NotFoundException("ProductImage", request.ImageId);

        image.Update(request.Title, request.Description);
        await _uow.SaveChangesAsync(ct);

        return Mapper.ToResponse(image);
    }
}

// ── Delete ───────────────────────────────────────────────────────────────────

public record DeleteProductImageCommand(Guid ImageId) : IRequest;

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public DeleteProductImageCommandHandler(
        IUnitOfWork uow, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _uow = uow;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteProductImageCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var image = await _uow.ProductImages.GetByIdAsync(request.ImageId, ct)
            ?? throw new NotFoundException("ProductImage", request.ImageId);

        if (image.TenantId != tenantId)
            throw new NotFoundException("ProductImage", request.ImageId);

        if (image.IsUploaded && image.StoredFileName is not null)
            await _fileStorage.DeleteAsync(image.StoredFileName, ct);

        image.MarkDeleted();
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Reorder ───────────────────────────────────────────────────────────────────

public record ReorderProductImagesCommand(
    Guid ProductId, IEnumerable<(Guid Id, int SortOrder)> Items) : IRequest;

public class ReorderProductImagesCommandHandler : IRequestHandler<ReorderProductImagesCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReorderProductImagesCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderProductImagesCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var images = (await _uow.ProductImages.GetByProductAsync(tenantId, request.ProductId, ct))
            .ToDictionary(i => i.Id);

        foreach (var (id, sortOrder) in request.Items)
        {
            if (images.TryGetValue(id, out var image))
                image.SetSortOrder(sortOrder);
        }

        await _uow.SaveChangesAsync(ct);
    }
}

// ── Shared mapper ────────────────────────────────────────────────────────────

file static class Mapper
{
    internal static ProductImageResponse ToResponse(ProductImage img) =>
        new(img.Id, img.ProductId, img.Title, img.Description, img.Url,
            img.IsUploaded, img.SortOrder, img.CreatedAt, img.UpdatedAt);
}
