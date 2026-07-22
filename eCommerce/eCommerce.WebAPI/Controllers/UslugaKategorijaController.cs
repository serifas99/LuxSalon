using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

public class UslugaKategorijaController : BaseCRUDController<UslugaKategorijaResponse, UslugaKategorijaSearchObject, UslugaKategorijaInsertRequest, UslugaKategorijaUpdateRequest, IUslugaKategorijaService>
{
    public UslugaKategorijaController(IUslugaKategorijaService service) : base(service)
    {
    }

    [AllowAnonymous]
    public override Task<PageResult<UslugaKategorijaResponse>> GetAll([FromQuery] UslugaKategorijaSearchObject? search)
    {
        return base.GetAll(search);
    }
}
