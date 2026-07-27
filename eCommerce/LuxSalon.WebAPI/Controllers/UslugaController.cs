using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class UslugaController : BaseCRUDController<UslugaResponse, UslugaSearchObject, UslugaInsertRequest, UslugaUpdateRequest, IUslugaService>
{
    public UslugaController(IUslugaService service) : base(service)
    {
    }

    public override Task<PageResult<UslugaResponse>> GetAll([FromQuery] UslugaSearchObject? search)
    {
        return base.GetAll(search);
    }
}
