using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class UslugaKategorijaController : BaseCRUDController<UslugaKategorijaResponse, UslugaKategorijaSearchObject, UslugaKategorijaInsertRequest, UslugaKategorijaUpdateRequest, IUslugaKategorijaService>
{
    public UslugaKategorijaController(IUslugaKategorijaService service) : base(service)
    {
    }

    public override Task<PageResult<UslugaKategorijaResponse>> GetAll([FromQuery] UslugaKategorijaSearchObject? search)
    {
        return base.GetAll(search);
    }
}
