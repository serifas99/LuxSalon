using System;
using System.Collections.Generic;
using System.Linq;
using eCommerce.Model.Exceptions;
using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services.Database;
using FluentValidation;

namespace eCommerce.Services
{
    public class CategoryService : BaseCRUDService<Category, CategoryResponse, CategorySearchObject, CategoriesInsertRequest, CategoriesUpdateRequest>, ICategoryService
    {
        // dummy in-memory collection with some hierarchical categories
      
        public CategoryService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<CategoriesInsertRequest> insertValidator, IValidator<CategoriesUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override IEnumerable<Category> ApplyFilters(IEnumerable<Category> query, CategorySearchObject? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Name))
                {
                    query = query.Where(c => c.Name.Contains(search.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (search.ParentCategoryId.HasValue)
                {
                    query = query.Where(c => c.ParentCategoryId == search.ParentCategoryId.Value);
                }
            }

            return query;
        }

       
    }
}