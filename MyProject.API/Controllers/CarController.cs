using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;

namespace MyProject.Controllers
{
    // צפייה ברכבים פתוחה לכולם. הוספה, עדכון, הוצאה מהשכרה ומחיקה - למנהל בלבד
    [Route("api/cars")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class CarController : ApiControllerBase
    {
        private readonly ICarService _carService;

        public CarController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CarDto>>> GetAll([FromQuery] CarQueryParameters query, CancellationToken ct)
        {
            return Ok(await _carService.GetAllAsync(query, ct));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _carService.GetByIdAsync(id, ct);
            return result.IsSuccess ? Ok(result.Value) : Failure(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CarDto>> Create([FromBody] CarRequestDto dto, CancellationToken ct)
        {
            var result = await _carService.CreateAsync(dto, ct);
            if (!result.IsSuccess)
            {
                return Failure(result);
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] CarUpdateDto dto, CancellationToken ct)
        {
            var result = await _carService.UpdateAsync(id, dto, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }

        // הוצאת רכב מההשכרה (IsActive = false) או החזרתו. רכב עם היסטוריית השכרות אי אפשר למחוק,
        // אבל אפשר להוציא אותו מההשכרה כך שלא יוצג ללקוחות ולא יהיה ניתן להזמנה
        [HttpPatch("{id:int}/availability")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SetAvailability([FromRoute] int id, [FromBody] CarAvailabilityDto dto, CancellationToken ct)
        {
            var result = await _carService.SetAvailabilityAsync(id, dto.IsActive!.Value, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var result = await _carService.DeleteAsync(id, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }
    }
}
