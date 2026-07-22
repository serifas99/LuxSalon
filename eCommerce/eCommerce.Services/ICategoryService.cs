using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;

namespace eCommerce.Services
{
    public interface ICategoryService : IBaseCRUDService<CategoryResponse, CategorySearchObject, CategoriesInsertRequest, CategoriesUpdateRequest>
    {

    }
}