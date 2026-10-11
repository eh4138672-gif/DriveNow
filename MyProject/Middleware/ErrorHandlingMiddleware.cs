using Microsoft.AspNetCore.Mvc;

namespace MyProject.Middleware
{
    // הראשון ב-pipeline: תופס כל חריגה שלא טופלה, כותב אותה ללוג ברמת Error
    // ומחזיר תשובת JSON אחידה (ProblemDetails) בלי לחשוף פרטים פנימיים
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // הלקוח סגר את החיבור. זו לא שגיאה של השרת
                _logger.LogInformation("Request {Path} was cancelled by the client", context.Request.Path);
            }
            catch (Exception ex)
            {
                // ה-scope של CorrelationIdMiddleware כבר נסגר כשהחריגה הגיעה לכאן, ולכן פותחים אותו מחדש
                using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = context.TraceIdentifier }))
                {
                    _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                }

                if (context.Response.HasStarted)
                {
                    throw;
                }

                var problem = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Title = "שגיאה פנימית בשרת",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "אירעה שגיאה לא צפויה. אפשר לפנות לתמיכה עם מזהה הבקשה (traceId)."
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
            }
        }
    }
}
