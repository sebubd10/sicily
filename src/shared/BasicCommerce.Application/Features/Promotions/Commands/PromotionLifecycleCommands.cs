using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Promotions;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Promotions.Commands;

public record ActivatePromotionCommand(Guid Id) : IRequest<PromotionResponse>;
public record PausePromotionCommand(Guid Id) : IRequest<PromotionResponse>;
public record CancelPromotionCommand(Guid Id) : IRequest<PromotionResponse>;

public class ActivatePromotionCommandHandler
    : IRequestHandler<ActivatePromotionCommand, PromotionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public ActivatePromotionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<PromotionResponse> Handle(ActivatePromotionCommand request, CancellationToken ct)
    {
        var p = await _uow.Promotions.GetByIdForTenantAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("Promotion", request.Id);
        p.Activate();
        _uow.Promotions.Update(p);
        await _uow.SaveChangesAsync(ct);
        return PromotionMapper.ToResponse(p);
    }
}

public class PausePromotionCommandHandler
    : IRequestHandler<PausePromotionCommand, PromotionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public PausePromotionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<PromotionResponse> Handle(PausePromotionCommand request, CancellationToken ct)
    {
        var p = await _uow.Promotions.GetByIdForTenantAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("Promotion", request.Id);
        p.Pause();
        _uow.Promotions.Update(p);
        await _uow.SaveChangesAsync(ct);
        return PromotionMapper.ToResponse(p);
    }
}

public class CancelPromotionCommandHandler
    : IRequestHandler<CancelPromotionCommand, PromotionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public CancelPromotionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<PromotionResponse> Handle(CancelPromotionCommand request, CancellationToken ct)
    {
        var p = await _uow.Promotions.GetByIdForTenantAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("Promotion", request.Id);
        p.Cancel();
        _uow.Promotions.Update(p);
        await _uow.SaveChangesAsync(ct);
        return PromotionMapper.ToResponse(p);
    }
}
