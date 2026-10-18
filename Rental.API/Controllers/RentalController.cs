using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;

namespace MyProject.Controllers
{
    // כל משתמש מחובר. ההפרדה בין לקוח למנהל (מי רואה ומשנה איזו השכרה) נאכפת ב-RentalService
    [Route("api/rentals")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class RentalController : ApiControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<RentalDto>>> GetAll([FromQuery] RentalQueryParameters query, CancellationToken ct)
        {
            return Ok(await _rentalService.GetAllAsync(query, ct));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RentalDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _rentalService.GetByIdAsync(id, ct);
            return result.IsSuccess ? Ok(result.Value) : Failure(result);
        }

        // הזמנת רכב - הפעולה על המשאב המוגבל. 409 אם הרכב תפוס או נתפס במקביל
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RentalDto>> Create([FromBody] RentalRequestDto dto, CancellationToken ct)
        {
            var result = await _rentalService.CreateAsync(dto, ct);
            if (!result.IsSuccess)
            {
                return Failure(result);
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        // תשלום על השכרה קיימת (סימולציה). 402 אם הכרטיס נדחה, 409 אם כבר שולמה
        [HttpPost("{id:int}/pay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RentalDto>> Pay([FromRoute] int id, [FromBody] PaymentDto payment, CancellationToken ct)
        {
            var result = await _rentalService.PayAsync(id, payment, ct);
            return result.IsSuccess ? Ok(result.Value) : Failure(result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] RentalRequestDto dto, CancellationToken ct)
        {
            var result = await _rentalService.UpdateAsync(id, dto, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var result = await _rentalService.DeleteAsync(id, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }
    }
}
