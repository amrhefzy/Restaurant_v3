using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Categories;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;

    public CategoryService(
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository
            .Query()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.NameEn)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public Task<CreateCategoryDto?> GetForCreateAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CreateCategoryDto?>(new CreateCategoryDto
        {
            BranchId = branchId
        });
    }

    public async Task<UpdateCategoryDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _categoryRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        return entity is null ? null : _mapper.Map<UpdateCategoryDto>(entity);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = _mapper.Map<Category>(request);

        await _categoryRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _categoryRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Category was not found for this branch.");

        _mapper.Map(request, entity);
        _categoryRepository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(entity);
    }
}
