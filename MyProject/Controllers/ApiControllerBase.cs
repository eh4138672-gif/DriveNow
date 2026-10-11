using Microsoft.AspNetCore.Mvc;
using MyProject.Core.Results;

namespace MyProject.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        // מתרגם כישלון משכבת ה-Service לתשובת ProblemDetails עם הסטטוס המתאים
        protected ActionResult Failure(Result result)
        {
            var statusCode = result.Status switch
            {
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
                ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(detail: result.Error, statusCode: statusCode);
        }
    }
}
