using Microsoft.AspNetCore.Mvc;

namespace MyProject.Middleware
{
    // כותב תשובת שגיאה אחידה (ProblemDetails) גם מחוץ ל-Controllers - למשל 401/403 של ה-JWT.
    // traceId הוא תמיד ה-CorrelationId של הבקשה, כמו בשאר תשובות השגיאה
    public static class ProblemResponse
    {
        public static Task WriteAsync(HttpContext context, int statusCode, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }
}
