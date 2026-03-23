using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Categories;
using RestaurantManagement.Application.DTOs.Products;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

public sealed class ProductsController : BranchScopedController
{
    private readonly IProductService _service;
    private readonly ICategoryService _categoryService;

    public ProductsController(
        IProductService service,
        ICategoryService categoryService,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var products = branchId == Guid.Empty
            ? Array.Empty<ProductDto>()
            : await _service.GetByBranchAsync(branchId, cancellationToken);

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CreateProductDto { BranchId = branchId };
        await LoadCategoryOptionsAsync(branchId, null, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto request, CancellationToken cancellationToken)
    {
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.CreateAsync(request, cancellationToken);
            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCategoryOptionsAsync(request.BranchId, request.CategoryId, cancellationToken);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var model = await _service.GetForEditAsync(branchId, id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = "Product was not found.";
            return RedirectToAction(nameof(Index));
        }

        await LoadCategoryOptionsAsync(branchId, model.CategoryId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateProductDto request, CancellationToken cancellationToken)
    {
        request.Id = id;
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.UpdateAsync(request, cancellationToken);
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCategoryOptionsAsync(request.BranchId, request.CategoryId, cancellationToken);
            return View(request);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadCategoryOptionsAsync(Guid branchId, Guid? selectedCategoryId, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetByBranchAsync(branchId, cancellationToken);
        ViewBag.Categories = new SelectList(categories, nameof(CategoryDto.Id), nameof(CategoryDto.NameEn), selectedCategoryId);
    }
}
