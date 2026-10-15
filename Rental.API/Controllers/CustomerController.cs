using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;

namespace MyProject.Controllers
{
    // ניהול לקוחות - למנהל בלבד. לקוח חדש נרשם דרך /api/auth/register
    [Route("api/customers")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class CustomerController : ApiControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll([FromQuery] CustomerQueryParameters query, CancellationToken ct)
        {
            return Ok(await _customerService.GetAllAsync(query, ct));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _customerService.GetByIdAsync(id, ct);
            return result.IsSuccess ? Ok(result.Value) : Failure(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerRequestDto dto, CancellationToken ct)
        {
            var result = await _customerService.CreateAsync(dto, ct);
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
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] CustomerRequestDto dto, CancellationToken ct)
        {
            var result = await _customerService.UpdateAsync(id, dto, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var result = await _customerService.DeleteAsync(id, ct);
            return result.IsSuccess ? NoContent() : Failure(result);
        }
    }
}
