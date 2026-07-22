using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

public class UslugaController : BaseCRUDController<UslugaResponse, UslugaSearchObject, UslugaInsertRequest, UslugaUpdateRequest, IUslugaService>
{
    public UslugaController(IUslugaService service) : base(service)
    {
    }

    // Javno dostupno - klijenti moraju moci pregledati ponudu usluga bez logina
    [AllowAnonymous]
    public override Task<PageResult<UslugaResponse>> GetAll([FromQuery] UslugaSearchObject? search)
    {
        return base.GetAll(search);
    }
}
