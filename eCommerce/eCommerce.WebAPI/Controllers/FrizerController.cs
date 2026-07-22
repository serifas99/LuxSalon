using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

public class FrizerController : BaseCRUDController<FrizerResponse, FrizerSearchObject, FrizerInsertRequest, FrizerUpdateRequest, IFrizerService>
{
    public FrizerController(IFrizerService service) : base(service)
    {
    }

    // Javno dostupno - klijenti biraju frizera prilikom zakazivanja bez logina
    [AllowAnonymous]
    public override Task<PageResult<FrizerResponse>> GetAll([FromQuery] FrizerSearchObject? search)
    {
        return base.GetAll(search);
    }
}
