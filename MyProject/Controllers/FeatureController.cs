using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;

namespace MyProject.Controllers
{
    [Route("api/features")]
    [AllowAnonymous]
    public class FeatureController : ApiControllerBase
    {
        private readonly ICarService _carService;

        public FeatureController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FeatureDto>>> GetAll(CancellationToken ct)
        {
            return Ok(await _carService.GetFeaturesAsync(ct));
        }
    }
}
