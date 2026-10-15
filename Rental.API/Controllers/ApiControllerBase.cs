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

            var response = Problem(detail: result.Error, statusCode: statusCode);

            // traceId זהה ל-CorrelationId שבלוגים וב-header, כדי שאפשר יהיה לאתר את הבקשה
            if (response.Value is ProblemDetails problem)
            {
                problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            }
            return response;
        }
    }
}
