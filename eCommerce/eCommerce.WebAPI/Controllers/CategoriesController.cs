using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

public class CategoriesController : BaseCRUDController<CategoryResponse, CategorySearchObject, CategoriesInsertRequest, CategoriesUpdateRequest, ICategoryService>
{
    public CategoriesController(ICategoryService categoryService) : base(categoryService)
    {
    }

    [AllowAnonymous]
    public override Task<PageResult<CategoryResponse>> GetAll([FromQuery] CategorySearchObject? search)
    {
        return base.GetAll(search);
    }
}