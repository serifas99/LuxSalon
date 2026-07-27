using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class FrizerController : BaseCRUDController<FrizerResponse, FrizerSearchObject, FrizerInsertRequest, FrizerUpdateRequest, IFrizerService>
{
    public FrizerController(IFrizerService service) : base(service)
    {
    }

    public override Task<PageResult<FrizerResponse>> GetAll([FromQuery] FrizerSearchObject? search)
    {
        return base.GetAll(search);
    }
}
