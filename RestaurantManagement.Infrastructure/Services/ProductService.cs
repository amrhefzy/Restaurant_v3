using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Products;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductService(
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _productRepository
            .Query()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.NameEn)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<UpdateProductDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _productRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        return entity is null ? null : _mapper.Map<UpdateProductDto>(entity);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateCategoryAsync(request.BranchId, request.CategoryId, cancellationToken);

        var entity = _mapper.Map<Product>(request);

        await _productRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(entity);
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateCategoryAsync(request.BranchId, request.CategoryId, cancellationToken);

        var entity = await _productRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Product was not found for this branch.");

        _mapper.Map(request, entity);
        _productRepository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(entity);
    }

    private async Task ValidateCategoryAsync(Guid branchId, Guid categoryId, CancellationToken cancellationToken)
    {
        var exists = await _categoryRepository.Query()
            .AnyAsync(x => x.Id == categoryId && x.BranchId == branchId && x.IsActive, cancellationToken);

        if (!exists)
        {
            throw new ValidationException("Selected category is invalid for this branch.");
        }
    }
}
