using MediatR;
using YCT.Application.Common;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Products.Delete;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ResponseBase<bool>>
{
    private readonly IGenericRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IGenericRepository<Product> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseBase<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product == null)
            return ResponseBase<bool>.Fail("Producto no encontrado");

        await _repository.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ResponseBase<bool>.Ok(true, "Producto eliminado exitosamente");
    }
}
